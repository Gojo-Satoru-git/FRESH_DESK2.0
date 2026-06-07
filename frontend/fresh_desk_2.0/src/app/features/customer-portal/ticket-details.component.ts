import { Component, inject, signal, computed } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CustomerHeaderComponent } from './customer-header.component';

// Define a structured interface for clean data modification
interface TicketDetails {
  id: string;
  subject: string;
  status: 'Open' | 'Pending' | 'Resolved';
  priority: 'Low' | 'Medium' | 'High' | 'Critical';
  createdDate: string;
  module: string;
  lastUpdated: string;
  description: string;       
  attachments: string[];     
}

@Component({
  standalone: true,
  selector: 'app-ticket-details',
  imports: [RouterLink],
  template: `
   
    
    <div class="px-10 mt-8">
      <a
        routerLink="/customer-portal"
        class="inline-flex items-center gap-2 ml-76 text-black font-bold text-2xl
               px-4 py-2 rounded-lg
               hover:bg-blue-500 hover:text-white transition"
      >
        ← Back to tickets
      </a>
    </div>

    @if (currentTicket()) {
      <div class="max-w-4xl mx-auto mt-6 flex flex-col gap-6">
        
        <div class="bg-white p-8 rounded-2xl shadow-xl border">
          <div class="flex justify-between items-start mb-6">
            <div>
              <p class="text-gray-400 text-xl mb-2 font-semibold">{{ currentTicket()?.id }}</p>
              <h1 class="text-3xl font-bold text-[#012A4A]">
                {{ currentTicket()?.subject }}
              </h1>
            </div>

            <button
              (click)="openFeedback()"
              class="px-5 py-2 rounded-xl bg-red-100 text-red-700 font-bold hover:bg-red-200"
            >
              Close Ticket
            </button>
          </div>

          <div class="flex gap-4 mb-6">
            <span 
              class="px-4 py-1 rounded-full font-semibold"
              [class.bg-green-100]="currentTicket()?.status === 'Open'"
              [class.text-green-700]="currentTicket()?.status === 'Open'"
              [class.bg-yellow-100]="currentTicket()?.status === 'Pending'"
              [class.text-yellow-700]="currentTicket()?.status === 'Pending'"
              [class.bg-gray-200]="currentTicket()?.status === 'Resolved'"
              [class.text-gray-700]="currentTicket()?.status === 'Resolved'"
            >
              {{ currentTicket()?.status }}
            </span>

            <span 
              class="px-4 py-1 rounded-full font-semibold"
              [class.bg-red-100]="currentTicket()?.priority === 'High' || currentTicket()?.priority === 'Critical'"
              [class.text-red-700]="currentTicket()?.priority === 'High' || currentTicket()?.priority === 'Critical'"
              [class.bg-yellow-100]="currentTicket()?.priority === 'Medium'"
              [class.text-yellow-700]="currentTicket()?.priority === 'Medium'"
              [class.bg-green-100]="currentTicket()?.priority === 'Low'"
              [class.text-green-700]="currentTicket()?.priority === 'Low'"
            >
              {{ currentTicket()?.priority }}
            </span>
          </div>
          
          <hr class="my-6 border-t border-gray-400">

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6 text-lg">
            <div>
              <p class="text-gray-400 mb-2 text-2xl font-semibold">Created</p>
              <p class="font-bold text-xl">{{ currentTicket()?.createdDate }}</p>
            </div>
            <div>
              <p class="text-gray-400 mb-2 text-2xl font-semibold">Module</p>
              <p class="font-bold text-xl">{{ currentTicket()?.module }}</p>
            </div>
            <div>
              <p class="text-gray-400 mb-2 text-2xl font-semibold">Last Updated</p>
              <p class="font-bold text-xl">{{ currentTicket()?.lastUpdated }}</p>
            </div>
          </div>
        </div>

        <div class="bg-white p-8 rounded-2xl shadow-xl border">
          <p class="text-[#012A4A] mb-4 text-2xl font-bold">Description</p>
          <div class="text-xl text-gray-800  whitespace-pre-line leading-relaxed">
            {{ currentTicket()?.description }}
          </div>
           <hr class="my-6 border-t border-gray-400">

          <p class="text-[#012A4A] mb-4 text-2xl font-bold">Attachments</p>
          @if (currentTicket()?.attachments && currentTicket()!.attachments.length > 0) {
            <div class="grid grid-cols-1 sm:grid-cols-1 gap-3">
              @for (file of currentTicket()?.attachments; track file) {
                <div class="flex items-center gap-3 bg-slate-50 border border-gray-200 px-4 py-3 rounded-xl hover:bg-slate-100 transition cursor-pointer">
                  
                  <span class="text-gray-700 font-bold truncate text-base">{{ file }}</span>
                </div>
              }
            </div>
          } @else {
            <div class="p-4 border border-dashed rounded-xl text-center text-gray-400 text-base">
              No attached files found for this ticket.
            </div>
          }
        </div>
        </div>

        

      
    } @else {
      <div class="max-w-4xl mx-auto mt-12 bg-white p-12 rounded-2xl shadow-xl border text-center text-gray-500">
        <p class="text-2xl font-bold">Ticket data could not be found.</p>
        <p class="mt-2 text-lg">Please check the URL or select an existing item from the list.</p>
      </div>
    }

    @if (showFeedback()) {
      <div class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
        <div class="bg-white w-full max-w-xl rounded-2xl p-6 relative">
          <button
            (click)="closeFeedback()"
            class="absolute top-4 right-4 text-gray-400 hover:text-black text-xl"
          >✕</button>

          <h2 class="text-2xl font-bold mb-1">How was your support experience?</h2>
          <p class="text-gray-500 mb-6">Your feedback helps us improve. Closing ticket {{ currentTicket()?.id }}.</p>

          <div class="flex gap-2 mb-6">
            @for (star of [1,2,3,4,5]; track star) {
              <span
                (click)="rating.set(star)"
                class="text-4xl cursor-pointer"
                [class.text-yellow-400]="rating() >= star"
                [class.text-gray-300]="rating() < star"
              >★</span>
            }
          </div>

          <textarea
            placeholder="Add an optional comment..."
            class="w-full h-28 p-4 border rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 mb-6"
          ></textarea>

          <div class="flex justify-end gap-4">
            <button (click)="closeFeedback()" class="px-6 py-2 rounded-xl border font-semibold hover:bg-gray-100">
              Cancel
            </button>
            <button (click)="submitFeedback()" class="px-6 py-2 rounded-xl bg-blue-600 text-white font-semibold hover:bg-blue-700">
              Submit and Close
            </button>
          </div>
        </div>
      </div>
    }
  `
})
export class TicketDetailsComponent {
  private route = inject(ActivatedRoute);

