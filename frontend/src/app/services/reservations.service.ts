import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { CreateReservationRequest, ReservationResult } from '../models/reservation.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ReservationsService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);

  create(request: CreateReservationRequest): Observable<ReservationResult> {
    const token = this.auth.token();
    if (!token) {
      return throwError(() => new Error('Connectez-vous avant de réserver.'));
    }

    return this.http.post<ReservationResult>('/api/reservations', request, {
      headers: { Authorization: `Bearer ${token}` }
    });
  }
}
