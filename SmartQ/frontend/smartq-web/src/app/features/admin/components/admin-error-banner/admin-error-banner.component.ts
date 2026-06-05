import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-error-banner',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (message) {
      <div class="error-banner">
        <div class="error-text">
          <span class="material-symbols-outlined">error</span>
          <div>
            <strong>{{ title }}</strong>
            <p>{{ message }}@if (status) { <span class="status">(HTTP {{ status }})</span> }</p>
          </div>
        </div>
        @if (showRetry) {
          <button type="button" class="retry-btn" (click)="retry.emit()">
            <span class="material-symbols-outlined">refresh</span> Retry
          </button>
        }
      </div>
    }
  `,
  styles: [`
    .error-banner { display:flex;align-items:center;justify-content:space-between;gap:16px;padding:14px 18px;border-radius:10px;background:rgba(186,26,26,.08);border:1px solid rgba(186,26,26,.2);margin-bottom:16px; }
    .error-text { display:flex;gap:12px;align-items:flex-start;color:#ba1a1a; }
    .error-text .material-symbols-outlined { font-size:22px; }
    strong { display:block;font-size:14px; }
    p { font-size:13px;margin-top:2px;color:#45464d; }
    .status { font-family:monospace;font-size:12px; }
    .retry-btn { display:flex;align-items:center;gap:4px;padding:6px 14px;border-radius:8px;border:1px solid #c6c6cd;background:#fff;cursor:pointer;font-weight:600;font-size:13px;white-space:nowrap; }
  `]
})
export class AdminErrorBannerComponent {
  @Input() title = 'Failed to load data';
  @Input() message = '';
  @Input() status = 0;
  @Input() showRetry = true;
  @Output() retry = new EventEmitter<void>();
}
