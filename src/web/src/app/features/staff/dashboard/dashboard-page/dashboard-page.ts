import { Component, OnInit, inject } from '@angular/core';
import { MetricCard } from '../metric-card/metric-card';
import { RequestTable } from '../request-table/request-table';
import { RequestService } from '../../../../core/services/request.service';
import { RequestSearchItem } from '../../../../core/models/request.model';
@Component({ selector: 'app-dashboard-page', standalone: true, imports: [MetricCard, RequestTable], templateUrl: './dashboard-page.html', styleUrl: './dashboard-page.css' })
export class DashboardPage implements OnInit {
  private service = inject(RequestService); requests: RequestSearchItem[] = [];
  ngOnInit(): void { this.service.staffRequests().subscribe({ next: r => this.requests = r.requests, error: () => this.requests = [] }); }
  count(status?: string): number { return this.requests.filter(r => !status ? r.status !== 'Closed' : r.status === status).length; }
}
