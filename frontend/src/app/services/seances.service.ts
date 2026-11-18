import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Seance } from '../models/seance.model';

@Injectable({ providedIn: 'root' })
export class SeancesService {
  private readonly http = inject(HttpClient);

  getSeances(filmId?: number): Observable<Seance[]> {
    const params = filmId === undefined
      ? new HttpParams()
      : new HttpParams().set('filmId', filmId.toString());

    return this.http.get<Seance[]>('/api/seances', { params });
  }
}
