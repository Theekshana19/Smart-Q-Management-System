import { Routes } from '@angular/router';
import { LanguageSelectionComponent } from './features/customer-kiosk/language-selection/language-selection.component';
import { ServiceSelectionComponent } from './features/customer-kiosk/service-selection/service-selection.component';
import { SubServiceSelectionComponent } from './features/customer-kiosk/sub-service-selection/sub-service-selection.component';
import { TokenSuccessComponent } from './features/customer-kiosk/token-success/token-success.component';
import { QueueBoardComponent } from './features/public-display/queue-board/queue-board.component';
import { StaffLayoutComponent } from './features/staff/staff-layout/staff-layout.component';
import { StaffDashboardComponent } from './features/staff/staff-dashboard/staff-dashboard.component';
import { QueueConsoleComponent } from './features/staff/queue-console/queue-console.component';
import { MyCounterComponent } from './features/staff/my-counter/my-counter.component';
import { TokenHistoryComponent } from './features/staff/token-history/token-history.component';
import { PerformanceComponent } from './features/staff/performance/performance.component';
import { AdminLayoutComponent } from './shared/layout/admin-layout/admin-layout.component';
import { DashboardComponent } from './features/admin/dashboard/dashboard.component';
import { ReportsComponent } from './features/admin/reports/reports.component';
import { CounterManagementComponent } from './features/admin/counter-management/counter-management.component';
import { ServiceManagementComponent } from './features/admin/service-management/service-management.component';

export const routes: Routes = [
  { path: '', redirectTo: 'customer/language', pathMatch: 'full' },
  { path: 'customer/language', component: LanguageSelectionComponent },
  { path: 'customer/services', component: ServiceSelectionComponent },
  { path: 'customer/services/:serviceId/sub-services', component: SubServiceSelectionComponent },
  { path: 'customer/token-success/:tokenId', component: TokenSuccessComponent },
  { path: 'display/queue-board', component: QueueBoardComponent },
  {
    path: 'staff',
    component: StaffLayoutComponent,
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
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'reports', component: ReportsComponent },
      { path: 'counters', component: CounterManagementComponent },
      { path: 'services', component: ServiceManagementComponent }
    ]
  }
];
