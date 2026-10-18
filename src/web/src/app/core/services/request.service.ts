import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RequestItem, RequestSearchItem, ServiceCategory, StaffMember, SubmitRequest } from '../models/request.model';
const API = '/api/v1';
@Injectable({ providedIn: 'root' })
export class RequestService {
  private http = inject(HttpClient);
  categories(): Observable<ServiceCategory[]> { return this.http.get<ServiceCategory[]>(`${API}/categories`); }
  submit(payload: SubmitRequest): Observable<{ requestId: number; status: string; submittedAt: string }> { return this.http.post<any>(`${API}/requests`, payload); }
  requesterRequests(): Observable<RequestItem[]> { return this.http.get<RequestItem[]>(`${API}/requests`); }
  request(id: number): Observable<RequestItem> { return this.http.get<RequestItem>(`${API}/requests/${id}`); }
  staffRequests(filters: Record<string, string | number | boolean> = {}): Observable<{ requests: RequestSearchItem[]; message: string | null }> { return this.http.get<{ requests: RequestSearchItem[]; message: string | null }>(`${API}/staff/requests`, { params: this.params(filters) }); }
  adminRequests(filters: Record<string, string | number | boolean> = {}): Observable<{ requests: RequestSearchItem[]; message: string | null }> { return this.http.get<{ requests: RequestSearchItem[]; message: string | null }>(`${API}/admin/requests`, { params: this.params(filters) }); }
  dashboard(): Observable<{ totalRequests: number; activeRequests: number; completedRequests: number; overdueRequests: number; activeStaff: number; completionRate: number; byStatus: { status: string; count: number }[]; byCategory: { category: string; count: number }[] }> { return this.http.get<any>(`${API}/management/dashboard`); }
  staffDirectory(): Observable<StaffMember[]> { return this.http.get<StaffMember[]>(`${API}/admin/staff`); }
  assign(id: number, employeeId: number, dueAt: string): Observable<unknown> { return this.http.post(`${API}/requests/${id}/assign`, { employeeId, dueAt }); }
  decide(id: number, decision: string, comment?: string): Observable<unknown> { return this.http.post(`${API}/requests/${id}/decide`, { decision, rejectionComment: comment }); }
  changeStatus(id: number, status: string, completionDescription?: string): Observable<unknown> { return this.http.patch(`${API}/requests/${id}/status`, { status, completionNote: completionDescription }); }
  private params(filters: Record<string, string | number | boolean>): HttpParams { let p = new HttpParams(); Object.entries(filters).forEach(([k,v]) => { if (v !== '' && v !== null && v !== undefined) p = p.set(k, String(v)); }); return p; }
}
