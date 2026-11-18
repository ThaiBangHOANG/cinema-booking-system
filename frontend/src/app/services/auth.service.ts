import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthResponse, RegisterResponse } from '../models/auth.model';

interface PendingSelection {
  seanceId: number;
  seatIds: number[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storageKey = 'grand-cinema-session';
  private readonly session = signal<AuthResponse | null>(this.restoreSession());
  private pendingSelection: PendingSelection | null = null;

  readonly user = this.session.asReadonly();
  readonly isAuthenticated = computed(() => this.user() !== null);

  register(email: string, password: string): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>('/api/auth/register', { email, password });
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', { email, password }).pipe(
      tap(response => {
        this.session.set(response);
        try {
          if (typeof sessionStorage !== 'undefined') {
            sessionStorage.setItem(this.storageKey, JSON.stringify(response));
          }
        } catch {
          // Private browsing restrictions: keep the token in memory only.
        }
      })
    );
  }

  token(): string | null {
    return this.session()?.token ?? null;
  }

  logout(): void {
    this.session.set(null);
    try {
      if (typeof sessionStorage !== 'undefined') {
        sessionStorage.removeItem(this.storageKey);
      }
    } catch {
      // Session may already be inaccessible.
    }
  }

  setPendingSelection(seanceId: number, seatIds: number[]): void {
    this.pendingSelection = { seanceId, seatIds: [...seatIds] };
  }

  consumePendingSelection(seanceId: number): number[] {
    if (this.pendingSelection?.seanceId !== seanceId) return [];
    const result = this.pendingSelection.seatIds;
    this.pendingSelection = null;
    return result;
  }

  private restoreSession(): AuthResponse | null {
    try {
      if (typeof sessionStorage === 'undefined') return null;
      const raw = sessionStorage.getItem(this.storageKey);
      if (!raw) return null;
      const stored = JSON.parse(raw) as AuthResponse;
      if (typeof stored?.token !== 'string' || typeof stored?.email !== 'string') {
        return null;
      }
      return stored;
    } catch {
      return null;
    }
  }
}
