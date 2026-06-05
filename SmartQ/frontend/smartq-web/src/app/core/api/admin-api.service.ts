import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AdminCounterItem,
  AdminDisplayMessageItem,
  AdminLanguageItem,
  AdminProfile,
  AdminServiceItem,
  AdminStaffItem,
  AdminSubServiceItem,
  AdminSystemSettingItem,
  AdminVoiceTemplateItem,
  AssignableService,
  CreateStaffRequest,
  LanguageManagementSummary,
  StaffManagementSummary,
  UpdateAdminSettingRequest,
  UpdateStaffRequest,
  UpsertDisplayMessageRequest,
  UpsertLanguageRequest,
  UpsertVoiceTemplateRequest,
  CounterAssignmentItem,
  CounterManagement,
  DashboardSummary,
  PagedResult,
  ServiceListQuery,
  ServiceManagementSummary,
  SubServiceListQuery,
  TokenHistoryFilter,
  TokenHistoryReport,
  UpsertCounterRequest,
  UpsertServiceRequest,
  UpsertSubServiceRequest,
} from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin`;

  getProfile() {
    return this.http.get<AdminProfile>(`${this.base}/profile`);
  }

  getDashboardSummary() {
    return this.http.get<DashboardSummary>(`${this.base}/dashboard/summary`);
  }

  getServiceSummary() {
    return this.http.get<ServiceManagementSummary>(`${this.base}/services/summary`);
  }

  getServices(query: ServiceListQuery = {}) {
    return this.http.get<PagedResult<AdminServiceItem>>(`${this.base}/services`, {
      params: this.buildParams(query as Record<string, string | number | boolean | undefined>)
    });
  }

  createService(body: UpsertServiceRequest) {
    return this.http.post<AdminServiceItem>(`${this.base}/services`, body);
  }

  updateService(id: number, body: UpsertServiceRequest) {
    return this.http.put<AdminServiceItem>(`${this.base}/services/${id}`, body);
  }

  patchServiceStatus(id: number, isActive: boolean) {
    return this.http.patch<AdminServiceItem>(`${this.base}/services/${id}/status`, { isActive });
  }

  deleteService(id: number) {
    return this.http.delete<void>(`${this.base}/services/${id}`);
  }

  getSubServices(query: SubServiceListQuery = {}) {
    return this.http.get<PagedResult<AdminSubServiceItem>>(`${this.base}/sub-services`, {
      params: this.buildParams(query as Record<string, string | number | boolean | undefined>)
    });
  }

  createSubService(body: UpsertSubServiceRequest) {
    return this.http.post<AdminSubServiceItem>(`${this.base}/sub-services`, body);
  }

  updateSubService(id: number, body: UpsertSubServiceRequest) {
    return this.http.put<AdminSubServiceItem>(`${this.base}/sub-services/${id}`, body);
  }

  patchSubServiceStatus(id: number, isActive: boolean) {
    return this.http.patch<AdminSubServiceItem>(`${this.base}/sub-services/${id}/status`, { isActive });
  }

  deleteSubService(id: number) {
    return this.http.delete<void>(`${this.base}/sub-services/${id}`);
  }

  getCounters() {
    return this.http.get<PagedResult<AdminCounterItem>>(`${this.base}/counters`);
  }

  getCounterManagement() {
    return this.http.get<CounterManagement>(`${this.base}/counters/management`);
  }

  createCounter(body: UpsertCounterRequest) {
    return this.http.post<AdminCounterItem>(`${this.base}/counters`, body);
  }

  updateCounter(id: number, body: UpsertCounterRequest) {
    return this.http.put<AdminCounterItem>(`${this.base}/counters/${id}`, body);
  }

  patchCounterStatus(id: number, status: string) {
    return this.http.patch<AdminCounterItem>(`${this.base}/counters/${id}/status`, { status });
  }

  deleteCounter(id: number) {
    return this.http.delete<void>(`${this.base}/counters/${id}`);
  }

  getCounterAssignments() {
    return this.http.get<CounterAssignmentItem[]>(`${this.base}/counter-assignments`);
  }

  getAssignableServices(counterId: number) {
    return this.http.get<AssignableService[]>(`${this.base}/counters/${counterId}/assignable-services`);
  }

  saveCounterAssignments(counterId: number, serviceIds: number[]) {
    return this.http.post<void>(`${this.base}/counter-assignments`, { counterId, serviceIds });
  }

  getTokenHistory(filter: TokenHistoryFilter) {
    return this.http.get<TokenHistoryReport>(`${this.base}/reports/token-history`, {
      params: this.buildParams(filter as Record<string, string | number | boolean | undefined>)
    });
  }

  getStaffSummary() {
    return this.http.get<StaffManagementSummary>(`${this.base}/staff/summary`);
  }

  getStaff(search?: string, role?: string, isActive?: boolean, page = 1, pageSize = 50) {
    return this.http.get<PagedResult<AdminStaffItem>>(`${this.base}/staff`, {
      params: this.buildParams({ search, role, isActive, page, pageSize })
    });
  }

  createStaff(request: CreateStaffRequest) {
    return this.http.post<AdminStaffItem>(`${this.base}/staff`, request);
  }

  updateStaff(id: number, request: UpdateStaffRequest) {
    return this.http.put<AdminStaffItem>(`${this.base}/staff/${id}`, request);
  }

  resetStaffPassword(id: number, newPassword: string) {
    return this.http.post(`${this.base}/staff/${id}/reset-password`, { newPassword });
  }

  patchStaffStatus(id: number, isActive: boolean) {
    return this.http.patch<AdminStaffItem>(`${this.base}/staff/${id}/status`, { isActive });
  }

  forceLogoutStaff(id: number) {
    return this.http.post(`${this.base}/staff/${id}/force-logout`, {});
  }

  getLanguageSummary() {
    return this.http.get<LanguageManagementSummary>(`${this.base}/languages/summary`);
  }

  getLanguages() {
    return this.http.get<AdminLanguageItem[]>(`${this.base}/languages`);
  }

  createLanguage(request: UpsertLanguageRequest) {
    return this.http.post<AdminLanguageItem>(`${this.base}/languages`, request);
  }

  updateLanguage(id: number, request: UpsertLanguageRequest) {
    return this.http.put<AdminLanguageItem>(`${this.base}/languages/${id}`, request);
  }

  patchLanguageStatus(id: number, isActive: boolean) {
    return this.http.patch<AdminLanguageItem>(`${this.base}/languages/${id}/status`, { isActive });
  }

  deleteLanguage(id: number) {
    return this.http.delete(`${this.base}/languages/${id}`);
  }

  getSettings(search?: string, dataType?: string) {
    return this.http.get<AdminSystemSettingItem[]>(`${this.base}/settings`, {
      params: this.buildParams({ search, dataType })
    });
  }

  getSetting(id: number) {
    return this.http.get<AdminSystemSettingItem>(`${this.base}/settings/${id}`);
  }

  updateSetting(id: number, request: UpdateAdminSettingRequest) {
    return this.http.put<AdminSystemSettingItem>(`${this.base}/settings/${id}`, request);
  }

  getDisplayMessages(languageId?: number, messageKey?: string, isActive?: boolean) {
    return this.http.get<AdminDisplayMessageItem[]>(`${this.base}/display-messages`, {
      params: this.buildParams({ languageId, messageKey, isActive })
    });
  }

  createDisplayMessage(request: UpsertDisplayMessageRequest) {
    return this.http.post<AdminDisplayMessageItem>(`${this.base}/display-messages`, request);
  }

  updateDisplayMessage(id: number, request: UpsertDisplayMessageRequest) {
    return this.http.put<AdminDisplayMessageItem>(`${this.base}/display-messages/${id}`, request);
  }

  patchDisplayMessageStatus(id: number, isActive: boolean) {
    return this.http.patch<AdminDisplayMessageItem>(`${this.base}/display-messages/${id}/status`, { isActive });
  }

  deleteDisplayMessage(id: number) {
    return this.http.delete(`${this.base}/display-messages/${id}`);
  }

  getVoiceTemplates(languageId?: number, eventType?: string, isActive?: boolean) {
    return this.http.get<AdminVoiceTemplateItem[]>(`${this.base}/voice-templates`, {
      params: this.buildParams({ languageId, eventType, isActive })
    });
  }

  createVoiceTemplate(request: UpsertVoiceTemplateRequest) {
    return this.http.post<AdminVoiceTemplateItem>(`${this.base}/voice-templates`, request);
  }

  updateVoiceTemplate(id: number, request: UpsertVoiceTemplateRequest) {
    return this.http.put<AdminVoiceTemplateItem>(`${this.base}/voice-templates/${id}`, request);
  }

  patchVoiceTemplateStatus(id: number, isActive: boolean) {
    return this.http.patch<AdminVoiceTemplateItem>(`${this.base}/voice-templates/${id}/status`, { isActive });
  }

  deleteVoiceTemplate(id: number) {
    return this.http.delete(`${this.base}/voice-templates/${id}`);
  }

  static parseError(err: unknown): { status: number; message: string } {
    if (err instanceof HttpErrorResponse) {
      const body = err.error as { message?: string; detail?: string } | null;
      const msg = body?.message ?? body?.detail ?? err.statusText ?? 'Request failed';
      return { status: err.status, message: msg };
    }
    return { status: 0, message: 'Network error — is the API running?' };
  }

  withErrorHandling<T>() {
    return (source: Observable<T>) =>
      source.pipe(catchError(err => throwError(() => AdminApiService.parseError(err))));
  }

  private buildParams(obj: Record<string, string | number | boolean | undefined>): HttpParams {
    let params = new HttpParams();
    for (const [key, val] of Object.entries(obj)) {
      if (val !== undefined && val !== null && val !== '') {
        params = params.set(key, String(val));
      }
    }
    return params;
  }
}
