import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { GenerateTokenRequest, GenerateTokenResponse, TokenDetail } from '../models';

@Injectable({ providedIn: 'root' })
export class TokenApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tokens`;

  generate(request: GenerateTokenRequest) {
    return this.http.post<GenerateTokenResponse>(`${this.base}/generate`, request);
  }

  getById(id: number) {
    return this.http.get<TokenDetail>(`${this.base}/${id}`);
  }

  recall(tokenId: number) {
    return this.http.post<TokenDetail>(`${this.base}/${tokenId}/recall`, {});
  }

  start(tokenId: number) {
    return this.http.post<TokenDetail>(`${this.base}/${tokenId}/start`, {});
  }

  complete(tokenId: number) {
    return this.http.post<TokenDetail>(`${this.base}/${tokenId}/complete`, {});
  }

  skip(tokenId: number) {
    return this.http.post<TokenDetail>(`${this.base}/${tokenId}/skip`, {});
  }
}
