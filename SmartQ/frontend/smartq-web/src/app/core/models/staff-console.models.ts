export interface StaffCounterContext {
  id: number;
  counterNo: string;
  counterName: string;
  status: string;
  branchName: string;
}

export interface StaffUserContext {
  id: number;
  fullName: string;
  role: string;
}

export interface StaffAssignedService {
  serviceId: number;
  code: string;
  name: string;
  icon?: string;
  tokenPrefixes: string[];
}

export interface StaffConsoleContext {
  counter: StaffCounterContext;
  staff: StaffUserContext | null;
  assignedServices: StaffAssignedService[];
  systemOnline: boolean;
  callNextLockWhenActiveToken: boolean;
  displayMessages: Record<string, string>;
  currentTime: string;
}

export interface StaffConsoleSummary {
  waiting: number;
  servedToday: number;
  completedToday: number;
  skippedToday: number;
  avgWaitMinutes: number;
  avgServiceTime: string;
  currentStatus: string;
  queuePressure: string;
}

export interface StaffActiveSession {
  tokenId: number;
  tokenNo: string;
  status: string;
  serviceName: string;
  subServiceName: string;
  priority: string;
  calledAt?: string;
  startedAt?: string;
  elapsedSeconds: number;
  estimatedServiceMinutes: number;
  displayedOnTv: boolean;
  voiceAnnouncementSent: boolean;
}

export interface StaffQueueItem {
  tokenId: number;
  tokenNo: string;
  serviceName: string;
  subServiceName: string;
  waitMinutes: number;
  priority: string;
  status: string;
  createdAt: string;
  queuePosition: number;
}

export interface CallNextActionResult {
  hasToken: boolean;
  message: string;
  token: StaffActiveSession | null;
}

export interface TokenActionResult {
  success: boolean;
  message: string;
  token: StaffActiveSession | null;
}

export interface StaffTransferTokenRequest {
  targetServiceId: number;
  targetSubServiceId: number;
  targetCounterId?: number | null;
  reason?: string | null;
}

export interface StaffTokenHistoryItem {
  tokenId: number;
  tokenNo: string;
  serviceType: string;
  calledTime?: string;
  duration: string;
  status: string;
}

export interface HourlyServedPoint {
  hourLabel: string;
  servedCount: number;
}

export interface HourlyTrafficPoint {
  hourLabel: string;
  cashCount: number;
  accountCount: number;
  loanCount: number;
}

export interface StaffTimelineItem {
  eventType: string;
  tokenNo: string;
  title: string;
  description: string;
  metricLabel?: string | null;
  metricValue?: string | null;
  timestamp: string;
}

export interface StaffPerformance {
  servedToday: number;
  avgServiceTime: string;
  skipped: number;
  completionRate: number;
  hourlyServed: HourlyServedPoint[];
  recentTimeline: StaffTimelineItem[];
  optimizationTip: string;
  staffName: string;
  reportDateLabel: string;
  rangeLabel: string;
  servedLabel: string;
  dailyTarget: number;
  servedProgressPercent: number;
  servedTrendLabel: string;
  avgServiceTimeTrendLabel: string;
  avgServiceProgressPercent: number;
  avgServiceHint: string;
  completionTrendLabel: string;
  completionProgressPercent: number;
  completionHint: string;
  hourlyTraffic: HourlyTrafficPoint[];
}

export interface StaffNotificationItem {
  type: string;
  title: string;
  description: string;
  createdAt: string;
  isNew: boolean;
}

export interface StaffNotificationResponse {
  newCount: number;
  items: StaffNotificationItem[];
}

export interface TokenJourneyItem {
  newStatus: string;
  changedAt: string;
  remarks?: string;
  title: string;
  subtitle: string;
}

export interface StaffTokenDetails {
  tokenId: number;
  tokenNo: string;
  status: string;
  serviceType: string;
  subService: string;
  preferredLanguage: string;
  priority: string;
  createdTime: string;
  waitingMinutes: number;
  queuePosition: number;
  customerName?: string | null;
  customerSubtitle?: string | null;
  journey: TokenJourneyItem[];
}

export interface StaffTransferOption {
  id: number;
  code: string;
  name: string;
}

export interface StaffTransferSubService {
  id: number;
  serviceId: number;
  code: string;
  name: string;
}

export interface StaffTransferCounter {
  id: number;
  counterNo: string;
  counterName: string;
  status: string;
}

export interface StaffTransferOptions {
  services: StaffTransferOption[];
  subServices: StaffTransferSubService[];
  counters: StaffTransferCounter[];
}

export interface StaffDashboardCounter {
  counterId: number;
  counterName: string;
  staffName?: string;
  status: string;
  currentToken?: string;
  loadPercent: number;
  loadLabel: string;
}

export interface StaffDashboardComposition {
  code: string;
  name: string;
  waitingCount: number;
}

export interface StaffSystemStatusItem {
  key: string;
  title: string;
  value: string;
  level: string;
}

export interface StaffDashboard {
  branchName: string;
  heroTitle: string;
  heroDescription: string;
  systemHealthPercent: number;
  avgWaitDisplay: string;
  tokensToday: number;
  staffEfficiencyPercent: number;
  activeCounters: StaffDashboardCounter[];
  queueComposition: StaffDashboardComposition[];
  systemStatuses: StaffSystemStatusItem[];
  notifications: StaffNotificationResponse;
  liveStreamTitle: string;
  liveStreamUrl: string;
  currentTime: string;
}

export interface StaffMyCounterActiveDetails {
  tokenIdLabel: string;
  customerLabel: string;
  waitTimeDisplay: string;
  waitMinutes: number;
}

export interface StaffMyCounterUpcomingToken {
  tokenId: number;
  tokenNo: string;
  tokenPrefixBadge: string;
  subServiceName: string;
  waitMinutes: number;
}

export interface StaffMyCounterEfficiency {
  efficiencyPercent: number;
  efficiencyTrend: string;
  breakTimeDisplay: string;
  successRateDisplay: string;
  shiftEndsInDisplay: string;
}

export interface StaffMyCounter {
  context: StaffConsoleContext;
  summary: StaffConsoleSummary;
  activeSession: StaffActiveSession | null;
  activeDetails: StaffMyCounterActiveDetails | null;
  upcomingTokens: StaffMyCounterUpcomingToken[];
  performance: StaffPerformance;
  efficiency: StaffMyCounterEfficiency;
  queuePressurePercent: number;
  queuePressureLabel: string;
}

export interface StaffCounterStatusRequest {
  status: string;
}

export interface StaffCounterStatusResult {
  success: boolean;
  message: string;
  status: string;
}
