import { Component, OnInit, inject, signal } from '@angular/core';
import { AdminApiService } from '../../../core/api/admin-api.service';

@Component({
  selector: 'app-counter-management',
  templateUrl: './counter-management.component.html',
  styleUrl: './counter-management.component.scss'
})
export class CounterManagementComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);
  readonly counters = signal<Record<string, unknown>[]>([]);

  ngOnInit(): void {
    this.adminApi.getCounters().subscribe(c => this.counters.set(c as Record<string, unknown>[]));
  }
}
