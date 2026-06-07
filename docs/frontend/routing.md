# Routing Guide

## Why Routing Matters

Without routing, Fresh Desk would be one huge page. Routing lets us:
- 📄 Show different pages (login, dashboard, tickets, etc.)
- 🔗 Use clean URLs (`/agent/dashboard` instead of `?page=dashboard`)
- ⚡ Load features on-demand (lazy-load)
- 🔐 Protect routes (block unauthenticated users)

## The Big Picture

```
User clicks link or types URL
      ↓
Angular Router checks app.routes.ts
      ↓
Does this route have a guard? (e.g., AuthGuard)
  ├─ YES → Run the guard (check if logged in)
  │ ├─ Passed → Load the component
  │ └─ Failed → Redirect to /login
  └─ NO → Load the component immediately
```

## Route Structure

### File Location
```
src/app/app.routes.ts  ← All route definitions
```

### How Routes Work

```typescript
{
  path: 'login',
  loadComponent: () => import('./features/auth/login.component')
                      .then(m => m.LoginComponent)
  // No guard needed - public page
}

{
  path: 'agent',
  canActivate: [AuthGuard],  ← Only logged-in users
  loadComponent: () => import('./layouts/agent-layout/...')
                      .then(m => m.AgentLayoutComponent),
  children: [
    {
      path: 'dashboard',
      loadComponent: () => import('./features/agent-dashboard/...')
    }
  ]
}
```

**What this means:**
- User visits `/agent/dashboard`
- Router checks: "Is this user logged in?" (AuthGuard)
- If YES: Load & show AgentLayoutComponent + DashboardComponent
- If NO: Redirect to `/login`

## Route Types

### Public Routes (No Login Required)
```
/login         → LoginComponent
/signup        → SignupComponent
```

**Use case:** First-time users, login/signup pages

### Protected Routes (Login Required)

#### Agent Routes
```
/agent/dashboard    → Agent workspace
/agent/tickets      → Ticket list
/agent/contacts     → Contact list
/agent/kb           → Knowledge base
/agent/reports      → Analytics
```

**Guard:** `AuthGuard` checks JWT token + user role = 'agent'

#### Customer Routes
```
/customer/portal           → Customer dashboard
/customer/raise-ticket     → Create new ticket
/customer/my-tickets       → View own tickets
```

**Guard:** `AuthGuard` checks JWT token + user role = 'customer'

#### Admin Routes
```
/admin/panel        → Admin settings
/admin/users        → User management
```

**Guard:** `AuthGuard` checks JWT token + user role = 'admin'

## Adding a New Route

### Step 1: Create Component
```bash
# Generate component
ng generate component features/my-feature/my-feature

# Manually create
src/app/features/my-feature/my-feature.component.ts
```

### Step 2: Add Route
Edit `src/app/app.routes.ts`:
```typescript
{
  path: 'agent',
  canActivate: [AuthGuard],
  loadComponent: () => import('./layouts/agent-layout/...'),
  children: [
    {
      path: 'my-feature',  ← New route!
      loadComponent: () => import('./features/my-feature/my-feature.component')
                          .then(m => m.MyFeatureComponent)
    }
  ]
}
```

### Step 3: Navigate to It
```html
<!-- In a template -->
<a routerLink="/agent/my-feature">Go to My Feature</a>
```

or

```typescript
// In TypeScript
constructor(private router: Router) {}

goToMyFeature() {
  this.router.navigate(['/agent/my-feature']);
}
```

## Lazy Loading (Why It Matters)

### Without Lazy Loading
```
Download MyFeatureComponent at startup
  ↓
App is 5MB, takes 10 seconds to load
  ↓
User opens dashboard, waits 10 seconds
  ↓
😡 Bad UX
```

### With Lazy Loading (Current)
```
Startup: Download only login page (200KB, fast)
  ↓
User logs in, navigates to /agent/tickets
  ↓
Router downloads TicketComponent on-demand (100KB)
  ↓
Component loads while user waits 1-2 seconds
  ↓
😊 Better UX
```

**Result:** Initial load ~5x faster, users don't notice component load times.

## Route Guards

### AuthGuard (Check If Logged In)
```typescript
// src/app/core/guards/auth.guard.ts

import { CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const token = localStorage.getItem('auth_token');
  
  if (token) {
    return true;  ← User logged in, allow access
  } else {
    return router.createUrlTree(['/login']);  ← Redirect to login
  }
};
```

### How to Add to a Route
```typescript
{
  path: 'agent',
  canActivate: [authGuard],  ← Add this
  loadComponent: () => import(...)
}
```

### Custom Guard Example (Admin Only)
```typescript
export const adminGuard: CanActivateFn = (route, state) => {
  const user = authService.getCurrentUser();
  
  if (user?.role === 'admin') {
    return true;  ← Is admin
  } else {
    return router.createUrlTree(['/login']);  ← Not admin, redirect
  }
};
```

## Nested Routes (Child Routes)

### What Are Nested Routes?

Agent layout has a sidebar + main area:
```
┌─────────────────────────────┐
│ AGENT LAYOUT                │
│ ┌──────────┬───────────────┐│
│ │ Sidebar  │  Main Content ││
│ │          │               ││
│ │          │  <router-     │
│ │          │   outlet>     │
│ └──────────┴───────────────┘│
└─────────────────────────────┘
```

Nested routes make this work:

```typescript
{
  path: 'agent',
  loadComponent: () => import('./layouts/agent-layout/...')
                      .then(m => m.AgentLayoutComponent),
  children: [  ← These render inside layout's <router-outlet>
    {
      path: 'dashboard',
      loadComponent: () => import('./features/agent-dashboard/...')
    },
    {
      path: 'tickets',
      loadComponent: () => import('./features/tickets/...')
    },
    {
      path: 'contacts',
      loadComponent: () => import('./features/contacts/...')
    }
  ]
}
```

**Result:**
- `/agent` loads AgentLayoutComponent
- `/agent/dashboard` loads dashboard INSIDE layout's `<router-outlet>`
- `/agent/tickets` loads tickets INSIDE layout's `<router-outlet>`

## Route Parameters (Dynamic Routes)

### Show Single Ticket by ID

```typescript
{
  path: 'agent',
  children: [
    {
      path: 'tickets/:id',  ← :id is a parameter (e.g., /tickets/123)
      loadComponent: () => import('./features/ticket-detail/...')
    }
  ]
}
```

### Get Parameter in Component
```typescript
import { ActivatedRoute } from '@angular/router';

export class TicketDetailComponent {
  ticketId: string;

  constructor(private route: ActivatedRoute) {
    this.route.params.subscribe(params => {
      this.ticketId = params['id'];  ← Get :id parameter
      this.loadTicket(this.ticketId);
    });
  }
}
```

### Navigate with Parameter
```typescript
this.router.navigate(['/agent/tickets', ticketId]);  ← /agent/tickets/123
```

## Troubleshooting

### Route Not Loading / Blank Page
1. Check browser console (F12) for errors
2. Verify route exists in `app.routes.ts`
3. Check guard logic (might be redirecting to `/login`)
4. Verify component import path is correct

### AuthGuard Redirects to Login Constantly
- Token might be expired
- Token might be malformed
- Backend endpoint changed

**Debug:** Open DevTools → Application → localStorage → Check `auth_token` value

### Lazy Loading Not Working
- Check syntax: `.then(m => m.ComponentName)`
- Verify component is standalone: `@Component({ standalone: true })`
- Check console for import errors