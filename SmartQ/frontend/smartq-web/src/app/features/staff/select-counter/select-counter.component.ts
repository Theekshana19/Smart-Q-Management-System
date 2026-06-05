import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { StaffAuthApiService } from '../../../core/api/staff-auth-api.service';
import { AuthService } from '../../../core/auth/auth.service';
import { AvailableCounter } from '../../../core/auth/auth.models';

@Component({
  selector: 'app-select-counter',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './select-counter.component.html',
  styleUrl: './select-counter.component.scss'
})
export class SelectCounterComponent implements OnInit {
  private readonly api = inject(StaffAuthApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly counters = signal<AvailableCounter[]>([]);
  readonly loading = signal(true);
  readonly selecting = signal<number | null>(null);
  readonly error = signal('');

  ngOnInit(): void {
    this.api.getAvailable().subscribe({
      next: list => {
        this.counters.set(list);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(err?.error?.message ?? 'Failed to load counters.');
        this.loading.set(false);
      }
    });
  }

  select(counter: AvailableCounter): void {
    if (!counter.isAvailableForLogin) return;
    this.selecting.set(counter.counterId);
    this.error.set('');
    this.api.select(counter.counterId, 'Staff Console').subscribe({
      next: session => {
        this.auth.setActiveCounterSession({
          sessionId: session.sessionId,
          counterId: session.counterId,
          counterNo: session.counterNo,
          counterName: session.counterName,
          status: session.status,
          startedAt: session.startedAt
        });
        this.router.navigate(['/staff/queue-console']);
      },
      error: err => {
        this.error.set(err?.error?.message ?? 'Could not select counter.');
        this.selecting.set(null);
      }
    });
  }
}
