import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { forkJoin } from 'rxjs';
import { TokenApiService } from '../../../core/api/token-api.service';
import { ServiceApiService } from '../../../core/api/service-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { KioskI18nService, KioskLabels } from '../../../core/services/kiosk-i18n.service';
import { PublicConfigService } from '../../../core/services/public-config.service';
import { GenerateTokenResponse, KioskStatus } from '../../../core/models';

@Component({
  selector: 'app-token-success',
  imports: [DatePipe],
  templateUrl: './token-success.component.html',
  styleUrl: './token-success.component.scss'
})
export class TokenSuccessComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tokenApi = inject(TokenApiService);
  private readonly serviceApi = inject(ServiceApiService);
  private readonly kioskState = inject(KioskStateService);
  private readonly i18n = inject(KioskI18nService);
  private readonly publicConfig = inject(PublicConfigService);

  readonly token = signal<GenerateTokenResponse | null>(null);
  readonly kioskStatus = signal<KioskStatus | null>(null);
  readonly loading = signal(true);
  readonly printEnabled = signal(true);
  labels: KioskLabels = this.i18n.labels('EN');
  langCode = 'EN';
  private autoReturnTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    const lang = this.kioskState.selectedLanguage();
    if (!lang) { this.router.navigate(['/customer/language']); return; }
    this.langCode = lang.code;

    const id = Number(this.route.snapshot.paramMap.get('tokenId'));
    forkJoin({
      settings: this.publicConfig.ensureSettings(),
      messages: this.publicConfig.ensureMessages(this.langCode),
      status: this.serviceApi.getKioskStatus(),
      token: this.tokenApi.getById(id)
    }).subscribe({
      next: ({ status, token }) => {
        this.labels = this.i18n.labels(this.langCode, (k, fb) => this.publicConfig.getMessage(k, fb));
        this.kioskStatus.set(status);
        this.printEnabled.set(this.publicConfig.getBoolSetting('ENABLE_PRINT_TOKEN', true));
        const secs = this.publicConfig.getIntSetting('KIOSK_AUTO_RETURN_SECONDS', 0);
        if (secs > 0) this.autoReturnTimer = setTimeout(() => this.finish(), secs * 1000);
        this.token.set({
          id: token.id,
          tokenNo: token.tokenNo,
          serviceName: token.serviceName,
          subServiceName: token.subServiceName,
          estimatedWaitMinutes: token.estimatedWaitMinutes,
          waitingBeforeYou: 0,
          createdAt: token.createdAt
        });
        this.loading.set(false);
      },
      error: () => this.router.navigate(['/customer/language'])
    });
  }

  ngOnDestroy(): void {
    if (this.autoReturnTimer) clearTimeout(this.autoReturnTimer);
  }

  print(): void { window.print(); }

  finish(): void {
    this.kioskState.reset();
    this.router.navigate(['/customer/language']);
  }
}
