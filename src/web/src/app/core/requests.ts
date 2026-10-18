
import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface RequestHistoryItem {
  requestId: number;
  requesterId: number;
  categoryId: number;
  category: string;
  status: string;
  description: string;
  submittedAt: string;
  dueAt: string | null;
  updatedAt: string;
  closedAt: string | null;
}

export interface WithdrawRequestResponse {
  requestId: number;
  status: string;
}

export interface SubmitRequestPayload {
  categoryId: number;
  description: string;
}

export interface SubmitRequestResponse {
  requestId: number;
  status: string;
  submittedAt: string;
}

export interface AdminRequestItem {
  requestId: number;
  categoryId: number;
  category: string;
  assignedEmployeeId: number | null;
  assignee: string | null;
  status: string;
  description: string;
  submittedAt: string;
  dueAt: string | null;
  updatedAt: string;
  closedAt: string | null;
}

export interface AdminRequestSearchResponse {
  requests: AdminRequestItem[];
  message: string | null;
}

export interface AssignableEmployee {
  employeeId: number;
  name: string;
}

export interface AssignRequestPayload {
  employeeId: number;
  dueAt: string;
}

@Injectable({
  providedIn: 'root',
})
export class RequestsService {
  private readonly http = inject(HttpClient);

  getMyRequests(): Observable<RequestHistoryItem[]> {
    return this.http.get<RequestHistoryItem[]>('/requests');
  }

  submitRequest(
    payload: SubmitRequestPayload,
  ): Observable<SubmitRequestResponse> {
    return this.http.post<SubmitRequestResponse>(
      '/requests',
      payload,
    );
  }

  withdrawRequest(requestId: number): Observable<void> {
    return this.http.delete<void>(`/requests/${requestId}`);
  }

  getAdminRequests(): Observable<AdminRequestSearchResponse> {
    return this.http.get<AdminRequestSearchResponse>('/admin/requests');
  }

  getAssignableEmployees(): Observable<AssignableEmployee[]> {
    return this.http.get<AssignableEmployee[]>('/admin/assignable-employees');
  }

  assignRequest(
    requestId: number,
    payload: AssignRequestPayload,
  ): Observable<unknown> {
    return this.http.post(`/requests/${requestId}/assign`, payload);
  }
}
