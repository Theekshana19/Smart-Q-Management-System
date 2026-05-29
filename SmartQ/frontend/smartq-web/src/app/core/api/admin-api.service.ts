import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin`;

  getDashboardSummary() {
    return this.http.get<unknown>(`${this.base}/dashboard/summary`);
  }

  getServiceSummary() {
    return this.http.get<{ totalServices: number; activeNow: number; totalTokensToday: number; avgWaitMinutes: number }>(`${this.base}/services/summary`);
  }

  getServices() {
    return this.http.get<unknown[]>(`${this.base}/services`);
  }

  createService(body: unknown) {
    return this.http.post(`${this.base}/services`, body);
  }

  updateService(id: number, body: unknown) {
    return this.http.put(`${this.base}/services/${id}`, body);
  }

  getCounters() {
    return this.http.get<unknown[]>(`${this.base}/counters`);
  }

  getTokenHistory(filter: Record<string, string | number | undefined>) {
    let params = new HttpParams();
    Object.entries(filter).forEach(([k, v]) => {
      if (v !== undefined && v !== '') params = params.set(k, String(v));
    });
    return this.http.get<unknown>(`${this.base}/reports/token-history`, { params });
  }
}
