import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DisplayBoard, CallNextResponse } from '../models';

@Injectable({ providedIn: 'root' })
export class QueueSignalRService {
  private connection?: signalR.HubConnection;

  readonly tokenCalled$ = new Subject<CallNextResponse>();
  readonly displayUpdated$ = new Subject<DisplayBoard>();
  readonly queueUpdated$ = new Subject<unknown>();

  async start(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl)
      .withAutomaticReconnect()
      .build();

    this.connection.on('TokenCalled', (data: CallNextResponse) => this.tokenCalled$.next(data));
    this.connection.on('DisplayUpdated', (data: DisplayBoard) => this.displayUpdated$.next(data));
    this.connection.on('QueueUpdated', (data: unknown) => this.queueUpdated$.next(data));

    await this.connection.start();
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
  }
}
