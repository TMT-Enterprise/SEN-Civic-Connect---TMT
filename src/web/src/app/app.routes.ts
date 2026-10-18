import { guestGuard, roleChildGuard, roleGuard } from './core/guards/role.guard';
import { Routes } from '@angular/router';
import { StatusComponent } from './components/status/status';
import { Login } from './components/login/login';
import { Register } from './components/register/register';
import { RequesterLayout } from './layout/requester-layout/requester-layout';
import { RequesterHome } from './features/requester/requester-home/requester-home';
import { NewRequest } from './features/requester/new-request/new-request';
import { RequestHistory } from './features/requester/request-history/request-history';
import { StaffLayout } from './layout/staff-layout/staff-layout';
import { DashboardPage } from './features/staff/dashboard/dashboard-page/dashboard-page';
import { HistoryPage } from './features/staff/dashboard/history-page/history-page';
import { AdminLayout } from './layout/admin-layout/admin-layout';
import { RequestsOverview } from './features/admin/requests-overview/requests-overview';
import { Analytics } from './features/admin/analytics/analytics';
import { StaffDirectory } from './features/admin/staff-directory/staff-directory';
import { RequestsHistory } from './features/admin/requests-history/requests-history';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  },
  {
    path: 'status',
    component: StatusComponent,
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    component: Login,
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    component: Register
  },
  {
    path: 'requester',
    canActivate: [roleGuard(['Requester'])],
    canActivateChild: [roleChildGuard(['Requester'])],
    component: RequesterLayout,
    children: [
      {
        path: '',
        component: RequesterHome
      },
      {
        path: 'new-request',
        component: NewRequest
      },
      {
        path: 'history',
        component: RequestHistory
      }
    ]
  },
  {
    path: 'staff',
    canActivate: [roleGuard(['Staff'])],
    canActivateChild: [roleChildGuard(['Staff'])],
    component: StaffLayout,
    children: [
      {
        path: '',
        component: DashboardPage
      },
      {
        path: 'history',
        component: HistoryPage
      }
    ]
  },
  {
    path: 'admin',
    canActivate: [roleGuard(['Admin'])],
    canActivateChild: [roleChildGuard(['Admin'])],
    component: AdminLayout,
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'requests'
      },
      {
        path: 'requests',
        component: RequestsOverview
      },
      {
        path: 'history',
        component: RequestsHistory
      },
      {
        path: 'analytics',
        component: Analytics
      },
      {
        path: 'staff',
        component: StaffDirectory
      }
    ]
  },

  {
    path: '**',
    redirectTo: 'login'
  }
];

