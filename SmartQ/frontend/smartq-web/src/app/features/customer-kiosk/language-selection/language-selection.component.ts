import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { LanguageApiService } from '../../../core/api/language-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { Language } from '../../../core/models';

interface WelcomeSlide {
  code: string;
  title: string;
  subtitle: string;
  footer: string;
}

const WELCOME_SLIDES: WelcomeSlide[] = [
  {
    code: 'EN',
    title: 'Welcome to SmartQ Bank',
    subtitle: 'Please select your language to begin',
    footer: 'TOUCH YOUR PREFERRED LANGUAGE TO CONTINUE'
  },
  {
    code: 'SI',
    title: 'SmartQ බැංකුවට සාදරයෙන් පිළිගනිමු',
    subtitle: 'ඉදිරියට යාමට ඔබේ භාෂාව තෝරන්න',
    footer: 'ඉදිරියට යාමට ඔබේ භාෂාව ස්පර්ශ කරන්න'
  },
  {
    code: 'TA',
    title: 'SmartQ வங்கிக்கு வரவேற்கிறோம்',
    subtitle: 'தொடர உங்கள் மொழியைத் தேர்ந்தெடுக்கவும்',
    footer: 'தொடர உங்கள் விருப்ப மொழியைத் தொடவும்'
  }
];

@Component({
  selector: 'app-language-selection',
  imports: [DatePipe],
  templateUrl: './language-selection.component.html',
  styleUrl: './language-selection.component.scss'
})
export class LanguageSelectionComponent implements OnInit, OnDestroy {
  private readonly languageApi = inject(LanguageApiService);
  private readonly kioskState = inject(KioskStateService);
  private readonly router = inject(Router);

  readonly languages = signal<Language[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly now = signal(new Date());
  readonly welcomeSlides = WELCOME_SLIDES;
  readonly slideIndex = signal(0);
  /** Drives CSS fade-in / fade-out (no vertical slide). */
  readonly textVisible = signal(true);

  private static readonly VISIBLE_MS = 1500;
  private static readonly FADE_MS = 650;

  private clockTimer?: ReturnType<typeof setInterval>;
  private carouselTimeout?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.clockTimer = setInterval(() => this.now.set(new Date()), 1000);
    this.startTextCarousel();

    this.languageApi.getLanguages().subscribe({
      next: langs => { this.languages.set(langs); this.loading.set(false); },
      error: () => { this.error.set('Unable to load languages'); this.loading.set(false); }
    });
  }

  ngOnDestroy(): void {
    if (this.clockTimer) clearInterval(this.clockTimer);
    if (this.carouselTimeout) clearTimeout(this.carouselTimeout);
  }

  private startTextCarousel(): void {
    const { VISIBLE_MS, FADE_MS } = LanguageSelectionComponent;
    const tick = () => {
      this.carouselTimeout = setTimeout(() => {
        this.textVisible.set(false);
        this.carouselTimeout = setTimeout(() => {
          this.slideIndex.update(i => (i + 1) % WELCOME_SLIDES.length);
          this.textVisible.set(true);
          this.carouselTimeout = setTimeout(tick, VISIBLE_MS);
        }, FADE_MS);
      }, VISIBLE_MS);
    };
    this.textVisible.set(true);
    this.carouselTimeout = setTimeout(tick, VISIBLE_MS);
  }

  currentSlide(): WelcomeSlide {
    return WELCOME_SLIDES[this.slideIndex()];
  }

  selectLanguage(lang: Language): void {
    this.kioskState.setLanguage(lang);
    this.router.navigate(['/customer/services']);
  }
}
