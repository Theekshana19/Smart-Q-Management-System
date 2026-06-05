import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly key = 'smartq.auth.token';

  store(token: string): void {
    if (typeof localStorage !== 'undefined') localStorage.setItem(this.key, token);
  }

  get(): string | null {
    if (typeof localStorage === 'undefined') return null;
    return localStorage.getItem(this.key);
  }

  clear(): void {
    if (typeof localStorage !== 'undefined') localStorage.removeItem(this.key);
  }
}
