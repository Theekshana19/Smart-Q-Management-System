import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminLanguageItem, LanguageManagementSummary, UpsertLanguageRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-languages',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminErrorBannerComponent],
  templateUrl: './languages.component.html',
  styleUrl: './languages.component.scss'
})
export class LanguagesComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly items = signal<AdminLanguageItem[]>([]);
  readonly summary = signal<LanguageManagementSummary | null>(null);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly modalOpen = signal(false);
  readonly editItem = signal<AdminLanguageItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');

  form: UpsertLanguageRequest = { code: '', name: '', nativeName: '', isDefault: false, isActive: true, displayOrder: 0 };

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    this.api.getLanguageSummary().subscribe({ next: s => this.summary.set(s), error: () => {} });
    this.api.getLanguages().subscribe({
      next: list => { this.items.set(list); this.loading.set(false); },
      error: err => {
        const p = AdminApiService.parseError(err);
        this.errorMsg.set(p.message);
        this.errorStatus.set(p.status);
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.editItem.set(null);
    this.form = { code: '', name: '', nativeName: '', isDefault: false, isActive: true, displayOrder: this.items().length + 1 };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  openEdit(item: AdminLanguageItem): void {
    this.editItem.set(item);
    this.form = { code: item.code, name: item.name, nativeName: item.nativeName, isDefault: item.isDefault, isActive: item.isActive, displayOrder: item.displayOrder };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  save(): void {
    this.saving.set(true);
    this.modalError.set('');
    const edit = this.editItem();
    const req = edit ? this.api.updateLanguage(edit.id, this.form) : this.api.createLanguage(this.form);
    req.subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.load(); },
      error: err => { this.saving.set(false); this.modalError.set(AdminApiService.parseError(err).message); }
    });
  }

  toggleStatus(item: AdminLanguageItem): void {
    this.api.patchLanguageStatus(item.id, !item.isActive).subscribe({ next: () => this.load() });
  }

  remove(item: AdminLanguageItem): void {
    if (!confirm(`Deactivate/delete language ${item.code}?`)) return;
    this.api.deleteLanguage(item.id).subscribe({ next: () => this.load() });
  }
}
