import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Ticket } from '../../models/ticket.model';
import { AuthService } from '../../services/auth.service';
import { ReservationsService } from '../../services/reservations.service';

@Component({
  selector: 'app-ticket-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './ticket.html',
  styleUrl: './ticket.css'
})
export class TicketPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly reservations = inject(ReservationsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly ticket = signal<Ticket | null>(null);
  readonly error = signal('');

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isSafeInteger(id) || id <= 0) {
      this.error.set('Numéro de réservation invalide.');
      this.loading.set(false);
      return;
    }

    if (!this.auth.token()) {
      this.redirectToLogin(id);
      return;
    }

    this.reservations.getTicket(id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: ticket => {
        this.ticket.set(ticket);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        if (error.status === 401) {
          this.auth.logout();
          this.redirectToLogin(id);
        } else if (error.status === 404) {
          // Do not reveal whether another user owns the requested ticket.
          this.error.set('Billet introuvable ou inaccessible avec ce compte.');
        } else if (error.status === 403) {
          this.error.set('Vous n’avez pas le droit de consulter ce billet.');
        } else {
          this.error.set('Impossible de charger le billet. Réessayez ultérieurement.');
        }
      }
    });
  }

  printTicket(): void {
    if (this.ticket()) {
      window.print();
    }
  }

  private redirectToLogin(id: number): void {
    void this.router.navigate(['/connexion'], {
      queryParams: { returnUrl: `/reservations/${id}/billet` }
    });
  }
}
