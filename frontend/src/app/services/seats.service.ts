import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SeanceSeats } from '../models/seat.model';

@Injectable({ providedIn: 'root' })
export class SeatsService {
  private readonly http = inject(HttpClient);

  getSeats(seanceId: number): Observable<SeanceSeats> {
    return this.http.get<SeanceSeats>(`/api/seances/${seanceId}/seats`);
  }
}
