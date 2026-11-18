import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, of, switchMap } from 'rxjs';
import { Seance } from '../../models/seance.model';
import { SeancesService } from '../../services/seances.service';

@Component({
  selector: 'app-seances-page',
  standalone: true,
  imports: [DatePipe, RouterLink],
  templateUrl: './seances.html',
  styleUrl: './seances.css'
})
export class SeancesPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly service = inject(SeancesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly seances = signal<Seance[]>([]);
  readonly selectedFilmId = signal<number | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly visibleCount = signal(24);
  readonly visibleSeances = computed(() =>
    this.seances().slice(0, this.visibleCount())
  );

  ngOnInit(): void {
    this.route.queryParamMap.pipe(
      switchMap(params => {
        this.loading.set(true);
        this.error.set('');
        this.seances.set([]);
        this.visibleCount.set(24);
        this.selectedFilmId.set(null);

        const rawId = params.get('filmId');
        let filmId: number | undefined;

        if (rawId !== null) {
          filmId = Number(rawId);
          if (!Number.isInteger(filmId) || filmId <= 0) {
            this.error.set('Identifiant de film invalide.');
            return of([] as Seance[]);
          }
          this.selectedFilmId.set(filmId);
        }

        return this.service.getSeances(filmId).pipe(
          catchError(() => {
            this.error.set('Impossible de charger les séances. Vérifiez que le serveur API fonctionne.');
            return of([] as Seance[]);
          })
        );
      }),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(seances => {
      this.seances.set(seances);
      this.loading.set(false);
    });
  }

  showMore(): void {
    this.visibleCount.update(count => count + 24);
  }
}
