import { Component, inject, signal, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { TicketService } from '../tickets/services/ticket.service';
import { environment } from '../../../environments/environment.development';

@Component({
  selector: 'app-raise-ticket',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './raise-ticket.component.html',
})
export class RaiseTicketComponent implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private router = inject(Router);
  private ticketService = inject(TicketService);
  
  @Output() ticketCreated = new EventEmitter<void>();

  ticketForm!: FormGroup;
  attachedFiles: File[] = [];
  isDragOver = false;
  isSubmitting = false;
  submitted = false;

  modules = ['Change Request', 'Clarification', 'Environment Issues', 'New Requirements', 'Service Requests', 'Software Enhancement','Software Problem', 'Other'];
  priorities = ['Low', 'Medium', 'High', 'Critical'];
  kbSuggestions = signal<any[]>([]);
  showCancelConfirm = signal<boolean>(false);
  toastMessage = signal<string | null>(null);

  private showToast(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }

  ngOnInit(): void {
    this.ticketForm = this.fb.group({
      subject: ['', [Validators.required, Validators.minLength(5)]],
      category: ['', Validators.required],
      module: ['', Validators.required],
      priority: ['Medium'],
      description: ['', [Validators.required, Validators.minLength(20)]],
    }); 
  }

  onFileSelect(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) this.attachedFiles.push(...Array.from(input.files));
  }

  onFileDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
    if (event.dataTransfer?.files) {
      this.attachedFiles.push(...Array.from(event.dataTransfer.files));
    }
  }

  removeFile(index: number) {
    this.attachedFiles.splice(index, 1);
  }

  onCancel(): void {
    if (this.ticketForm.dirty || this.attachedFiles.length > 0) {
      this.showCancelConfirm.set(true);
    } else {
      this.confirmCancel();
    }
  }

  confirmCancel(): void {
    this.ticketForm.reset({
      subject: '',
      category: '',
      module: '',
      priority: 'Medium',
      description: ''
    });
    this.attachedFiles = [];
    this.kbSuggestions.set([]);
    this.submitted = false;
    this.showCancelConfirm.set(false);
    this.ticketForm.markAsPristine();
    this.ticketForm.markAsUntouched();
    this.ticketCreated.emit();
  }

  keepEditing(): void {
    this.showCancelConfirm.set(false);
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }
    
    this.isSubmitting = true;
    
    const payload = {
      title: this.ticketForm.value.subject,
      description: this.ticketForm.value.description,
      priority: this.ticketForm.value.priority,
      category: this.ticketForm.value.category,
      module: this.ticketForm.value.module,
      tags: []
    };

    this.ticketService.createTicket(payload).subscribe({
      next: (res: any) => {
        const ticketId = res.ticketId;
        if (this.attachedFiles.length > 0) {
          this.uploadFiles(ticketId);
        } else {
          this.onSuccess();
        }
      },
      error: (err: any) => {
        this.showToast('Failed to submit ticket: ' + (err.error?.error || 'Unknown error'));
        this.isSubmitting = false;
      }
    });
  }

  private uploadFiles(ticketId: string): void {
    let uploadedCount = 0;
    this.attachedFiles.forEach(file => {
      const formData = new FormData();
      formData.append('File', file);
      
      this.http.post(`${environment.apiUrl}/api/tickets/${ticketId}/attachments`, formData).subscribe({
        next: () => {
          uploadedCount++;
          if (uploadedCount === this.attachedFiles.length) {
            this.onSuccess();
          }
        },
        error: () => {
          uploadedCount++;
          if (uploadedCount === this.attachedFiles.length) {
            this.onSuccess();
          }
        }
      });
    });
  }

  private onSuccess(): void {
    this.isSubmitting = false;
    this.showToast('Ticket submitted successfully!');
    setTimeout(() => {
      this.ticketCreated.emit();
    }, 1500);
  }
}