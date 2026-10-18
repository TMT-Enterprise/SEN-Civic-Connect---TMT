import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { RequestCard, ServiceRequest } from '../../../shared/request-card/request-card';
import { RequestService } from '../../../core/services/request.service';
import { RequestItem } from '../../../core/models/request.model';
@Component({ selector: 'app-requester-home', standalone: true, imports: [RouterLink, RequestCard], templateUrl: './requester-home.html', styleUrl: './requester-home.css' })
export class RequesterHome implements OnInit {
  private service = inject(RequestService); requests: ServiceRequest[] = []; loading = true; errorMessage = '';
  ngOnInit(): void { this.service.requesterRequests().subscribe({ next: items => { this.requests = items.slice(0, 3).map(r => this.map(r)); this.loading = false; }, error: () => { this.errorMessage = 'Could not load your requests.'; this.loading = false; } }); }
  private map(r: RequestItem): ServiceRequest { return { id: `REQ-${r.requestId}`, citizen: '', category: r.category || `Category ${r.categoryId}`, description: r.description, priority: 'Normal', status: r.status as ServiceRequest['status'], dueDate: r.dueAt || '' }; }
}
