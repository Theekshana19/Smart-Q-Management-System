export interface Language {
  id: number;
  code: string;
  name: string;
  nativeName: string;
  isDefault: boolean;
}

export interface ServiceItem {
  id: number;
  code: string;
  name: string;
  description: string;
  icon: string;
  displayOrder: number;
}

export interface SubServiceItem {
  id: number;
  serviceId: number;
  code: string;
  name: string;
  description: string;
  tokenPrefix: string;
  icon: string;
  estimatedServiceMinutes: number;
  waitingCount: number;
  estimatedWaitMinutes: number;
}

export interface GenerateTokenRequest {
  languageId: number;
  serviceId: number;
  subServiceId: number;
}

export interface GenerateTokenResponse {
  id: number;
  tokenNo: string;
  serviceName: string;
  subServiceName: string;
  estimatedWaitMinutes: number;
  waitingBeforeYou: number;
  createdAt: string;
}

export interface TokenDetail {
  id: number;
  tokenNo: string;
  serviceName: string;
  subServiceName: string;
  status: string;
  priority: string;
  counterId?: number;
  counterName?: string;
  estimatedWaitMinutes: number;
  createdAt: string;
  calledAt?: string;
}

export interface KioskStatus {
  branchId: string;
  branchName: string;
  kioskVersion: string;
  systemOnline: boolean;
  activeStaffCount: number;
  averageWaitMinutes: number;
}

export interface DisplayBoard {
  nowServing: NowServing | null;
  recentlyCalled: RecentlyCalled[];
  waitingQueue: WaitingQueue;
  tickerMessages: string[];
}

export interface NowServing {
  tokenNo: string;
  counterName: string;
  counterNo: string;
  serviceName: string;
  calledAt?: string;
}

export interface RecentlyCalled {
  tokenNo: string;
  counterName: string;
  counterNo: string;
  serviceName: string;
  calledAt: string;
}

export interface WaitingQueue {
  expectedWaitMinutes: number;
  items: { tokenNo: string; serviceName: string; subServiceName: string; waitMinutes: number }[];
}

export interface CallNextResponse {
  tokenId: number;
  tokenNo: string;
  serviceName: string;
  subServiceName: string;
  counterName: string;
  counterNo: string;
}

export interface StaffConsoleSummary {
  waitingCount: number;
  servedToday: number;
  avgWaitMinutes: number;
  queue: CounterQueue;
}

export interface CounterQueue {
  counterId: number;
  counterName: string;
  counterStatus: string;
  assignedServiceNames: string[];
  activeToken: TokenDetail | null;
  nextToken: TokenDetail | null;
  waitingTokens: QueueToken[];
}

export interface CallNextResult {
  success: boolean;
  message: string;
  data: CallNextResponse | null;
}

export interface QueueToken {
  id: number;
  tokenNo: string;
  serviceName: string;
  subServiceName: string;
  status: string;
  priority: string;
  waitMinutes: number;
  createdAt: string;
}

export interface VoiceTemplate {
  eventType: string;
  templateText: string;
  languageCode: string;
}
