import { Component } from '@angular/core';
import { MetricCard } from '../metric-card/metric-card';
import { RequestTable } from '../request-table/request-table';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    MetricCard,
    RequestTable
  ],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.css',
})
export class DashboardPage {}
