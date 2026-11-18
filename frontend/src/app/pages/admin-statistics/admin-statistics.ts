import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { AffluenceReport, OccupancySummary } from '../../models/admin-statistics.model';
import { AdminReportsService } from '../../services/admin-reports.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-admin-statistics',
  standalone: true,
  imports: [DatePipe, DecimalPipe, FormsModule, RouterLink],
  templateUrl: './admin-statistics.html',
  styleUrl: './admin-statistics.css'
})
export class AdminStatisticsPage implements OnInit {
  private readonly reports = inject(AdminReportsService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly occupancy = signal<OccupancySummary | null>(null);
  readonly affluence = signal<AffluenceReport | null>(null);
  readonly hottestSeances = computed(() => [...(this.occupancy()?.seances ?? [])]
    .sort((a, b) => b.occupancyRate - a.occupancyRate)
    .slice(0, 12));

  days = 14;
  festivalStart = '';
  festivalEnd = '';

  ngOnInit(): void { this.load(); }

  load(): void {
    if ((this.festivalStart && !this.festivalEnd) || (!this.festivalStart && this.festivalEnd) ||
        (this.festivalStart && this.festivalEnd && this.festivalStart > this.festivalEnd)) {
      this.error.set('Sélectionnez deux dates de festival dans le bon ordre, ou laissez-les vides.');
      return;
    }
    this.error.set('');
    this.loading.set(true);
    const festival = this.festivalStart && this.festivalEnd
      ? { festivalStart: this.festivalStart, festivalEnd: this.festivalEnd }
      : {};

    forkJoin({
      occupancy: this.reports.occupancy(),
      affluence: this.reports.affluence({ days: Number(this.days), ...festival })
    }).pipe(
      finalize(() => this.loading.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: ({ occupancy, affluence }) => {
        this.occupancy.set(occupancy);
        this.affluence.set(affluence);
      },
      error: err => this.handleError(err)
    });
  }

  private handleError(err: HttpErrorResponse): void {
    if (err.status === 401) {
      this.auth.logout();
      void this.router.navigate(['/connexion'], { queryParams: { returnUrl: '/admin' } });
    } else if (err.status === 403) {
      void this.router.navigateByUrl('/acces-refuse');
    } else if (err.status === 400) {
      this.error.set('Les paramètres statistiques sont invalides.');
    } else {
      this.error.set('Impossible de charger les statistiques. Vérifiez la connexion à l’API.');
    }
  }
}
