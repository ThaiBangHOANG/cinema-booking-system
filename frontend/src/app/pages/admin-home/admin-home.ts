import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

interface OccupancySummary {
  totalSeances: number;
  totalSeats: number;
  reservedSeats: number;
  availableSeats: number;
  occupancyRate: number;
}

@Component({
  selector: 'app-admin-home',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './admin-home.html',
  styleUrl: './admin-home.css'
})
export class AdminHomePage implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  readonly auth = inject(AuthService);

  readonly loading = signal(true);
  readonly error = signal('');
  readonly overview = signal<OccupancySummary | null>(null);

  ngOnInit(): void {
    const token = this.auth.token();
    if (!token) {
      void this.router.navigate(['/connexion'], {
        queryParams: { returnUrl: '/admin' }
      });
      return;
    }

    // Server-side role authorization proves the current JWT can access admin APIs.
    this.http.get<OccupancySummary>('/api/admin/statistics/occupancy', {
      headers: { Authorization: `Bearer ${token}` }
    }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: result => {
        this.overview.set(result);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        if (error.status === 401) {
          this.auth.logout();
          void this.router.navigate(['/connexion'], {
            queryParams: { returnUrl: '/admin' }
          });
        } else if (error.status === 403) {
          void this.router.navigateByUrl('/acces-refuse');
        } else {
          this.error.set('Impossible de charger les données administrateur. Vérifiez que l’API est démarrée.');
        }
      }
    });
  }
}
