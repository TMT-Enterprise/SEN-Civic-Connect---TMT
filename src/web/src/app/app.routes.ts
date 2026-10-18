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
    component: Login,
  },
  {
    path: 'register',
    component: Register
  },
  {
    path: 'requester',
      component: RequesterLayout,
      children: [
        {
          path: 'home',
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
        component: RequestHistory
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
    redirectTo: 'admin/requests'
  }
];

