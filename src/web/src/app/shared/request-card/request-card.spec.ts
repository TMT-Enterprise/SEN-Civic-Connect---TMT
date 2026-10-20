import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RequestCard } from './request-card';
import { ServiceRequest } from './request-card';

describe('RequestCard', () => {
  let component: RequestCard;
  let fixture: ComponentFixture<RequestCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequestCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RequestCard);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    // Provide a valid mock request object to satisfy the required input
    component.request = {
      id: 'req-1',
      citizen: 'John Doe',
      category: 'Infrastructure',
      description: 'Broken streetlight on Main Road',
      priority: 'Medium',
      status: 'Assigned',
      dueDate: '2026-10-25'
    } as ServiceRequest;

    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('offers withdrawal only when enabled for a submitted request', () => {
    component.request = {
      id: 'REQ-001',
      requestId: 1,
      citizen: 'Test User',
      category: 'IT support',
      description: 'Cannot connect to the network.',
      priority: null,
      status: 'Submitted',
      dueDate: 'Not scheduled',
    };
    component.allowWithdraw = true;
    let didEmitWithdrawal = false;
    component.withdraw.subscribe(() => {
      didEmitWithdrawal = true;
    });
    fixture.detectChanges();

    const withdrawButton = fixture.nativeElement.querySelector(
      'button.withdraw-request',
    );
    expect(withdrawButton).not.toBeNull();
    withdrawButton.click();
    expect(didEmitWithdrawal).toBe(true);

    component.request = { ...component.request, status: 'Assigned' };
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('button.withdraw-request'),
    ).toBeNull();
  });
});