import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of, catchError } from 'rxjs';
import { AuthService } from './auth.service';

export const roleGuard = (role: 'ADMIN' | 'STAFF'): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const loginPath = role === 'ADMIN' ? '/admin/login' : '/staff/login';

  if (!auth.isAuthenticated()) {
    router.navigate([loginPath]);
    return false;
  }

  const check = () => {
    if (!auth.hasRole(role)) {
      router.navigate([loginPath]);
      return false;
    }
    return true;
  };

  if (auth.getCurrentUser()) return check();

  return auth.me().pipe(
    map(() => check()),
    catchError(() => {
      auth.clearSession();
      router.navigate([loginPath]);
      return of(false);
    })
  );
};
