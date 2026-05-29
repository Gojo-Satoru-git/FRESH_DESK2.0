import { Component, HostListener, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';

@Component({
  selector: 'app-raise-ticket',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './raise-ticket.component.html',
})
export class RaiseTicketComponent implements OnInit {

  // ===== HEADER LOGIC (SAME AS CUSTOMER PORTAL) =====
  private router = inject(Router);
  isMenuOpen = signal(false);

  toggleMenu() {
    this.isMenuOpen.update(v => !v);
  }

  goToProfile() {
    this.isMenuOpen.set(false);
    this.router.navigate(['/customer/profile']);
  }

  logout() {
    this.isMenuOpen.set(false);
    this.router.navigate(['/login']);
  }

  @HostListener('document:click', ['$event'])
  closeOnOutsideClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('button') && !target.closest('.shadow-lg')) {
      this.isMenuOpen.set(false);
    }
  }

  // ===== FORM LOGIC =====
  ticketForm!: FormGroup;
  attachedFiles: File[] = [];
  isDragOver = false;
  isSubmitting = false;
  submitted = false;

  modules = [
    'Authentication',
    'Billing',
    'Dashboard',
    'Reporting',
    'Integrations',
    'Settings',
    'Other',
  ];

  priorities = ['Low', 'Medium', 'High', 'Critical'];

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.ticketForm = this.fb.group({
      subject: ['', [Validators.required, Validators.minLength(5)]],
      module: ['', Validators.required],
      priority: ['Medium'],
      description: ['', [Validators.required, Validators.minLength(20)]],
    });
  }
onFileSelect(event: Event) {
  const input = event.target as HTMLInputElement;
  if (!input.files) return;

  this.attachedFiles.push(...Array.from(input.files));
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
  isFieldInvalid(field: string): boolean {
    const control = this.ticketForm.get(field);
    return !!(control?.invalid && (control.dirty || control.touched || this.submitted));
  }

  onCancel(): void {
    this.ticketForm.reset({ priority: 'Medium' });
    this.attachedFiles = [];
    this.submitted = false;
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.ticketForm.invalid) {
      this.ticketForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    setTimeout(() => {
      alert('Ticket submitted successfully!');
      this.isSubmitting = false;
      this.onCancel();
    }, 1500);
  }
}