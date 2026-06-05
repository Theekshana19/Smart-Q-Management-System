import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminServiceItem, AdminSubServiceItem, UpsertSubServiceRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';
import { SubServiceFormModalComponent } from '../components/sub-service-form-modal/sub-service-form-modal.component';
import { ConfirmDialogComponent } from '../components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-sub-services',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminErrorBannerComponent, SubServiceFormModalComponent, ConfirmDialogComponent],
  templateUrl: './sub-services.component.html',
  styleUrl: './sub-services.component.scss'
})
export class SubServicesComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);
  private readonly route = inject(ActivatedRoute);

  readonly items = signal<AdminSubServiceItem[]>([]);
  readonly services = signal<AdminServiceItem[]>([]);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  searchTerm = '';
  filterServiceId?: number;
  filterActive?: boolean;

  readonly modalOpen = signal(false);
  readonly editItem = signal<AdminSubServiceItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');
  readonly confirmOpen = signal(false);
  readonly deleteTarget = signal<AdminSubServiceItem | null>(null);

  ngOnInit(): void {
    this.route.queryParams.subscribe(p => {
      this.filterServiceId = p['serviceId'] ? +p['serviceId'] : undefined;
      this.load();
    });
    this.adminApi.getServices({ page: 1, pageSize: 100 }).subscribe({
      next: r => this.services.set(r.items),
      error: () => {}
    });
  }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.adminApi.getSubServices({
      serviceId: this.filterServiceId,
      search: this.searchTerm || undefined,
      isActive: this.filterActive,
      page: 1, pageSize: 200
    }).subscribe({
      next: r => { this.items.set(r.items); this.loading.set(false); },
      error: err => { const e = AdminApiService.parseError(err); this.errorMsg.set(e.message); this.errorStatus.set(e.status); this.loading.set(false); }
    });
  }

  openCreate(): void { this.editItem.set(null); this.modalError.set(''); this.modalOpen.set(true); }
  openEdit(s: AdminSubServiceItem): void { this.editItem.set(s); this.modalError.set(''); this.modalOpen.set(true); }

  save(req: UpsertSubServiceRequest): void {
    this.saving.set(true);
    const edit = this.editItem();
    const op = edit ? this.adminApi.updateSubService(edit.id, req) : this.adminApi.createSubService(req);
    op.subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.load(); },
      error: err => { this.modalError.set(AdminApiService.parseError(err).message); this.saving.set(false); }
    });
  }

  toggleStatus(s: AdminSubServiceItem): void {
    this.adminApi.patchSubServiceStatus(s.id, !s.isActive).subscribe({ next: () => this.load() });
  }

  confirmDelete(s: AdminSubServiceItem): void { this.deleteTarget.set(s); this.confirmOpen.set(true); }
  doDelete(): void {
    const t = this.deleteTarget();
    if (!t) return;
    this.adminApi.deleteSubService(t.id).subscribe({ next: () => { this.confirmOpen.set(false); this.load(); } });
  }
}