  showFeedback = signal(false);
  rating = signal(0);

  ticketsData = signal<TicketDetails[]>([
    {
      id: 'TKT-1042',
      subject: 'Payroll calculation mismatch for May cycle',
      status: 'Open',
      priority: 'High',
      createdDate: '2026-05-22',
      module: 'Payroll',
      lastUpdated: '2 hours ago',
      description: 'The automated payroll system calculated the standard deductions incorrectly for the May billing cycle. The gross payment matches, but the overall tax breakdown contains clear discrepancies that require immediate engineering attention.',
      attachments: ['may_payslip_error.pdf', 'calculation_mismatch_log.xlsx']
    },
    {
      id: 'TCK-0987',
      subject: 'Invoice mismatch for April',
      status: 'Pending',
      priority: 'Medium',
      createdDate: '2026-04-18',
      module: 'Billing',
      lastUpdated: 'Yesterday',
      description: 'The final invoiced amount displays an extra balance charge that was not communicated during our plan modification setup. Requesting support to review and adjust line items.',
      attachments: ['april_invoice_draft.pdf']
    },
    {
      id: 'TCK-0911',
      subject: 'Password reset request',
      status: 'Resolved',
      priority: 'Low',
      createdDate: '2026-03-05',
      module: 'Authentication',
      lastUpdated: '3 days ago',
      description: 'Unable to receive the verification OTP on the registered mobile endpoint while attempts were made to update credentials via security settings panel.',
      attachments: []
    }
  ]);

  private activeId = computed(() => this.route.snapshot.paramMap.get('id'));

  currentTicket = computed(() => {
    const id = this.activeId();
    return this.ticketsData().find(t => t.id === id) || null;
  });

  openFeedback() {
    this.showFeedback.set(true);
  }

  closeFeedback() {
    this.showFeedback.set(false);
    this.rating.set(0);
  }

  submitFeedback() {
    console.log('Rating:', this.rating(), 'Ticket ID:', this.currentTicket()?.id);
    this.closeFeedback();
  }
}