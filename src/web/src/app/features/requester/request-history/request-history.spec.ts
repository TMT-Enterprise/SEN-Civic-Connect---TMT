import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AuthService } from '../../../core/auth';
import { RequestsService } from '../../../core/requests';
import { RequestHistory } from './request-history';

describe('RequestHistory', () => {
  let component: RequestHistory;
  let fixture: ComponentFixture<RequestHistory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequestHistory],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: { userName: 'Test User' } },
        {
          provide: RequestsService,
          useValue: {
            getMyRequests: () => of([]),
            withdrawRequest: () => of(undefined),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestHistory);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
