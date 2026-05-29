import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminApiService } from '../../../core/api/admin-api.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);
  readonly data = signal<Record<string, unknown> | null>(null);

  ngOnInit(): void {
    this.adminApi.getDashboardSummary().subscribe(d => this.data.set(d as Record<string, unknown>));
  }
}
