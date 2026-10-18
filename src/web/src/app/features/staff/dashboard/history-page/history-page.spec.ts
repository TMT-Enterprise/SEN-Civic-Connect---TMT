import { RequestService } from '../../../../core/services/request.service';
import { mockRequestService } from '../../../../../testing/mock-request-service';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HistoryPage } from './history-page';

describe('HistoryPage', () => {
  let component: HistoryPage;
  let fixture: ComponentFixture<HistoryPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [{ provide: RequestService, useValue: mockRequestService },],
      imports: [HistoryPage],
    }).compileComponents();

    fixture = TestBed.createComponent(HistoryPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
