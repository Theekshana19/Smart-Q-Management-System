import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { StaffTokenHistoryItem } from '../../../core/models/staff-console.models';
import { StaffConsoleApiService } from '../services/staff-console-api.service';
import { StaffStateService } from '../services/staff-state.service';

@Component({
  selector: 'app-token-history',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <h2>Token History</h2>
      <button (click)="load()">Reload Data</button>
      <table>
        <thead><tr><th>Token No</th><th>Service Type</th><th>Called Time</th><th>Duration</th><th>Status</th></tr></thead>
        <tbody>
          <tr *ngFor="let item of items()">
            <td>{{ item.tokenNo }}</td><td>{{ item.serviceType }}</td><td>{{ item.calledTime || '-' }}</td><td>{{ item.duration }}</td><td>{{ item.status }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
  styles: [`.card{background:#fff;border:1px solid #e6eaf2;border-radius:14px;padding:16px}table{width:100%;margin-top:10px;border-collapse:collapse}th,td{padding:10px;border-bottom:1px solid #eef1f6;text-align:left}`]
})
export class TokenHistoryComponent implements OnInit {
  readonly items = signal<StaffTokenHistoryItem[]>([]);
  private readonly api = inject(StaffConsoleApiService);
  private readonly state = inject(StaffStateService);
  ngOnInit(): void { this.load(); }
  load(): void {
    this.api.getTokenHistory(this.state.counterId()).subscribe((res) => this.items.set(res));
  }
}
