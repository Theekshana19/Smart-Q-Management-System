import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { StaffSidebarComponent } from '../components/staff-sidebar/staff-sidebar.component';
import { StaffTopbarComponent } from '../components/staff-topbar/staff-topbar.component';
import { StaffStateService } from '../services/staff-state.service';

@Component({
  selector: 'app-staff-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, StaffSidebarComponent, StaffTopbarComponent],
  templateUrl: './staff-layout.component.html',
  styleUrl: './staff-layout.component.scss'
})
export class StaffLayoutComponent {
  readonly state = inject(StaffStateService);
  constructor() {
    this.state.init();
  }
}
