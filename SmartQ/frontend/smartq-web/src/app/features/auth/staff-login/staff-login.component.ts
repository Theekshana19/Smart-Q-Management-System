import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-staff-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './staff-login.component.html',
  styleUrl: './staff-login.component.scss'
})
export class StaffLoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  username = '';
  password = '';
  readonly loading = signal(false);
  readonly error = signal('');

  ngOnInit(): void {
    if (this.auth.isAuthenticated() && this.auth.hasRole('STAFF')) {
      this.auth.me().subscribe({
        next: () => {
          if (this.auth.getActiveCounterSession()) this.router.navigate(['/staff/queue-console']);
          else this.router.navigate(['/staff/select-counter']);
        }
      });
    }
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.username, this.password).subscribe({
      next: res => {
        if (res.user.role !== 'STAFF') {
          this.auth.clearSession();
          this.error.set('This account is not a staff user.');
          this.loading.set(false);
          return;
        }
        this.auth.redirectAfterLogin(res);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.message ?? 'Login failed. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
