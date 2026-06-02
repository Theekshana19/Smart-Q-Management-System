import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit } from '@angular/core';
import { StatusBadgeComponent } from '../components/status-badge/status-badge.component';
import { SummaryCardComponent } from '../components/summary-card/summary-card.component';
import { StaffStateService } from '../services/staff-state.service';

@Component({
  selector: 'app-staff-dashboard',
  standalone: true,
  imports: [CommonModule, SummaryCardComponent, StatusBadgeComponent],
  templateUrl: './staff-dashboard.component.html',
  styleUrl: './staff-dashboard.component.scss'
})
export class StaffDashboardComponent implements OnInit {
  readonly state = inject(StaffStateService);
  readonly loading = computed(() => this.state.loading());
  readonly context = computed(() => this.state.context());
  readonly summary = computed(() => this.state.summary());
  readonly queue = computed(() => this.state.queue());
  readonly notifications = computed(() => this.state.notifications());
  readonly compositionMax = computed(() => Math.max(1, ...this.queue().map((x) => x.waitMinutes)));

  ngOnInit(): void {
    this.state.loadAll('all-branch');
  }

  statusKind(status: string): 'ok' | 'warn' | 'danger' | 'neutral' {
    const s = status.toUpperCase();
    if (s.includes('OVER') || s.includes('OFFLINE') || s.includes('ERROR')) return 'danger';
    if (s.includes('BREAK') || s.includes('INFO') || s.includes('MEDIUM')) return 'warn';
    if (s.includes('ACTIVE') || s.includes('AVAILABLE') || s.includes('OK') || s.includes('SERVING')) return 'ok';
    return 'neutral';
  }

  waitDisplay(minutes: number): string {
    const mm = `${Math.max(0, minutes)}`.padStart(2, '0');
    return `${mm}:${new Date().getSeconds().toString().padStart(2, '0')}`;
  }
}
