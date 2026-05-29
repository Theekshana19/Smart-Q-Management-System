import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Language } from '../models';

@Injectable({ providedIn: 'root' })
export class LanguageApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/languages`;

  getLanguages() {
    return this.http.get<Language[]>(this.base);
  }
}
