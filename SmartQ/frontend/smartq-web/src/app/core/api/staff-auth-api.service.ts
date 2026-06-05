import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AvailableCounter, StaffCounterSessionResult } from '../auth/auth.models';

@Injectable({ providedIn: 'root' })
export class StaffAuthApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/staff/counters`;

  getAvailable() {
    return this.http.get<AvailableCounter[]>(`${this.base}/available`);
  }

  select(counterId: number, deviceName?: string) {
    return this.http.post<StaffCounterSessionResult>(`${this.base}/select`, { counterId, deviceName });
  }

  endSession() {
    return this.http.post<{ success: boolean }>(`${this.base}/end-session`, {});
  }
}
