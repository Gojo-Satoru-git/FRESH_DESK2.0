# Frontend Getting Started Guide

## Quick Start

### Prerequisites
- **Node.js**: v18+ (comes with npm)
- **.NET Backend**: Running on `https://localhost:5001` (development only)
- **VS Code**: Recommended IDE

### Installation (5 minutes)
```bash
# Navigate to frontend directory
cd frontend/fresh_desk_2.0

# Install dependencies
npm install

# Start development server
npm start
```

✅ **Done!** Your application is running at `http://localhost:4200`

## First Run Checklist

### Before Starting (`npm start`)
- [ ] Backend .NET API is running on `https://localhost:5001`
- [ ] No other process using port 4200
- [ ] Node.js and npm installed (`npm -v`, `node -v`)
- [ ] Dependencies installed (`npm install` completed)

### After Starting
- [ ] Browser opens at `http://localhost:4200`
- [ ] Login page displays correctly
- [ ] Theme switcher visible in top-right corner
- [ ] API calls go to `https://localhost:5001` (check Network tab)

## Understanding the Project

### Key Files
| File | Purpose |
|------|---------|
| `package.json` | Project metadata, dependencies, scripts |
| `angular.json` | Angular build configuration |
| `tailwind.config.js` | Tailwind CSS configuration |
| `tsconfig.json` | TypeScript configuration |
| `src/main.ts` | Application entry point |
| `src/app/app.routes.ts` | Routing definitions |
| `src/styles.css` | Global styles & theme system |

### Directory Structure
```
fresh_desk_2.0/
├── src/
│   ├── app/              # Application components & services
│   │   ├── features/     # Feature modules (auth, tickets, etc.)
│   │   ├── core/         # Singleton services (theme, auth)
│   │   ├── shared/       # Reusable components & utilities
│   │   └── layouts/      # Layout templates
│   ├── assets/           # Static files
│   ├── environments/     # Environment configs
│   ├── styles.css        # Global styles
│   └── main.ts           # Entry point
├── public/               # Static root files
├── dist/                 # Production build output
├── node_modules/         # Dependencies
├── angular.json          # Angular CLI config
├── tailwind.config.js    # Tailwind config
└── package.json          # Project metadata
```

## Available Commands

### Development
```bash
# Start development server with hot reload
npm start

# Run in watch mode (continuous compilation)
npm run watch

# Run tests in watch mode
npm test
```

### Production
```bash
# Build for production
npm run build

# Output goes to dist/
```

## Configuration

### Environment Setup

#### Development (automatic on `npm start`)
```typescript
// src/environments/environment.development.ts
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
