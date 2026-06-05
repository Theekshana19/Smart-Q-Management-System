import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (open) {
      <div class="overlay" (click)="cancel.emit()">
        <div class="dialog" (click)="$event.stopPropagation()">
          <h3>{{ title }}</h3>
          <p>{{ message }}</p>
          <div class="actions">
            <button type="button" class="btn-cancel" (click)="cancel.emit()">Cancel</button>
            <button type="button" class="btn-confirm" (click)="confirm.emit()">{{ confirmLabel }}</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .overlay { position:fixed;inset:0;background:rgba(0,0,0,.4);display:flex;align-items:center;justify-content:center;z-index:1000; }
    .dialog { background:#fff;border-radius:12px;padding:24px;max-width:400px;width:90%;box-shadow:0 8px 32px rgba(0,0,0,.15); }
    h3 { font-size:18px;font-weight:600;margin-bottom:8px;color:#0b1c30; }
    p { font-size:14px;color:#45464d;margin-bottom:20px;line-height:1.5; }
    .actions { display:flex;gap:12px;justify-content:flex-end; }
    .btn-cancel { padding:8px 16px;border-radius:8px;border:1px solid #c6c6cd;background:#fff;cursor:pointer;font-weight:600; }
    .btn-confirm { padding:8px 16px;border-radius:8px;border:none;background:#000;color:#fff;cursor:pointer;font-weight:600; }
  `]
})
export class ConfirmDialogComponent {
  @Input() open = false;
  @Input() title = 'Confirm';
  @Input() message = 'Are you sure?';
  @Input() confirmLabel = 'Confirm';
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();
}
