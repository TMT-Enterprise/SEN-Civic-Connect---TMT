import { Routes } from '@angular/router';
import { StatusComponent } from './components/status/status';
import { Login } from './components/login/login';
import { Layout } from './components/layout/layout';

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
    path: '',
    component: Layout,
    children: [],
  },
];
