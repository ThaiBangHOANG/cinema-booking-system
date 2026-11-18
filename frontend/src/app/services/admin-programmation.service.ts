import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminSeance, AdminSeanceRequest } from '../models/admin-seance.model';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AdminProgrammationService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly url = '/api/admin/seances';

  list(): Observable<AdminSeance[]> {
    return this.http.get<AdminSeance[]>(this.url, this.options());
  }

  create(input: AdminSeanceRequest): Observable<unknown> {
    return this.http.post(this.url, input, this.options());
  }

  update(id: number, input: AdminSeanceRequest): Observable<void> {
    return this.http.put<void>(`${this.url}/${id}`, input, this.options());
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`, this.options());
  }

  private options(): { headers: HttpHeaders } {
    const token = this.auth.token();
    return {
      headers: new HttpHeaders(token ? { Authorization: `Bearer ${token}` } : {})
    };
  }
}
