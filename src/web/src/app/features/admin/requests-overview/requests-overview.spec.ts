import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestsOverview } from './requests-overview';

describe('RequestsOverview', () => {
  let component: RequestsOverview;
  let fixture: ComponentFixture<RequestsOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequestsOverview],
    }).compileComponents();

    fixture = TestBed.createComponent(RequestsOverview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
