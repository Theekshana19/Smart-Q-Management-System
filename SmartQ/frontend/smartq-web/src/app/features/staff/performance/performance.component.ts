import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { StaffPerformance } from '../../../core/models/staff-console.models';
import { StaffConsoleApiService } from '../services/staff-console-api.service';
import { StaffStateService } from '../services/staff-state.service';

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card" *ngIf="data() as p">
      <h2>Performance Analytics</h2>
      <div class="stats">
        <article><h4>Avg Service Time</h4><strong>{{ p.avgServiceTime }}</strong></article>
        <article><h4>Completion Rate</h4><strong>{{ p.completionRate }}%</strong></article>
        <article><h4>Served Today</h4><strong>{{ p.servedToday }}</strong></article>
      </div>
      <h3>Live Timeline</h3>
      <div *ngFor="let t of p.recentTimeline">{{ t.title }} - {{ t.description }}</div>
      <p class="tip">{{ p.optimizationTip }}</p>
    </div>
  `,
  styles: [`.card{background:#fff;border:1px solid #e6eaf2;border-radius:14px;padding:16px}.stats{display:grid;grid-template-columns:repeat(3,1fr);gap:8px}.tip{margin-top:16px;padding:12px;border:1px dashed #00796b;border-radius:10px}`]
})
export class PerformanceComponent implements OnInit {
  readonly data = signal<StaffPerformance | null>(null);
  private readonly api = inject(StaffConsoleApiService);
  private readonly state = inject(StaffStateService);
  ngOnInit(): void {
    this.api.getPerformance(this.state.counterId(), this.state.context()?.staff?.id).subscribe((res) => this.data.set(res));
  }
}
