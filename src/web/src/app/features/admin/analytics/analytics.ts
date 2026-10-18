import { Component, OnInit, inject } from '@angular/core';
import { RequestService } from '../../../core/services/request.service';
@Component({ selector: 'app-analytics', standalone: true, templateUrl: './analytics.html', styleUrl: './analytics.css' })
export class Analytics implements OnInit {
  private service = inject(RequestService); data: any = null; loading = true; errorMessage = '';
  ngOnInit(): void { this.service.dashboard().subscribe({ next: d => { this.data = d; this.loading = false; }, error: () => { this.errorMessage = 'Unable to load analytics.'; this.loading = false; } }); }
}
