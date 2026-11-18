import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Film } from '../models/film.model';

@Injectable({ providedIn: 'root' })
export class FilmsService {
  private readonly http = inject(HttpClient);

  getFilms(): Observable<Film[]> {
    return this.http.get<Film[]>('/api/films');
  }

  getFilm(id: number): Observable<Film> {
    return this.http.get<Film>(`/api/films/${id}`);
  }
}
