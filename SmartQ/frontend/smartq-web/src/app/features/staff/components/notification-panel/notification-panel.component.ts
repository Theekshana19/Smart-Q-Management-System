import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { StaffNotificationResponse } from '../../../../core/models/staff-console.models';

@Component({
  selector: 'app-notification-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-panel.component.html',
  styleUrl: './notification-panel.component.scss'
})
export class NotificationPanelComponent {
  @Input() notifications: StaffNotificationResponse | null = null;
  @Output() close = new EventEmitter<void>();

  icon(type: string): string {
    if (type === 'WARNING') return 'timer';
    if (type === 'LOAD') return 'warning';
    if (type === 'QUEUE') return 'payments';
    return 'info';
  }
}
