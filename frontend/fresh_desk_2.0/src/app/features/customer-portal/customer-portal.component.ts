import { Component } from '@angular/core';

@Component({
  selector: 'app-customer-portal',
  standalone: true,
  template: `
    <!-- Company logo and title -->
    <header class="flex flex-col items-start px-4 sm:px-8 md:px-12 lg:px-16">
      
      <!-- Logo -->
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

      <!-- Text below image -->
      <p class="
        -mt-22 px-2 text-bold
        text-4xl
        sm:text-base
        md:text-lg
        lg:text-3xl
        font-medium
        
      
      ">
        Customer Portal
      </p>

    </header>
  `,
})
export class CustomerPortalComponent {}