import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap, map, of, catchError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ActiveCounterSession, AuthUser, LoginResponse, MeResponse } from './auth.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly router = inject(Router);
  private readonly base = `${environment.apiUrl}/auth`;

  readonly currentUser = signal<AuthUser | null>(null);
  readonly activeCounterSession = signal<ActiveCounterSession | null>(null);

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.base}/login`, { username, password }).pipe(
      tap(res => {
        this.tokenStorage.store(res.accessToken);
        this.currentUser.set(res.user);
      })
    );
  }

  me(): Observable<MeResponse> {
    return this.http.get<MeResponse>(`${this.base}/me`).pipe(
      tap(res => {
        this.currentUser.set({
          id: res.id,
          fullName: res.fullName,
          username: res.username,
          role: res.role
        });
        this.activeCounterSession.set(res.activeCounterSession);
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<{ success: boolean }>(`${this.base}/logout`, {}).pipe(
      map(() => undefined),
      tap(() => this.clearSession()),
      catchError(() => {
        this.clearSession();
        return of(undefined);
      })
    );
  }

  clearSession(): void {
    this.tokenStorage.clear();
    this.currentUser.set(null);
    this.activeCounterSession.set(null);
  }

  isAuthenticated(): boolean {
    return !!this.tokenStorage.get();
  }

  hasRole(role: 'ADMIN' | 'STAFF'): boolean {
    return this.currentUser()?.role === role;
  }

  getCurrentUser(): AuthUser | null {
    return this.currentUser();
  }

  getActiveCounterSession(): ActiveCounterSession | null {
    return this.activeCounterSession();
  }

  setActiveCounterSession(session: ActiveCounterSession | null): void {
    this.activeCounterSession.set(session);
  }

  redirectAfterLogin(res: LoginResponse): void {
    if (res.user.role === 'ADMIN') {
      this.router.navigate(['/admin/dashboard']);
      return;
    }
    if (res.requiresCounterSelection) {
      this.router.navigate(['/staff/select-counter']);
      return;
    }
    this.me().subscribe(() => this.router.navigate(['/staff/queue-console']));
  }

  redirectAfterLogout(role?: string): void {
    this.router.navigate([role === 'ADMIN' ? '/admin/login' : '/staff/login']);
  }
}
