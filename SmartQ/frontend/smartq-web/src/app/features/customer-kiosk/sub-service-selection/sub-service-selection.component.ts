import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { ServiceApiService } from '../../../core/api/service-api.service';
import { TokenApiService } from '../../../core/api/token-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { KioskI18nService, KioskLabels } from '../../../core/services/kiosk-i18n.service';
import { SubServiceItem, KioskStatus } from '../../../core/models';

@Component({
  selector: 'app-sub-service-selection',
  imports: [RouterLink, DatePipe],
  templateUrl: './sub-service-selection.component.html',
  styleUrl: './sub-service-selection.component.scss'
})
export class SubServiceSelectionComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly serviceApi = inject(ServiceApiService);
  private readonly tokenApi = inject(TokenApiService);
  private readonly kioskState = inject(KioskStateService);
  private readonly i18n = inject(KioskI18nService);

  readonly subServices = signal<SubServiceItem[]>([]);
  readonly kioskStatus = signal<KioskStatus | null>(null);
  readonly loading = signal(true);
  readonly generating = signal(false);
  readonly error = signal('');
  serviceId = 0;
  serviceName = '';
  langCode = 'EN';
  labels!: KioskLabels;
  pageTitle = '';
  now = new Date();

  ngOnInit(): void {
    setInterval(() => (this.now = new Date()), 1000);
    const lang = this.kioskState.selectedLanguage();
    if (!lang) { this.router.navigate(['/customer/language']); return; }
    this.langCode = lang.code;
    this.labels = this.i18n.labels(this.langCode);
    this.serviceId = Number(this.route.snapshot.paramMap.get('serviceId'));
    this.serviceName = this.kioskState.selectedServiceName() || '';
    this.pageTitle = this.i18n.format(this.labels.selectSubTitle, { service: this.serviceName });
    this.serviceApi.getSubServices(this.serviceId, this.langCode).subscribe({
      next: s => { this.subServices.set(s); this.loading.set(false); },
      error: () => { this.error.set(this.labels.loadError); this.loading.set(false); }
    });
    this.serviceApi.getKioskStatus().subscribe(s => this.kioskStatus.set(s));
  }

  selectSub(sub: SubServiceItem): void {
    const lang = this.kioskState.selectedLanguage();
    if (!lang || this.generating()) return;
    this.generating.set(true);
    this.tokenApi.generate({
      languageId: lang.id,
      serviceId: this.serviceId,
      subServiceId: sub.id
    }).subscribe({
      next: res => this.router.navigate(['/customer/token-success', res.id]),
      error: () => { this.error.set(this.labels.loadError); this.generating.set(false); }
    });
  }
}
