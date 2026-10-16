import { Routes } from '@angular/router';
import { StatusComponent } from './components/status/status';
import { Login } from './components/login/login';
import { Layout } from './components/layout/layout';
import { Register } from './components/register/register';
import { RequesterDashboard } from './components/requester-dashboard/requester-dashboard';

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
    path: 'layout',
    component: Layout,
    children: [
      { path: '', redirectTo: 'requester', pathMatch: 'full' },
      { path: 'requester', component: RequesterDashboard }
    ],
  },
];
