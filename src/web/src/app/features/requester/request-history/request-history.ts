import { Component, OnInit, inject } from '@angular/core';
import { RequestCard, ServiceRequest } from '../../../shared/request-card/request-card';
import { RequestService } from '../../../core/services/request.service';
import { RequestItem } from '../../../core/models/request.model';
@Component({ selector: 'app-request-history', standalone: true, imports: [RequestCard], templateUrl: './request-history.html', styleUrl: './request-history.css' })
export class RequestHistory implements OnInit {
  private service = inject(RequestService); requests: ServiceRequest[] = []; loading = true; errorMessage = '';
  ngOnInit(): void { this.service.requesterRequests().subscribe({ next: items => { this.requests = items.map(r => this.map(r)); this.loading = false; }, error: () => { this.errorMessage = 'Could not load your request history.'; this.loading = false; } }); }
  private map(r: RequestItem): ServiceRequest { return { id: `REQ-${r.requestId}`, citizen: '', category: r.category || `Category ${r.categoryId}`, description: r.description, priority: 'Normal', status: r.status as ServiceRequest['status'], dueDate: r.dueAt || '' }; }
}
