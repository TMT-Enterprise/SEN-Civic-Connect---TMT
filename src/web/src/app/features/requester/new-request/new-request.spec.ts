import { provideRouter } from '@angular/router';
import { RequestService } from '../../../core/services/request.service';
import { mockRequestService } from '../../../../testing/mock-request-service';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewRequest } from './new-request';

describe('NewRequest', () => {
  let component: NewRequest;
  let fixture: ComponentFixture<NewRequest>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [{ provide: RequestService, useValue: mockRequestService }, provideRouter([])],
      imports: [NewRequest],
    }).compileComponents();

    fixture = TestBed.createComponent(NewRequest);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
