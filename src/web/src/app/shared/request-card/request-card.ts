import { Component, Input } from '@angular/core';


export interface ServiceRequest {
  id: string;
  citizen: string;
  category: string;
  description: string;
  priority: 'Critical' | 'High' | 'Medium' | 'Normal' | null;
  status:
    | 'Submitted'
    | 'Assigned'
    | 'In Progress'
    | 'Resolved'
    | 'Closed'
    | 'Overdue'
    | 'Rejected';
  dueDate: string;
}


@Component({
  selector: 'app-request-card',
  standalone: true,
  templateUrl: './request-card.html',
  styleUrl: './request-card.css'
})
export class RequestCard {
  @Input({ required: true }) request!: ServiceRequest;

  get statusLabel(): string {
    return this.request.status === 'Assigned'
      ? 'Pending Accept'
      : this.request.status;
  }
}

