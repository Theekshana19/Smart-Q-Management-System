import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminServiceItem, TokenHistoryFilter, TokenHistoryReport, TokenHistoryRow } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, AdminErrorBannerComponent],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);

  readonly report = signal<TokenHistoryReport | null>(null);
  readonly services = signal<AdminServiceItem[]>([]);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);

  dateFrom = '';
  dateTo = '';
  selectedServiceId?: number;
  selectedSubServiceId?: number;
  selectedCounterId?: number;
  selectedStatus?: string;

  readonly currentPage = signal(1);
  readonly pageSize = 50;

  readonly rows = computed(() => this.report()?.items ?? []);
  readonly summary = computed(() => this.report()?.summary ?? null);
  readonly totalCount = computed(() => this.report()?.totalCount ?? 0);

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  readonly pages = computed(() =>
    Array.from({ length: Math.min(this.totalPages(), 10) }, (_, i) => i + 1));

  ngOnInit(): void {
    this.adminApi.getServices({ page: 1, pageSize: 100 }).subscribe({
      next: r => this.services.set(r.items),
      error: () => {}
    });
    this.applyFilters();
  }

  applyFilters(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.currentPage.set(1);

    const filter: TokenHistoryFilter = {
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined,
      serviceId: this.selectedServiceId,
      subServiceId: this.selectedSubServiceId,
      counterId: this.selectedCounterId,
      status: this.selectedStatus,
      page: this.currentPage(),
      pageSize: this.pageSize
    };

    this.adminApi.getTokenHistory(filter).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: err => {
        const e = AdminApiService.parseError(err);
        this.errorMsg.set(e.message);
        this.errorStatus.set(e.status);
        this.loading.set(false);
      }
    });
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    this.loading.set(true);
    const filter: TokenHistoryFilter = {
      dateFrom: this.dateFrom || undefined,
      dateTo: this.dateTo || undefined,
      serviceId: this.selectedServiceId,
      subServiceId: this.selectedSubServiceId,
      counterId: this.selectedCounterId,
      status: this.selectedStatus,
      page,
      pageSize: this.pageSize
    };
    this.adminApi.getTokenHistory(filter).subscribe({
      next: r => { this.report.set(r); this.loading.set(false); },
      error: err => {
        const e = AdminApiService.parseError(err);
        this.errorMsg.set(e.message);
        this.loading.set(false);
      }
    });
  }

  resetFilters(): void {
    this.dateFrom = '';
    this.dateTo = '';
    this.selectedServiceId = undefined;
    this.selectedSubServiceId = undefined;
    this.selectedCounterId = undefined;
    this.selectedStatus = undefined;
    this.applyFilters();
  }

  statusClass(status: string): string {
    const s = status.toUpperCase();
    if (s === 'COMPLETED') return 'badge-completed';
    if (s === 'SKIPPED' || s === 'ABANDONED') return 'badge-skipped';
    if (s === 'SERVING' || s === 'CALLED') return 'badge-serving';
    return 'badge-waiting';
  }

  insightClass(tone: string): string {
    if (tone === 'positive') return 'insight-positive';
    if (tone === 'warning') return 'insight-warning';
    return 'insight-info';
  }

  waitTime(row: TokenHistoryRow): string {
    if (row.waitingMinutes == null) return '--';
    return `${row.waitingMinutes.toFixed(1)}m`;
  }

  exportCsv(): void {
    const rows = this.report()?.items ?? [];
    if (!rows.length) return;
    const header = ['Token No', 'Service', 'Sub-Service', 'Counter', 'Created', 'Status', 'Wait (min)'];
    const csvRows = [header, ...rows.map(r => [
      r.tokenNo, r.serviceName, r.subServiceName, r.counterName ?? '',
      r.createdAt, r.status, r.waitingMinutes ?? ''
    ])];
    const blob = new Blob([csvRows.map(row => row.join(',')).join('\n')], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `SmartQ_Report_${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  exportPdf(): void {
    import('jspdf').then(({ jsPDF }) => {
      import('jspdf-autotable').then(({ default: autoTable }) => {
        const doc = new jsPDF();
        doc.setFontSize(16);
        doc.text('SmartQ Token History Report', 14, 18);
        const s = this.summary();
        if (s) doc.text(`Total: ${s.totalTokens}  Completed: ${s.completed}  Avg Wait: ${s.averageWaitMinutes.toFixed(1)}m`, 14, 28);
        autoTable(doc, {
          head: [['Token', 'Service', 'Counter', 'Created', 'Status']],
          body: (this.report()?.items ?? []).map(row => [
            row.tokenNo, row.serviceName, row.counterName ?? '--',
            new Date(row.createdAt).toLocaleString(), row.status
          ]),
          startY: 36,
          styles: { fontSize: 9 },
          headStyles: { fillColor: [0, 106, 102] }
        });
        doc.save(`SmartQ_Report_${new Date().toISOString().slice(0, 10)}.pdf`);
      });
    });
  }
}
