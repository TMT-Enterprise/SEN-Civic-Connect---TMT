import { Injectable, signal } from '@angular/core';
import { RequestItem } from '../models/request.model';
@Injectable({ providedIn: 'root' })
export class ModalService {
  readonly selectedRequest = signal<RequestItem | null>(null);
  open(request: RequestItem): void { this.selectedRequest.set(request); }
  close(): void { this.selectedRequest.set(null); }
}
