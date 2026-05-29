import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminApiService } from '../../../core/api/admin-api.service';

@Component({
  selector: 'app-service-management',
  templateUrl: './service-management.component.html',
  styleUrl: './service-management.component.scss'
})
export class ServiceManagementComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);
  readonly services = signal<Record<string, unknown>[]>([]);
  readonly summary = signal<{ totalServices: number; activeNow: number; totalTokensToday: number; avgWaitMinutes: number } | null>(null);

  ngOnInit(): void {
    this.adminApi.getServiceSummary().subscribe(s => this.summary.set(s));
    this.adminApi.getServices().subscribe(s => this.services.set(s as Record<string, unknown>[]));
  }
}
