import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-sidebar-link',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  template: `
    <a
      [routerLink]="route()"
      routerLinkActive="bg-primary text-white font-semibold shadow-md"
      [routerLinkActiveOptions]="{ exact: exact() }"
      class="flex items-center gap-3 px-3 py-2.5 rounded-md border-l-4 border-transparent hover:bg-gray-100 dark:hover:bg-blue-500 hover:text-white transition-colors text-text-muted group"
    >
      <ng-content></ng-content>
      
      <span class="hidden lg:block">{{ label() }}</span>
      
      @if (badge()) {
        <span class="hidden lg:flex ml-auto bg-red-500 text-white text-[10px] font-bold px-2 py-0.5 rounded-full shadow-sm">
          {{ badge() }}
        </span>
      }
    </a>
  `
})
export class SidebarLinkComponent {
  route = input.required<string>();
  label = input.required<string>();
  exact = input<boolean>(false);
  badge = input<string | number | null>(null);
}