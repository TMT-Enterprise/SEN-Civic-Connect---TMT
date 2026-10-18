import { Component, input, output, signal, HostListener } from '@angular/core';
import { ServiceRequest } from '../../../shared/request-card/request-card';

@Component({
  selector: 'app-assign-staff-modal',
  standalone: true,
  imports: [],
  templateUrl: './assign-staff-modal.html',
  styleUrl: './assign-staff-modal.css',
})
export class AssignStaffModal {
  request = input<ServiceRequest | null>(null);
  isOpen = input(false);
  staff = input<string[]>([]);

  closed = output<void>();
  assigned = output<string>();

  selectedStaff = signal('');

  onSelect(event: Event): void {
    this.selectedStaff.set((event.target as HTMLSelectElement).value);
  }

  confirm(): void {
    const name = this.selectedStaff();
    if (!name) {
      return;
    }
    this.assigned.emit(name);
    this.selectedStaff.set('');
  }

  close(): void {
    this.selectedStaff.set('');
    this.closed.emit();
  }

    @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.isOpen()) {
      this.close();
    }
  }
}