export interface AuthUser {
  id: number;
  fullName: string;
  username: string;
  role: 'ADMIN' | 'STAFF';
}

export interface ActiveCounterSession {
  sessionId: number;
  counterId: number;
  counterNo: string;
  counterName: string;
  status: string;
  startedAt: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  user: AuthUser;
  requiresCounterSelection: boolean;
}

export interface MeResponse {
  id: number;
  fullName: string;
  username: string;
  role: 'ADMIN' | 'STAFF';
  activeCounterSession: ActiveCounterSession | null;
}

export interface AvailableCounter {
  counterId: number;
  counterNo: string;
  counterName: string;
  status: string;
  isAvailableForLogin: boolean;
  assignedServices: {
    serviceId: number;
    serviceName: string;
    serviceCode: string;
    tokenPrefixes: string[];
  }[];
  activeStaffName: string | null;
}

export interface StaffCounterSessionResult {
  sessionId: number;
  counterId: number;
  counterNo: string;
  counterName: string;
  status: string;
  startedAt: string;
  assignedServices: AvailableCounter['assignedServices'];
}
