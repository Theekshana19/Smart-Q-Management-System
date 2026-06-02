import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { StaffStateService } from '../../services/staff-state.service';

@Component({
  selector: 'app-staff-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './staff-sidebar.component.html',
  styleUrl: './staff-sidebar.component.scss'
})
export class StaffSidebarComponent {
  readonly state = inject(StaffStateService);

  initials(name: string): string {
    return name.split(' ').filter(Boolean).slice(0, 2).map((p) => p[0]?.toUpperCase() ?? '').join('');
  }
}
