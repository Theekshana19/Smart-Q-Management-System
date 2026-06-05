import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { ServiceApiService } from '../../../core/api/service-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { KioskI18nService, KioskLabels } from '../../../core/services/kiosk-i18n.service';
import { PublicConfigService } from '../../../core/services/public-config.service';
import { KioskStatus, ServiceItem } from '../../../core/models';

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
  private readonly publicConfig = inject(PublicConfigService);
  private readonly router = inject(Router);

  readonly services = signal<ServiceItem[]>([]);
  readonly kioskStatus = signal<KioskStatus | null>(null);
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

    forkJoin({
      messages: this.publicConfig.ensureMessages(this.langCode),
      services: this.serviceApi.getServices(this.langCode),
      status: this.serviceApi.getKioskStatus()
    }).subscribe({
      next: ({ services, status }) => {
        this.labels = this.i18n.labels(this.langCode, (k, fb) => this.publicConfig.getMessage(k, fb));
        this.services.set(services);
        this.kioskStatus.set(status);
        this.loading.set(false);
      },
      error: () => {
        this.labels = this.i18n.labels(this.langCode);
        this.error.set(this.labels.loadError);
        this.loading.set(false);
      }
    });
  }

  waitTimesDesc(): string {
    const mins = this.kioskStatus()?.averageWaitMinutes ?? 8;
    return `Current average wait time is approximately ${mins} minutes. We value your time.`;
  }

  staffActiveLabel(): string {
    const count = this.kioskStatus()?.activeStaffCount ?? 0;
    return `${count} Staff Active`;
  }

  selectService(s: ServiceItem): void {
    this.kioskState.setService(s.id, s.name);
    this.router.navigate(['/customer/services', s.id, 'sub-services']);
  }
}
