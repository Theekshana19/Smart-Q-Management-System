import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminServiceItem, ServiceManagementSummary, UpsertServiceRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';
import { ServiceFormModalComponent } from '../components/service-form-modal/service-form-modal.component';
import { ConfirmDialogComponent } from '../components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-service-management',
  standalone: true,
  imports: [CommonModule, RouterLink, AdminErrorBannerComponent, ServiceFormModalComponent, ConfirmDialogComponent],
  templateUrl: './service-management.component.html',
  styleUrl: './service-management.component.scss'
})
export class ServiceManagementComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);

  readonly services = signal<AdminServiceItem[]>([]);
  readonly totalCount = signal(0);
  readonly summary = signal<ServiceManagementSummary | null>(null);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly searchTerm = signal('');
  readonly activeTab = signal<'all' | 'active' | 'maintenance'>('all');

  readonly modalOpen = signal(false);
  readonly editItem = signal<AdminServiceItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');
  readonly confirmOpen = signal(false);
  readonly deleteTarget = signal<AdminServiceItem | null>(null);

  readonly filteredServices = computed(() => {
    let list = this.services();
    const term = this.searchTerm().toLowerCase();
    const tab = this.activeTab();
    if (term) list = list.filter(s => s.name.toLowerCase().includes(term) || s.code.toLowerCase().includes(term));
    if (tab === 'active') list = list.filter(s => s.isActive);
    if (tab === 'maintenance') list = list.filter(s => !s.isActive);
    return list;
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    const isActive = this.activeTab() === 'active' ? true : this.activeTab() === 'maintenance' ? false : undefined;
    this.adminApi.getServiceSummary().subscribe({ next: s => this.summary.set(s), error: () => {} });
    this.adminApi.getServices({ search: this.searchTerm() || undefined, isActive, page: 1, pageSize: 100 })
      .subscribe({
        next: r => { this.services.set(r.items); this.totalCount.set(r.totalCount); this.loading.set(false); },
        error: err => { const e = AdminApiService.parseError(err); this.errorMsg.set(e.message); this.errorStatus.set(e.status); this.loading.set(false); }
      });
  }

  openCreate(): void { this.editItem.set(null); this.modalError.set(''); this.modalOpen.set(true); }
  openEdit(s: AdminServiceItem): void { this.editItem.set(s); this.modalError.set(''); this.modalOpen.set(true); }
  closeModal(): void { this.modalOpen.set(false); }

  saveService(req: UpsertServiceRequest): void {
    this.saving.set(true);
    this.modalError.set('');
    const edit = this.editItem();
    const op = edit ? this.adminApi.updateService(edit.id, req) : this.adminApi.createService(req);
    op.subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.load(); },
      error: err => { const e = AdminApiService.parseError(err); this.modalError.set(e.message); this.saving.set(false); }
    });
  }

  toggleStatus(s: AdminServiceItem): void {
    this.adminApi.patchServiceStatus(s.id, !s.isActive).subscribe({ next: () => this.load(), error: () => this.load() });
  }

  confirmDelete(s: AdminServiceItem): void { this.deleteTarget.set(s); this.confirmOpen.set(true); }
  doDelete(): void {
    const t = this.deleteTarget();
    if (!t) return;
    this.adminApi.deleteService(t.id).subscribe({
      next: () => { this.confirmOpen.set(false); this.load(); },
      error: err => { const e = AdminApiService.parseError(err); this.errorMsg.set(e.message); this.confirmOpen.set(false); }
    });
  }

  setSearch(ev: Event): void { this.searchTerm.set((ev.target as HTMLInputElement).value); }
  setTab(tab: 'all' | 'active' | 'maintenance'): void { this.activeTab.set(tab); this.load(); }

  formatWait(m: number): string {
    if (m <= 0) return '--';
    return `${Math.floor(m)}m ${Math.round((m % 1) * 60)}s`;
  }
}
