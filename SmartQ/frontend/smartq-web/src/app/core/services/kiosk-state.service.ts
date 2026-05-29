import { Injectable, signal } from '@angular/core';
import { Language } from '../models';

const LANG_KEY = 'smartq-selected-language';

@Injectable({ providedIn: 'root' })
export class KioskStateService {
  readonly selectedLanguage = signal<Language | null>(this.loadLanguage());
  readonly selectedServiceId = signal<number | null>(null);
  readonly selectedServiceName = signal<string>('');

  setLanguage(lang: Language): void {
    this.selectedLanguage.set(lang);
    sessionStorage.setItem(LANG_KEY, JSON.stringify(lang));
  }

  private loadLanguage(): Language | null {
    try {
      const raw = sessionStorage.getItem(LANG_KEY);
      return raw ? (JSON.parse(raw) as Language) : null;
    } catch {
      return null;
    }
  }

  setService(id: number, name: string): void {
    this.selectedServiceId.set(id);
    this.selectedServiceName.set(name);
  }

  reset(): void {
    this.selectedLanguage.set(null);
    this.selectedServiceId.set(null);
    this.selectedServiceName.set('');
    sessionStorage.removeItem(LANG_KEY);
  }
}
