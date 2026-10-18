import { Component, OnInit, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RequestService } from '../../../core/services/request.service';
import { RequestSearchItem, StaffMember } from '../../../core/models/request.model';
@Component({ selector: 'app-requests-overview', standalone: true, imports: [DatePipe, FormsModule], templateUrl: './requests-overview.html', styleUrl: './requests-overview.css' })
export class RequestsOverview implements OnInit {
  private service = inject(RequestService); requests: RequestSearchItem[] = []; staff: StaffMember[] = []; loading = true; errorMessage = ''; search = ''; status = '';
  assignments: Record<number, { employeeId: string; dueAt: string }> = {};
  ngOnInit(): void { this.service.staffDirectory().subscribe({ next: s => this.staff = s, error: () => this.errorMessage = 'Unable to load staff directory.' }); this.load(); }
  load(): void { this.loading = true; const filters: Record<string, string> = {}; if (this.search) filters['search'] = this.search; if (this.status) filters['status'] = this.status; this.service.adminRequests(filters).subscribe({ next: r => { this.requests = r.requests; this.loading = false; }, error: e => { this.errorMessage = e?.error?.detail || 'Unable to load management queue.'; this.loading = false; } }); }
  get unassignedCount(): number { return this.requests.filter(r => !r.assignedEmployeeId && r.status !== 'Closed').length; }
  activeCount(status?: string): number { return this.requests.filter(r => !['Closed'].includes(r.status) && (!status || r.status === status)).length; }
  assign(item: RequestSearchItem): void { const value = this.assignments[item.requestId]; if (!value?.employeeId || !value.dueAt) { this.errorMessage = 'Choose a staff member and due date before assigning.'; return; } this.service.assign(item.requestId, Number(value.employeeId), new Date(value.dueAt).toISOString()).subscribe({ next: () => { this.errorMessage = ''; this.load(); }, error: e => this.errorMessage = e?.error?.error || 'Assignment failed.' }); }
  assignment(item: RequestSearchItem): { employeeId: string; dueAt: string } { return this.assignments[item.requestId] ?? (this.assignments[item.requestId] = { employeeId: '', dueAt: '' }); }
}
