import { Component, OnInit, inject } from '@angular/core';
import { RequestService } from '../../../core/services/request.service';
import { StaffMember } from '../../../core/models/request.model';
@Component({ selector: 'app-staff-directory', standalone: true, templateUrl: './staff-directory.html', styleUrl: './staff-directory.css' })
export class StaffDirectory implements OnInit {
  private service = inject(RequestService); staff: StaffMember[] = []; loading = true; errorMessage = '';
  ngOnInit(): void { this.service.staffDirectory().subscribe({ next: people => { this.staff = people; this.loading = false; }, error: () => { this.errorMessage = 'Unable to load staff directory.'; this.loading = false; } }); }
  initials(name: string): string { return name.split(/\s+/).filter(Boolean).slice(0, 2).map(part => part[0]).join('').toUpperCase(); }
}
