import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminSystemSettingItem, UpdateAdminSettingRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-system-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminErrorBannerComponent],
  templateUrl: './system-settings.component.html',
  styleUrl: './system-settings.component.scss'
})
export class SystemSettingsComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly items = signal<AdminSystemSettingItem[]>([]);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly search = signal('');
  readonly dataType = signal('');
  readonly modalOpen = signal(false);
  readonly editItem = signal<AdminSystemSettingItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');

  form: UpdateAdminSettingRequest = { settingValue: '', description: '', isActive: true };

  readonly filtered = computed(() => {
    let list = this.items();
    const term = this.search().toLowerCase();
    if (term) list = list.filter(s => s.settingKey.toLowerCase().includes(term) || s.description.toLowerCase().includes(term));
    const dt = this.dataType();
    if (dt) list = list.filter(s => s.dataType.toUpperCase() === dt.toUpperCase());
    return list;
  });

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.api.getSettings().subscribe({
      next: list => { this.items.set(list); this.loading.set(false); },
      error: err => {
        const p = AdminApiService.parseError(err);
        this.errorMsg.set(p.message);
        this.errorStatus.set(p.status);
        this.loading.set(false);
      }
    });
  }

  openEdit(item: AdminSystemSettingItem): void {
    this.editItem.set(item);
    this.form = { settingValue: item.settingValue, description: item.description, isActive: item.isActive };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  isBoolean(item: AdminSystemSettingItem | null): boolean {
    const t = item?.dataType?.toUpperCase() ?? '';
    return t === 'BOOL' || t === 'BOOLEAN';
  }

  save(): void {
    const edit = this.editItem();
    if (!edit) return;
    this.saving.set(true);
    this.modalError.set('');
    this.api.updateSetting(edit.id, this.form).subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.load(); },
      error: err => { this.saving.set(false); this.modalError.set(AdminApiService.parseError(err).message); }
    });
  }
}
