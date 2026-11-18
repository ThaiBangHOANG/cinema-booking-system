import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Film } from '../../models/film.model';
import { FilmsService } from '../../services/films.service';

@Component({
  selector: 'app-films-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './films.html',
  styleUrl: './films.css'
})
export class FilmsPage implements OnInit {
  private readonly filmsService = inject(FilmsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly films = signal<Film[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit(): void {
    this.filmsService.getFilms()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: films => {
          this.films.set(films);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Impossible de charger les films. Verifiez que le serveur API est demarre.');
          this.loading.set(false);
        }
      });
  }
}
