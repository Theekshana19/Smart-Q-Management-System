import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminCounterItem, UpsertCounterRequest } from '../../../../core/models/admin.models';

const COUNTER_STATUSES = ['AVAILABLE', 'SERVING', 'OFFLINE', 'MAINTENANCE', 'BREAK'];

@Component({
  selector: 'app-counter-form-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (open) {
      <div class="overlay" (click)="closed.emit()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>{{ editItem ? 'Edit Counter' : 'Add New Counter' }}</h3>
          <form (ngSubmit)="submit()">
            <label>Counter No *<input [(ngModel)]="form.counterNo" name="counterNo" required /></label>
            <label>Counter Name *<input [(ngModel)]="form.counterName" name="counterName" required /></label>
            <label>Status
              <select [(ngModel)]="form.status" name="status">
                @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
              </select>
            </label>
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
    .modal { background:#fff;border-radius:12px;padding:24px;width:420px;max-width:95vw; }
    h3 { font-size:18px;font-weight:600;margin-bottom:16px; }
    label { display:block;font-size:12px;font-weight:600;color:#45464d;margin-bottom:12px; }
    input, select { display:block;width:100%;margin-top:4px;padding:8px 12px;border:1px solid #c6c6cd;border-radius:8px;font-size:14px; }
    .check { display:flex;align-items:center;gap:8px; }
    .check input { width:auto;margin:0; }
    .err { color:#ba1a1a;font-size:13px; }
    .actions { display:flex;gap:12px;justify-content:flex-end;margin-top:16px; }
    .btn-cancel { padding:8px 16px;border-radius:8px;border:1px solid #c6c6cd;background:#fff;cursor:pointer;font-weight:600; }
    .btn-save { padding:8px 20px;border-radius:8px;border:none;background:#000;color:#fff;cursor:pointer;font-weight:600; }
  `]
})
export class CounterFormModalComponent implements OnChanges {
  readonly statuses = COUNTER_STATUSES;
  @Input() open = false;
  @Input() editItem: AdminCounterItem | null = null;
  @Input() saving = false;
  @Input() error = '';
  @Output() saved = new EventEmitter<UpsertCounterRequest>();
  @Output() closed = new EventEmitter<void>();

  form: UpsertCounterRequest = { counterNo: '', counterName: '', status: 'AVAILABLE', isActive: true };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editItem'] || changes['open']) {
      this.form = this.editItem
        ? { counterNo: this.editItem.counterNo, counterName: this.editItem.counterName,
            status: this.editItem.status, isActive: this.editItem.isActive }
        : { counterNo: '', counterName: '', status: 'AVAILABLE', isActive: true };
    }
  }

  submit(): void {
    if (!this.form.counterNo.trim() || !this.form.counterName.trim()) return;
    this.saved.emit({ ...this.form });
  }
}
