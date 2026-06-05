import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { PublicDisplayMessageItem, PublicSettingItem } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class PublicConfigApiService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiUrl;

  getPublicSettings() {
    return this.http.get<PublicSettingItem[]>(`${this.api}/settings/public`);
  }

  getPublicDisplayMessages(languageCode = 'EN') {
    return this.http.get<PublicDisplayMessageItem[]>(`${this.api}/display-messages/public`, {
      params: new HttpParams().set('languageCode', languageCode)
    });
  }

  getVoiceTemplate(languageCode: string, eventType: string) {
    return this.http.get<{ eventType: string; templateText: string; languageCode: string }>(
      `${this.api}/voice-templates/template`,
      { params: new HttpParams().set('languageCode', languageCode).set('eventType', eventType) }
    );
  }
}
