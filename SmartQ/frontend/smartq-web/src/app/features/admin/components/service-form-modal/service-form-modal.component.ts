import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminServiceItem, UpsertServiceRequest } from '../../../../core/models/admin.models';

@Component({
  selector: 'app-service-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (open) {
      <div class="overlay" (click)="closed.emit()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>{{ editItem ? 'Edit Service' : 'Add New Service' }}</h3>
          <form (ngSubmit)="submit()">
            <label>Code *<input [(ngModel)]="form.code" name="code" required maxlength="20" /></label>
            <label>Name *<input [(ngModel)]="form.name" name="name" required maxlength="100" /></label>
            <label>Description<textarea [(ngModel)]="form.description" name="description" rows="2"></textarea></label>
            <label>Icon<input [(ngModel)]="form.icon" name="icon" placeholder="cash" /></label>
            <label>Display Order<input type="number" [(ngModel)]="form.displayOrder" name="displayOrder" min="0" /></label>
            <label class="check"><input type="checkbox" [(ngModel)]="form.isActive" name="isActive" /> Active</label>
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
    h3 { font-size:18px;font-weight:600;margin-bottom:16px; }
    label { display:block;font-size:12px;font-weight:600;color:#45464d;margin-bottom:12px; }
    input, textarea { display:block;width:100%;margin-top:4px;padding:8px 12px;border:1px solid #c6c6cd;border-radius:8px;font-size:14px; }
    .check { display:flex;align-items:center;gap:8px; }
    .check input { width:auto;margin:0; }
    .err { color:#ba1a1a;font-size:13px;margin:8px 0; }
    .actions { display:flex;gap:12px;justify-content:flex-end;margin-top:16px; }
    .btn-cancel { padding:8px 16px;border-radius:8px;border:1px solid #c6c6cd;background:#fff;cursor:pointer;font-weight:600; }
    .btn-save { padding:8px 20px;border-radius:8px;border:none;background:#000;color:#fff;cursor:pointer;font-weight:600; }
    .btn-save:disabled { opacity:.6; }
  `]
})
export class ServiceFormModalComponent implements OnChanges {
  @Input() open = false;
  @Input() editItem: AdminServiceItem | null = null;
  @Input() saving = false;
  @Input() error = '';
  @Output() saved = new EventEmitter<UpsertServiceRequest>();
  @Output() closed = new EventEmitter<void>();

  form: UpsertServiceRequest = this.emptyForm();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editItem'] || changes['open']) {
      this.form = this.editItem
        ? { code: this.editItem.code, name: this.editItem.name, description: this.editItem.description,
            icon: this.editItem.icon, displayOrder: this.editItem.displayOrder, isActive: this.editItem.isActive }
        : this.emptyForm();
    }
  }

  submit(): void {
    if (!this.form.code.trim() || !this.form.name.trim()) return;
    this.saved.emit({ ...this.form, code: this.form.code.trim(), name: this.form.name.trim() });
  }

  private emptyForm(): UpsertServiceRequest {
    return { code: '', name: '', description: '', icon: 'miscellaneous_services', displayOrder: 0, isActive: true };
  }
}
