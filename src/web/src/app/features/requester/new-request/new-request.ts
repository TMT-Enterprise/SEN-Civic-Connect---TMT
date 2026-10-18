import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RequestService } from '../../../core/services/request.service';
import { ToastService } from '../../../core/services/toast.service';
import { ServiceCategory } from '../../../core/models/request.model';

@Component({ selector: 'app-new-request', standalone: true, imports: [FormsModule], templateUrl: './new-request.html', styleUrl: './new-request.css' })
export class NewRequest implements OnInit {
  private requests = inject(RequestService); private toast = inject(ToastService); private router = inject(Router);
  categories: ServiceCategory[] = []; loadingCategories = false; submitting = false; errorMessage = '';
  model = { categoryId: '', description: '' };
  ngOnInit(): void { this.loadingCategories = true; this.requests.categories().subscribe({ next: c => { this.categories = c; this.loadingCategories = false; }, error: () => { this.loadingCategories = false; this.errorMessage = 'Unable to load categories. Check the API connection.'; } }); }
  submit(): void {
    if (this.submitting || !this.model.categoryId || !this.model.description.trim()) return;
    this.submitting = true; this.errorMessage = '';
    this.requests.submit({ categoryId: Number(this.model.categoryId), description: this.model.description.trim() }).subscribe({
      next: result => { this.submitting = false; this.toast.success(`Request #${result.requestId} submitted successfully.`); void this.router.navigateByUrl('/requester/history'); },
      error: e => { this.submitting = false; this.errorMessage = e?.error?.detail || 'Unable to submit the request. Please try again.'; }
    });
  }
}
