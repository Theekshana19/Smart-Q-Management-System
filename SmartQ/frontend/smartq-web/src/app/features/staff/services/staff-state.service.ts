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
  private readonly counterStorageKey = 'staff.selectedCounterId';

  readonly counterId = signal(2);
  readonly context = signal<StaffConsoleContext | null>(null);
  readonly summary = signal<StaffConsoleSummary | null>(null);
  readonly activeSession = signal<StaffActiveSession | null>(null);
  readonly queue = signal<StaffQueueItem[]>([]);
  readonly notifications = signal<StaffNotificationResponse | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');

  init(counterId = 2): void {
    if (this.initialized) return;

    this.counterId.set(this.restoreCounterId(counterId));
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

  msg(key: string, fallback = ''): string {
    return this.context()?.displayMessages?.[key] ?? fallback;
  }

  callNextDisabled(): boolean {
    const ctx = this.context();
    if (!ctx?.callNextLockWhenActiveToken) return false;
    return !!this.activeSession();
  }

  setCounter(counterId: number, scope: 'my-services' | 'all-branch' = 'my-services'): void {
    if (!counterId || counterId <= 0 || counterId === this.counterId()) return;
    this.counterId.set(counterId);
    if (typeof localStorage !== 'undefined') localStorage.setItem(this.counterStorageKey, String(counterId));
    this.loadAll(scope);
  }

  loadAll(scope: 'my-services' | 'all-branch' = 'my-services'): void {
    this.loading.set(true);
    this.error.set('');
    forkJoin({
      context: this.api.getContext(this.counterId()),
      summary: this.api.getSummary(this.counterId()),
      activeSession: this.api.getActiveSession(this.counterId()),
      queue: this.api.getQueue(this.counterId(), scope),
      notifications: this.api.getNotifications(this.counterId())
    }).subscribe({
      next: (res) => {
        this.context.set(res.context);
        this.summary.set(res.summary);
        this.activeSession.set(res.activeSession);
        this.queue.set(res.queue);
        this.notifications.set(res.notifications);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load staff console. Please check API connection.');
        this.loading.set(false);
      }
    });
  }

  private restoreCounterId(fallback: number): number {
    if (typeof localStorage === 'undefined') return fallback;
    const raw = localStorage.getItem(this.counterStorageKey);
    const parsed = Number(raw);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
  }
}
