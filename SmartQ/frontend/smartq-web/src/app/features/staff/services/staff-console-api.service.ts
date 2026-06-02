import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../../environments/environment';
import {
  CallNextActionResult,
  StaffActiveSession,
  StaffConsoleContext,
  StaffConsoleSummary,
  StaffNotificationResponse,
  StaffPerformance,
  StaffQueueItem,
  StaffDashboard,
  StaffTokenDetails,
  StaffTokenHistoryItem,
  StaffTransferOptions,
  StaffTransferTokenRequest,
  StaffMyCounter,
  StaffCounterStatusRequest,
  StaffCounterStatusResult,
  TokenActionResult
} from '../../../core/models/staff-console.models';

@Injectable({ providedIn: 'root' })
export class StaffConsoleApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/staff-console`;

  getContext(counterId: number) {
    return this.http.get<StaffConsoleContext>(`${this.base}/context`, { params: new HttpParams().set('counterId', counterId) });
  }
  getSummary(counterId: number) {
    return this.http.get<StaffConsoleSummary>(`${this.base}/summary`, { params: new HttpParams().set('counterId', counterId) });
  }
  getActiveSession(counterId: number) {
    return this.http.get<StaffActiveSession | null>(`${this.base}/active-session`, { params: new HttpParams().set('counterId', counterId) });
  }
  getQueue(counterId: number, scope: 'my-services' | 'all-branch' = 'my-services') {
    return this.http.get<StaffQueueItem[]>(`${this.base}/queue`, { params: new HttpParams().set('counterId', counterId).set('scope', scope) });
  }
  callNext(counterId: number) {
    return this.http.post<CallNextActionResult>(`${this.base}/${counterId}/call-next`, {});
  }
  recall(tokenId: number, counterId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/recall`, {}, { params: new HttpParams().set('counterId', counterId) });
  }
  startService(tokenId: number, counterId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/start-service`, {}, { params: new HttpParams().set('counterId', counterId) });
  }
  complete(tokenId: number, counterId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/complete`, {}, { params: new HttpParams().set('counterId', counterId) });
  }
  noShow(tokenId: number, counterId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/no-show`, {}, { params: new HttpParams().set('counterId', counterId) });
  }
  transfer(tokenId: number, counterId: number, request: StaffTransferTokenRequest) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/transfer`, request, { params: new HttpParams().set('counterId', counterId) });
  }
  getTokenHistory(counterId: number, date?: string, status?: string, serviceId?: number) {
    let params = new HttpParams().set('counterId', counterId);
    if (date) params = params.set('date', date);
    if (status) params = params.set('status', status);
    if (serviceId) params = params.set('serviceId', serviceId);
    return this.http.get<StaffTokenHistoryItem[]>(`${this.base}/token-history`, { params });
  }
  getPerformance(counterId: number, staffUserId?: number, range = 'today') {
    let params = new HttpParams().set('counterId', counterId).set('range', range);
    if (staffUserId) params = params.set('staffUserId', staffUserId);
    return this.http.get<StaffPerformance>(`${this.base}/performance`, { params });
  }
  getNotifications(counterId: number) {
    return this.http.get<StaffNotificationResponse>(`${this.base}/notifications`, { params: new HttpParams().set('counterId', counterId) });
  }
  getDashboard(counterId: number) {
    return this.http.get<StaffDashboard>(`${this.base}/dashboard`, { params: new HttpParams().set('counterId', counterId) });
  }
  getTokenDetails(tokenId: number) {
    return this.http.get<StaffTokenDetails>(`${this.base}/token-details/${tokenId}`);
  }
  getTransferOptions() {
    return this.http.get<StaffTransferOptions>(`${this.base}/transfer-options`);
  }
  getMyCounter(counterId: number, staffUserId?: number) {
    let params = new HttpParams().set('counterId', counterId);
    if (staffUserId) params = params.set('staffUserId', staffUserId);
    return this.http.get<StaffMyCounter>(`${this.base}/my-counter`, { params });
  }
  updateCounterStatus(counterId: number, request: StaffCounterStatusRequest) {
    return this.http.post<StaffCounterStatusResult>(`${this.base}/${counterId}/status`, request);
  }
}
