import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { LanguageApiService } from '../../../core/api/language-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { PublicConfigService } from '../../../core/services/public-config.service';
import { Language } from '../../../core/models';

interface WelcomeSlide {
  code: string;
  title: string;
  subtitle: string;
  footer: string;
}

const WELCOME_FALLBACK: WelcomeSlide[] = [
  { code: 'EN', title: 'Welcome to SmartQ Bank', subtitle: 'Please select your language to begin', footer: 'Touch your preferred language to continue' },
  { code: 'SI', title: 'SmartQ බැංකුවට සාදරයෙන් පිළිගනිමු', subtitle: 'ඉදිරියට යාමට ඔබේ භාෂාව තෝරන්න', footer: 'ඉදිරියට යාමට ඔබේ භාෂාව ස්පර්ශ කරන්න' },
  { code: 'TA', title: 'SmartQ வங்கிக்கு வரவேற்கிறோம்', subtitle: 'தொடர உங்கள் மொழியைத் தேர்ந்தெடுக்கவும்', footer: 'தொடர உங்கள் விருப்ப மொழியைத் தொடவும்' }
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
  private readonly publicConfig = inject(PublicConfigService);
  private readonly router = inject(Router);

  readonly brandName = signal('SmartQ Sri Lanka');
  readonly languages = signal<Language[]>([]);
  readonly welcomeSlides = signal<WelcomeSlide[]>(WELCOME_FALLBACK);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly now = signal(new Date());
  readonly slideIndex = signal(0);
  readonly textVisible = signal(true);

  private static readonly VISIBLE_MS = 1500;
  private static readonly FADE_MS = 650;

  private clockTimer?: ReturnType<typeof setInterval>;
  private carouselTimeout?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.clockTimer = setInterval(() => this.now.set(new Date()), 1000);
    this.startTextCarousel();

    forkJoin({
      settings: this.publicConfig.ensureSettings(),
      langs: this.languageApi.getLanguages()
    }).subscribe({
      next: ({ settings, langs }) => {
        this.brandName.set(settings['BRANCH_NAME'] ?? settings['BANK_NAME'] ?? 'SmartQ Sri Lanka');
        this.languages.set(langs);
        this.buildSlides(langs);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load languages');
        this.loading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.clockTimer) clearInterval(this.clockTimer);
    if (this.carouselTimeout) clearTimeout(this.carouselTimeout);
  }

  private buildSlides(langs: Language[]): void {
    const codes = langs.length ? langs.map(l => l.code.toUpperCase()) : ['EN', 'SI', 'TA'];
    const messageLoads = codes.reduce(
      (acc, code) => ({ ...acc, [code]: this.publicConfig.ensureMessages(code) }),
      {} as Record<string, ReturnType<PublicConfigService['ensureMessages']>>
    );
    forkJoin(messageLoads).subscribe({
      next: () => {
        const slides = codes.map(code => {
          const fb = WELCOME_FALLBACK.find(s => s.code === code) ?? WELCOME_FALLBACK[0];
          return {
            code,
            title: fb.title,
            subtitle: fb.subtitle,
            footer: this.publicConfig.getMessageForLang(code, 'KIOSK_LANGUAGE_HELP', fb.footer)
          };
        });
        this.welcomeSlides.set(slides.length ? slides : WELCOME_FALLBACK);
      },
      error: () => this.welcomeSlides.set(WELCOME_FALLBACK)
    });
  }

  private startTextCarousel(): void {
    const { VISIBLE_MS, FADE_MS } = LanguageSelectionComponent;
    const tick = () => {
      this.carouselTimeout = setTimeout(() => {
        this.textVisible.set(false);
        this.carouselTimeout = setTimeout(() => {
          const len = this.welcomeSlides().length || 1;
          this.slideIndex.update(i => (i + 1) % len);
          this.textVisible.set(true);
          this.carouselTimeout = setTimeout(tick, VISIBLE_MS);
        }, FADE_MS);
      }, VISIBLE_MS);
    };
    this.textVisible.set(true);
    this.carouselTimeout = setTimeout(tick, VISIBLE_MS);
  }

  currentSlide(): WelcomeSlide {
    const slides = this.welcomeSlides();
    return slides[this.slideIndex()] ?? slides[0] ?? WELCOME_FALLBACK[0];
  }

  selectLanguage(lang: Language): void {
    this.kioskState.setLanguage(lang);
    this.router.navigate(['/customer/services']);
  }
}
