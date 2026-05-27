# Authentication Setup

## Overview
The authentication system for Fresh Desk 2.0 is built around a standalone Angular `LoginComponent` with reactive forms, form validation, and theme support. The component communicates with the backend API for user authentication.

## Login Component Architecture

### File Structure
```
src/app/features/auth/
├── login.component.ts
├── login.component.html
├── login.component.css (if needed)
└── login.spec.ts (tests)
```

### Component Details

#### LoginComponent (`login.component.ts`)
- **Selector**: `app-login`
- **Standalone**: Yes (uses standalone API)
- **Imports**: 
  - `ReactiveFormsModule` - For reactive form handling
  - `ThemeSwitcherComponent` - For theme toggle in top-right corner

#### Form Structure
```typescript
loginForm = new FormGroup({
  email: new FormControl('', [Validators.required, Validators.email]),
  password: new FormControl('', [Validators.required, Validators.minLength(6)])
});
```

**Validation Rules:**
- **Email**: Required, must be valid email format
- **Password**: Required, minimum 6 characters

#### State Management
The component uses Angular Signals for reactive state:
```typescript
isLoading = signal<boolean>(false);        // Loading state during API call
errorMessage = signal<string | null>(null); // Error notifications
successMessage = signal<string | null>(null); // Success notifications
```

### Form Submission Flow
1. Validates form before submission
2. Marks all fields as touched (shows validation errors)
3. Sets `isLoading` to true
4. Calls backend API via HTTP
5. Handles success/error responses
6. Updates corresponding signal

## Environment Configuration

### Files Location
```
src/environments/
├── environment.ts           # Production environment
└── environment.development.ts  # Development environment
```

### Development Environment (`environment.development.ts`)
```typescript
export const environment = {
    production: false,
    apiUrl: 'https://localhost:5001'
};
```
- **Use**: Local development with .NET backend running on port 5001
- **HTTPS**: Yes, with self-signed certificates for local dev

### Production Environment (`environment.ts`)
```typescript
export const environment = {
    production: true,
    apiUrl: 'https://api.adrenalin-support.com'
};
```
- **Use**: Production deployment
- **Base URL**: Update to your actual production API domain

### Usage in Components
```typescript
import { environment } from '../../environments/environment';

// Use environment.apiUrl in HTTP calls
this.http.post(`${environment.apiUrl}/auth/login`, credentials)
```

## Angular Environment Selection
Angular CLI automatically selects the correct environment based on build mode:
- **Development**: `ng serve` uses `environment.development.ts`
- **Production**: `ng build` uses `environment.ts`

### File Replacement Configuration
Configured in `angular.json`:
```json
"fileReplacements": [
  {
    "replace": "src/environments/environment.ts",
    "with": "src/environments/environment.development.ts"
  }
]
```

## Theme Integration

### ThemeSwitcher Component
Located in the login page's top-right corner via `<app-theme-switcher>` component.

**Functionality**:
- Toggles between light and dark theme
- Persists user preference in localStorage
- Uses Angular Signals for reactive theme state

### Theme Service (`core/theme/theme.service.ts`)
```typescript
export class ThemeService {
  currentTheme = signal<Theme>(...);
  
  toggleTheme() { ... }
  setTheme(theme: Theme) { ... }
}
```

### CSS Custom Variables
Defined in `src/styles.css`:
```css
:root {
  --color-primary: 37 99 235;
  --color-background: 249 250 251;
  --color-text: 17 24 39;
  /* Light theme variables */
}

.dark {
  --color-primary: 59 130 246;
  --color-background: 17 24 39;
  --color-text: 243 244 246;
  /* Dark theme variables */
}
```

## UI Components & Styling

### Tailwind CSS Integration
The login form uses Tailwind CSS with custom color utilities:
- `bg-background` - Background color (light: gray-50, dark: gray-900)
- `text-text` - Text color (light: gray-900, dark: gray-100)
- `text-text-muted` - Muted text (light: gray-500, dark: gray-400)
- `bg-surface` - Card/surface color (light: white, dark: gray-800)

### Login Card Layout
- **Container**: Centered, responsive (min-height: screen)
- **Card**: Max-width 448px, rounded corners, shadow
- **Spacing**: Consistent padding and gap

### Form Fields
- Email and Password inputs with validation feedback
- Real-time error messages
- Placeholder text: "test@adrenalin.com"
- Focus states with ring styling

### Status Messages
- **Error**: Red background with left border
- **Success**: Green background with left border
- Both support dark mode styling

## API Integration

### Backend Endpoint
The login form submits to: `POST /auth/login`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Expected Response (Success)**:
```json
{
  "token": "jwt_token_here",
  "user": { "id": "...", "name": "...", "email": "..." }
}
```

**Expected Response (Error)**:
```json
{
  "error": "Invalid credentials"
}
```

## Development Workflow

### Running Locally
```bash
# Terminal 1: Start Angular dev server
npm start
# Access at http://localhost:4200

# Terminal 2: Start .NET backend
# On port 5001
```

### Testing the Login
1. Visit `http://localhost:4200`
2. Enter test email: `test@adrenalin.com`
3. Enter test password: (6+ characters)
4. Click "Sign in"
5. Test theme switcher in top-right corner

### Debugging
- Use `ng serve --verbose` for detailed build info
- Check browser console for API errors
- Verify .NET backend is running on port 5001
- Ensure environment files have correct URLs

## Security Considerations

### Current Implementation
- Passwords are sent via HTTPS
- Form validation prevents submission of invalid data
- Reactive forms prevent XSS through Angular's sanitization

### Future Enhancements
- Store JWT token securely (HttpOnly cookies recommended)
- Implement token refresh logic
- Add CSRF protection
- Implement password strength indicator
- Add two-factor authentication support

## Related Documentation
- [Theme Setup](../common/theme-setup.md)
- [Folder Structure](./folder-structure.md)
- [API Design](../backend/api-design.md)
- [Environment Variables](../deployment/environment-variables.md)
