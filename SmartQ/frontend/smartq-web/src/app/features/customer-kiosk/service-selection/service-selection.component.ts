import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ServiceApiService } from '../../../core/api/service-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { KioskI18nService, KioskLabels } from '../../../core/services/kiosk-i18n.service';
import { ServiceItem } from '../../../core/models';

@Component({
  selector: 'app-service-selection',
  imports: [RouterLink, DatePipe],
  templateUrl: './service-selection.component.html',
  styleUrl: './service-selection.component.scss'
})
export class ServiceSelectionComponent implements OnInit {
  private readonly serviceApi = inject(ServiceApiService);
  private readonly kioskState = inject(KioskStateService);
  private readonly i18n = inject(KioskI18nService);
  private readonly router = inject(Router);

  readonly services = signal<ServiceItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  now = new Date();
  langCode = 'EN';
  labels!: KioskLabels;

  ngOnInit(): void {
    setInterval(() => (this.now = new Date()), 1000);
    const lang = this.kioskState.selectedLanguage();
    if (!lang) { this.router.navigate(['/customer/language']); return; }
    this.langCode = lang.code;
    this.labels = this.i18n.labels(this.langCode);
    this.serviceApi.getServices(this.langCode).subscribe({
      next: s => { this.services.set(s); this.loading.set(false); },
      error: () => { this.error.set(this.labels.loadError); this.loading.set(false); }
    });
  }

  selectService(s: ServiceItem): void {
    this.kioskState.setService(s.id, s.name);
    this.router.navigate(['/customer/services', s.id, 'sub-services']);
  }
}
