import { Injectable, signal } from '@angular/core';
export type ToastKind = 'success' | 'error' | 'info';
export interface ToastMessage { id: number; message: string; kind: ToastKind; }
@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly messages = signal<ToastMessage[]>([]); private sequence = 0;
  success(message: string): void { this.add(message, 'success'); }
  error(message: string): void { this.add(message, 'error'); }
  info(message: string): void { this.add(message, 'info'); }
  dismiss(id: number): void { this.messages.update(items => items.filter(item => item.id !== id)); }
  private add(message: string, kind: ToastKind): void { this.messages.update(items => [...items, { id: ++this.sequence, message, kind }]); }
}
