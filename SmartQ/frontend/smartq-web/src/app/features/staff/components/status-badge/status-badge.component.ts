import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="badge" [ngClass]="levelClass">{{ text }}</span>`,
  styles: [
    `.badge{display:inline-flex;padding:4px 10px;border-radius:999px;font-size:12px;font-weight:600}
    .ok{background:#e6fbf8;color:#00796b}
    .warn{background:#fff3e6;color:#b45309}
    .danger{background:#fdecec;color:#c5161d}
    .neutral{background:#eef1f6;color:#667085}`
  ]
})
export class StatusBadgeComponent {
  @Input({ required: true }) text = '';
  @Input() kind: 'ok' | 'warn' | 'danger' | 'neutral' = 'neutral';
  get levelClass(): string { return this.kind; }
}
