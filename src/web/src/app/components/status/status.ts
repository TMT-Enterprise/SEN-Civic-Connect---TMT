import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import {
  HealthService,
  HealthResponse,
  ReadyResponse,
} from '../../core/health';

@Component({
  selector: 'app-status',
  standalone: true,
  templateUrl: './status.html',
  styleUrl: './status.css',
})
export class StatusComponent implements OnInit {
  private readonly healthService = inject(HealthService);
  private readonly cdr = inject(ChangeDetectorRef);

  liveness: HealthResponse | null = null;
  readiness: ReadyResponse | null = null;
  apiError: string | null = null;
  databaseError: string | null = null;

  ngOnInit(): void {
    this.healthService.getLiveness().subscribe({
      next: (data) => {
        this.liveness = data;
        this.apiError = null;
        this.cdr.detectChanges();
      },
      error: () => {
        this.apiError = 'Unable to connect to the CivicConnect API.';
        this.cdr.detectChanges();
      },
    });

    this.healthService.getReadiness().subscribe({
      next: (data) => {
        this.readiness = data;
        this.databaseError = null;
        this.cdr.detectChanges();
      },
      error: () => {
        this.databaseError = 'Unable to retrieve database status.';
        this.cdr.detectChanges();
      },
    });
  }
}
