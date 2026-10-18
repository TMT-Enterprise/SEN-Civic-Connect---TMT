export type RequestStatus = 'Submitted' | 'Assigned' | 'In Progress' | 'Resolved' | 'Closed' | 'Overdue';

export interface ServiceCategory { categoryId: number; type: string; }
export interface RequestItem {
  requestId: number; categoryId: number; category?: string; assignedEmployeeId?: number | null;
  assignee?: string | null; status: RequestStatus | string; description: string;
  submittedAt: string; dueAt?: string | null; updatedAt?: string; closedAt?: string | null;
}
export interface SubmitRequest { categoryId: number; description: string; }
export interface LoginResponse { accessToken: string; tokenType: string; expiresAt: string; role: 'Requester' | 'Staff' | 'Admin'; }
export interface UserProfile { id: string; accountType: string; email: string; role: string; }

export interface RequestSearchItem { requestId: number; categoryId: number; category: string; assignedEmployeeId: number | null; assignee: string | null; status: string; currentStatus: string; description: string; submittedAt: string; dueAt: string | null; updatedAt: string; closedAt: string | null; assignmentDecision: string | null; }
export interface StaffMember { employeeId: number; name: string; email: string; }
