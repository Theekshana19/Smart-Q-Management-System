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
  now = new Date();
  private timer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.timer = setInterval(() => (this.now = new Date()), 1000);
    this.loadBoard();
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
    if (this.timer) clearInterval(this.timer);
    this.signalr.stop();
  }

  private loadBoard(): void {
    this.displayApi.getBoard().subscribe(b => this.applyBoard(b, false));
  }

  private applyBoard(b: DisplayBoard, announce: boolean): void {
    this.board.set(b);
  }
}
