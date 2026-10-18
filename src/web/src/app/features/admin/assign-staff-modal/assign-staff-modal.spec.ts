import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssignStaffModal } from './assign-staff-modal';

describe('AssignStaffModal', () => {
  let component: AssignStaffModal;
  let fixture: ComponentFixture<AssignStaffModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssignStaffModal],
    }).compileComponents();

    fixture = TestBed.createComponent(AssignStaffModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
