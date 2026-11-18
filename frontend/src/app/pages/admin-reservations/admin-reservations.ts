import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AdminReservation } from '../../models/admin-reservation.model';
import { AdminReportsService } from '../../services/admin-reports.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-reservations',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './admin-reservations.html',
  styleUrl: './admin-reservations.css'
})
export class AdminReservationsPage implements OnInit {
  private readonly reports = inject(AdminReportsService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly items = signal<AdminReservation[]>([]);
  readonly total = signal(0);

  seanceIdText = '';
  limit = 50;

  ngOnInit(): void { this.load(); }

  load(): void {
    const text = this.seanceIdText.trim();
    let seanceId: number | null = null;
    if (text) {
      const parsed = Number(text);
      if (!Number.isSafeInteger(parsed) || parsed <= 0) {
        this.error.set('Entrez un identifiant de séance entier et positif.');
        return;
      }
      seanceId = parsed;
    }

    this.error.set('');
    this.loading.set(true);
    this.reports.reservations(seanceId, Number(this.limit)).pipe(
      finalize(() => this.loading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: result => {
        this.items.set(result.items);
        this.total.set(result.total);
      },
      error: error => this.handleError(error)
    });
  }

  private handleError(err: HttpErrorResponse): void {
    if (err.status === 401) {
      this.auth.logout();
      void this.router.navigate(['/connexion'], { queryParams: { returnUrl: '/admin' } });
    } else if (err.status === 403) {
      void this.router.navigateByUrl('/acces-refuse');
    } else if (err.status === 400) {
      this.error.set('Paramètres de recherche invalides.');
    } else {
      this.error.set('Impossible de charger les réservations. Vérifiez la connexion à l’API.');
    }
  }
}
