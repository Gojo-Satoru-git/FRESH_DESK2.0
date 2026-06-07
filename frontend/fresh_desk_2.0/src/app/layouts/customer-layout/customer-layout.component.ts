import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CustomerHeaderComponent } from '../../features/customer-portal/customer-header.component';
@Component({
  selector: 'app-customer-layout',
  standalone: true,
  imports: [CustomerHeaderComponent,RouterOutlet],
  template: `
     <app-customer-header>
      <router-outlet></router-outlet>
    </app-customer-header>
  `,
})
export class CustomerLayoutComponent {}