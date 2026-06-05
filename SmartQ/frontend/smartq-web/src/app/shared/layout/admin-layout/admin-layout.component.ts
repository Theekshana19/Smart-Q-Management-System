import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss'
})
export class AdminLayoutComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly profile = signal({ fullName: 'Admin User', role: 'ADMIN' });

  ngOnInit(): void {
    if (this.auth.getCurrentUser()) {
      const user = this.auth.getCurrentUser()!;
      this.profile.set({ fullName: user.fullName, role: user.role });
      return;
    }
    this.auth.me().subscribe({
      next: res => this.profile.set({ fullName: res.fullName, role: res.role }),
      error: () => {}
    });
  }

  logout(): void {
    const role = this.auth.getCurrentUser()?.role;
    this.auth.logout().subscribe({
      complete: () => this.auth.redirectAfterLogout(role)
    });
  }
}
