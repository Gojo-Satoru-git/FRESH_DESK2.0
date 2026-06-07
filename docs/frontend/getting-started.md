# Getting Started

## Before You Start (2 minutes)

Make sure you have:
- **Node.js 18+** ([Download](https://nodejs.org/)) — comes with npm
- **.NET backend running** on `https://localhost:5001` — frontend won't work without it
- **VS Code** ([Download](https://code.visualstudio.com/)) — optional but recommended
- **Git** — already used it to clone the repo

**Check your setup:**
```bash
node -v      # Should be 18+
npm -v       # Should be 8+
```

## Installation (5 minutes)

### Step 1: Navigate to Frontend
```bash
cd frontend/fresh_desk_2.0
```

### Step 2: Install Dependencies
```bash
npm install
```
This downloads ~300MB of libraries. ☕ Grab a coffee, it takes 1-3 minutes on first run.

### Step 3: Start Dev Server
```bash
npm start
```

**Expected output:**
```
✔ Browser application bundle generation complete. (45.2 seconds)
✔ Compiled successfully.
```

Browser automatically opens `http://localhost:4200` → You see login page ✅

## After Setup

### First Test: Can I Login?

1. Make sure **.NET backend** is running on `https://localhost:5001`
2. Open DevTools: `F12` → **Network** tab
3. Try logging in with test credentials
4. In Network tab, look for POST request to `/api/auth/login`
5. Should return `200 OK` with a JWT token

**Not working?** → See Troubleshooting section below

## Understanding the Project Structure

### What's in Each Folder?

```
fresh_desk_2.0/
│
├── src/                      ← All your code
│   ├── app/                  ← Angular components & services
│   ├── environments/         ← API URLs (dev vs prod)
│   ├── styles.css            ← Global colors & theme
│   └── main.ts               ← Startup file
│
├── public/                   ← Static files (favicon, robots.txt)
├── node_modules/             ← Downloaded libraries (don't edit!)
├── dist/                     ← Production build output (generated)
│
├── package.json              ← Project metadata (dependencies)
├── angular.json              ← Build configuration
├── tailwind.config.js        ← CSS configuration
├── tsconfig.json             ← TypeScript rules
└── README.md                 ← Original project README
```

### Key Files You'll Edit

| File | What It Does | Example |
|------|--------------|---------|
| `src/app/features/auth/login.component.ts` | Login page logic | Form validation, API call |
| `src/app/app.routes.ts` | Page routing | `/agent/dashboard` → DashboardComponent |
| `src/styles.css` | Global colors | Change primary color from blue to green |
| `src/environments/environment.*.ts` | API URLs | Backend host/port |

**Don't edit:**
- `node_modules/` — Auto-generated, will be lost
- `dist/` — Generated on build, overwritten

## Available Commands

### Development (What You'll Use)

```bash
npm start
# Starts dev server at http://localhost:4200
# Hot reload: Save a file → page updates automatically
# Perfect for building features
```

```bash
npm test
# Runs unit tests
# Watches for file changes, re-runs tests
```

### Production Build

```bash
npm run build
# Creates optimized build in ./dist/
# Minified, tree-shaken, ~50KB smaller
# Used for deployment
```

```bash
npm run watch
# Continuous compilation without dev server
# Useful if you have your own server
```

## Configuration

### Development vs Production

**How does Angular know which environment to use?**

When you run `npm start`:
```
angular.json → Check "serve" configuration
  → It says: "Use environment.development.ts"
  → environment.development.ts says: "API is at https://localhost:5001"
  → All HTTP calls go there ✓
```

When you run `npm run build`:
```
angular.json → Check "build" configuration
  → It says: "Use environment.ts"
  → environment.ts says: "API is at https://api.adrenalin-support.com"
  → All HTTP calls go there ✓
```

**Can I use production API while developing?**
Yes, edit `src/environments/environment.development.ts`:
```typescript
export const environment = {
  apiUrl: 'https://api.adrenalin-support.com'  // ← Change this
};
```

## Troubleshooting

### "Port 4200 Already in Use"
```bash
# Find what's using port 4200
netstat -ano | findstr :4200

# Kill it (replace PID with the number shown)
taskkill /PID 12345 /F

# Try again
npm start
```

### "Cannot Find Module..."
```bash
# Clear and reinstall
npm install

# Still broken? Nuclear option
rm -r node_modules
npm install
npm start
```

### "API calls are failing / 404 errors"
Check 3 things:
1. **Backend running?** → Should see output in backend terminal
2. **Port correct?** → Check `environment.development.ts` says `localhost:5001`
3. **Endpoint exists?** → Try `https://localhost:5001/health` in browser
4. **DevTools Network tab** → See actual request & response

### "Login page appears blank / white screen"
```bash
# Clear browser cache
Ctrl+Shift+Del  # (or Cmd+Shift+Del on Mac)

# Clear local dev data
localStorage.clear()  # Run in DevTools console

# Hard refresh
Ctrl+Shift+R  # (or Cmd+Shift+R on Mac)
```

### Browser Extension Conflicts
Try private/incognito window. If it works, a browser extension is interfering.

## Next: Start Coding

1. Read [Architecture](./architecture.md) to understand the design
2. Read [Routing](./routing.md) to add new pages
3. Read [Authentication](./authentication-setup.md) to understand login flow
4. Pick a feature and start hacking! 🚀
export const environment = {
    production: false,
    apiUrl: 'https://localhost:5001'
};
```
- Used for: Local development
- Backend: Your local .NET API on port 5001
- HTTPS: Yes (with self-signed certificate)

#### Production (`npm run build` uses this)
```typescript
// src/environments/environment.ts
export const environment = {
    production: true,
    apiUrl: 'https://api.adrenalin-support.com'
};
```
- Used for: Production deployment
- Backend: Your production API domain
- HTTPS: Yes (with valid certificate)

### Switching Environments
Angular automatically selects based on build command:
```bash
npm start                                  # Uses development environment
npm start -- --configuration production   # Uses production environment
npm run build                              # Uses production environment
npm run build -- --configuration development # Uses development environment
```

## Features Overview

### 1. Login Page
- **Location**: `src/app/features/auth/login.component.ts`
- **Features**: 
  - Email/password validation
  - Real-time error messages
  - Loading state during submission
  - Theme switcher button
  - Responsive design

### 2. Theme System
- **Service**: `src/app/core/theme/theme.service.ts`
- **Features**:
  - Light/dark mode switching
  - Persistent user preference (localStorage)
  - CSS variables for dynamic colors
  - Tailwind CSS integration
- **Usage**: Click moon/sun icon in top-right corner

### 3. Responsive Design
- All components built mobile-first
- Tailwind CSS for responsive utilities
- Max-width containers for large screens

### 4. Type Safety
- Full TypeScript configuration
- Strict mode enabled
- Type-safe reactive forms

## Common Tasks

### Testing the Login Form
1. Navigate to `http://localhost:4200`
2. Try submitting empty form → See validation errors
3. Enter invalid email → See email validation error
4. Enter valid email & password (6+ chars) → Success or API error
5. Check Network tab to see API calls

### Switching Theme
1. Look for moon icon in top-right corner
2. Click to toggle between light and dark
3. Refresh page → Theme persists
4. Open DevTools → Check localStorage for `app_theme`

### Debugging API Calls
1. Open DevTools (F12)
2. Go to Network tab
3. Submit login form
4. Check POST request to `/auth/login`
5. Verify request goes to `https://localhost:5001`

### Inspecting Component State
```typescript
// In browser console, if component exported:
import { LoginComponent } from './app/features/auth/login.component';
component.loginForm.value // Check form values
component.isLoading()      // Check loading state
component.errorMessage()   // Check error state
```

## Styling & Customization

### Using Tailwind CSS
All styling uses Tailwind utilities:
```html
<!-- Examples from login page -->
<div class="min-h-screen flex items-center justify-center bg-background">
  <!-- Responsive, centered, light gray background -->
</div>

<button class="bg-primary hover:bg-primary-hover text-white px-4 py-2 rounded-lg">
  <!-- Blue button with hover effect and rounded corners -->
</button>
```

### Custom Colors
Colors defined in `src/styles.css` and available via Tailwind:
```html
<div class="bg-background">          <!-- Light gray (dev) or dark gray (prod) -->
<p class="text-text">                 <!-- Dark text (dev) or light text (prod) -->
<p class="text-text-muted">           <!-- Muted text color -->
<button class="bg-primary">           <!-- Primary blue color -->
```

### Adding Custom Styles
Create component-level styles:
```typescript
@Component({
  selector: 'app-example',
  template: `<div class="custom-class">Content</div>`,
  styles: [`
    .custom-class {
      color: var(--color-text);
      padding: 1rem;
    }
  `]
})
```

Or add global styles to `src/styles.css` in `@layer components` section.

## Troubleshooting

### Port 4200 Already in Use
```bash
# Windows: Find process using port 4200
netstat -ano | findstr :4200

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Then start dev server again
npm start
```

### Build Errors
```bash
# Clear cache and reinstall dependencies
rm -r node_modules
npm install

# Or use clean cache install
npm ci

# Then rebuild
npm start
```

### API Connection Failures
1. Check backend is running on `https://localhost:5001`
2. Verify environment file has correct URL
3. Check browser console for CORS errors
4. Verify SSL certificate is trusted
5. Disable browser cache during testing

### Theme Not Switching
1. Open DevTools → Application tab
2. Check localStorage has `app_theme` key
3. Verify `.dark` class appears on `<html>` element when toggled
4. Check `tailwind.config.js` has `darkMode: 'class'`

### Dependencies Issues
```bash
# Update to latest compatible versions
npm update

# Or update specific package
npm install @angular/core@latest

# Check for vulnerabilities
npm audit

# Fix vulnerabilities automatically
npm audit fix
```

## Development Best Practices

### Code Organization
- ✅ Use standalone components
- ✅ Keep components focused (single responsibility)
- ✅ Use services for shared logic
- ✅ Use Signals for state management
- ✅ Type everything (avoid `any`)

### Styling Guidelines
- ✅ Use Tailwind utilities instead of custom CSS
- ✅ Use CSS variables for theme colors
- ✅ Implement dark mode support
- ✅ Keep responsive design in mind
- ✅ Test on multiple screen sizes

### Form Handling
- ✅ Use reactive forms with FormGroup/FormControl
- ✅ Validate on blur and submit
- ✅ Show meaningful error messages
- ✅ Disable submit button during submission
- ✅ Handle API errors gracefully

### Performance
- ✅ Use standalone components (tree-shaking)
- ✅ Lazy load routes
- ✅ Minimize bundle size
- ✅ Use production builds for testing
- ✅ Monitor performance with DevTools

## Next Steps

### After Getting Familiar
1. **Create a new feature**: Add a new route and component
2. **Expand login**: Add "Forgot Password" or "Sign Up" functionality
3. **Add routing guards**: Protect routes with authentication
4. **Connect to backend**: Implement actual API integration
5. **Add more tests**: Increase test coverage

### Learning Resources
- [Angular Documentation](https://angular.io/docs)
- [Tailwind CSS Guide](https://tailwindcss.com/docs)
- [TypeScript Handbook](https://www.typescriptlang.org/docs)
- [Frontend Architecture](./architecture.md)
- [Authentication Setup](./authentication-setup.md)
- [Theme System](./theme-system.md)

## Getting Help

### Debugging
1. Use browser DevTools (F12)
2. Check console for errors
3. Use Network tab to verify API calls
4. Inspect elements for styling issues
5. Check Application tab for localStorage

### Common Issues
- Review [Troubleshooting](#troubleshooting) section above
- Check [Frontend Architecture](./architecture.md) for detailed explanations
- Review related documentation in `/docs` folder

### Project Documentation
- **Architecture**: [architecture.md](./architecture.md)
- **Authentication**: [authentication-setup.md](./authentication-setup.md)
- **Styling**: [theme-system.md](./theme-system.md)
- **Environment**: [environment-configuration.md](../deployment/environment-configuration.md)
- **Folder Structure**: [folder-structure.md](./folder-structure.md)

## Running Production Build Locally

### Build for Production
```bash
npm run build
# Creates dist/ folder with optimized output
```

### Serve Production Build
```bash
# Using npx
npx http-server dist/

# Or using other servers
python -m http.server 8000
# Then visit http://localhost:8000
```

### Key Differences from Development
- ✅ Code minified and tree-shaken
- ✅ Smaller bundle size (~40% smaller)
- ✅ Faster load times
- ✅ Production API URL used
- ✅ Change detection optimized
- ✅ Console warnings removed

## Summary

You now have a fully functional Angular 21 application with:
- ✅ Authentication page with validation
- ✅ Theme system with dark/light mode
- ✅ Environment-aware configuration
- ✅ Tailwind CSS styling
- ✅ Type-safe development
- ✅ Development and production builds

**Next**: Read the [Authentication Setup](./authentication-setup.md) guide to understand how to extend the login functionality and connect to your backend API.

---

**Questions?** Review the documentation in the `/docs` folder or check the inline code comments.
