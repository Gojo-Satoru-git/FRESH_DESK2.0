# Theme System & Styling

## Overview
Fresh Desk 2.0 uses a modern theming system built on Tailwind CSS v4 with custom CSS variables, Signals for state management, and localStorage persistence. The application supports light and dark themes that switch dynamically.

## Architecture

### Technology Stack
- **Tailwind CSS v4**: Utility-first CSS framework
- **PostCSS**: CSS processing with Tailwind directives
- **CSS Variables**: Dynamic theme color management
- **Angular Signals**: Reactive state management
- **localStorage**: Theme preference persistence

### File Structure
```
src/
├── styles.css                 # Global styles & theme variables
├── tailwind.config.js         # Tailwind configuration
├── app/
│   └── core/
│       └── theme/
│           ├── theme.service.ts
│           └── theme-switcher.component.ts
```

## CSS Variables & Color Palette

### Variable Definitions (`src/styles.css`)

#### Light Theme (Default)
```css
:root {
  --color-primary: rgb(37 99 235);           /* Blue 600 */
  --color-primary-hover: rgb(29 78 216);     /* Blue 700 */
  --color-surface: rgb(255 255 255);         /* White */
  --color-background: rgb(249 250 251);      /* Gray 50 */
  --color-text: rgb(17 24 39);               /* Gray 900 */
  --color-text-muted: rgb(107 114 128);      /* Gray 500 */
}
```

#### Dark Theme
```css
.dark {
  --color-primary: rgb(59 130 246);          /* Blue 500 */
  --color-primary-hover: rgb(96 165 250);    /* Blue 400 */
  --color-surface: rgb(31 41 55);            /* Gray 800 */
  --color-background: rgb(17 24 39);         /* Gray 900 */
  --color-text: rgb(243 244 246);            /* Gray 100 */
  --color-text-muted: rgb(156 163 175);      /* Gray 400 */
}
```

### Color Meanings
| Variable | Purpose | Light | Dark |
|----------|---------|-------|------|
| `--color-primary` | Main action color | Blue 600 | Blue 500 |
| `--color-surface` | Cards, modals | White | Gray 800 |
| `--color-background` | Page background | Gray 50 | Gray 900 |
| `--color-text` | Main text | Gray 900 | Gray 100 |
| `--color-text-muted` | Secondary text | Gray 500 | Gray 400 |

## Tailwind Configuration

### Configuration File (`tailwind.config.js`)
```javascript
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  darkMode: 'class', // Uses .dark class on html element
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: 'rgb(var(--color-primary) / <alpha-value>)',
          hover: 'rgb(var(--color-primary-hover) / <alpha-value>)',
        },
        surface: 'rgb(var(--color-surface) / <alpha-value>)',
        background: 'rgb(var(--color-background) / <alpha-value>)',
        text: {
          DEFAULT: 'rgb(var(--color-text) / <alpha-value>)',
          muted: 'rgb(var(--color-text-muted) / <alpha-value>)',
        }
      }
    },
  },
  plugins: [],
}
```

### Dark Mode Configuration
- **Mode**: `'class'` - Uses `.dark` class selector
- **Activation**: Add `dark` class to `<html>` element
- **Child Elements**: All children inherit dark theme styles

## Theme Service

### Service Implementation (`core/theme/theme.service.ts`)
```typescript
import { Injectable, signal, inject, effect } from '@angular/core';
import { DOCUMENT } from '@angular/common';

export type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private document = inject(DOCUMENT);
  
  currentTheme = signal<Theme>(
    (localStorage.getItem('app_theme') as Theme) || 'light'
  );

  constructor() {
    effect(() => {
      const theme = this.currentTheme();
      const htmlTag = this.document.documentElement;

      if (theme === 'dark') {
        htmlTag.classList.add('dark');
      } else {
        htmlTag.classList.remove('dark');
      }

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
```

### Key Features
1. **Signals-based**: Reactive state management
2. **localStorage Persistence**: Theme preference saved across sessions
3. **Auto-initialization**: Reads theme from localStorage on app start
4. **Automatic DOM Updates**: Uses Angular effects to update HTML class

## Theme Switcher Component

### Component (`core/theme/theme-switcher.component.ts`)
```typescript
import { Component, inject } from '@angular/core';
import { ThemeService } from './theme.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-theme-switcher',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button (click)="toggleTheme()" class="theme-switcher">
      <span *ngIf="themeService.currentTheme() === 'light'">🌙</span>
      <span *ngIf="themeService.currentTheme() === 'dark'">☀️</span>
    </button>
  `,
  styles: [`
    .theme-switcher {
      padding: 0.5rem;
      border-radius: 0.5rem;
      background: transparent;
      border: 1px solid var(--color-text-muted);
      cursor: pointer;
      font-size: 1.25rem;
      transition: all 0.2s;
    }
    
    .theme-switcher:hover {
      background: var(--color-surface);
    }
  `]
})
export class ThemeSwitcherComponent {
  themeService = inject(ThemeService);

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}
```

### Usage in Components
```html
<app-theme-switcher></app-theme-switcher>
```

## Tailwind Utility Classes

### Using Custom Colors
```html
<!-- Background colors -->
<div class="bg-background">Light mode: Gray 50, Dark mode: Gray 900</div>
<div class="bg-surface">Cards and containers</div>

