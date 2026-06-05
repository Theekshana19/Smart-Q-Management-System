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

  getContext() {
    return this.http.get<StaffConsoleContext>(`${this.base}/context`);
  }
  getSummary() {
    return this.http.get<StaffConsoleSummary>(`${this.base}/summary`);
  }
  getActiveSession() {
    return this.http.get<StaffActiveSession | null>(`${this.base}/active-session`);
  }
  getQueue(scope: 'my-services' | 'all-branch' = 'my-services') {
    return this.http.get<StaffQueueItem[]>(`${this.base}/queue`, { params: new HttpParams().set('scope', scope) });
  }
  callNext() {
    return this.http.post<CallNextActionResult>(`${this.base}/call-next`, {});
  }
  recall(tokenId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/recall`, {});
  }
  startService(tokenId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/start-service`, {});
  }
  complete(tokenId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/complete`, {});
  }
  noShow(tokenId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/no-show`, {});
  }
  cancel(tokenId: number) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/cancel`, {});
  }
  transfer(tokenId: number, request: StaffTransferTokenRequest) {
    return this.http.post<TokenActionResult>(`${this.base}/tokens/${tokenId}/transfer`, request);
  }
  getTokenHistory(options?: { date?: string; dateFrom?: string; dateTo?: string; status?: string; serviceId?: number }) {
    let params = new HttpParams();
    if (options?.dateFrom) params = params.set('dateFrom', options.dateFrom);
    if (options?.dateTo) params = params.set('dateTo', options.dateTo);
    if (options?.date) params = params.set('date', options.date);
    if (options?.status) params = params.set('status', options.status);
    if (options?.serviceId) params = params.set('serviceId', options.serviceId);
    return this.http.get<StaffTokenHistoryItem[]>(`${this.base}/token-history`, { params });
  }
  getPerformance(range = 'today') {
    return this.http.get<StaffPerformance>(`${this.base}/performance`, { params: new HttpParams().set('range', range) });
  }
  getNotifications() {
    return this.http.get<StaffNotificationResponse>(`${this.base}/notifications`);
  }
  getDashboard() {
    return this.http.get<StaffDashboard>(`${this.base}/dashboard`);
  }
  getTokenDetails(tokenId: number) {
    return this.http.get<StaffTokenDetails>(`${this.base}/token-details/${tokenId}`);
  }
  getTransferOptions() {
    return this.http.get<StaffTransferOptions>(`${this.base}/transfer-options`);
  }
  getMyCounter() {
    return this.http.get<StaffMyCounter>(`${this.base}/my-counter`);
  }
  updateCounterStatus(request: StaffCounterStatusRequest) {
    return this.http.post<StaffCounterStatusResult>(`${this.base}/status`, request);
  }
}
