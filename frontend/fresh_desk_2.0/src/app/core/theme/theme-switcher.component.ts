import { Component, inject } from '@angular/core';
import { ThemeService } from '../../core/theme/theme.service';

@Component({
  selector: 'app-theme-switcher',
  standalone: true,
  template: `
    <button 
      (click)="themeService.toggleTheme()"
      class="px-4 py-2 bg-surface text-text border border-primary rounded-md hover:bg-primary hover:text-white transition-all"
    >
      @if (themeService.currentTheme() === 'light') {
        <span>🌙 Switch to Dark Mode</span>
      } @else {
        <span>☀️ Switch to Light Mode</span>
      }
    </button>
  `
})
export class ThemeSwitcherComponent {
  themeService = inject(ThemeService);
}