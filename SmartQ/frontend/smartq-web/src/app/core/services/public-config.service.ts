import { Injectable, inject, signal } from '@angular/core';
import { Observable, of, tap, map, shareReplay } from 'rxjs';
import { PublicConfigApiService } from '../api/public-config-api.service';

@Injectable({ providedIn: 'root' })
export class PublicConfigService {
  private readonly api = inject(PublicConfigApiService);
  private readonly settingsMap = signal<Record<string, string>>({});
  private readonly messagesMap = signal<Record<string, string>>({});
  private settingsLoaded = false;
  private messagesLang = '';
  private settingsRequest$: Observable<Record<string, string>> | null = null;
  private messagesRequests = new Map<string, Observable<Record<string, string>>>();
  private readonly messagesByLang = new Map<string, Record<string, string>>();

  loadSettings(): void {
    this.ensureSettings().subscribe();
  }

  ensureSettings(): Observable<Record<string, string>> {
    if (this.settingsLoaded) return of(this.settingsMap());
    if (!this.settingsRequest$) {
      this.settingsRequest$ = this.api.getPublicSettings().pipe(
        map(items => {
          const map: Record<string, string> = {};
          items.forEach(i => { map[i.key] = i.value; });
          return map;
        }),
        tap(map => {
          this.settingsMap.set(map);
          this.settingsLoaded = true;
        }),
        shareReplay(1)
      );
    }
    return this.settingsRequest$;
  }

  loadMessages(languageCode: string): void {
    this.ensureMessages(languageCode).subscribe();
  }

  ensureMessages(languageCode: string): Observable<Record<string, string>> {
    const code = languageCode.toUpperCase();
    const cached = this.messagesByLang.get(code);
    if (cached && Object.keys(cached).length > 0) {
      this.messagesMap.set(cached);
      this.messagesLang = code;
      return of(cached);
    }
    let req = this.messagesRequests.get(code);
    if (!req) {
      req = this.api.getPublicDisplayMessages(code).pipe(
        map(items => {
          const map: Record<string, string> = {};
          items.forEach(i => { map[i.messageKey] = i.messageText; });
          return map;
        }),
        tap(map => {
          this.messagesByLang.set(code, map);
          this.messagesMap.set(map);
          this.messagesLang = code;
        }),
        shareReplay(1)
      );
      this.messagesRequests.set(code, req);
    }
    return req;
  }

  getMessageForLang(languageCode: string, key: string, fallback = ''): string {
    return this.messagesByLang.get(languageCode.toUpperCase())?.[key] ?? fallback;
  }

  getSetting(key: string, fallback = ''): string {
    return this.settingsMap()[key] ?? fallback;
  }

  getBoolSetting(key: string, fallback = false): boolean {
    const v = this.getSetting(key, String(fallback));
    return v.toLowerCase() === 'true';
  }

  getIntSetting(key: string, fallback = 0): number {
    const n = Number(this.getSetting(key, String(fallback)));
    return Number.isFinite(n) ? n : fallback;
  }

  getMessage(key: string, fallback = ''): string {
    return this.messagesMap()[key] ?? fallback;
  }
}
