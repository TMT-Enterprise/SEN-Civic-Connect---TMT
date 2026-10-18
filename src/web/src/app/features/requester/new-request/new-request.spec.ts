import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Subject } from 'rxjs';

import {
  RequestsService,
  SubmitRequestPayload,
  SubmitRequestResponse,
} from '../../../core/requests';
import { NewRequest } from './new-request';

describe('NewRequest', () => {
  let component: NewRequest;
  let fixture: ComponentFixture<NewRequest>;
  let submittedPayload: SubmitRequestPayload | undefined;
  let submitSubject: Subject<SubmitRequestResponse>;

  const submitResponse: SubmitRequestResponse = {
    requestId: 42,
    status: 'Submitted',
    submittedAt: '2026-10-18T12:00:00Z',
  };

  beforeEach(async () => {
    submittedPayload = undefined;
    submitSubject = new Subject<SubmitRequestResponse>();

    await TestBed.configureTestingModule({
      imports: [NewRequest],
      providers: [
        provideRouter([]),
        {
          provide: RequestsService,
          useValue: {
            submitRequest: (payload: SubmitRequestPayload) => {
              submittedPayload = payload;
              return submitSubject.asObservable();
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NewRequest);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('submits the selected category and trimmed description, then renders the created request', async () => {
    component.categoryId = 4;
    component.description = '  Computer cannot connect to the network.  ';

    component.submitRequest();

    expect(submittedPayload).toEqual({
      categoryId: 4,
      description: 'Computer cannot connect to the network.',
    });
    expect(component.isSubmitting()).toBe(true);

    submitSubject.next(submitResponse);
    await fixture.whenStable();

    expect(component.createdRequestId()).toBe(42);
    expect(component.successMessage()).toContain('REQ-042');
    expect(fixture.nativeElement.textContent).toContain('Request submitted');
  });

  it('does not submit a missing category or description', () => {
    component.categoryId = null;
    component.description = '   ';

    component.submitRequest();

    expect(submittedPayload).toBeUndefined();
    expect(component.errorMessage()).toContain('select a category');
  });
});
