import { of } from 'rxjs';

export const mockRequestService = {
  categories: () => of([]),
  submit: () => of({ requestId: 1, status: 'Submitted', submittedAt: new Date().toISOString() }),
  requesterRequests: () => of([]),
  request: () => of(null),
  staffRequests: () => of({ requests: [], message: null }),
  adminRequests: () => of({ requests: [], message: null }),
  staffDirectory: () => of([]),
  dashboard: () => of({ totalRequests: 0, activeRequests: 0, completedRequests: 0, overdueRequests: 0, activeStaff: 0, completionRate: 0, byStatus: [], byCategory: [] }),
  assign: () => of({}),
  decide: () => of({}),
  changeStatus: () => of({}),
};
