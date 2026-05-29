import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CallNextResult, CounterQueue, StaffConsoleSummary } from '../models';

@Injectable({ providedIn: 'root' })
export class CounterApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/counters`;

  getCounters() {
    return this.http.get<{ id: number; counterNo: string; counterName: string; status: string; isActive: boolean }[]>(this.base);
  }

  getQueue(counterId: number) {
    return this.http.get<CounterQueue>(`${this.base}/${counterId}/queue`);
  }

  callNext(counterId: number) {
    return this.http.post<CallNextResult>(`${this.base}/${counterId}/call-next`, {});
  }

  getConsoleSummary(counterId: number) {
    return this.http.get<StaffConsoleSummary>(`${this.base}/${counterId}/console-summary`);
  }
}
