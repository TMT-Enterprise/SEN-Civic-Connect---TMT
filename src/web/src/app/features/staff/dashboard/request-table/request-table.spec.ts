import { RequestService } from '../../../../core/services/request.service';
import { mockRequestService } from '../../../../../testing/mock-request-service';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestTable } from './request-table';

describe('RequestTable', () => {
  let component: RequestTable;
  let fixture: ComponentFixture<RequestTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [{ provide: RequestService, useValue: mockRequestService },],
      imports: [RequestTable],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestTable);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
