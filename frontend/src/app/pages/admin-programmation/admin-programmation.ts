import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { Film } from '../../models/film.model';
import { AdminSeance, AdminSeanceRequest, RoomOption } from '../../models/admin-seance.model';
import { AdminProgrammationService } from '../../services/admin-programmation.service';
import { AuthService } from '../../services/auth.service';
import { FilmsService } from '../../services/films.service';

interface ProgrammationForm {
  filmId: number | null;
  roomId: number | null;
  startsAtLocal: string;
}

@Component({
  selector: 'app-admin-programmation',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './admin-programmation.html',
  styleUrl: './admin-programmation.css'
})
export class AdminProgrammationPage implements OnInit {
  private readonly service = inject(AdminProgrammationService);
  private readonly filmsService = inject(FilmsService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly films = signal<Film[]>([]);
  readonly seances = signal<AdminSeance[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly deletingId = signal<number | null>(null);
  readonly editingId = signal<number | null>(null);
  readonly error = signal('');
  readonly notice = signal('');
  readonly search = signal('');
  readonly upcomingOnly = signal(true);
  readonly displayedCount = signal(25);

  form: ProgrammationForm = this.emptyForm();

  readonly rooms = computed<RoomOption[]>(() => {
    // The current backend does not expose a standalone Rooms endpoint.
    // The 10 seeded rooms are obtained from their scheduled screenings.
    const distinct = new Map<number, RoomOption>();
    for (const s of this.seances()) {
      distinct.set(s.roomId, { id: s.roomId, name: s.roomName });
    }
    return Array.from(distinct.values()).sort((a, b) => a.id - b.id);
  });

  readonly filtered = computed(() => {
    const text = this.search().trim().toLowerCase();
    const futureOnly = this.upcomingOnly();
    const now = Date.now();
    return this.seances()
      .filter(s => !futureOnly || Date.parse(s.startsAtUtc) >= now)
      .filter(s => !text ||
        s.filmTitle.toLowerCase().includes(text) ||
        s.roomName.toLowerCase().includes(text) ||
        String(s.id).includes(text))
      .sort((a, b) => Date.parse(a.startsAtUtc) - Date.parse(b.startsAtUtc));
  });

  readonly visible = computed(() => this.filtered().slice(0, this.displayedCount()));

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.error.set('');
    this.loading.set(true);
    forkJoin({
      films: this.filmsService.getFilms(),
      seances: this.service.list()
    }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({ films, seances }) => {
        this.films.set(films);
        this.seances.set(seances);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        this.handleError(err, 'Impossible de charger la programmation.');
      }
    });
  }

  setSearch(value: string): void {
    this.search.set(value);
    this.displayedCount.set(25);
  }

  setUpcomingOnly(value: boolean): void {
    this.upcomingOnly.set(value);
    this.displayedCount.set(25);
  }

  showMore(): void {
    this.displayedCount.update(n => n + 25);
  }

  newSeance(): void {
    this.editingId.set(null);
    this.form = this.emptyForm();
    this.error.set('');
    this.notice.set('');
  }

  edit(s: AdminSeance): void {
    this.editingId.set(s.id);
    this.form = {
      filmId: s.filmId,
      roomId: s.roomId,
      startsAtLocal: this.toLocalInput(s.startsAtUtc)
    };
    this.error.set('');
    this.notice.set('');
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  save(): void {
    if (this.saving()) return;
    this.error.set('');
    this.notice.set('');

    const filmId = Number(this.form.filmId);
    const roomId = Number(this.form.roomId);
    const localDate = new Date(this.form.startsAtLocal);
    if (!Number.isInteger(filmId) || filmId <= 0 ||
        !Number.isInteger(roomId) || roomId <= 0 ||
        !Number.isFinite(localDate.getTime()) ||
        localDate.getTime() <= Date.now()) {
      this.error.set('Choisissez un film, une salle et une date future valides.');
      return;
    }

    const payload: AdminSeanceRequest = {
      filmId,
      roomId,
      startsAtUtc: localDate.toISOString() // UTC, with trailing Z
    };
    const id = this.editingId();
    const request$ = id === null
      ? this.service.create(payload)
      : this.service.update(id, payload);

    this.saving.set(true);
    request$.pipe(
      finalize(() => this.saving.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notice.set(id === null
          ? 'Seance creee avec succes.'
          : 'Seance modifiee avec succes.');
        this.editingId.set(null);
        this.form = this.emptyForm();
        this.loadSeances();
      },
      error: (err: HttpErrorResponse) => {
        this.handleError(err, "Impossible d'enregistrer la seance.");
      }
    });
  }

  remove(s: AdminSeance): void {
    if (this.deletingId() !== null || this.saving()) return;
    if (!window.confirm(`Supprimer la seance #${s.id} (${s.filmTitle}) ?`)) return;

    this.error.set('');
    this.notice.set('');
    this.deletingId.set(s.id);
    this.service.delete(s.id).pipe(
      finalize(() => this.deletingId.set(null)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.notice.set(`Seance #${s.id} supprimee.`);
        if (this.editingId() === s.id) this.newSeance();
        this.loadSeances();
      },
      error: (err: HttpErrorResponse) => {
        this.handleError(err, 'Impossible de supprimer la seance.');
      }
    });
  }

  private loadSeances(): void {
    this.service.list().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: seances => this.seances.set(seances),
      error: (err: HttpErrorResponse) => this.handleError(err, 'Actualisation impossible.')
    });
  }

  private handleError(err: HttpErrorResponse, fallback: string): void {
    if (err.status === 401) {
      this.auth.logout();
      void this.router.navigate(['/connexion'], {
        queryParams: { returnUrl: '/admin' }
      });
      return;
    }
    if (err.status === 403) {
      void this.router.navigateByUrl('/acces-refuse');
      return;
    }
    const serverMessage = err.error && typeof err.error.message === 'string'
      ? err.error.message
      : '';
    if (err.status === 409) {
      this.error.set(serverMessage || 'Conflit de programmation ou seance avec reservations.');
    } else if (err.status === 400) {
      this.error.set(serverMessage || 'Les donnees de la seance sont invalides.');
    } else if (err.status === 404) {
      this.error.set("La seance n'existe plus. Actualisez la liste.");
    } else {
      this.error.set(serverMessage || fallback);
    }
  }

  private emptyForm(): ProgrammationForm {
    return { filmId: null, roomId: null, startsAtLocal: '' };
  }

  private toLocalInput(utc: string): string {
    const date = new Date(utc);
    if (!Number.isFinite(date.getTime())) return '';
    const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return local.toISOString().slice(0, 16);
  }
}
