import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-login.component.html',
  styleUrl: './admin-login.component.scss'
})
export class AdminLoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  username = '';
  password = '';
  readonly loading = signal(false);
  readonly error = signal('');

  ngOnInit(): void {
    if (this.auth.isAuthenticated() && this.auth.hasRole('ADMIN')) {
      this.router.navigate(['/admin/dashboard']);
    }
  }

  submit(): void {
    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.username, this.password).subscribe({
      next: res => {
        if (res.user.role !== 'ADMIN') {
          this.auth.clearSession();
          this.error.set('This account is not an administrator.');
          this.loading.set(false);
          return;
        }
        this.auth.me().subscribe({
          next: () => {
            this.loading.set(false);
            this.router.navigate(['/admin/dashboard']);
          },
          error: () => {
            this.loading.set(false);
            this.router.navigate(['/admin/dashboard']);
          }
        });
      },
      error: err => {
        this.error.set(err?.error?.message ?? 'Login failed. Please try again.');
        this.loading.set(false);
      }
    });
  }
}
