import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminDisplayMessageItem, AdminLanguageItem, UpsertDisplayMessageRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-display-messages',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminErrorBannerComponent],
  templateUrl: './display-messages.component.html',
  styleUrl: './display-messages.component.scss'
})
export class DisplayMessagesComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly items = signal<AdminDisplayMessageItem[]>([]);
  readonly languages = signal<AdminLanguageItem[]>([]);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly filterLang = signal<number | ''>('');
  readonly filterKey = signal('');
  readonly filterActive = signal<boolean | ''>('');
  readonly modalOpen = signal(false);
  readonly editItem = signal<AdminDisplayMessageItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');

  form: UpsertDisplayMessageRequest = { languageId: null, messageKey: '', messageText: '', isActive: true, displayOrder: 0 };

  ngOnInit(): void {
    this.api.getLanguages().subscribe({ next: l => this.languages.set(l), error: () => {} });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    const lang = this.filterLang();
    const active = this.filterActive();
    this.api.getDisplayMessages(
      lang === '' ? undefined : Number(lang),
      this.filterKey() || undefined,
      active === '' ? undefined : active
    ).subscribe({
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
    this.form = { languageId: null, messageKey: '', messageText: '', isActive: true, displayOrder: 0 };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  openEdit(item: AdminDisplayMessageItem): void {
    this.editItem.set(item);
    this.form = { languageId: item.languageId, messageKey: item.messageKey, messageText: item.messageText, isActive: item.isActive, displayOrder: item.displayOrder };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  save(): void {
    this.saving.set(true);
    this.modalError.set('');
    const edit = this.editItem();
    const req = edit ? this.api.updateDisplayMessage(edit.id, this.form) : this.api.createDisplayMessage(this.form);
    req.subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.load(); },
      error: err => { this.saving.set(false); this.modalError.set(AdminApiService.parseError(err).message); }
    });
  }

  toggleStatus(item: AdminDisplayMessageItem): void {
    this.api.patchDisplayMessageStatus(item.id, !item.isActive).subscribe({ next: () => this.load() });
  }

  remove(item: AdminDisplayMessageItem): void {
    if (!confirm(`Deactivate message ${item.messageKey}?`)) return;
    this.api.deleteDisplayMessage(item.id).subscribe({ next: () => this.load() });
  }
}
