import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { TokenApiService } from '../../../core/api/token-api.service';
import { ServiceApiService } from '../../../core/api/service-api.service';
import { KioskStateService } from '../../../core/services/kiosk-state.service';
import { KioskI18nService, KioskLabels } from '../../../core/services/kiosk-i18n.service';
import { GenerateTokenResponse, KioskStatus } from '../../../core/models';

@Component({
  selector: 'app-token-success',
  imports: [DatePipe],
  templateUrl: './token-success.component.html',
  styleUrl: './token-success.component.scss'
})
export class TokenSuccessComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly tokenApi = inject(TokenApiService);
  private readonly serviceApi = inject(ServiceApiService);
  private readonly kioskState = inject(KioskStateService);
  private readonly i18n = inject(KioskI18nService);

  readonly token = signal<GenerateTokenResponse | null>(null);
  readonly kioskStatus = signal<KioskStatus | null>(null);
  readonly loading = signal(true);
  private readonly i18nSvc = inject(KioskI18nService);
  labels: KioskLabels = this.i18nSvc.labels('EN');
  langCode = 'EN';

  ngOnInit(): void {
    const lang = this.kioskState.selectedLanguage();
    if (!lang) { this.router.navigate(['/customer/language']); return; }
    this.langCode = lang.code;
    this.labels = this.i18n.labels(this.langCode);

    const id = Number(this.route.snapshot.paramMap.get('tokenId'));
    this.serviceApi.getKioskStatus().subscribe(s => this.kioskStatus.set(s));
    this.tokenApi.getById(id).subscribe({
      next: t => {
        this.token.set({
          id: t.id,
          tokenNo: t.tokenNo,
          serviceName: t.serviceName,
          subServiceName: t.subServiceName,
          estimatedWaitMinutes: t.estimatedWaitMinutes,
          waitingBeforeYou: 0,
          createdAt: t.createdAt
        });
        this.loading.set(false);
      },
      error: () => this.router.navigate(['/customer/language'])
    });
  }

  print(): void { window.print(); }

  finish(): void {
    this.kioskState.reset();
    this.router.navigate(['/customer/language']);
  }
}
