import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { KioskStatus, ServiceItem, SubServiceItem } from '../models';

@Injectable({ providedIn: 'root' })
export class ServiceApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/services`;

  getServices(languageCode: string) {
    return this.http.get<ServiceItem[]>(`${this.base}?languageCode=${languageCode}`);
  }

  getSubServices(serviceId: number, languageCode: string) {
    return this.http.get<SubServiceItem[]>(`${this.base}/${serviceId}/sub-services?languageCode=${languageCode}`);
  }

  getKioskStatus() {
    return this.http.get<KioskStatus>(`${this.base}/kiosk-status`);
  }
}
