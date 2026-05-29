import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { AdminApiService } from '../../../core/api/admin-api.service';

@Component({
  selector: 'app-reports',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);
  readonly report = signal<Record<string, unknown> | null>(null);

  ngOnInit(): void {
    this.adminApi.getTokenHistory({}).subscribe(r => this.report.set(r as Record<string, unknown>));
  }
}
