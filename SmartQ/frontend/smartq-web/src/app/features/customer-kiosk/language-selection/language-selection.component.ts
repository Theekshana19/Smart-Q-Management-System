import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { LanguageApiService } from '../../../core/api/language-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { Language } from '../../../core/models';

@Component({
  selector: 'app-language-selection',
  imports: [DatePipe],
  templateUrl: './language-selection.component.html',
  styleUrl: './language-selection.component.scss'
})
export class LanguageSelectionComponent implements OnInit {
  private readonly languageApi = inject(LanguageApiService);
  private readonly kioskState = inject(KioskStateService);
  private readonly router = inject(Router);

  readonly languages = signal<Language[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  now = new Date();

  ngOnInit(): void {
    setInterval(() => (this.now = new Date()), 1000);
    this.languageApi.getLanguages().subscribe({
      next: langs => { this.languages.set(langs); this.loading.set(false); },
      error: () => { this.error.set('Unable to load languages'); this.loading.set(false); }
    });
  }

  selectLanguage(lang: Language): void {
    this.kioskState.setLanguage(lang);
    this.router.navigate(['/customer/services']);
  }
}
