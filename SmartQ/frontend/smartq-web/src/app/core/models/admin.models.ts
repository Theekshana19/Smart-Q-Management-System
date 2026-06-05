export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

export interface AdminProfile {
  fullName: string;
  role: string;
}

export interface HourlyFlowPoint {
  hour: string;
  general: number;
  priority: number;
}

export interface TokenDistributionPoint {
  category: string;
  count: number;
  color: string;
}

export interface ActivityItem {
  title: string;
  description: string;
  type: string;
  timeAgo: string;
}

export interface CounterStatus {
  counterId: string;
  staffName: string;
  status: string;
  loadPercent: number;
  isVip: boolean;
}

export interface DashboardSummary {
  activeTokens: number;
  avgWaitMinutes: number;
  staffOnline: number;
  staffTotal: number;
  satisfactionRate: number | null;
  tokensToday: number;
  waitTrendMinutes: number;
  activeTokensTrendPercent: number;
  hourlyFlow: HourlyFlowPoint[];
  tokenDistribution: TokenDistributionPoint[];
  recentActivity: ActivityItem[];
  counterStatuses: CounterStatus[];
}

export interface AdminServiceItem {
  id: number;
  code: string;
  name: string;
  description: string;
  icon: string;
  displayOrder: number;
  isActive: boolean;
  subServiceCount: number;
  assignedCounterCount: number;
  tokensToday: number;
  averageWaitMinutes: number;
}

export interface ServiceManagementSummary {
  totalServices: number;
  activeNow: number;
  totalTokensToday: number;
  avgWaitMinutes: number;
}

export interface UpsertServiceRequest {
  code: string;
  name: string;
  description: string;
  icon: string;
  displayOrder: number;
  isActive: boolean;
}

export interface AdminSubServiceItem {
  id: number;
  serviceId: number;
  serviceName: string;
  code: string;
  name: string;
  description: string;
  tokenPrefix: string;
  icon: string;
  estimatedServiceMinutes: number;
  displayOrder: number;
  isActive: boolean;
  tokensToday: number;
}

export interface UpsertSubServiceRequest {
  serviceId: number;
  code: string;
  name: string;
  description: string;
  tokenPrefix: string;
  icon: string;
  estimatedServiceMinutes: number;
  displayOrder: number;
  isActive: boolean;
}

export interface AdminCounterItem {
  id: number;
  counterNo: string;
  counterName: string;
  status: string;
  isActive: boolean;
  assignedServices: string[];
  activeStaffName: string | null;
  currentTokenNo: string | null;
  tokensToday: number;
}

export interface UpsertCounterRequest {
  counterNo: string;
  counterName: string;
  status: string;
  isActive: boolean;
}

export interface CounterManagementSummary {
  activeCounters: number;
  totalCounters: number;
  staffLive: number;
  staffCapacityPercent: number;
  avgServiceTime: string;
  pendingTickets: number;
  trafficAlert: string;
}

export interface CounterManagementCard {
  id: number;
  counterNo: string;
  counterName: string;
  unitName: string;
  status: string;
  staffName: string | null;
  staffRole: string | null;
  currentTicket: string | null;
  progressPercent: number;
  waitTimeLabel: string;
  waitLimitLabel: string | null;
  todayVolume: number;
  feedbackScore: number | null;
  isOffline: boolean;
  offlineMessage: string | null;
}

export interface CounterManagement {
  summary: CounterManagementSummary;
  counters: CounterManagementCard[];
}

export interface CounterAssignmentItem {
  counterId: number;
  counterNo: string;
  counterName: string;
  assignedServiceIds: number[];
  assignedServiceNames: string[];
}

export interface AssignableService {
  id: number;
  code: string;
  name: string;
  isActive: boolean;
  isAssigned: boolean;
  tokenPrefixes: string[];
}

export interface TokenHistoryRow {
  id: number;
  tokenNo: string;
  serviceName: string;
  subServiceName: string;
  counterName: string | null;
  createdAt: string;
  calledAt: string | null;
  completedAt: string | null;
  waitingMinutes: number | null;
  serviceMinutes: number | null;
  status: string;
}

export interface TokenHistorySummary {
  totalTokens: number;
  completed: number;
  skipped: number;
  averageWaitMinutes: number;
}

export interface TrafficDistributionPoint {
  hourLabel: string;
  count: number;
  percent: number;
}

export interface OperationalInsight {
  icon: string;
  text: string;
  tone: string;
}

export interface TokenHistoryReport {
  items: TokenHistoryRow[];
  totalCount: number;
  summary: TokenHistorySummary;
  trafficDistribution: TrafficDistributionPoint[];
  peakHoursSummary: string;
  insights: OperationalInsight[];
}

export interface TokenHistoryFilter {
  dateFrom?: string;
  dateTo?: string;
  serviceId?: number;
  subServiceId?: number;
  counterId?: number;
  status?: string;
  page?: number;
  pageSize?: number;
}

export interface ServiceListQuery {
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface SubServiceListQuery {
  serviceId?: number;
  search?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ApiErrorInfo {
  status: number;
  message: string;
}

export interface StaffManagementSummary {
  totalStaff: number;
  activeStaff: number;
  onlineNow: number;
  adminUsers: number;
}

export interface AdminStaffItem {
  id: number;
  fullName: string;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
  activeCounterName: string | null;
  sessionStatus: string | null;
  servedToday: number;
}

export interface CreateStaffRequest {
  fullName: string;
  username: string;
  email: string;
  password: string;
  role: string;
  isActive: boolean;
}

export interface UpdateStaffRequest {
  fullName: string;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
}

export interface LanguageManagementSummary {
  totalLanguages: number;
  activeLanguages: number;
  defaultLanguageCode: string | null;
}

export interface AdminLanguageItem {
  id: number;
  code: string;
  name: string;
  nativeName: string;
  isDefault: boolean;
  isActive: boolean;
  displayOrder: number;
}

export interface UpsertLanguageRequest {
  code: string;
  name: string;
  nativeName: string;
  isDefault: boolean;
  isActive: boolean;
  displayOrder: number;
}

export interface AdminSystemSettingItem {
  id: number;
  settingKey: string;
  settingValue: string;
  dataType: string;
  description: string;
  isActive: boolean;
}

export interface UpdateAdminSettingRequest {
  settingValue: string;
  description?: string;
  isActive?: boolean;
}

export interface AdminDisplayMessageItem {
  id: number;
  languageId: number | null;
  languageCode: string | null;
  messageKey: string;
  messageText: string;
  isActive: boolean;
  displayOrder: number;
}

export interface UpsertDisplayMessageRequest {
  languageId: number | null;
  messageKey: string;
  messageText: string;
  isActive: boolean;
  displayOrder: number;
}

export interface AdminVoiceTemplateItem {
  id: number;
  languageId: number;
  languageCode: string;
  eventType: string;
  templateText: string;
  isActive: boolean;
}

export interface UpsertVoiceTemplateRequest {
  languageId: number;
  eventType: string;
  templateText: string;
  isActive: boolean;
}

export interface PublicSettingItem {
  key: string;
  value: string;
}

export interface PublicDisplayMessageItem {
  messageKey: string;
  messageText: string;
}
