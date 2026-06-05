import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AssignableService, CounterAssignmentItem } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-counter-assignments',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminErrorBannerComponent],
  templateUrl: './counter-assignments.component.html',
  styleUrl: './counter-assignments.component.scss'
})
export class CounterAssignmentsComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);

  readonly counters = signal<CounterAssignmentItem[]>([]);
  readonly assignable = signal<AssignableService[]>([]);
  readonly selectedCounterId = signal<number | null>(null);
  readonly selectedIds = signal<Set<number>>(new Set());
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly saveMsg = signal('');

  ngOnInit(): void { this.loadCounters(); }

  loadCounters(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.adminApi.getCounterAssignments().subscribe({
      next: list => {
        this.counters.set(list);
        this.loading.set(false);
        if (list.length && !this.selectedCounterId()) this.selectCounter(list[0].counterId);
      },
      error: err => {
        const e = AdminApiService.parseError(err);
        this.errorMsg.set(e.message);
        this.errorStatus.set(e.status);
        this.loading.set(false);
      }
    });
  }

  selectCounter(id: number): void {
    this.selectedCounterId.set(id);
    this.saveMsg.set('');
    this.adminApi.getAssignableServices(id).subscribe({
      next: services => {
        this.assignable.set(services);
        this.selectedIds.set(new Set(services.filter(s => s.isAssigned).map(s => s.id)));
      },
      error: err => this.errorMsg.set(AdminApiService.parseError(err).message)
    });
  }

  toggleService(id: number): void {
    const set = new Set(this.selectedIds());
    if (set.has(id)) set.delete(id); else set.add(id);
    this.selectedIds.set(set);
  }

  isChecked(id: number): boolean { return this.selectedIds().has(id); }

  save(): void {
    const counterId = this.selectedCounterId();
    if (!counterId) return;
    this.saving.set(true);
    this.saveMsg.set('');
    this.adminApi.saveCounterAssignments(counterId, [...this.selectedIds()]).subscribe({
      next: () => {
        this.saving.set(false);
        this.saveMsg.set('Assignments saved successfully.');
        this.loadCounters();
        this.selectCounter(counterId);
      },
      error: err => {
        this.saving.set(false);
        this.errorMsg.set(AdminApiService.parseError(err).message);
      }
    });
  }
}
