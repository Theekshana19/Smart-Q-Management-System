import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminCounterItem, CounterManagement, UpsertCounterRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';
import { CounterFormModalComponent } from '../components/counter-form-modal/counter-form-modal.component';
import { ConfirmDialogComponent } from '../components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-counter-management',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminErrorBannerComponent, CounterFormModalComponent, ConfirmDialogComponent],
  templateUrl: './counter-management.component.html',
  styleUrl: './counter-management.component.scss'
})
export class CounterManagementComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);

  readonly management = signal<CounterManagement | null>(null);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);

  readonly modalOpen = signal(false);
  readonly editItem = signal<AdminCounterItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');
  readonly confirmOpen = signal(false);
  readonly deleteTarget = signal<AdminCounterItem | null>(null);

  readonly summary = computed(() => this.management()?.summary ?? null);
  readonly counters = computed(() => this.management()?.counters ?? []);

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.adminApi.getCounterManagement().subscribe({
      next: d => { this.management.set(d); this.loading.set(false); },
      error: err => {
        const e = AdminApiService.parseError(err);
        this.errorMsg.set(e.message);
        this.errorStatus.set(e.status);
        this.loading.set(false);
      }
    });
  }

  openCreate(): void { this.editItem.set(null); this.modalError.set(''); this.modalOpen.set(true); }
  openEdit(card: { id: number; counterNo: string; counterName: string; status: string }): void {
    this.editItem.set({ id: card.id, counterNo: card.counterNo, counterName: card.counterName,
      status: card.status, isActive: true, assignedServices: [], activeStaffName: null, currentTokenNo: null, tokensToday: 0 });
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  save(req: UpsertCounterRequest): void {
    this.saving.set(true);
    const edit = this.editItem();
    const op = edit?.id ? this.adminApi.updateCounter(edit.id, req) : this.adminApi.createCounter(req);
    op.subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.load(); },
      error: err => { this.modalError.set(AdminApiService.parseError(err).message); this.saving.set(false); }
    });
  }

  statusClass(status: string): string {
    const s = status.toUpperCase();
    if (s === 'SERVING' || s === 'CALLED') return 'status-serving';
    if (s === 'AVAILABLE') return 'status-available';
    if (s === 'OFFLINE' || s === 'MAINTENANCE' || s === 'BREAK') return 'status-offline';
    return 'status-available';
  }

  statusLabel(status: string): string {
    const s = status.toUpperCase();
    if (s === 'SERVING' || s === 'CALLED') return 'Serving';
    if (s === 'AVAILABLE') return 'Available';
    if (s === 'BREAK') return 'On Break';
    if (s === 'OFFLINE') return 'Offline';
    if (s === 'MAINTENANCE') return 'Maintenance';
    return status;
  }

  headerColor(status: string): string {
    const s = status.toUpperCase();
    if (s === 'SERVING' || s === 'CALLED') return 'header-serving';
    if (s === 'AVAILABLE') return 'header-available';
    return 'header-offline';
  }

  initials(name: string | null): string {
    if (!name) return '??';
    return name.split(' ').map(n => n[0]).slice(0, 2).join('').toUpperCase();
  }
}
