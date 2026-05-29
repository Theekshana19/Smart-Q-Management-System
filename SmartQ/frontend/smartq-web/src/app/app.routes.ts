import { Routes } from '@angular/router';
import { LanguageSelectionComponent } from './features/customer-kiosk/language-selection/language-selection.component';
import { ServiceSelectionComponent } from './features/customer-kiosk/service-selection/service-selection.component';
import { SubServiceSelectionComponent } from './features/customer-kiosk/sub-service-selection/sub-service-selection.component';
import { TokenSuccessComponent } from './features/customer-kiosk/token-success/token-success.component';
import { QueueBoardComponent } from './features/public-display/queue-board/queue-board.component';
import { ServiceConsoleComponent } from './features/staff/service-console/service-console.component';
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
  { path: 'staff/console', component: ServiceConsoleComponent },
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
