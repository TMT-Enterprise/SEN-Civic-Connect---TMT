import { RequestService } from '../../../core/services/request.service';
import { mockRequestService } from '../../../../testing/mock-request-service';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestHistory } from './request-history';

describe('RequestHistory', () => {
  let component: RequestHistory;
  let fixture: ComponentFixture<RequestHistory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [{ provide: RequestService, useValue: mockRequestService },],
      imports: [RequestHistory],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestHistory);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
