
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface HealthResponse {
  status: string;
  version: string;
  commit: string;
}

export interface ReadyResponse {
  status: string;
  checks: {
    name: string;
    status: string;
    error: string | null;
  }[];
}

@Injectable({
  providedIn: 'root',
})
export class HealthService {
  private readonly http = inject(HttpClient);

  getLiveness(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>('/health');
  }

  getReadiness(): Observable<ReadyResponse> {
    return this.http.get<ReadyResponse>('/health/ready');
  }
}
