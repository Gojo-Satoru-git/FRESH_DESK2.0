# Theme System

## Why Dark Mode?

Users spend 8+ hours a day in Fresh Desk. Dark mode at night:
- 😴 Reduces eye strain (less blue light)
- 🔋 Saves battery on OLED screens (~15% power savings)
- 🎨 Looks professional
- ♿ Helps dyslexic/low-vision users

Fresh Desk auto-detects your system preference and switches when you toggle the theme button.

## How It Works (Simple Version)

### 1. Light Mode (Default)
```css
:root {
  --color-primary: rgb(37 99 235);      /* Blue */
  --color-background: rgb(249 250 251); /* Light gray */
  --color-text: rgb(17 24 39);          /* Dark gray (for contrast) */
}
```

### 2. Dark Mode
```css
.dark {
  --color-primary: rgb(59 130 246);     /* Lighter blue */
  --color-background: rgb(17 24 39);    /* Almost black */
  --color-text: rgb(243 244 246);       /* Light gray (for contrast) */
}
```

### 3. The Toggle
User clicks theme button → JavaScript adds/removes `.dark` class on `<html>` → CSS variables change → All colors update instantly.

**Why CSS variables?** 
- Single source of truth (one place to change blue)
- No need to update every component
- Fast (no JavaScript re-rendering)
- Easy to debug (open DevTools → Styles tab)

## File Structure

```
src/
├── styles.css                 ← All color variables & Tailwind setup
├── app/
│   └── core/theme/
│       ├── theme.service.ts   ← Logic (get/set theme, detect OS preference)
│       └── theme-switcher.component.ts  ← Toggle button (visible in corner)
```

## Color Palette

### Light Theme Colors
| Name | Purpose | Color |
|------|---------|-------|
| Primary | Buttons, links | Blue 600 |
| Surface | Cards, modals | White |
| Background | Page background | Gray 50 (light) |
| Text | Main text | Gray 900 (dark) |
| Text Muted | Secondary text | Gray 500 |

### Dark Theme Colors (Same Names, Different Values)
| Name | Purpose | Color |
|------|---------|-------|
| Primary | Buttons, links | Blue 500 (lighter) |
| Surface | Cards, modals | Gray 800 (dark) |
| Background | Page background | Gray 900 (almost black) |
| Text | Main text | Gray 100 (light) |
| Text Muted | Secondary text | Gray 400 |

**Why different shades?** Contrast. In light mode, dark text on white is readable. In dark mode, light text on dark gray is readable.

## Using Colors in Components

### Tailwind Utility Classes
```html
<!-- Easy way: Use Tailwind classes -->
<button class="bg-primary text-white">Login</button>
<div class="bg-surface text-text">Card content</div>
<p class="text-text-muted">Optional field</p>
```

Tailwind automatically swaps colors based on dark mode.

### Direct CSS
```css
.my-card {
  background-color: var(--color-surface);
  color: var(--color-text);
  border-color: var(--color-primary);
}
```

## Theme Service

Located in `src/app/core/theme/theme.service.ts`

### Check Current Theme
```typescript
constructor(private theme: ThemeService) {}

isDarkMode() {
  return this.theme.isDarkMode();  // true or false
}
```

### Toggle Theme
```typescript
toggleTheme() {
  this.theme.toggle();  // Switches dark ↔ light
}
```

### Detect System Preference
```typescript
ngOnInit() {
  this.theme.initializeFromSystemPreference(); // Dark if OS is dark
}
```

## Persistence

### How It Persists
1. User toggles theme
2. Service saves to `localStorage.setItem('theme', 'dark')`
3. On page reload, service reads localStorage
4. Applies theme immediately (no flash)

**Result:** Users return tomorrow, their theme preference is remembered.

## Customizing Colors

### Change Primary Color (Blue → Purple)

Edit `src/styles.css`:
```css
:root {
  --color-primary: rgb(147 51 234);       /* Purple instead of Blue */
  --color-primary-hover: rgb(126 34 206); /* Darker purple for hover */
}
```

**Where will it change?**
- Every button
- Every link
- Primary text
- Anything using `.bg-primary` or `.text-primary`

### Add a New Color

```css
:root {
  --color-success: rgb(34 197 94);  /* Green */
  --color-error: rgb(239 68 68);    /* Red */
  --color-warning: rgb(217 119 6);  /* Orange */
}

.dark {
  --color-success: rgb(74 222 128);  /* Lighter green */
  --color-error: rgb(248 113 113);   /* Lighter red */
  --color-warning: rgb(253 164 40);  /* Lighter orange */
}
```

Then use in Tailwind config and components.

## Theme Architecture

### Why Signals Instead of RxJS?
```typescript
// Old way (RxJS Subject)
themeChanged$ = new Subject<'light' | 'dark'>();

// New way (Angular Signal)
currentTheme = signal<'light' | 'dark'>('light');
```

**Why signals?** 
- Cleaner syntax (no subscribe/unsubscribe)
- Automatic change detection (faster)
- Smaller bundle size
- More modern Angular approach

### Dark Mode Implementation

#### Method 1: Class-Based (Current)
```html
<!-- Light mode -->
<html>

<!-- Dark mode -->
<html class="dark">
```

CSS:
```css
:root { /* light colors */ }
.dark { /* dark colors */ }
```

**Pros:** Works everywhere, CSS-based, fast

**Cons:** Requires class management

#### Method 2: CSS Media Query (Alternative)
```css
@media (prefers-color-scheme: dark) {
  /* automatic dark mode */
}
```

**Why we didn't use this:** Limited control, can't override user preference

## Troubleshooting

### Theme Not Saving
- Check localStorage: Open DevTools → Application → Storage → localStorage
- Look for key `theme`
- If missing, theme service might not have loaded yet

### Colors Look Wrong in Dark Mode
1. Check CSS variable is defined in `.dark` block
2. Verify Tailwind class uses the variable: `bg-primary`, not `bg-blue-600`
3. Hard refresh browser: `Ctrl+Shift+R`

### Flash of Wrong Color on Page Load
This can happen if:
- localStorage read is slow
- Default colors conflict with system theme

**Fix:** Add theme detection script in `src/index.html` before app loads.

## Dark Mode Best Practices

✅ **Do:**
- Test both light & dark modes before merging
- Use theme colors, not hardcoded colors
- Ensure text contrast is readable (WCAG AA standard)

❌ **Don't:**
- Hardcode colors in components (`style="color: blue"`)
- Use pure black on pure white (too harsh)
- Forget to test on mobile (where dark mode is most useful)

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
