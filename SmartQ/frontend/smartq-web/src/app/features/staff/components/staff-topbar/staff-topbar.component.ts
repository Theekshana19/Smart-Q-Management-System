import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { StaffStateService } from '../../services/staff-state.service';
import { NotificationPanelComponent } from '../notification-panel/notification-panel.component';

@Component({
  selector: 'app-staff-topbar',
  standalone: true,
  imports: [CommonModule, NotificationPanelComponent],
  templateUrl: './staff-topbar.component.html',
  styleUrl: './staff-topbar.component.scss'
})
export class StaffTopbarComponent {
  readonly state = inject(StaffStateService);
  readonly showNotifications = signal(false);

  toggleNotifications(): void {
    this.showNotifications.update((v) => !v);
  }
}
