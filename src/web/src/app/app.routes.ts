import { Routes } from '@angular/router';
import { StatusComponent } from './status/status';

export const routes: Routes = [
  { path: 'status', component: StatusComponent },
  { path: '', redirectTo: 'status', pathMatch: 'full' },
];
