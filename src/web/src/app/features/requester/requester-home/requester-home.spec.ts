import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequesterHome } from './requester-home';
import { provideRouter } from '@angular/router';

describe('RequesterHome', () => {
  let component: RequesterHome;
  let fixture: ComponentFixture<RequesterHome>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequesterHome],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(RequesterHome);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
