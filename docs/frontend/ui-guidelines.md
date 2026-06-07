# UI Guidelines

## Design Philosophy

Fresh Desk UI is:
- 🎯 **Simple** - Minimal, focused on tasks (no flashy animations)
- 🏃 **Fast** - Instant feedback (no loading surprises)
- ♿ **Accessible** - Works for everyone (keyboard, screen readers)
- 📱 **Responsive** - Works on phone/tablet/desktop
- 🎨 **Consistent** - Same button everywhere looks same

## Color System

### Primary Colors

| Color | Usage | Hex |
|-------|-------|-----|
| Primary Blue | Action buttons, links, hover states | Light: `#2563eb`, Dark: `#3b82f6` |
| Gray 900 | Text, headings | Light: `#111827`, Dark: `#f3f4f6` |
| Gray 500 | Secondary text, disabled | Light: `#6b7280`, Dark: `#9ca3af` |
| White/Dark | Cards, surfaces | Light: `#ffffff`, Dark: `#1f2937` |

### Usage
```html
<!-- Primary button -->
<button class="bg-primary text-white">Save</button>

<!-- Secondary button -->
<button class="border border-gray-300 text-text">Cancel</button>

<!-- Text -->
<p class="text-text">Important info</p>
<p class="text-text-muted">Optional field</p>
```

### Status Colors (Add These When Needed)

| Status | Color | Usage |
|--------|-------|-------|
| Success | Green 500 | Confirmed, resolved, success |
| Error | Red 500 | Validation errors, failures |
| Warning | Orange 500 | Caution, pending action |
| Info | Blue 500 | FYI messages |

## Typography

### Font Stack
```css
font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
```

**Why this list?** Works on Windows, Mac, iOS, Android.

### Sizes

| Level | Size | Weight | Use |
|-------|------|--------|-----|
| H1 | 32px | 700 Bold | Page title |
| H2 | 24px | 600 Bold | Section heading |
| H3 | 20px | 600 Bold | Subsection |
| Body | 16px | 400 | Normal text |
| Small | 14px | 400 | Secondary text, labels |
| Tiny | 12px | 400 | Captions, timestamps |

### Examples

```html
<h1 class="text-4xl font-bold">Page Title</h1>
<h2 class="text-2xl font-semibold">Section</h2>
<p class="text-base">Normal paragraph</p>
<p class="text-sm text-text-muted">Small text</p>
```

## Components

### Buttons

```html
<!-- Primary (Use for main actions) -->
<button class="bg-primary text-white px-4 py-2 rounded">
  Save Changes
</button>

<!-- Secondary (Use for less important actions) -->
<button class="border border-gray-300 text-text px-4 py-2 rounded hover:bg-gray-50">
  Cancel
</button>

<!-- Disabled -->
<button disabled class="bg-gray-300 text-gray-500 px-4 py-2 rounded cursor-not-allowed">
  Processing...
</button>
```

**Rule:** One primary action per section. Rest are secondary.

### Form Inputs

```html
<!-- Text Input -->
<input 
  type="email" 
  placeholder="you@example.com"
  class="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary"
/>

<!-- Textarea -->
<textarea 
  placeholder="Describe issue..."
  class="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-primary"
  rows="4"
></textarea>

<!-- Select -->
<select class="border border-gray-300 rounded px-3 py-2">
  <option>Pick one...</option>
  <option>Option 1</option>
</select>

<!-- Checkbox -->
<input type="checkbox" id="agree" />
<label for="agree">I agree to terms</label>

<!-- Radio -->
<input type="radio" name="priority" value="high" />
<label>High Priority</label>
```

**Best practices:**
- Always include labels: `<label for="email">Email</label>`
- Show validation errors: "Email must be valid"
- Use placeholder for hints, not labels
- Disable while loading

### Cards

```html
<div class="bg-surface rounded-lg border border-gray-200 p-4">
  <h3 class="text-lg font-semibold text-text mb-2">Card Title</h3>
  <p class="text-text-muted">Card content goes here</p>
</div>
```

### Lists

```html
<!-- Simple List -->
<ul class="space-y-2">
  <li class="text-text">Item 1</li>
  <li class="text-text">Item 2</li>
</ul>

<!-- Data Table -->
<table class="w-full border-collapse">
  <thead class="border-b border-gray-300">
    <tr>
      <th class="text-left py-2 px-4">Name</th>
      <th class="text-left py-2 px-4">Status</th>
    </tr>
  </thead>
  <tbody>
    <tr class="border-b border-gray-200 hover:bg-gray-50">
      <td class="py-2 px-4">John Doe</td>
      <td class="py-2 px-4"><span class="bg-green-100 text-green-800 px-2 py-1 rounded">Active</span></td>
    </tr>
  </tbody>
</table>
```

## Spacing & Layout

