import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { StaffPerformance, StaffTimelineItem } from '../../../core/models/staff-console.models';
import { ReportContext, StaffReportPdfService } from '../services/staff-report-pdf.service';
import { StaffConsoleApiService } from '../services/staff-console-api.service';
import { StaffStateService } from '../services/staff-state.service';

type PerformanceRange = 'today' | 'week';

interface ChartBarView {
  hourLabel: string;
  cashPct: number;
  accountPct: number;
  loanPct: number;
  hasData: boolean;
}

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './performance.component.html',
  styleUrl: './performance.component.scss'
})
export class PerformanceComponent implements OnInit {
  private readonly api = inject(StaffConsoleApiService);
  private readonly reportPdf = inject(StaffReportPdfService);
  readonly state = inject(StaffStateService);

  readonly data = signal<StaffPerformance | null>(null);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly range = signal<PerformanceRange>('today');
  readonly chartReady = signal(false);

  readonly timeline = computed(() => this.data()?.recentTimeline ?? []);

  readonly chartBars = computed(() => {
    const performance = this.data();
    const traffic = performance?.hourlyTraffic ?? [];
    if (!traffic.length) return [] as ChartBarView[];

    const maxTotal = Math.max(
      1,
      ...traffic.map((h) => h.cashCount + h.accountCount + h.loanCount)
    );

    return traffic.map((hour) => {
      const total = hour.cashCount + hour.accountCount + hour.loanCount;
      return {
        hourLabel: hour.hourLabel,
        cashPct: (hour.cashCount / maxTotal) * 100,
        accountPct: (hour.accountCount / maxTotal) * 100,
        loanPct: (hour.loanCount / maxTotal) * 100,
        hasData: total > 0
      };
    });
  });

  readonly hasChartActivity = computed(() => this.chartBars().some((b) => b.hasData));

  ngOnInit(): void {
    this.load();
  }

  toggleRange(): void {
    this.range.set(this.range() === 'today' ? 'week' : 'today');
    this.load();
  }

  exportPdf(): void {
    const performance = this.data();
    if (!performance) {
      this.error.set('No performance data to export.');
      return;
    }
    this.reportPdf.exportPerformancePdf(performance, this.buildReportContext(performance));
  }

  viewPdf(): void {
    const performance = this.data();
    if (!performance) {
      this.error.set('No performance data to preview.');
      return;
    }
    this.reportPdf.viewPerformancePdf(performance, this.buildReportContext(performance));
  }

  timelineIcon(eventType: string): string {
    const normalized = eventType.toUpperCase();
    if (normalized === 'SKIPPED') return 'block';
    if (normalized === 'BREAK') return 'pause';
    if (normalized === 'CALLED') return 'campaign';
    if (normalized === 'SERVING') return 'hourglass_top';
    return 'check';
  }

  timelineIconClass(eventType: string): string {
    const normalized = eventType.toUpperCase();
    if (normalized === 'SKIPPED') return 'skipped';
    if (normalized === 'BREAK') return 'break';
    if (normalized === 'CALLED') return 'called';
    if (normalized === 'SERVING') return 'serving';
    return 'completed';
  }

  timeAgo(timestamp: string): string {
    const date = new Date(timestamp);
    if (Number.isNaN(date.getTime())) return '--';

    const diffMs = Date.now() - date.getTime();
    const diffMinutes = Math.floor(diffMs / 60000);
    if (diffMinutes < 1) return 'Just now';
    if (diffMinutes < 60) return `${diffMinutes}m ago`;

    const diffHours = Math.floor(diffMinutes / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays}d ago`;
  }

  private buildReportContext(performance: StaffPerformance): ReportContext {
    const ctx = this.state.context();
    return {
      branchName: ctx?.counter.branchName ?? 'Branch',
      counterLabel: ctx ? `Counter ${ctx.counter.counterNo} — ${ctx.counter.counterName}` : 'Counter',
      staffName: performance.staffName,
      periodLabel: performance.reportDateLabel,
      filtersLabel: performance.rangeLabel
    };
  }

  private load(): void {
    this.loading.set(true);
    this.error.set('');
    this.chartReady.set(false);

    this.api
      .getPerformance(this.range())
      .subscribe({
        next: (res) => {
          this.data.set(this.normalizePerformance(res));
          this.loading.set(false);
          setTimeout(() => this.chartReady.set(true), 100);
        },
        error: () => {
          this.error.set('Unable to load performance analytics. Please check API connection.');
          this.loading.set(false);
        }
      });
  }

  private normalizePerformance(raw: StaffPerformance): StaffPerformance {
    const legacy = raw as StaffPerformance & {
      RecentTimeline?: StaffTimelineItem[];
      HourlyTraffic?: StaffPerformance['hourlyTraffic'];
      HourlyServed?: Array<{ hourLabel?: string; HourLabel?: string; servedCount?: number; ServedCount?: number }>;
    };
    const timelineRaw = raw.recentTimeline ?? legacy.RecentTimeline ?? [];
    const trafficRaw = this.normalizeHourlyTraffic(
      raw.hourlyTraffic ?? legacy.HourlyTraffic,
      raw.hourlyServed ?? legacy.HourlyServed
    );

    return {
      ...raw,
      recentTimeline: timelineRaw.map((item) => {
        const row = item as StaffTimelineItem & {
          EventType?: string;
          TokenNo?: string;
          Title?: string;
          Description?: string;
          MetricLabel?: string | null;
          MetricValue?: string | null;
          Timestamp?: string;
        };
        return {
          eventType: row.eventType ?? row.EventType ?? 'COMPLETED',
          tokenNo: row.tokenNo ?? row.TokenNo ?? '',
          title: row.title ?? row.Title ?? row.tokenNo ?? row.TokenNo ?? 'Activity',
          description: row.description ?? row.Description ?? '',
          metricLabel: row.metricLabel ?? row.MetricLabel,
          metricValue: row.metricValue ?? row.MetricValue,
          timestamp: row.timestamp ?? row.Timestamp ?? new Date().toISOString()
        };
      }),
      hourlyTraffic: trafficRaw
    };
  }

  private normalizeHourlyTraffic(
    traffic: StaffPerformance['hourlyTraffic'] | undefined,
    served: Array<{ hourLabel?: string; HourLabel?: string; servedCount?: number; ServedCount?: number }> | undefined
  ): StaffPerformance['hourlyTraffic'] {
    const mapTrafficPoint = (
      point: StaffPerformance['hourlyTraffic'][number] & {
        HourLabel?: string;
        CashCount?: number;
        AccountCount?: number;
        LoanCount?: number;
      }
    ) => ({
      hourLabel: point.hourLabel ?? point.HourLabel ?? '',
      cashCount: point.cashCount ?? point.CashCount ?? 0,
      accountCount: point.accountCount ?? point.AccountCount ?? 0,
      loanCount: point.loanCount ?? point.LoanCount ?? 0
    });

    if (traffic?.length) {
      return traffic.map(mapTrafficPoint).filter((p) => p.hourLabel);
    }

    if (served?.length) {
      return served.map((point) => ({
        hourLabel: point.hourLabel ?? point.HourLabel ?? '',
        cashCount: point.servedCount ?? point.ServedCount ?? 0,
        accountCount: 0,
        loanCount: 0
      })).filter((p) => p.hourLabel);
    }

    return Array.from({ length: 10 }, (_, index) => {
      const hour = 8 + index;
      return { hourLabel: `${hour.toString().padStart(2, '0')}:00`, cashCount: 0, accountCount: 0, loanCount: 0 };
    });
  }
}
