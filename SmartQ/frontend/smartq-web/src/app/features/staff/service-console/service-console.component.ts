import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CounterApiService } from '../../../core/api/counter-api.service';
import { TokenApiService } from '../../../core/api/token-api.service';
import { QueueSignalRService } from '../../../core/signalr/queue-signalr.service';
import { StaffConsoleSummary } from '../../../core/models';

/** Officer Sarah is seeded on counter 2 (Cash). */
const DEFAULT_COUNTER_ID = 2;

@Component({
  selector: 'app-service-console',
  imports: [FormsModule],
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
  readonly counters = signal<{ id: number; counterNo: string; counterName: string }[]>([]);
  /** Must stay in sync with the &lt;select&gt; — do not load the queue until counters are loaded. */
  readonly counterId = signal(DEFAULT_COUNTER_ID);

  ngOnInit(): void {
    this.counterApi.getCounters().subscribe(c => {
      this.counters.set(c);
      if (c.length && !c.some(x => x.id === this.counterId())) {
        this.counterId.set(c[0].id);
      }
      this.load();
    });
    this.signalr.start().then(() => this.signalr.queueUpdated$.subscribe(() => this.load()));
  }

  ngOnDestroy(): void { this.signalr.stop(); }

  onCounterSelect(id: number): void {
    if (!id || id === this.counterId()) return;
    this.counterId.set(id);
    this.loading.set(true);
    this.actionMsg.set('');
    this.load();
  }

  load(): void {
    this.counterApi.getConsoleSummary(this.counterId()).subscribe({
      next: s => { this.summary.set(s); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  callNext(): void {
    this.actionMsg.set('');
    this.counterApi.callNext(this.counterId()).subscribe({
      next: res => {
        this.actionMsg.set(res.success && res.data
          ? `Called token ${res.data.tokenNo}`
          : res.message);
        this.load();
      },
      error: () => this.actionMsg.set('Could not call next token. Check API connection.')
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
