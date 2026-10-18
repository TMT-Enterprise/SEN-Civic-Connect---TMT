import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RequestService } from '../../../../core/services/request.service';
import { RequestSearchItem } from '../../../../core/models/request.model';
@Component({ selector: 'app-history-page', standalone: true, imports: [DatePipe], templateUrl: './history-page.html', styleUrl: './history-page.css' })
export class HistoryPage implements OnInit {
  private service = inject(RequestService); requests: RequestSearchItem[] = []; loading = true; errorMessage = '';
  ngOnInit(): void { this.service.staffRequests().subscribe({ next: r => { this.requests = r.requests.filter(x => ['Resolved', 'Closed', 'Overdue'].includes(x.status)); this.loading = false; }, error: () => { this.errorMessage = 'Unable to load assigned request history.'; this.loading = false; } }); }
}
