import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  HealthService,
  HealthResponse,
  ReadyResponse
} from '../core/health';

@Component({
  selector: 'app-status',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status.html',
  styleUrl: './status.css',
})
export class StatusComponent implements OnInit {
  liveness: HealthResponse | null = null;
  readiness: ReadyResponse | null = null;
  apiError: string | null = null;
  databaseError: string | null = null;

  constructor(
    private healthService: HealthService,
    private cdr: ChangeDetectorRef
  ) {}

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
