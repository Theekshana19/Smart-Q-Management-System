import { CommonModule, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StaffAssignedService, StaffTokenDetails, StaffTokenHistoryItem } from '../../../core/models/staff-console.models';
import { TokenDetailsDrawerComponent } from '../components/token-details-drawer/token-details-drawer.component';
import { ReportContext, StaffReportPdfService } from '../services/staff-report-pdf.service';
import { StaffConsoleApiService } from '../services/staff-console-api.service';
import { StaffStateService } from '../services/staff-state.service';

type DateRange = 'today' | 'yesterday' | 'thisWeek' | 'last30days' | 'custom';

@Component({
  selector: 'app-token-history',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe, TokenDetailsDrawerComponent],
  templateUrl: './token-history.component.html',
  styleUrl: './token-history.component.scss'
})
export class TokenHistoryComponent implements OnInit {
  private readonly api = inject(StaffConsoleApiService);
  private readonly reportPdf = inject(StaffReportPdfService);
  readonly state = inject(StaffStateService);

  readonly summary = this.state.summary;
  readonly services = computed(() => this.state.context()?.assignedServices ?? []);
  readonly items = signal<StaffTokenHistoryItem[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly dateRange = signal<DateRange>('today');
  readonly periodFrom = signal(this.todayParam());
  readonly periodTo = signal(this.todayParam());
  readonly maxDate = this.todayParam();
  readonly serviceId = signal<number | null>(null);
  readonly status = signal('');
  readonly page = signal(1);
  readonly pageSize = 5;
  readonly selectedDetails = signal<StaffTokenDetails | null>(null);
  readonly detailsLoading = signal(false);
  readonly drawerOpen = signal(false);

  readonly totalItems = computed(() => this.items().length);
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalItems() / this.pageSize)));
  readonly pagedItems = computed(() => {
    const start = (this.page() - 1) * this.pageSize;
    return this.items().slice(start, start + this.pageSize);
  });
  readonly rangeStart = computed(() => (this.totalItems() === 0 ? 0 : (this.page() - 1) * this.pageSize + 1));
  readonly rangeEnd = computed(() => Math.min(this.page() * this.pageSize, this.totalItems()));
  readonly pageNumbers = computed(() => Array.from({ length: this.totalPages() }, (_, i) => i + 1));
  readonly periodLabel = computed(() => this.formatPeriodLabel(this.periodFrom(), this.periodTo()));

  ngOnInit(): void {
    this.applyPreset('today');
    this.load();
  }

  reload(): void {
    this.state.loadAll();
    this.load();
  }

  onDateRangeChange(value: DateRange): void {
    if (value === 'custom') {
      this.dateRange.set('custom');
      this.page.set(1);
      this.load();
      return;
    }
    this.applyPreset(value);
    this.page.set(1);
    this.load();
  }

  onPeriodFromChange(value: string): void {
    if (!value) return;
    this.periodFrom.set(value);
    this.dateRange.set('custom');
    if (this.periodTo() < value) this.periodTo.set(value);
    this.page.set(1);
    this.load();
  }

  onPeriodToChange(value: string): void {
    if (!value) return;
    this.periodTo.set(value);
    this.dateRange.set('custom');
    if (value < this.periodFrom()) this.periodFrom.set(value);
    this.page.set(1);
    this.load();
  }

  onServiceChange(value: number | null): void {
    this.serviceId.set(value);
    this.page.set(1);
    this.load();
  }

  onStatusChange(value: string): void {
    this.status.set(value);
    this.page.set(1);
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');

    const from = this.periodFrom();
    const to = this.periodTo();
    if (!from || !to) {
      this.error.set('Please select a valid date period.');
      this.loading.set(false);
      return;
    }

    this.api.getTokenHistory({
      dateFrom: from,
      dateTo: to,
      status: this.status() || undefined,
      serviceId: this.serviceId() ?? undefined
    }).subscribe({
      next: (rows) => this.onRowsLoaded(rows),
      error: () => this.onLoadError()
    });
  }

  openDetails(tokenId: number): void {
    this.drawerOpen.set(true);
    this.detailsLoading.set(true);
    this.selectedDetails.set(null);
    this.api.getTokenDetails(tokenId).subscribe({
      next: (details) => {
        this.selectedDetails.set(details);
        this.detailsLoading.set(false);
      },
      error: () => {
        this.error.set('Unable to load token details.');
        this.detailsLoading.set(false);
        this.drawerOpen.set(false);
      }
    });
  }

  closeDetails(): void {
    this.drawerOpen.set(false);
    this.selectedDetails.set(null);
    this.detailsLoading.set(false);
  }

  exportPdf(): void {
    const rows = this.items();
    if (!rows.length) {
      this.error.set('No records to export for the selected period.');
      return;
    }
    this.reportPdf.exportTokenHistoryPdf(rows, this.buildReportContext(), this.buildExportSummary(rows));
  }

  viewPdf(): void {
    const rows = this.items();
    if (!rows.length) {
      this.error.set('No records to preview for the selected period.');
      return;
    }
    this.reportPdf.viewTokenHistoryPdf(rows, this.buildReportContext(), this.buildExportSummary(rows));
  }

  prevPage(): void {
    if (this.page() > 1) this.page.update((p) => p - 1);
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) this.page.update((p) => p + 1);
  }

  goToPage(page: number): void {
    this.page.set(page);
  }

  formatCalledTime(value?: string): string {
    if (!value) return '--';
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '--' : date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }

  statusClass(status: string): string {
    const normalized = status.toUpperCase();
    if (normalized === 'COMPLETED') return 'completed';
    if (normalized === 'SKIPPED') return 'skipped';
    return 'neutral';
  }

  serviceDotIndex(serviceType: string): number {
    const services = this.services();
    const idx = services.findIndex((s: StaffAssignedService) => s.name === serviceType);
    return idx >= 0 ? idx % 6 : Math.abs(this.hashCode(serviceType)) % 6;
  }

  private applyPreset(range: DateRange): void {
    this.dateRange.set(range);
    const today = this.startOfToday();
    let from = new Date(today);
    let to = new Date(today);

    if (range === 'yesterday') {
      from.setDate(from.getDate() - 1);
      to = new Date(from);
    } else if (range === 'thisWeek') {
      from.setDate(from.getDate() - 6);
    } else if (range === 'last30days') {
      from.setDate(from.getDate() - 29);
    }

    this.periodFrom.set(this.toDateParam(from));
    this.periodTo.set(this.toDateParam(to));
  }

  private buildReportContext(): ReportContext {
    const ctx = this.state.context();
    const serviceName = this.serviceId()
      ? this.services().find((s) => s.serviceId === this.serviceId())?.name ?? 'Selected service'
      : 'All Services';
    const statusLabel = this.status() || 'All Status';
    return {
      branchName: ctx?.counter.branchName ?? 'Branch',
      counterLabel: ctx ? `Counter ${ctx.counter.counterNo} — ${ctx.counter.counterName}` : 'Counter',
      staffName: ctx?.staff?.fullName,
      periodLabel: this.periodLabel(),
      filtersLabel: `${serviceName} • ${statusLabel}`
    };
  }

  private buildExportSummary(items: StaffTokenHistoryItem[]) {
    const completed = items.filter((i) => i.status.toUpperCase() === 'COMPLETED').length;
    const skipped = items.filter((i) => i.status.toUpperCase() === 'SKIPPED').length;
    const avg = this.dateRange() === 'today' && this.summary()
      ? this.summary()!.avgServiceTime
      : this.computeAvgDuration(items);
    return {
      served: items.length,
      completed,
      skipped,
      avgServiceTime: avg
    };
  }

  private formatPeriodLabel(from: string, to: string): string {
    if (from === to) {
      return this.formatDisplayDate(from);
    }
    return `${this.formatDisplayDate(from)} – ${this.formatDisplayDate(to)}`;
  }

  private formatDisplayDate(value: string): string {
    const date = new Date(`${value}T00:00:00`);
    if (Number.isNaN(date.getTime())) return value;
    return date.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' });
  }

  private computeAvgDuration(items: StaffTokenHistoryItem[]): string {
    const seconds = items
      .map((i) => this.parseDuration(i.duration))
      .filter((s): s is number => s !== null);
    if (!seconds.length) return '--';
    const avg = Math.round(seconds.reduce((a, b) => a + b, 0) / seconds.length);
    const mins = Math.floor(avg / 60);
    const sec = avg % 60;
    return `${mins}m ${sec.toString().padStart(2, '0')}s`;
  }

  private parseDuration(value: string): number | null {
    const match = value.match(/(\d+)m\s*(\d+)s/i);
    if (!match) return null;
    return Number(match[1]) * 60 + Number(match[2]);
  }

  private onRowsLoaded(rows: StaffTokenHistoryItem[]): void {
    this.items.set(rows);
    this.loading.set(false);
    this.error.set('');
  }

  private onLoadError(): void {
    this.error.set('Unable to load token history. Please check API connection.');
    this.loading.set(false);
  }

  private startOfToday(): Date {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return today;
  }

  private todayParam(): string {
    return this.toDateParam(this.startOfToday());
  }

  private toDateParam(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private hashCode(value: string): number {
    let hash = 0;
    for (let i = 0; i < value.length; i++) {
      hash = (hash << 5) - hash + value.charCodeAt(i);
      hash |= 0;
    }
    return hash;
  }
}
