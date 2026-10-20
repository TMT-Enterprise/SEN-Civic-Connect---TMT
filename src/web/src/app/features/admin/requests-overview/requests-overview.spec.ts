import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import {
  AdminRequestItem,
  RequestsService,
} from '../../../core/requests';
import { RequestsOverview } from './requests-overview';

describe('RequestsOverview', () => {
  let component: RequestsOverview;
  let fixture: ComponentFixture<RequestsOverview>;
  let assignRequest: ReturnType<typeof vi.fn>;
  let getAssignableEmployees: ReturnType<typeof vi.fn>;

  const queueItem: AdminRequestItem = {
    requestId: 13,
    categoryId: 4,
    category: 'IT support',
    assignedEmployeeId: null,
    assignee: null,
    status: 'Submitted',
    description: 'A test request',
    submittedAt: '2026-10-18T12:00:00Z',
    dueAt: null,
    updatedAt: '2026-10-18T12:00:00Z',
    closedAt: null,
  };

  beforeEach(async () => {
    assignRequest = vi.fn(() => of({ requestId: 13 }));
    getAssignableEmployees = vi.fn(() =>
      of([{ employeeId: 27, name: 'Staff User' }]),
    );

    await TestBed.configureTestingModule({
      imports: [RequestsOverview],
      providers: [
        {
          provide: RequestsService,
          useValue: {
            getAdminRequests: () =>
              of({ requests: [queueItem], message: null }),
            getAssignableEmployees,
            assignRequest,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestsOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads actual admin requests and provides an assignment action for submitted requests', () => {
    expect(component.requests()).toEqual([queueItem]);
    expect(fixture.nativeElement.textContent).toContain('REQ-013');
    expect(fixture.nativeElement.textContent).toContain('A test request');
    expect(fixture.nativeElement.textContent).toContain('Assign Staff');
  });

  it('loads staff and posts an assignment with the selected due date', () => {
    component.openAssignment(queueItem);
    expect(getAssignableEmployees).toHaveBeenCalled();
    expect(component.employees()).toEqual([
      { employeeId: 27, name: 'Staff User' },
    ]);

    component.selectedEmployeeId.set(27);
    component.dueAt.set('2026-10-21T12:00');
    component.submitAssignment();

    expect(assignRequest).toHaveBeenCalledWith(13, {
      employeeId: 27,
      dueAt: new Date('2026-10-21T12:00').toISOString(),
    });
    expect(component.assignmentRequest()).toBeNull();
    expect(component.successMessage()).toContain('REQ-013');
  });

  it('does not send a request without an employee or a valid due date', () => {
    component.openAssignment(queueItem);
    component.dueAt.set('2026-10-19T00:00');

    component.submitAssignment();

    expect(assignRequest).not.toHaveBeenCalled();
    expect(component.assignmentError()).toBeTruthy();
  });
});
