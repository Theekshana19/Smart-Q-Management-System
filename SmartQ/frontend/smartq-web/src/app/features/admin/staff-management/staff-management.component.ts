import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminStaffItem, CreateStaffRequest, StaffManagementSummary, UpdateStaffRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-staff-management',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminErrorBannerComponent],
  templateUrl: './staff-management.component.html',
  styleUrl: './staff-management.component.scss'
})
export class StaffManagementComponent implements OnInit {
  private readonly adminApi = inject(AdminApiService);

  readonly staff = signal<AdminStaffItem[]>([]);
  readonly summary = signal<StaffManagementSummary | null>(null);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly search = signal('');
  readonly modalOpen = signal(false);
  readonly resetOpen = signal(false);
  readonly editItem = signal<AdminStaffItem | null>(null);
  readonly resetTarget = signal<AdminStaffItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');

  form: CreateStaffRequest = {
    fullName: '',
    username: '',
    email: '',
    password: '',
    role: 'STAFF',
    isActive: true
  };
  resetPassword = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.adminApi.getStaffSummary().subscribe({
      next: s => this.summary.set(s),
      error: () => {}
    });
    this.adminApi.getStaff(this.search()).subscribe({
      next: res => {
        this.staff.set(res.items);
        this.loading.set(false);
      },
      error: err => {
        const parsed = AdminApiService.parseError(err);
        this.errorMsg.set(parsed.message);
        this.errorStatus.set(parsed.status);
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.editItem.set(null);
    this.form = { fullName: '', username: '', email: '', password: '', role: 'STAFF', isActive: true };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  openEdit(item: AdminStaffItem): void {
    this.editItem.set(item);
    this.form = {
      fullName: item.fullName,
      username: item.username,
      email: item.email,
      password: '',
      role: item.role,
      isActive: item.isActive
    };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  save(): void {
    this.saving.set(true);
    this.modalError.set('');
    const edit = this.editItem();
    const req = edit
      ? this.adminApi.updateStaff(edit.id, this.form as UpdateStaffRequest)
      : this.adminApi.createStaff(this.form);

    req.subscribe({
      next: () => {
        this.saving.set(false);
        this.modalOpen.set(false);
        this.load();
      },
      error: err => {
        this.saving.set(false);
        this.modalError.set(AdminApiService.parseError(err).message);
      }
    });
  }

  openReset(item: AdminStaffItem): void {
    this.resetTarget.set(item);
    this.resetPassword = '';
    this.modalError.set('');
    this.resetOpen.set(true);
  }

  submitReset(): void {
    const target = this.resetTarget();
    if (!target) return;
    this.saving.set(true);
    this.adminApi.resetStaffPassword(target.id, this.resetPassword).subscribe({
      next: () => {
        this.saving.set(false);
        this.resetOpen.set(false);
      },
      error: err => {
        this.saving.set(false);
        this.modalError.set(AdminApiService.parseError(err).message);
      }
    });
  }

  toggleStatus(item: AdminStaffItem): void {
    this.adminApi.patchStaffStatus(item.id, !item.isActive).subscribe({ next: () => this.load() });
  }

  forceLogout(item: AdminStaffItem): void {
    this.adminApi.forceLogoutStaff(item.id).subscribe({ next: () => this.load() });
  }
}
