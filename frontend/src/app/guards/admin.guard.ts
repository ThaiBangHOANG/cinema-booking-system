import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// This guard improves navigation only; the ASP.NET Core API enforces access rights.
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.token()) {
    return router.createUrlTree(['/connexion'], {
      queryParams: { returnUrl: '/admin' }
    });
  }

  if (auth.user()?.role !== 'Admin') {
    return router.createUrlTree(['/acces-refuse']);
  }

  return true;
};
