import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RequestService } from '../../../core/services/request.service';
import { RequestSearchItem } from '../../../core/models/request.model';
@Component({ selector: 'app-requests-history', standalone: true, imports: [DatePipe], templateUrl: './requests-history.html', styleUrl: './requests-history.css' })
export class RequestsHistory implements OnInit {
  private service = inject(RequestService); requests: RequestSearchItem[] = []; loading = true; errorMessage = '';
  ngOnInit(): void { this.service.adminRequests().subscribe({ next: r => { this.requests = r.requests.filter(x => ['Resolved', 'Closed', 'Overdue'].includes(x.status)); this.loading = false; }, error: () => { this.errorMessage = 'Unable to load request history.'; this.loading = false; } }); }
}
