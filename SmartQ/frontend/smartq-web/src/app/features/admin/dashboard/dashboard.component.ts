import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { DashboardSummary } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminErrorBannerComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);

  readonly data = signal<DashboardSummary | null>(null);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly activeTab = signal<'all' | 'vip'>('all');

  readonly filteredCounters = computed(() => {
    const d = this.data();
    if (!d) return [];
    return this.activeTab() === 'vip'
      ? d.counterStatuses.filter(c => c.isVip)
      : d.counterStatuses;
  });

  readonly chartBars = computed((): { hour: string; general: number; generalPct: number; priority: number; priorityPct: number }[] => {
    const d = this.data();
    if (!d) return [];
    const visible = d.hourlyFlow.filter(h => {
      const h24 = parseInt(h.hour.split(':')[0], 10);
      return h24 >= 8 && h24 <= 18;
    });
    const maxTotal = Math.max(1, ...visible.map(h => h.general + h.priority));
    return visible.map(h => ({
      hour: h.hour,
      general: h.general,
      priority: h.priority,
      generalPct: Math.round((h.general / maxTotal) * 100),
      priorityPct: Math.round((h.priority / maxTotal) * 100),
    }));
  });

  readonly donutConicGradient = computed(() => {
    const d = this.data();
    if (!d || !d.tokenDistribution.length) return 'conic-gradient(#e5eeff 0deg 360deg)';
    const total = Math.max(1, d.tokenDistribution.reduce((s, t) => s + t.count, 0));
    let cumDeg = 0;
    const segments: string[] = [];
    for (const item of d.tokenDistribution) {
      const deg = Math.round((item.count / total) * 360);
      segments.push(`${item.color} ${cumDeg}deg ${cumDeg + deg}deg`);
      cumDeg += deg;
    }
    if (cumDeg < 360) segments.push(`#e5eeff ${cumDeg}deg 360deg`);
    return `conic-gradient(${segments.join(', ')})`;
  });

  readonly processedPercent = computed(() => {
    const d = this.data();
    if (!d || !d.tokenDistribution.length) return 0;
    const total = d.tokenDistribution.reduce((s, t) => s + t.count, 0);
    const top = d.tokenDistribution[0]?.count ?? 0;
    return total === 0 ? 0 : Math.round((top / total) * 100);
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.adminApi.getDashboardSummary().subscribe({
      next: d => { this.data.set(d); this.loading.set(false); },
      error: err => {
        const e = AdminApiService.parseError(err);
        this.errorMsg.set(e.message);
        this.errorStatus.set(e.status);
        this.loading.set(false);
      }
    });
  }

  activityDot(type: string): string {
    if (type === 'error') return 'error-dot';
    if (type === 'info') return 'info-dot';
    return 'success-dot';
  }

  statusBadge(status: string): string {
    const s = status.toUpperCase();
    if (s === 'SERVING' || s === 'CALLED') return 'badge-serving';
    if (s === 'AVAILABLE') return 'badge-available';
    return 'badge-idle';
  }

  statusLabel(status: string): string {
    const s = status.toUpperCase();
    if (s === 'SERVING' || s === 'CALLED') return 'Processing';
    if (s === 'AVAILABLE') return 'Available';
    if (s === 'OFFLINE') return 'Offline';
    return 'Idle';
  }

  formatTrend(v: number): string {
    return v >= 0 ? `+${v}%` : `${v}%`;
  }

  formatWait(v: number): string {
    return v > 0 ? `${v.toFixed(1)}m` : '--';
  }

  avgWaitTrendLabel(v: number): string {
    return v > 0 ? `+${v.toFixed(1)}m` : `${v.toFixed(1)}m`;
  }

  staffStars(n: number | null): string[] {
    const rate = n ?? 0;
    return Array.from({ length: 5 }, (_, i) => i < Math.round(rate) ? 'star' : 'star_border');
  }

  satisfactionLabel(n: number | null): string {
    return n != null ? `${n}/5.0` : 'N/A';
  }
}
