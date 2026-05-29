import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { DisplayBoard, VoiceTemplate } from '../models';

@Injectable({ providedIn: 'root' })
export class DisplayApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/display`;

  getBoard() {
    return this.http.get<DisplayBoard>(`${this.base}/board`);
  }

  getVoiceTemplate(eventType = 'TOKEN_CALLED', languageCode = 'EN') {
    return this.http.get<VoiceTemplate>(`${this.base}/voice-template?eventType=${eventType}&languageCode=${languageCode}`);
  }
}
