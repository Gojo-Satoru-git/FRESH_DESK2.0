import { Injectable, signal, inject, effect } from '@angular/core';
import { DOCUMENT } from '@angular/common';

export type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private document = inject(DOCUMENT);
  
  // Initialize the signal from localStorage, default to 'light'
  currentTheme = signal<Theme>(
    (localStorage.getItem('app_theme') as Theme) || 'light'
  );
  constructor() {
    // This effect runs automatically whenever the currentTheme signal changes
    effect(() => {
      const theme = this.currentTheme();
      const htmlTag = this.document.documentElement;

      if (theme === 'dark') {
        htmlTag.classList.add('dark');
      } else {
        htmlTag.classList.remove('dark');
      }

      // Persist the choice
      localStorage.setItem('app_theme', theme);
    });
  }

  toggleTheme() {
    this.currentTheme.update(theme => theme === 'light' ? 'dark' : 'light');
  }

  setTheme(theme: Theme) {
    this.currentTheme.set(theme);
  }
}