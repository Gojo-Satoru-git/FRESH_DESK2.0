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
 
  category = ['Bug','Enhancement','Feature Requests','Service Requests','Customization','Incident','Environment Issues','Change Request','New Features'];
  modules = ['Data Correction','Patch deployment','Configuration','Clarification','Server Outage','Ad hoc','Known issue'];
  priorities = ['Low', 'Medium', 'High', 'Urgent'];
  kbSuggestions = signal<any[]>([]);
  showCancelConfirm = signal<boolean>(false);
  toastMessage = signal<string | null>(null);

  readonly MAX_WORDS = 300;
descriptionWordCount = signal(0);

onDescriptionInput(event: Event): void {
  const textarea = event.target as HTMLTextAreaElement;
  const words = textarea.value
    .trim()
    .split(/\s+/)
    .filter(w => w.length > 0);

  if (words.length > this.MAX_WORDS) {
    textarea.value = words.slice(0, this.MAX_WORDS).join(' ');
    this.ticketForm.get('description')?.setValue(textarea.value);
    this.descriptionWordCount.set(this.MAX_WORDS);
    return;
  }

  this.descriptionWordCount.set(words.length);
}

  private showToast(msg: string) {
    this.toastMessage.set(msg);
    setTimeout(() => this.toastMessage.set(null), 3000);
  }

  ngOnInit(): void {
    this.ticketForm = this.fb.group({
      subject: ['', [Validators.required, Validators.minLength(5)]],
      category: ['', Validators.required],
      module: [''],
      priority: ['Medium'],
      description: ['', [
  Validators.required,
  Validators.minLength(20),
  Validators.maxLength(3000) // safety fallback
]],
    }); 
  }

  private readonly MAX_FILE_SIZE = 50 * 1024 * 1024; // 50MB

onFileSelect(event: Event) {
  const input = event.target as HTMLInputElement;
  if (!input.files) return;

  for (const file of Array.from(input.files)) {
    if (file.size > this.MAX_FILE_SIZE) {
      this.showToast(`"${file.name}" exceeds 50 MB limit`);
      continue;
    }
    this.attachedFiles.push(file);
  }
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
    if (this.isSubmitting) return;
    this.submitted = true;
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      this.showToast('Please fill all required fields correctly (Subject > 5 chars, Description > 20 chars).');
      return;
    }
    
    this.isSubmitting = true;
    
    const payload = {
      title: this.ticketForm.value.subject,
      description: this.ticketForm.value.description,
      priority: this.ticketForm.value.priority,
      type: this.ticketForm.value.category,
      moduleName: this.ticketForm.value.module,
      groupId: '466b8a16-7910-4e20-891f-59fbdb0ca009',
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