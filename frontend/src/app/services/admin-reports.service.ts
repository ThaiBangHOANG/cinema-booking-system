import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminReservationList } from '../models/admin-reservation.model';
import { AffluenceQuery, AffluenceReport, OccupancySummary } from '../models/admin-statistics.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AdminReportsService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);

  reservations(seanceId: number | null, limit = 50): Observable<AdminReservationList> {
    let params = new HttpParams().set('limit', String(limit));
    if (seanceId !== null) params = params.set('seanceId', String(seanceId));
    return this.http.get<AdminReservationList>('/api/admin/reservations', {
      ...this.authorized(), params
    });
  }

  occupancy(): Observable<OccupancySummary> {
    return this.http.get<OccupancySummary>('/api/admin/statistics/occupancy', this.authorized());
  }

  affluence(query: AffluenceQuery): Observable<AffluenceReport> {
    let params = new HttpParams().set('days', String(query.days));
    if (query.festivalStart && query.festivalEnd) {
      params = params
        .set('festivalStart', query.festivalStart)
        .set('festivalEnd', query.festivalEnd);
    }
    return this.http.get<AffluenceReport>('/api/admin/statistics/affluence', {
      ...this.authorized(), params
    });
  }

  private authorized(): { headers: HttpHeaders } {
    const token = this.auth.token();
    return {
      headers: new HttpHeaders(token ? { Authorization: `Bearer ${token}` } : {})
    };
  }
}
