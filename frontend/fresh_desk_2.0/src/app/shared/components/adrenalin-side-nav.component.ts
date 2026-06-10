import { Component, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { SafeHtmlPipe } from '../pipes/safe-html.pipes';
import { NavItem } from '../models/nav-item.interface';

@Component({
  selector: 'app-adrenalin-side-nav',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, SafeHtmlPipe],
  template: `
    <div
      class="relative h-full transition-all duration-300 flex-shrink-0 z-40"
      [class.w-16]="!isExpanded()"
      [class.md:w-72]="isExpanded()"
    >
      @if (isExpanded()) {
        <div class="fixed inset-0 bg-black/50 z-40 md:hidden" (click)="toggleNav()"></div>
      }

      <nav
        class="absolute top-0 left-0 h-full flex flex-col text-text-white rounded-r-[2rem] transition-all duration-300 ease-in-out shadow-xl z-50 border-r border-transparent dark:border-gray-800"
        [style.backgroundColor]="'var(--color-primary)'"
        [class.w-16]="!isExpanded()"
        [class.w-72]="isExpanded()"
      >
        <div class="h-16 flex items-center px-5 flex-shrink-0">
          <button
            (click)="toggleNav()"
            class="text-text-white hover:text-secondary-green dark:hover:text-secondary-green transition-colors outline-none cursor-pointer"
          >
            @if (!isExpanded()) {
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 6h16M4 12h16M4 18h16"
                ></path>
              </svg>
            } @else {
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                ></path>
              </svg>
            }
          </button>
        </div>

        <div
          class="flex-1 py-2 flex flex-col scrollbar-hide"
          [class.overflow-y-auto]="isExpanded()"
          [class.overflow-visible]="!isExpanded()"
        >
          @for (item of items(); track item.label) {
            <div
              class="relative group flex flex-col"
              (mouseenter)="hoveredItem.set(item.label)"
              (mouseleave)="hoveredItem.set(null)"
            >
              @if (item.route) {
                <a
                  [routerLink]="[item.route]"
                  routerLinkActive="is-active"
                  #rla="routerLinkActive"
                  [routerLinkActiveOptions]="{ exact: false }"
                  class="flex items-center px-5 py-3.5 cursor-pointer transition-colors block"
                  [ngClass]="{
                    'text-secondary-green': rla.isActive,
                    'text-text-white hover:bg-primary-hover dark:hover:bg-background/20':
                      !rla.isActive,
                  }"
                >
                  <ng-container
                    *ngTemplateOutlet="navContent; context: { active: rla.isActive }"
                  ></ng-container>
                </a>
              } @else {
                <div
                  (click)="toggleSubmenu(item.label)"
                  class="flex items-center px-5 py-3.5 cursor-pointer transition-colors"
                  [ngClass]="{
                    'text-secondary-green': isSubmenuOpen(item.label),
                    'text-text-white hover:bg-primary-hover dark:hover:bg-background/20':
                      !isSubmenuOpen(item.label),
                  }"
                >
                  <ng-container
                    *ngTemplateOutlet="navContent; context: { active: isSubmenuOpen(item.label) }"
                  ></ng-container>
                </div>
              }

              <ng-template #navContent let-active="active">
                <div
                  class="w-6 h-6 flex-shrink-0 flex items-center justify-center transition-colors"
                  [class.text-secondary-green]="active"
                  [innerHTML]="item.iconSvg | safeHtml"
                ></div>
                @if (isExpanded()) {
                  <span class="ml-4 font-semibold text-sm whitespace-nowrap flex-1 truncate">{{
                    item.label
                  }}</span>
                  @if (item.children) {
                    <svg
                      class="w-4 h-4 transition-transform duration-200"
                      [class.rotate-180]="isSubmenuOpen(item.label)"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M19 9l-7 7-7-7"
                      ></path>
                    </svg>
                  }
                }
              </ng-template>

              @if (!isExpanded() && hoveredItem() === item.label) {
                <div
                  class="absolute left-[4.5rem] top-1/2 -translate-y-1/2 flex items-center z-[100] pointer-events-none animate-fade-in"
                >
                  <div
                    class="w-2.5 h-2.5 bg-primary-hover dark:bg-gray-800 border-l border-b border-transparent dark:border-gray-700 rotate-45 -mr-1.5 z-0"
                  ></div>
                  <div
                    class="bg-primary-hover dark:bg-gray-800 text-text-white font-semibold text-sm px-4 py-2 rounded shadow-lg z-10 whitespace-nowrap border border-transparent dark:border-gray-700"
                  >
                    {{ item.label }}
                  </div>
                </div>
              }

              @if (isExpanded() && item.children && isSubmenuOpen(item.label)) {
                <div class="bg-primary-hover dark:bg-background/40 flex flex-col py-2 shadow-inner">
                  @for (child of item.children; track child.label) {
                    <a
                      [routerLink]="[child.route]"
                      routerLinkActive="is-active"
                      #rlaChild="routerLinkActive"
                      class="pl-16 pr-5 py-2.5 text-sm font-medium transition-colors whitespace-nowrap block"
                      [ngClass]="{
                        'bg-background/20 dark:bg-primary/20 text-secondary-green font-bold':
                          rlaChild.isActive,
                        'text-text-white hover:text-secondary-green hover:bg-background/10 dark:hover:bg-surface/30':
                          !rlaChild.isActive,
                      }"
                    >
                      {{ child.label }}
                    </a>
                  }
                </div>
              }
            </div>
          }
        </div>
      </nav>
    </div>
  `,
})
export class AdrenalinSideNavComponent {
  items = input.required<NavItem[]>();

  isExpanded = signal<boolean>(false);
  hoveredItem = signal<string | null>(null);
  openSubmenus = signal<Record<string, boolean>>({});

  toggleNav() {
    this.isExpanded.update((v) => !v);
    if (!this.isExpanded()) {
      this.openSubmenus.set({});
    }
  }

  toggleSubmenu(label: string) {
    if (!this.isExpanded()) {
      this.isExpanded.set(true);
    }
    this.openSubmenus.update((state) => ({
      ...state,
      [label]: !state[label],
    }));
  }

  isSubmenuOpen(label: string): boolean {
    return !!this.openSubmenus()[label];
  }
}
