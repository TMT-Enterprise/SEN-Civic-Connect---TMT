import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RequestService } from '../../../../core/services/request.service';
import { RequestSearchItem } from '../../../../core/models/request.model';
@Component({ selector: 'app-request-table', standalone: true, imports: [DatePipe, FormsModule], templateUrl: './request-table.html', styleUrl: './request-table.css' })
export class RequestTable implements OnInit {
  private service = inject(RequestService); requests: RequestSearchItem[] = []; loading = true; errorMessage = ''; search = '';
  ngOnInit(): void { this.load(); }
  load(): void { this.loading = true; this.service.staffRequests(this.search ? { search: this.search } : {}).subscribe({ next: r => { this.requests = r.requests; this.loading = false; }, error: () => { this.errorMessage = 'Could not load your assigned requests.'; this.loading = false; } }); }
  updateStatus(item: RequestSearchItem, status: string): void {
    const completionNote = status === 'Closed' ? window.prompt('Enter the completion note required to close this request:') : undefined;
    if (status === 'Closed' && !completionNote?.trim()) return;
    this.service.changeStatus(item.requestId, status, completionNote || undefined).subscribe({ next: () => this.load(), error: e => this.errorMessage = e?.error?.error || 'Status update failed.' });
  }
  accept(item: RequestSearchItem): void { this.service.decide(item.requestId, 'Accepted').subscribe({ next: () => this.load(), error: e => this.errorMessage = e?.error?.error || 'Unable to accept assignment.' }); }
  reject(item: RequestSearchItem): void { const comment = window.prompt('Why are you rejecting this assignment?'); if (!comment?.trim()) return; this.service.decide(item.requestId, 'Rejected', comment).subscribe({ next: () => this.load(), error: e => this.errorMessage = e?.error?.error || 'Unable to reject assignment.' }); }
}
