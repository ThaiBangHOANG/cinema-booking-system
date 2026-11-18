import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Film } from '../../models/film.model';
import { FilmsService } from '../../services/films.service';

@Component({
  selector: 'app-film-detail-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './film-detail.html',
  styleUrl: './film-detail.css'
})
export class FilmDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly filmsService = inject(FilmsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly film = signal<Film | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      this.error.set('Identifiant du film invalide.');
      this.loading.set(false);
      return;
    }

    this.filmsService.getFilm(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: film => {
          this.film.set(film);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Ce film est introuvable ou le serveur API est indisponible.');
          this.loading.set(false);
        }
      });
  }
}
