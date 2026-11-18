import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EMPTY, Subject, catchError, finalize, forkJoin, merge, switchMap, timer } from 'rxjs';
import { SeatAvailability, SeanceSeats } from '../../models/seat.model';
import { ReservationResult } from '../../models/reservation.model';
import { Seance } from '../../models/seance.model';
import { SeatsService } from '../../services/seats.service';
import { SeancesService } from '../../services/seances.service';
import { AuthService } from '../../services/auth.service';
import { ReservationsService } from '../../services/reservations.service';

@Component({
  selector: 'app-seats-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './seats.html',
  styleUrl: './seats.css'
})
export class SeatsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);
  private readonly reservations = inject(ReservationsService);
  private readonly seatsService = inject(SeatsService);
  private readonly seancesService = inject(SeancesService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly refreshRequests$ = new Subject<void>();

  readonly loading = signal(true);
  readonly error = signal('');
  readonly notice = signal('');
  readonly seance = signal<Seance | null>(null);
  readonly availability = signal<SeanceSeats | null>(null);
  readonly selectedIds = signal<number[]>([]);
  readonly booking = signal(false);
  readonly bookingResult = signal<ReservationResult | null>(null);
  readonly maxSeats = 6;

  readonly rows = computed(() => {
    const groups = new Map<string, SeatAvailability[]>();
    for (const seat of this.availability()?.seats ?? []) {
      const rowName = seat.seatNumber.match(/^[A-Za-z]+/)?.[0] ?? '?';
      if (!groups.has(rowName)) groups.set(rowName, []);
      groups.get(rowName)!.push(seat);
    }
    return Array.from(groups, ([label, seats]) => ({ label, seats }));
  });

  readonly selectedSeatNumbers = computed(() => {
    const selected = new Set(this.selectedIds());
    return (this.availability()?.seats ?? [])
      .filter(seat => selected.has(seat.seatId))
      .map(seat => seat.seatNumber);
  });

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const seanceId = Number(params.get('id'));
        this.error.set('');
        this.notice.set('');
        this.loading.set(true);
        this.availability.set(null);
        this.seance.set(null);
        this.selectedIds.set([]);
        this.bookingResult.set(null);

        if (!Number.isInteger(seanceId) || seanceId <= 0) {
          this.error.set('Identifiant de séance invalide.');
          this.loading.set(false);
          return EMPTY;
        }

        // Refresh every 15 seconds and whenever the user clicks Actualiser.
        return merge(timer(0, 15000), this.refreshRequests$).pipe(
          switchMap(() => forkJoin({
            seats: this.seatsService.getSeats(seanceId),
            seance: this.seancesService.getSeance(seanceId)
          }).pipe(
            catchError((error: { status?: number }) => {
              this.error.set(error.status === 404
                ? 'Cette séance est introuvable.'
                : 'Impossible de charger les places. Vérifiez que l’API fonctionne.');
              this.loading.set(false);
              return EMPTY;
            })
          ))
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(({ seats, seance }) => {
      const stillAvailable = new Set(
        seats.seats.filter(seat => seat.isAvailable).map(seat => seat.seatId)
      );
      const pending = this.auth.consumePendingSelection(seats.seanceId);
      const previous = this.selectedIds().length ? this.selectedIds() : pending;
      const retained = previous.filter(id => stillAvailable.has(id));
      if (retained.length !== previous.length) {
        this.notice.set('Une place sélectionnée vient d’être réservée par un autre client.');
      }

      this.selectedIds.set(retained);
      this.availability.set(seats);
      this.seance.set(seance);
      this.error.set('');
      this.loading.set(false);
    });
  }

  isSelected(id: number): boolean {
    return this.selectedIds().includes(id);
  }

  toggleSeat(seat: SeatAvailability): void {
    if (!seat.isAvailable || this.booking() || this.bookingResult()) return;
    const current = this.selectedIds();
    this.notice.set('');
    if (current.includes(seat.seatId)) {
      this.selectedIds.set(current.filter(id => id !== seat.seatId));
    } else if (current.length >= this.maxSeats) {
      this.notice.set(`Maximum ${this.maxSeats} places par réservation.`);
    } else {
      this.selectedIds.set([...current, seat.seatId]);
    }
  }

  refresh(): void {
    this.refreshRequests$.next();
  }

  confirmReservation(): void {
    const seanceId = this.seance()?.id;
    const seatIds = [...this.selectedIds()];
    if (!seanceId || !seatIds.length || this.booking() || this.bookingResult()) return;

    this.error.set('');
    this.notice.set('');

    if (!this.auth.isAuthenticated()) {
      this.auth.setPendingSelection(seanceId, seatIds);
      void this.router.navigate(['/connexion'], {
        queryParams: { returnUrl: `/seances/${seanceId}/places` }
      });
      return;
    }

    this.booking.set(true);
    this.reservations.create({ seanceId, seatIds }).pipe(
      finalize(() => this.booking.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: result => {
        this.bookingResult.set(result);
        this.selectedIds.set([]);
        this.refresh();
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 409) {
          this.error.set('Une ou plusieurs places ont déjà été réservées. Choisissez de nouvelles places.');
          this.refresh();
        } else if (error.status === 401) {
          this.auth.setPendingSelection(seanceId, seatIds);
          this.auth.logout();
          this.notice.set('Session expirée : reconnectez-vous pour continuer.');
          void this.router.navigate(['/connexion'], {
            queryParams: { returnUrl: `/seances/${seanceId}/places` }
          });
        } else if (error.status === 403) {
          this.error.set('Seuls les comptes spectateur peuvent réserver des places.');
        } else if (error.status === 400) {
          this.error.set('Réservation impossible : vérifiez vos places et l’heure de la séance.');
        } else if (error.status === 404) {
          this.error.set('Cette séance n’existe plus.');
        } else {
          this.error.set('Impossible de confirmer la réservation pour le moment. Réessayez.');
        }
      }
    });
  }
}
