import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequesterLayout } from './requester-layout';
import { provideRouter } from '@angular/router';

describe('RequesterLayout', () => {
  let component: RequesterLayout;
  let fixture: ComponentFixture<RequesterLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequesterLayout],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(RequesterLayout);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
