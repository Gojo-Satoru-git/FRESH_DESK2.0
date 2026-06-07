# Authentication Setup

## Why Authentication Matters

Fresh Desk handles sensitive HR data—tickets, employees, salaries, reviews. We need:
- ✅ Only the right person can view their data
- ✅ Agents can't access customer data (and vice versa)
- ✅ Passwords are never stored in the browser
- ✅ Sessions don't expire randomly (JWT tokens)

That's what this guide covers.

## The Login Flow (Step by Step)

### 1. User Types Email & Password
The **LoginComponent** validates in real-time:
- Email must be valid format (`user@example.com`)
- Password must be 6+ characters

### 2. We Send to Backend
```typescript
// src/app/features/auth/login.component.ts
this.authService.login(email, password)
  .subscribe(response => {
    // response contains JWT token
    localStorage.setItem('auth_token', response.token);
    this.router.navigate(['/agent/dashboard']);
  });
```

### 3. Backend Validates Password
Backend checks database, compares hashed password, returns JWT token if valid.

**Why JWT?** 
- No need to store session on server
- Token expires after N hours (user logs out automatically)
- Works with mobile apps, single-page apps, microservices

### 4. We Store Token Locally
JWT lives in browser's `localStorage` with key `auth_token`.

**Risk?** If hacker accesses computer, they get the token. That's why:
- We only send tokens over HTTPS (encrypted)
- Tokens expire after 24 hours
- Sensitive operations need re-authentication

### 5. Every API Request Adds Token
**JwtInterceptor** automatically adds the token to all requests:
```
Authorization: Bearer <token_here>
```
Backend validates token, returns data if valid.

### 6. If Token Expired or Missing
**AuthGuard** redirects to `/login`. User logs in again.

## File Structure

```
src/app/
├── features/auth/
│   ├── login.component.ts       ← Main login form
│   ├── login.component.html     ← Template (email, password inputs)
│   ├── signup.component.ts      ← Registration form
│   └── signup.component.css
│
├── core/auth/
│   ├── auth.service.ts          ← Login logic, token storage
│   ├── jwt.interceptor.ts       ← Adds token to every request
│   └── auth.guard.ts            ← Protects routes (/agent/*)
```

## Environment Configuration

### Why Two Environments?

**Development** (`npm start`)
- Backend runs locally at `https://localhost:5001`
- Self-signed SSL certificate (browser shows warning, ignore it)
- Good for testing with hot-reload

**Production** (`npm run build`)
- Backend at `https://api.adrenalin-support.com`
- Real SSL certificate from Let's Encrypt
- Minified code, smaller file size

### Files

```typescript
// Development (auto-selected by ng serve)
src/environments/environment.development.ts
export const environment = {
  apiUrl: 'https://localhost:5001'
};

// Production (auto-selected by ng build)
src/environments/environment.ts
export const environment = {
  apiUrl: 'https://api.adrenalin-support.com'
};
```

## How to Use in Components

```typescript
import { AuthService } from './core/auth/auth.service';

export class SomeComponent {
  constructor(private auth: AuthService) {}

  logout() {
    this.auth.logout(); // Clears token, redirects to /login
  }

  getCurrentUser() {
    return this.auth.currentUser(); // Returns logged-in user
  }
}
```

## Common Tasks

### Check If User Is Logged In
```typescript
this.auth.isAuthenticated() // true/false
```

### Get User Info from Token
```typescript
const user = this.auth.getCurrentUser();
console.log(user.email, user.role); // e.g., "agent" or "customer"
```

### Manual Logout
```typescript
this.auth.logout(); // Clears token, redirects to /login
```

## Token Storage Security

| Approach | Pros | Cons |
|----------|------|------|
| **localStorage** (current) | Simple, works on refresh | XSS attacks can steal it |
| **sessionStorage** | Clears on tab close | Still vulnerable to XSS |
| **HttpOnly Cookie** | Inaccessible to JavaScript | More complex to manage |

**We use localStorage because:** It's the standard for SPAs, easy to debug, and we use HTTPS + token expiration to mitigate risks.

## Troubleshooting

### "Unauthorized" API Error
- Token expired → User logs out automatically, redirected to `/login`
- Token invalid → Check backend is running, check JWT format

### Login Button Doesn't Work
- Check browser console (F12 → Console tab) for errors
- Verify backend is running at `https://localhost:5001`
- Try in private/incognito window (no conflicting browser extensions)

### Stuck on Login Loop
- Clear localStorage: `localStorage.clear()` in console
- Try different browser
- Check that backend auth endpoint is accessible
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
