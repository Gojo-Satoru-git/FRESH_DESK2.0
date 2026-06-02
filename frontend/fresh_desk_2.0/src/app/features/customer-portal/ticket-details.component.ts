import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-ticket-details',
  imports: [RouterLink],
  template: `
   <header class="flex items-center justify-between px-4 sm:px-8 md:px-12 lg:px-16 py-4">

      <!-- LEFT: LOGO -->
      <img
        src="log.png"
        alt="Customer Portal Logo"
        class="
          object-contain
          h-32 w-32
          sm:h-40 sm:w-40
          md:h-48 md:w-48
          lg:h-64 lg:w-64
          -mt-6 sm:-mt-8 md:-mt-10 lg:-mt-14
        "
      />

      <!-- RIGHT: ACTIONS -->
      <div class="flex items-center gap-4 relative">

        <!-- Notification Button -->
        <button
           class="
            h-10 w-12
            rounded-xl
            bg-gray-100
            hover:bg-blue-500
            active:bg-blue-500
            flex items-center justify-center
            mr-2
            -mt-6 sm:-mt-8 md:-mt-10 lg:-mt-20
            transition-colors duration-200
            focus:outline-none
          "
        >
          <img src="notification.png" alt="Notifications" class="h-5 w-5" />
        </button>

        <!-- Avatar -->
        <button
          (click)="toggleMenu()"
          class="
            h-14 w-14
            rounded-full
            bg-[#012A4A]
            text-white
            flex items-center justify-center
            font-semibold text-lg
            -mt-6 sm:-mt-8 md:-mt-10 lg:-mt-20
            focus:outline-none
          "
        >
          A
        </button>

        <!-- DROPDOWN -->
        @if (isMenuOpen()) {
          <div
            class="
              absolute right-0 top-2
              w-44
              bg-white
              rounded-lg
              shadow-lg
              border
              z-50
            "
          >
           <!-- My Profile -->
          <button
            class="
              w-full px-4 py-2
              flex items-center gap-3
              text-left
              hover:bg-gray-100
            "
            (click)="goToProfile()"
          >
            <img
              src="profile.png"
              alt="Profile"
              class="h-6 w-6 object-contain"
            />
            <span class="font-bold">My Profile</span>
          </button>



            <!-- Logout -->
            <button
              class="
                w-full px-4 py-2
                flex items-center gap-3
                text-left
                text-red-600
                hover:bg-gray-100
              "
              (click)="logout()"
            >
              <img
                src="logout.png"
                alt="Logout"
                class="h-6 w-6 object-contain"
              />
              <span class = "font-bold">Logout</span>
            </button>
            
          </div>
        }
        
      </div>
    </header>
    <!-- BACK -->
    <div class="px-10 mt-8">
      <a
        routerLink="/customer-portal"
        class="flex items-center gap-2 ml-108  text-black font-bold text-2xl
               px-4 py-2 rounded-lg
               hover:bg-blue-600 hover:text-white transition"
      >
        ← Back to tickets
      </a>
    </div>

    <!-- TICKET CARD -->
    <div class="max-w-4xl mx-auto mt-6 bg-white p-8 rounded-2xl shadow-xl border">

      <!-- HEADER -->
      <div class="flex justify-between items-start mb-6">
        <div>
          <p class="text-gray-400 text-lg font-semibold">TKT-1042</p>
          <h1 class="text-3xl font-bold text-[#012A4A]">
            Payroll calculation mismatch for May cycle
          </h1>
        </div>

        <!-- CLOSE TICKET -->
        <button
          (click)="openFeedback()"
          class="px-5 py-2 rounded-xl bg-red-100 text-red-700 font-bold hover:bg-red-200"
        >
          Close Ticket
        </button>
      </div>

      <!-- STATUS -->
      <div class="flex gap-4 mb-6">
        <span class="px-4 py-1 rounded-full bg-green-100 text-green-700 font-semibold">
          Open
        </span>
        <span class="px-4 py-1 rounded-full bg-red-100 text-red-700 font-semibold">
          High
        </span>
        
      </div>
      <hr class="my-6 border-t border-gray-400">

      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 text-lg">

        <div>

          <p class="text-gray-400 font-semibold">Created</p>

          <p class="font-bold">2026-05-22</p>

        </div>



        <div>

          <p class="text-gray-400 font-semibold">Module</p>

          <p class="font-bold">Payroll</p>

        </div>



        <div>

          <p class="text-gray-400 font-semibold">Last Updated</p>

          <p class="font-bold">2 hours ago</p>

        </div>

      </div>
    </div>

    <!-- ================= MODAL ================= -->
    @if (showFeedback()) {
      <div class="fixed inset-0 bg-black/40 flex items-center justify-center z-50">

        <div class="bg-white w-full max-w-xl rounded-2xl p-6 relative">

          <!-- CLOSE ICON -->
          <button
            (click)="closeFeedback()"
            class="absolute top-4 right-4 text-gray-400 hover:text-black text-xl"
          >
            ✕
          </button>

          <!-- TITLE -->
          <h2 class="text-2xl font-bold mb-1">
            How was your support experience?
          </h2>
          <p class="text-gray-500 mb-6">
            Your feedback helps us improve. Closing ticket TKT-1042.
          </p>

          <!-- STARS -->
          <div class="flex gap-2 mb-6">
            @for (star of [1,2,3,4,5]; track star) {
              <span
                (click)="rating.set(star)"
                class="text-4xl cursor-pointer"
                [class.text-yellow-400]="rating() >= star"
                [class.text-gray-300]="rating() < star"
              >
                ★
              </span>
            }
          </div>

          <!-- COMMENT -->
          <textarea
            placeholder="Add an optional comment..."
            class="w-full h-28 p-4 border rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 mb-6"
          ></textarea>

          <!-- ACTIONS -->
          <div class="flex justify-end gap-4">
            <button
              (click)="closeFeedback()"
              class="px-6 py-2 rounded-xl border font-semibold hover:bg-gray-100"
            >
              Cancel
            </button>

            <button
              (click)="submitFeedback()"
              class="px-6 py-2 rounded-xl bg-blue-600 text-white font-semibold hover:bg-blue-700"
            >
              Submit and Close
            </button>
          </div>
          


        </div>
        
        

      </div>
      
    }
  `
})
export class TicketDetailsComponent {

  showFeedback = signal(false);
    private router = inject(Router);
  private route = inject(ActivatedRoute);
  rating = signal(0);
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

  openFeedback() {
    this.showFeedback.set(true);
  }

  closeFeedback() {
    this.showFeedback.set(false);
    this.rating.set(0);
  }

  submitFeedback() {
    console.log('Rating:', this.rating());
    this.closeFeedback();
    // here later call API + mark ticket closed
  }
}