### Tailwind Spacing Scale
```
px-1  = 4px     px-2  = 8px     px-3  = 12px
px-4  = 16px    px-6  = 24px    px-8  = 32px
py-1  = 4px     py-2  = 8px     py-3  = 12px
py-4  = 16px    py-6  = 24px    py-8  = 32px
```

### Layout Examples

```html
<!-- Centered content with max-width -->
<div class="max-w-2xl mx-auto p-6">
  <h1>Centered Page</h1>
</div>

<!-- Two column grid -->
<div class="grid grid-cols-2 gap-6">
  <div>Column 1</div>
  <div>Column 2</div>
</div>

<!-- Flex row with space-between -->
<div class="flex justify-between items-center">
  <h2>Title</h2>
  <button>Action</button>
</div>

<!-- Mobile responsive -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  <!-- 1 column on mobile, 2 on tablet, 3 on desktop -->
</div>
```

## Icons

### Using Icons (When Available)

```html
<!-- If using Font Awesome or similar -->
<i class="fas fa-check text-green-500"></i>
<i class="fas fa-exclamation text-orange-500"></i>
<i class="fas fa-times text-red-500"></i>
```

Or create simple SVG icons:
```html
<svg class="w-5 h-5 text-primary" fill="currentColor" viewBox="0 0 20 20">
  <path d="..."></path>
</svg>
```

## Messages & Alerts

### Success Message
```html
<div class="bg-green-50 border border-green-200 rounded p-4">
  <p class="text-green-800">✓ Changes saved successfully</p>
</div>
```

### Error Message
```html
<div class="bg-red-50 border border-red-200 rounded p-4">
  <p class="text-red-800">✗ Email already exists</p>
</div>
```

### Warning Message
```html
<div class="bg-yellow-50 border border-yellow-200 rounded p-4">
  <p class="text-yellow-800">⚠ This action cannot be undone</p>
</div>
```

## Responsive Design

### Mobile-First Approach

```html
<!-- Default (mobile) = 1 column -->
<!-- md: = tablet = 2 columns -->
<!-- lg: = desktop = 3 columns -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
  <div>Card 1</div>
  <div>Card 2</div>
  <div>Card 3</div>
</div>
```

### Common Breakpoints
- **sm**: 640px (large phones)
- **md**: 768px (tablets)
- **lg**: 1024px (laptops)
- **xl**: 1280px (desktops)

```html
<!-- Hide on mobile, show on tablet+ -->
<div class="hidden md:block">Desktop only content</div>

<!-- Show on mobile, hide on desktop -->
<div class="md:hidden">Mobile menu</div>

<!-- Responsive text size -->
<h1 class="text-xl md:text-2xl lg:text-4xl">Title</h1>
```

## Accessibility (A11y)

### For Everyone

- **Keyboard navigation:** Tab through links/buttons, Enter to click
- **Color contrast:** Dark text on light = good, light on light = bad
- **Screen readers:** Describe images with `alt="..."`, use semantic HTML

### Quick Checklist

```html
<!-- ✓ Good: Image has description -->
<img src="ticket.png" alt="Ticket status icon" />

<!-- ✗ Bad: Image has no description -->
<img src="ticket.png" />

<!-- ✓ Good: Button has clear text -->
<button>Delete Ticket</button>

<!-- ✗ Bad: Generic button text -->
<button>Click Here</button>

<!-- ✓ Good: Form has associated label -->
<label for="email">Email</label>
<input id="email" type="email" />

<!-- ✗ Bad: No label connection -->
<input type="email" placeholder="Email" />
```

## Dark Mode

All components automatically work in dark mode (colors swap via CSS variables).

**Test both modes:**
1. Light mode: Click theme button, should switch
2. Dark mode: Text readable, contrast is good
3. Check images/icons visible in both

## Common Mistakes

❌ **Hardcoding colors**
```typescript
// Bad
style="color: blue; background: white;"
```

✅ **Use Tailwind classes**
```html
<!-- Good -->
<div class="text-primary bg-surface">...</div>
```

❌ **Inconsistent spacing**
```html
<!-- Bad: Different padding everywhere -->
<div class="p-2">...</div>
<div class="p-8">...</div>
<div class="p-1">...</div>
```

✅ **Consistent scale**
```html
<!-- Good: Use standard scale -->
<div class="p-4">...</div>
<div class="p-4 md:p-6">...</div>
```

❌ **Not responsive**
```html
<!-- Bad: Fixed width, breaks on phone -->
<div style="width: 1200px">...</div>
```

✅ **Responsive by default**
```html
<!-- Good: Adapts to screen size -->
<div class="max-w-4xl mx-auto">...</div>
```

## Testing Your Design

1. **Light & Dark mode** - Click theme button, everything readable?
2. **Mobile** - Resize browser to 375px, layout stacks nicely?
3. **Keyboard** - Tab through page, all buttons accessible?
4. **Contrast** - Use Chrome DevTools, check contrast ratio ≥ 4.5:1
5. **Different browsers** - Chrome, Firefox, Safari all look same?