import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { Subject } from 'rxjs';

import { AuthService } from '../../../core/auth';
import {
  RequestHistoryItem,
  RequestsService,
} from '../../../core/requests';
import { RequesterHome } from './requester-home';

describe('RequesterHome', () => {
  let component: RequesterHome;
  let fixture: ComponentFixture<RequesterHome>;
  let historyResponse: Subject<RequestHistoryItem[]>;

  beforeEach(async () => {
    historyResponse = new Subject<RequestHistoryItem[]>();

    await TestBed.configureTestingModule({
      imports: [RequesterHome],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { userName: 'Test User' } },
        {
          provide: RequestsService,
          useValue: { getMyRequests: () => historyResponse.asObservable() },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RequesterHome);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows loading while the history request is pending', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Loading your requests...');
    expect(getSubmitRequestLink()).not.toBeNull();
  });

  it('displays the request and category when requester history arrives', async () => {
    fixture.detectChanges();
    historyResponse.next([
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
    ]);
    await fixture.whenStable();

    expect(component.isLoading()).toBe(false);
    expect(fixture.nativeElement.textContent).toContain('IT support');
    expect(fixture.nativeElement.textContent).toContain('Computer cannot connect');
    expect(getSubmitRequestLink()).not.toBeNull();
  });

  it('shows the empty state after an empty history response arrives', async () => {
    fixture.detectChanges();
    historyResponse.next([]);
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('No requests yet');
  });

  it('keeps the submit request action available when history fails', async () => {
    fixture.detectChanges();
    historyResponse.error(new HttpErrorResponse({ status: 401 }));
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain(
      'Your session may have expired.',
    );
    expect(getSubmitRequestLink()).not.toBeNull();
  });

  function getSubmitRequestLink(): HTMLAnchorElement | null {
    return fixture.nativeElement.querySelector(
      'a[routerLink="/requester/new-request"]',
    );
  }
});
