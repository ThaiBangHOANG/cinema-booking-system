import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './auth.html',
  styleUrl: './auth.css'
})
export class AuthPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly mode = signal<'login' | 'register'>('login');
  readonly submitting = signal(false);
  readonly error = signal('');

  email = '';
  password = '';

  // Never accept external redirect URLs after a login.
  private readonly returnUrl = this.getSafeReturnUrl();

  setMode(mode: 'login' | 'register'): void {
    if (this.submitting()) return;
    this.mode.set(mode);
    this.error.set('');
  }

  submit(form: NgForm): void {
    if (this.submitting() || form.invalid) return;
    this.error.set('');
    this.submitting.set(true);

    const email = this.email.trim();
    const password = this.password;
    const request$ = this.mode() === 'register'
      ? this.auth.register(email, password).pipe(
          switchMap(() => this.auth.login(email, password))
        )
      : this.auth.login(email, password);

    request$.pipe(
      finalize(() => this.submitting.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        void this.router.navigateByUrl(this.returnUrl);
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 409) {
          this.error.set('Cette adresse e-mail est déjà utilisée.');
        } else if (error.status === 401) {
          this.error.set('Adresse e-mail ou mot de passe incorrect.');
        } else if (error.status === 400) {
          this.error.set('Vérifiez votre adresse e-mail et votre mot de passe (8 caractères minimum).');
        } else {
          this.error.set('Le service est momentanément indisponible. Réessayez.');
        }
      }
    });
  }

  private getSafeReturnUrl(): string {
    const raw = this.route.snapshot.queryParamMap.get('returnUrl');
    return raw && /^\/(?:seances\/\d+\/places|reservations\/\d+\/billet)$/.test(raw) ? raw : '/films';
  }
}
