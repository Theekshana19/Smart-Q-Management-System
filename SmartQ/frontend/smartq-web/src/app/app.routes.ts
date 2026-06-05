import { Routes } from '@angular/router';
import { LanguageSelectionComponent } from './features/customer-kiosk/language-selection/language-selection.component';
import { ServiceSelectionComponent } from './features/customer-kiosk/service-selection/service-selection.component';
import { SubServiceSelectionComponent } from './features/customer-kiosk/sub-service-selection/sub-service-selection.component';
import { TokenSuccessComponent } from './features/customer-kiosk/token-success/token-success.component';
import { QueueBoardComponent } from './features/public-display/queue-board/queue-board.component';
import { AdminLoginComponent } from './features/auth/admin-login/admin-login.component';
import { StaffLoginComponent } from './features/auth/staff-login/staff-login.component';
import { StaffLayoutComponent } from './features/staff/staff-layout/staff-layout.component';
import { StaffDashboardComponent } from './features/staff/staff-dashboard/staff-dashboard.component';
import { QueueConsoleComponent } from './features/staff/queue-console/queue-console.component';
import { MyCounterComponent } from './features/staff/my-counter/my-counter.component';
import { TokenHistoryComponent } from './features/staff/token-history/token-history.component';
import { PerformanceComponent } from './features/staff/performance/performance.component';
import { SelectCounterComponent } from './features/staff/select-counter/select-counter.component';
import { AdminLayoutComponent } from './shared/layout/admin-layout/admin-layout.component';
import { DashboardComponent } from './features/admin/dashboard/dashboard.component';
import { ReportsComponent } from './features/admin/reports/reports.component';
import { CounterManagementComponent } from './features/admin/counter-management/counter-management.component';
import { ServiceManagementComponent } from './features/admin/service-management/service-management.component';
import { SubServicesComponent } from './features/admin/sub-services/sub-services.component';
import { CounterAssignmentsComponent } from './features/admin/counter-assignments/counter-assignments.component';
import { StaffManagementComponent } from './features/admin/staff-management/staff-management.component';
import { LanguagesComponent } from './features/admin/languages/languages.component';
import { SystemSettingsComponent } from './features/admin/system-settings/system-settings.component';
import { DisplayMessagesComponent } from './features/admin/display-messages/display-messages.component';
import { VoiceTemplatesComponent } from './features/admin/voice-templates/voice-templates.component';
import { roleGuard } from './core/auth/role.guard';
import { staffCounterGuard } from './core/auth/staff-counter.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'customer/language', pathMatch: 'full' },
  { path: 'customer/language', component: LanguageSelectionComponent },
  { path: 'customer/services', component: ServiceSelectionComponent },
  { path: 'customer/services/:serviceId/sub-services', component: SubServiceSelectionComponent },
  { path: 'customer/token-success/:tokenId', component: TokenSuccessComponent },
  { path: 'display/queue-board', component: QueueBoardComponent },
  { path: 'admin/login', component: AdminLoginComponent },
  { path: 'staff/login', component: StaffLoginComponent },
  {
    path: 'staff/select-counter',
    component: SelectCounterComponent,
    canActivate: [roleGuard('STAFF')]
  },
  {
    path: 'staff',
    component: StaffLayoutComponent,
    canActivate: [roleGuard('STAFF'), staffCounterGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: StaffDashboardComponent },
      { path: 'queue-console', component: QueueConsoleComponent },
      { path: 'my-counter', component: MyCounterComponent },
      { path: 'token-history', component: TokenHistoryComponent },
      { path: 'performance', component: PerformanceComponent }
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [roleGuard('ADMIN')],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'reports', component: ReportsComponent },
      { path: 'counters', component: CounterManagementComponent },
      { path: 'counter-assignments', component: CounterAssignmentsComponent },
      { path: 'services', component: ServiceManagementComponent },
      { path: 'sub-services', component: SubServicesComponent },
      { path: 'staff', component: StaffManagementComponent },
      { path: 'languages', component: LanguagesComponent },
      { path: 'settings', component: SystemSettingsComponent },
      { path: 'display-messages', component: DisplayMessagesComponent },
      { path: 'voice-templates', component: VoiceTemplatesComponent }
    ]
  }
];
