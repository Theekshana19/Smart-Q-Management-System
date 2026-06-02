import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DisplayApiService } from '../../../core/api/display-api.service';
import { QueueSignalRService } from '../../../core/signalr/queue-signalr.service';
import { VoiceAnnouncementService } from '../../../core/services/voice-announcement.service';
import { DisplayBoard } from '../../../core/models';

@Component({
  selector: 'app-queue-board',
  imports: [DatePipe],
  templateUrl: './queue-board.component.html',
  styleUrl: './queue-board.component.scss'
})
export class QueueBoardComponent implements OnInit, OnDestroy {
  private readonly displayApi = inject(DisplayApiService);
  private readonly signalr = inject(QueueSignalRService);
  private readonly voice = inject(VoiceAnnouncementService);

  readonly board = signal<DisplayBoard | null>(null);
  readonly flash = signal(false);
  readonly now = signal(new Date());
  private clockTimer?: ReturnType<typeof setInterval>;
  private refreshTimer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.clockTimer = setInterval(() => this.now.set(new Date()), 1000);
    this.loadBoard();
    this.refreshTimer = setInterval(() => this.loadBoard(), 30_000);
    this.signalr.start().then(() => {
      this.signalr.displayUpdated$.subscribe(b => this.applyBoard(b, false));
      this.signalr.tokenCalled$.subscribe(call => {
        this.flash.set(true);
        setTimeout(() => this.flash.set(false), 3000);
        this.displayApi.getVoiceTemplate('TOKEN_CALLED', 'EN').subscribe(t =>
          this.voice.announceTokenCalled(call.tokenNo, call.counterName, t)
        );
        this.loadBoard();
      });
    });
  }

  ngOnDestroy(): void {
    if (this.clockTimer) clearInterval(this.clockTimer);
    if (this.refreshTimer) clearInterval(this.refreshTimer);
    this.signalr.stop();
  }

  private loadBoard(): void {
    this.displayApi.getBoard().subscribe(b => this.applyBoard(b, false));
  }

  private applyBoard(b: DisplayBoard, announce: boolean): void {
    const waitingItems = [...b.waitingQueue.items];
    waitingItems.sort((a, z) => {
      const aTransferred = this.isTransferredToken(a.tokenNo);
      const zTransferred = this.isTransferredToken(z.tokenNo);
      if (aTransferred === zTransferred) return 0;
      return aTransferred ? 1 : -1;
    });

    this.board.set({
      ...b,
      waitingQueue: {
        ...b.waitingQueue,
        items: waitingItems
      }
    });
  }

  isTransferredToken(tokenNo: string): boolean {
    return tokenNo.toUpperCase().includes('(T)');
  }
}
