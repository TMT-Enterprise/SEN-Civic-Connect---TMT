import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequesterDashboard } from './requester-dashboard';

describe('RequesterDashboard', () => {
  let component: RequesterDashboard;
  let fixture: ComponentFixture<RequesterDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequesterDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(RequesterDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
