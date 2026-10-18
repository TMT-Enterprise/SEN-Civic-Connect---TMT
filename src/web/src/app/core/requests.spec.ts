import {
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { authInterceptor } from './auth';
import {
  RequestHistoryItem,
  RequestsService,
  AdminRequestSearchResponse,
  AssignableEmployee,
  AssignRequestPayload,
  SubmitRequestPayload,
  SubmitRequestResponse,
} from './requests';

describe('RequestsService', () => {
  let service: RequestsService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    localStorage.setItem('civic-connect.access-token', 'test-access-token');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(RequestsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.removeItem('civic-connect.access-token');
  });

  it('posts only the supported fields with the bearer token and accepts 201 Created', () => {
    const payload: SubmitRequestPayload = {
      categoryId: 4,
      description: 'Computer cannot connect to the network.',
    };
    const response: SubmitRequestResponse = {
      requestId: 42,
      status: 'Submitted',
      submittedAt: '2026-10-18T12:00:00Z',
    };
    let received: SubmitRequestResponse | undefined;

    service.submitRequest(payload).subscribe((result) => {
      received = result;
    });

    const request = httpTesting.expectOne('/requests');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    expect(request.request.headers.get('Authorization')).toBe(
      'Bearer test-access-token',
    );
    request.flush(response, { status: 201, statusText: 'Created' });

    expect(received).toEqual(response);
  });

  it('loads the requester history returned by GET /requests', () => {
    const history: RequestHistoryItem[] = [
      {
        requestId: 42,
        requesterId: 7,
        categoryId: 4,
        category: 'IT support',
        status: 'Submitted',
        description: 'Computer cannot connect to the network.',
        submittedAt: '2026-10-18T12:00:00Z',
        dueAt: null,
        updatedAt: '2026-10-18T12:00:00Z',
        closedAt: null,
      },
    ];
    let received: RequestHistoryItem[] | undefined;

    service.getMyRequests().subscribe((result) => {
      received = result;
    });

    const request = httpTesting.expectOne('/requests');
    expect(request.request.method).toBe('GET');
    expect(request.request.headers.get('Authorization')).toBe(
      'Bearer test-access-token',
    );
    request.flush(history);

    expect(received).toEqual(history);
  });

  it('loads the admin queue and available staff through admin endpoints', () => {
    const queue: AdminRequestSearchResponse = { requests: [], message: null };
    const employees: AssignableEmployee[] = [{ employeeId: 12, name: 'Staff User' }];
    let receivedQueue: AdminRequestSearchResponse | undefined;
    let receivedEmployees: AssignableEmployee[] | undefined;

    service.getAdminRequests().subscribe((result) => {
      receivedQueue = result;
    });
    service.getAssignableEmployees().subscribe((result) => {
      receivedEmployees = result;
    });

    const queueRequest = httpTesting.expectOne('/admin/requests');
    expect(queueRequest.request.method).toBe('GET');
    expect(queueRequest.request.headers.get('Authorization')).toBe(
      'Bearer test-access-token',
    );
    queueRequest.flush(queue);

    const employeesRequest = httpTesting.expectOne('/admin/assignable-employees');
    expect(employeesRequest.request.method).toBe('GET');
    expect(employeesRequest.request.headers.get('Authorization')).toBe(
      'Bearer test-access-token',
    );
    employeesRequest.flush(employees);

    expect(receivedQueue).toEqual(queue);
    expect(receivedEmployees).toEqual(employees);
  });

  it('posts assignment details to the request assignment endpoint', () => {
    const payload: AssignRequestPayload = {
      employeeId: 12,
      dueAt: '2026-10-20T12:00:00.000Z',
    };
    let completed = false;

    service.assignRequest(13, payload).subscribe(() => {
      completed = true;
    });

    const request = httpTesting.expectOne('/requests/13/assign');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    expect(request.request.headers.get('Authorization')).toBe(
      'Bearer test-access-token',
    );
    request.flush({ requestId: 13 });

    expect(completed).toBe(true);
  });

  it('withdraws a request with DELETE and the bearer token', () => {
    let completed = false;

    service.withdrawRequest(42).subscribe(() => {
      completed = true;
    });

    const request = httpTesting.expectOne('/requests/42');
    expect(request.request.method).toBe('DELETE');
    expect(request.request.headers.has('Authorization')).toBe(true);
    request.flush(null, { status: 204, statusText: 'No Content' });

    expect(completed).toBe(true);
  });
});
