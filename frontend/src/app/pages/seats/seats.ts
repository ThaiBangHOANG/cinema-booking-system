import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { EMPTY, Subject, catchError, forkJoin, merge, switchMap, timer } from 'rxjs';
import { SeatAvailability, SeanceSeats } from '../../models/seat.model';
import { Seance } from '../../models/seance.model';
import { SeatsService } from '../../services/seats.service';
import { SeancesService } from '../../services/seances.service';

@Component({
  selector: 'app-seats-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './seats.html',
  styleUrl: './seats.css'
})
export class SeatsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
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
      const previous = this.selectedIds();
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
    if (!seat.isAvailable) return;
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
}
