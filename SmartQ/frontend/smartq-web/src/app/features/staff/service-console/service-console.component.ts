import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CounterApiService } from '../../../core/api/counter-api.service';
import { TokenApiService } from '../../../core/api/token-api.service';
import { QueueSignalRService } from '../../../core/signalr/queue-signalr.service';
import { StaffConsoleSummary } from '../../../core/models';

@Component({
  selector: 'app-service-console',
  templateUrl: './service-console.component.html',
  styleUrl: './service-console.component.scss'
})
export class ServiceConsoleComponent implements OnInit, OnDestroy {
  private readonly counterApi = inject(CounterApiService);
  private readonly tokenApi = inject(TokenApiService);
  private readonly signalr = inject(QueueSignalRService);

  readonly summary = signal<StaffConsoleSummary | null>(null);
  readonly loading = signal(true);
  readonly actionMsg = signal('');
  counterId = 2; // Default: Counter 02 (seed data)

  ngOnInit(): void {
    this.load();
    this.signalr.start().then(() => this.signalr.queueUpdated$.subscribe(() => this.load()));
  }

  ngOnDestroy(): void { this.signalr.stop(); }

  load(): void {
    this.counterApi.getConsoleSummary(this.counterId).subscribe({
      next: s => { this.summary.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  callNext(): void {
    this.counterApi.callNext(this.counterId).subscribe({
      next: () => { this.actionMsg.set('Token called'); this.load(); },
      error: () => this.actionMsg.set('No tokens waiting')
    });
  }

  complete(): void {
    const id = this.summary()?.queue.activeToken?.id;
    if (!id) return;
    this.tokenApi.complete(id).subscribe(() => this.load());
  }

  skip(): void {
    const id = this.summary()?.queue.activeToken?.id;
    if (!id) return;
    this.tokenApi.skip(id).subscribe(() => this.load());
  }

  recall(): void {
    const id = this.summary()?.queue.activeToken?.id;
    if (!id) return;
    this.tokenApi.recall(id).subscribe(() => this.load());
  }
}
