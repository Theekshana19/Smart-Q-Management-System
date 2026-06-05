import { CommonModule } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
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
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly displayName = computed(() => this.state.context()?.staff?.fullName ?? this.auth.getCurrentUser()?.fullName ?? 'Staff');
  readonly counterLabel = computed(() => {
    const counter = this.state.context()?.counter;
    if (counter) return `Counter ${counter.counterNo}`;
    const session = this.auth.getActiveCounterSession();
    return session ? `Counter ${session.counterNo}` : 'No counter';
  });

  initials(name: string): string {
    return name.split(' ').filter(Boolean).slice(0, 2).map((p) => p[0]?.toUpperCase() ?? '').join('');
  }

  logout(): void {
    this.state.reset();
    this.auth.logout().subscribe({
      complete: () => this.auth.redirectAfterLogout('STAFF')
    });
  }
}
