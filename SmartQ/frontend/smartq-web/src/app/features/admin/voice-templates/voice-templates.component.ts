import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminApiService } from '../../../core/api/admin-api.service';
import { AdminLanguageItem, AdminVoiceTemplateItem, UpsertVoiceTemplateRequest } from '../../../core/models/admin.models';
import { AdminErrorBannerComponent } from '../components/admin-error-banner/admin-error-banner.component';

@Component({
  selector: 'app-voice-templates',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminErrorBannerComponent],
  templateUrl: './voice-templates.component.html',
  styleUrl: './voice-templates.component.scss'
})
export class VoiceTemplatesComponent implements OnInit {
  private readonly api = inject(AdminApiService);

  readonly items = signal<AdminVoiceTemplateItem[]>([]);
  readonly languages = signal<AdminLanguageItem[]>([]);
  readonly loading = signal(true);
  readonly errorMsg = signal('');
  readonly errorStatus = signal(0);
  readonly filterLang = signal<number | ''>('');
  readonly filterEvent = signal('');
  readonly filterActive = signal<boolean | ''>('');
  readonly modalOpen = signal(false);
  readonly editItem = signal<AdminVoiceTemplateItem | null>(null);
  readonly saving = signal(false);
  readonly modalError = signal('');

  form: UpsertVoiceTemplateRequest = { languageId: 1, eventType: 'TOKEN_CALLED', templateText: '', isActive: true };

  readonly preview = computed(() =>
    this.form.templateText
      .replaceAll('{tokenNo}', 'CD-001')
      .replaceAll('{counterName}', 'Counter 02')
      .replaceAll('{serviceName}', 'Cash Services')
      .replaceAll('{subServiceName}', 'Cash Deposit')
  );

  ngOnInit(): void {
    this.api.getLanguages().subscribe({
      next: l => {
        this.languages.set(l);
        if (l.length) this.form.languageId = l[0].id;
      },
      error: () => {}
    });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.errorMsg.set('');
    const lang = this.filterLang();
    const active = this.filterActive();
    this.api.getVoiceTemplates(
      lang === '' ? undefined : Number(lang),
      this.filterEvent() || undefined,
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
    this.form = { languageId: this.languages()[0]?.id ?? 1, eventType: 'TOKEN_CALLED', templateText: '', isActive: true };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  openEdit(item: AdminVoiceTemplateItem): void {
    this.editItem.set(item);
    this.form = { languageId: item.languageId, eventType: item.eventType, templateText: item.templateText, isActive: item.isActive };
    this.modalError.set('');
    this.modalOpen.set(true);
  }

  save(): void {
    this.saving.set(true);
    this.modalError.set('');
    const edit = this.editItem();
    const req = edit ? this.api.updateVoiceTemplate(edit.id, this.form) : this.api.createVoiceTemplate(this.form);
    req.subscribe({
      next: () => { this.saving.set(false); this.modalOpen.set(false); this.load(); },
      error: err => { this.saving.set(false); this.modalError.set(AdminApiService.parseError(err).message); }
    });
  }

  toggleStatus(item: AdminVoiceTemplateItem): void {
    this.api.patchVoiceTemplateStatus(item.id, !item.isActive).subscribe({ next: () => this.load() });
  }

  remove(item: AdminVoiceTemplateItem): void {
    if (!confirm(`Deactivate template ${item.eventType} (${item.languageCode})?`)) return;
    this.api.deleteVoiceTemplate(item.id).subscribe({ next: () => this.load() });
  }
}