<!-- Text colors -->
<p class="text-text">Main text color</p>
<p class="text-text-muted">Muted/secondary text</p>

<!-- Primary button -->
<button class="bg-primary hover:bg-primary-hover text-white">
  Action Button
</button>

<!-- Transitions -->
<div class="transition-colors duration-300">
  Smooth color transitions on theme change
</div>
```

### Responsive & Dark Mode
```html
<!-- Dark mode specific -->
<div class="bg-white dark:bg-gray-800">
  Adapts for dark mode
</div>

<!-- Responsive -->
<div class="bg-background md:px-8">
  Changes layout based on screen size
</div>
```

## Implementing Theme in New Components

### Step 1: Use CSS Variables
```css
.card {
  background-color: var(--color-surface);
  color: var(--color-text);
  border: 1px solid var(--color-text-muted);
}
```

### Step 2: Apply Tailwind Classes
```html
<div class="bg-surface text-text border border-text-muted">
  Content here
</div>
```

### Step 3: Support Theme Transitions
```html
<div class="bg-surface text-text transition-colors duration-300">
  Smooth transition when theme changes
</div>
```

## Global Styles Configuration

### Entry Point (`src/styles.css`)
```css
@import "tailwindcss";

@layer base {
  /* Define all CSS variables here */
  :root { ... }
  .dark { ... }

  /* Global resets */
  body {
    @apply bg-background text-text transition-colors duration-300;
  }
}

@layer components {
  /* Reusable component styles */
  .btn-primary {
    @apply px-4 py-2 bg-primary text-white rounded-md 
           hover:bg-primary-hover transition-all;
  }
}
```

## Testing the Theme System

### Manual Testing
1. **Light Mode**
   - Background appears light gray
   - Text appears dark
   - Primary button is blue 600

2. **Dark Mode**
   - Click theme switcher (moon icon)
   - Background appears dark gray
   - Text appears light
   - Primary button is blue 500

3. **Persistence**
   - Switch to dark mode
   - Refresh page
   - Theme should remain dark

### Browser DevTools Check
```javascript
// In Console:
document.documentElement.classList.contains('dark') // true/false
localStorage.getItem('app_theme')                    // 'light' or 'dark'
getComputedStyle(document.body).backgroundColor      // RGB values
```

## Design System Integration

### Color Usage Guidelines
- **Primary**: CTAs, links, important elements
- **Surface**: Cards, modals, containers
- **Background**: Page background, secondary containers
- **Text**: Main content
- **Text-muted**: Descriptions, hints, disabled states

### Example Component
```html
<div class="bg-surface rounded-lg shadow-lg p-6 space-y-4">
  <h2 class="text-2xl font-bold text-text">Title</h2>
  <p class="text-text-muted">Description or subtitle</p>
  <button class="bg-primary hover:bg-primary-hover text-white px-4 py-2 rounded-lg transition-colors">
    Action
  </button>
</div>
```

## Performance Considerations

### CSS Variables Benefits
- **Efficient**: Single property change updates all usages
- **Fast**: No DOM manipulation for colors
- **Small**: Minimal CSS payload

### Tailwind CSS Optimization
- **PurgeCSS**: Only unused styles are removed in production
- **Build Size**: ~16KB for production styles (gzipped)
- **Performance**: No runtime CSS generation

### Theme Switch Performance
- **Instant**: `.dark` class application is instantaneous
- **No Flash**: All elements update simultaneously
- **Smooth**: CSS transitions handle visual changes

## Browser Support

| Browser | Support | Notes |
|---------|---------|-------|
| Chrome | ✅ | Full support for CSS variables |
| Firefox | ✅ | Full support for CSS variables |
| Safari | ✅ | Full support for CSS variables |
| Edge | ✅ | Full support for CSS variables |
| IE 11 | ❌ | CSS variables not supported |

## Customization Guide

### Adding New Colors
1. **Update CSS Variables**
   ```css
   :root {
     --color-success: rgb(34 197 94);
   }
   .dark {
     --color-success: rgb(22 163 74);
   }
   ```

2. **Add to Tailwind Config**
   ```javascript
   theme: {
     extend: {
       colors: {
         success: 'rgb(var(--color-success) / <alpha-value>)'
       }
     }
   }
   ```

3. **Use in Templates**
   ```html
   <div class="bg-success">Success message</div>
   ```

### Extending Themes
Create theme variants in CSS:
```css
.theme-highcontrast {
  --color-primary: rgb(0 0 0);
  --color-background: rgb(255 255 255);
  /* High contrast colors */
}
```

## Related Documentation
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Authentication Setup](./authentication-setup.md)
- [Frontend Architecture](./architecture.md)
- [UI Guidelines](./ui-guidelines.md)
