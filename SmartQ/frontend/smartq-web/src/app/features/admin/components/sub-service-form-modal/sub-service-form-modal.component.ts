import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminServiceItem, AdminSubServiceItem, UpsertSubServiceRequest } from '../../../../core/models/admin.models';

@Component({
  selector: 'app-sub-service-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (open) {
      <div class="overlay" (click)="closed.emit()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>{{ editItem ? 'Edit Sub-Service' : 'Add New Sub-Service' }}</h3>
          <p class="hint">Token prefix controls generated token number, e.g. CD-001.</p>
          <form (ngSubmit)="submit()">
            <label>Parent Service *
              <select [(ngModel)]="form.serviceId" name="serviceId" required>
                <option [ngValue]="0" disabled>Select service</option>
                @for (s of services; track s.id) {
                  <option [ngValue]="s.id">{{ s.name }}</option>
                }
              </select>
            </label>
            <label>Code *<input [(ngModel)]="form.code" name="code" required /></label>
            <label>Name *<input [(ngModel)]="form.name" name="name" required /></label>
            <label>Token Prefix *<input [(ngModel)]="form.tokenPrefix" name="tokenPrefix" required maxlength="10" /></label>
            <label>Description<textarea [(ngModel)]="form.description" name="description" rows="2"></textarea></label>
            <label>Est. Minutes *<input type="number" [(ngModel)]="form.estimatedServiceMinutes" name="est" min="1" /></label>
            <label>Display Order<input type="number" [(ngModel)]="form.displayOrder" name="order" min="0" /></label>
            <label class="check"><input type="checkbox" [(ngModel)]="form.isActive" name="active" /> Active</label>
            @if (error) { <p class="err">{{ error }}</p> }
            <div class="actions">
              <button type="button" class="btn-cancel" (click)="closed.emit()">Cancel</button>
              <button type="submit" class="btn-save" [disabled]="saving">{{ saving ? 'Saving…' : 'Save' }}</button>
            </div>
          </form>
        </div>
      </div>
    }
  `,
  styles: [`
    .overlay { position:fixed;inset:0;background:rgba(0,0,0,.4);display:flex;align-items:center;justify-content:center;z-index:1000; }
    .modal { background:#fff;border-radius:12px;padding:24px;width:480px;max-width:95vw;max-height:90vh;overflow-y:auto; }
    h3 { font-size:18px;font-weight:600;margin-bottom:4px; }
    .hint { font-size:12px;color:#45464d;margin-bottom:16px; }
    label { display:block;font-size:12px;font-weight:600;color:#45464d;margin-bottom:12px; }
    input, textarea, select { display:block;width:100%;margin-top:4px;padding:8px 12px;border:1px solid #c6c6cd;border-radius:8px;font-size:14px; }
    .check { display:flex;align-items:center;gap:8px; }
    .check input { width:auto;margin:0; }
    .err { color:#ba1a1a;font-size:13px; }
    .actions { display:flex;gap:12px;justify-content:flex-end;margin-top:16px; }
    .btn-cancel { padding:8px 16px;border-radius:8px;border:1px solid #c6c6cd;background:#fff;cursor:pointer;font-weight:600; }
    .btn-save { padding:8px 20px;border-radius:8px;border:none;background:#000;color:#fff;cursor:pointer;font-weight:600; }
  `]
})
export class SubServiceFormModalComponent implements OnChanges {
  @Input() open = false;
  @Input() editItem: AdminSubServiceItem | null = null;
  @Input() services: AdminServiceItem[] = [];
  @Input() defaultServiceId?: number;
  @Input() saving = false;
  @Input() error = '';
  @Output() saved = new EventEmitter<UpsertSubServiceRequest>();
  @Output() closed = new EventEmitter<void>();

  form: UpsertSubServiceRequest = this.emptyForm();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editItem'] || changes['open']) {
      this.form = this.editItem
        ? { serviceId: this.editItem.serviceId, code: this.editItem.code, name: this.editItem.name,
            description: this.editItem.description, tokenPrefix: this.editItem.tokenPrefix,
            icon: this.editItem.icon, estimatedServiceMinutes: this.editItem.estimatedServiceMinutes,
            displayOrder: this.editItem.displayOrder, isActive: this.editItem.isActive }
        : { ...this.emptyForm(), serviceId: this.defaultServiceId ?? 0 };
    }
  }

  submit(): void {
    if (!this.form.serviceId || !this.form.code.trim() || !this.form.tokenPrefix.trim()) return;
    this.saved.emit({ ...this.form, code: this.form.code.trim(), tokenPrefix: this.form.tokenPrefix.trim().toUpperCase() });
  }

  private emptyForm(): UpsertSubServiceRequest {
    return { serviceId: 0, code: '', name: '', description: '', tokenPrefix: '', icon: 'label',
      estimatedServiceMinutes: 10, displayOrder: 0, isActive: true };
  }
}
