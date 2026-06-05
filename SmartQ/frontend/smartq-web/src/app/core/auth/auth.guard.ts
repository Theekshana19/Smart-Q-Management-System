import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of, catchError } from 'rxjs';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/admin/login']);
    return false;
  }

  if (auth.getCurrentUser()) return true;

  return auth.me().pipe(
    map(() => true),
    catchError(() => {
      auth.clearSession();
      router.navigate(['/admin/login']);
      return of(false);
    })
  );
};
