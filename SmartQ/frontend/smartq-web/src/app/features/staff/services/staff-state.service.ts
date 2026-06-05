import { Injectable, inject, signal } from '@angular/core';
import { forkJoin } from 'rxjs';
import { StaffActiveSession, StaffConsoleContext, StaffConsoleSummary, StaffNotificationResponse, StaffQueueItem } from '../../../core/models/staff-console.models';
import { QueueSignalRService } from '../../../core/signalr/queue-signalr.service';
import { StaffConsoleApiService } from './staff-console-api.service';

@Injectable({ providedIn: 'root' })
export class StaffStateService {
  private readonly api = inject(StaffConsoleApiService);
  private readonly signalr = inject(QueueSignalRService);
  private initialized = false;

  readonly context = signal<StaffConsoleContext | null>(null);
  readonly summary = signal<StaffConsoleSummary | null>(null);
  readonly activeSession = signal<StaffActiveSession | null>(null);
  readonly queue = signal<StaffQueueItem[]>([]);
  readonly notifications = signal<StaffNotificationResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');

  init(): void {
    if (this.initialized) return;
    this.loadAll('my-services');
    this.signalr.start().then(() => {
      this.signalr.queueUpdated$.subscribe(() => this.loadAll('my-services'));
      this.signalr.tokenCalled$.subscribe(() => this.loadAll('my-services'));
      this.signalr.tokenCompleted$.subscribe(() => this.loadAll('my-services'));
      this.signalr.tokenSkipped$.subscribe(() => this.loadAll('my-services'));
      this.signalr.displayUpdated$.subscribe(() => this.loadAll('my-services'));
    });
    this.initialized = true;
  }

  reset(): void {
    this.initialized = false;
    this.context.set(null);
    this.summary.set(null);
    this.activeSession.set(null);
    this.queue.set([]);
    this.notifications.set(null);
    this.loading.set(true);
    this.error.set('');
  }

  msg(key: string, fallback = ''): string {
    return this.context()?.displayMessages?.[key] ?? fallback;
  }

  callNextDisabled(): boolean {
    const ctx = this.context();
    if (!ctx?.callNextLockWhenActiveToken) return false;
    return !!this.activeSession();
  }

  loadAll(scope: 'my-services' | 'all-branch' = 'my-services'): void {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      context: this.api.getContext(),
      summary: this.api.getSummary(),
      activeSession: this.api.getActiveSession(),
      queue: this.api.getQueue(scope),
      notifications: this.api.getNotifications()
    }).subscribe({
      next: (res) => {
        this.context.set(res.context);
        this.summary.set(res.summary);
        this.activeSession.set(res.activeSession);
        this.queue.set(res.queue);
        this.notifications.set(res.notifications);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err?.error?.message ?? 'Unable to load staff console. Please check API connection.');
        this.loading.set(false);
      }
    });
  }
}
