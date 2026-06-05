import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of, catchError } from 'rxjs';
import { AuthService } from './auth.service';

export const staffCounterGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    router.navigate(['/staff/login']);
    return false;
  }

  const hasSession = () => {
    if (!auth.getActiveCounterSession()) {
      router.navigate(['/staff/select-counter']);
      return false;
    }
    return true;
  };

  if (auth.getActiveCounterSession()) return true;

  return auth.me().pipe(
    map(() => hasSession()),
    catchError(() => {
      auth.clearSession();
      router.navigate(['/staff/login']);
      return of(false);
    })
  );
};
