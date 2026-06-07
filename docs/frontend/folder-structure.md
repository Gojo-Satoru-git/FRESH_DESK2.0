# Frontend Folder Structure

## The Mental Model

Think of the folder structure like organizing a company:

```
Company (Angular App)
  ├── Executive Team (core/)
  │   ├── HR (auth/)          - Hiring, authentication
  │   ├── Finance (theme/)    - Budget, colors
  │   └── IT (http/)          - Infrastructure
  │
  ├── Departments (features/)
  │   ├── Sales (tickets/)    - Manage sales tickets
  │   ├── HR (auth/)          - Employee login
  │   ├── Operations (reports/)
  │   └── ...
  │
  ├── Shared Services (shared/)
  │   ├── Accounting          - Shared calculations
  │   ├── Legal               - Shared contracts
  │   └── ...
  │
  └── Reception (layouts/)
      ├── Sales Office Layout
      ├── HR Office Layout
      └── ...
```

Each folder has a job. Let's explore.

## Complete Structure with Explanations

```
src/
│
├── main.ts                          ← App startup (don't touch unless you know what you're doing)
├── app.ts                           ← Root component (renders layout + router-outlet)
├── app.config.ts                    ← Angular configuration
├── app.routes.ts                    ← ALL routing definitions (add routes here)
├── styles.css                       ← Global colors + Tailwind setup (change primary color here)
├── index.html                       ← Entry HTML (has <app-root>)
│
├── app/
│   │
│   ├── core/                        ← Singleton services (one instance per app lifetime)
│   │   ├── auth/
│   │   │   ├── auth.service.ts      ← Login, logout, JWT token management
│   │   │   ├── jwt.interceptor.ts   ← Automatically adds token to API requests
│   │   │   └── auth.guard.ts        ← Blocks unauthenticated users from routes
│   │   │
│   │   ├── theme/
│   │   │   ├── theme.service.ts     ← Dark/light mode logic
│   │   │   └── theme-switcher.component.ts  ← The toggle button (top-right corner)
│   │   │
│   │   ├── guards/
│   │   │   ├── auth.guard.ts        ← Check: is user logged in?
│   │   │   └── role.guard.ts        ← Check: does user have admin role? (future)
│   │   │
│   │   ├── http/
│   │   │   └── http.service.ts      ← Wrapper around Angular HttpClient (optional)
│   │   │
│   │   ├── state/
│   │   │   └── (global state services go here)
│   │   │
│   │   └── validators/
│   │       ├── password-match.validator.ts  ← Signup form validation
│   │       └── (custom validators here)
│   │
│   ├── features/                    ← Feature modules (lazy-loaded)
│   │   │
│   │   ├── auth/                    ← Authentication (login, signup)
│   │   │   ├── login.component.ts
│   │   │   ├── login.component.html
│   │   │   ├── login.component.css
│   │   │   ├── signup.component.ts
│   │   │   └── signup.component.css
│   │   │
│   │   ├── agent-dashboard/         ← Agent main page
│   │   │   ├── agent-dashboard.component.ts
│   │   │   └── agent-dashboard.component.css
│   │   │
│   │   ├── tickets/                 ← Ticket management
│   │   │   ├── components/
│   │   │   │   ├── ticket-list.component.ts      ← Show all tickets
│   │   │   │   ├── ticket-detail.component.ts    ← View single ticket
│   │   │   │   └── ticket-create.component.ts    ← Create new
│   │   │   │
│   │   │   ├── models/
│   │   │   │   ├── ticket.model.ts  ← TypeScript interfaces for Ticket type
│   │   │   │   └── ticket-filter.model.ts
│   │   │   │
│   │   │   └── services/
│   │   │       └── ticket.service.ts ← API calls, state management for tickets
│   │   │
│   │   ├── contacts/                ← Contact/customer management
│   │   │   ├── components/
│   │   │   │   └── contact-list.component.ts
│   │   │   ├── models/
│   │   │   └── services/
│   │   │
│   │   ├── customer-portal/         ← Customer self-service
│   │   │   ├── customer-portal.component.ts
│   │   │   ├── customer-header.component.ts
│   │   │   ├── raise-ticket.component.ts    ← Create ticket as customer
│   │   │   └── ticket-details.component.ts  ← View own ticket
│   │   │
│   │   ├── knowledgebase/           ← FAQ/docs for customers
│   │   │   └── components/
│   │   │       └── knowledge-base.component.ts
│   │   │
│   │   ├── reports/                 ← Analytics & dashboards
│   │   │   └── components/
│   │   │
│   │   └── admin-panel/             ← Settings, user management
│   │       └── (admin components)
│   │
│   ├── layouts/                     ← UI shells (sidebar, header, footer)
│   │   ├── agent-layout/
│   │   │   ├── agent-layout.component.ts    ← Wrapper for /agent/* routes
│   │   │   ├── agent-layout.component.html  ← Has <router-outlet> for child routes
│   │   │   └── components/
│   │   │       ├── sidebar.component.ts     ← Left navigation
│   │   │       ├── top-nav.component.ts     ← Top bar with logo
│   │   │       └── ...
│   │   │
│   │   ├── customer-layout/         ← Wrapper for /customer/* routes
│   │   │   ├── customer-layout.component.ts
│   │   │   └── customer-header.component.ts
│   │   │
│   │   └── portal-layout/           ← Wrapper for /login, /signup (public)
│   │       └── portal-layout.component.ts
│   │
│   └── shared/                      ← Reusable components (used by multiple features)
│       ├── components/
│       │   ├── ui-button/           ← Button component
│       │   │   ├── ui-button.component.ts
│       │   │   └── ui-button.component.css
│       │   │
│       │   ├── ui-input/            ← Text input wrapper
│       │   ├── ui-card/             ← Card wrapper
│       │   ├── loading-spinner/     ← Spinner animation
│       │   └── modal/               ← Modal dialog
│       │
│       ├── pipes/                   ← Angular pipes (data transformers)
│       │   ├── safe-html.pipe.ts    ← Display HTML safely
│       │   └── date-format.pipe.ts  ← Format dates nicely
│       │
│       ├── directives/              ← Custom attribute directives
│       │   ├── highlight.directive.ts  ← Highlight on hover
│       │   └── ...
│       │
│       └── utils/                   ← Utility functions
│           ├── string.utils.ts      ← String manipulation
│           ├── date.utils.ts        ← Date helpers
│           └── validators.ts        ← Form validators
│
├── environments/                    ← Environment-specific configs
│   ├── environment.ts               ← Used on `npm run build` (production)
│   └── environment.development.ts   ← Used on `npm start` (local dev)
│
├── assets/                          ← Static files (images, icons, fonts)
│   └── (images, logos, etc.)
│
├── styles/                          ← Additional stylesheets (optional)
│   └── (variables, mixins, etc.)
│
└── public/                          ← Static root files
    └── (favicon, robots.txt, etc.)
```

## Quick Reference: Where to Add What?

| What | Where | Why |
|------|-------|-----|
| New auth method | `core/auth/auth.service.ts` | Singleton, used everywhere |
| New feature page | `features/new-feature/` | Self-contained, lazy-loaded |
| New reusable button | `shared/components/ui-button/` | Multiple features use it |
| API call for tickets | `features/tickets/services/ticket.service.ts` | Keeps API calls organized |
| Common utility function | `shared/utils/` | Reusable, no dependencies on features |
| Custom form validator | `core/validators/` | Used by multiple forms |
| New page route | `app.routes.ts` | Single source of truth for routing |
| Global color | `styles.css` | Theme changes affect everywhere |

## File Naming Convention

**Components:**
```
feature-name.component.ts        ← TypeScript logic
feature-name.component.html      ← Template
feature-name.component.css       ← Styles
feature-name.component.spec.ts   ← Tests
```

**Services:**
```
feature.service.ts               ← Business logic, API calls
```

**Models:**
```
feature.model.ts                 ← TypeScript types/interfaces
```

**Example:**
```
ticket-list.component.ts         ← Display list of tickets
ticket.service.ts                ← Fetch tickets from API
ticket.model.ts                  ← Type: interface Ticket { ... }
```

## Why This Structure?

### core/
- **Why separate?** Singleton services are app-wide. Keep them isolated.
- **Example:** Auth service used by every component. Put it in core.

### features/
- **Why separate?** Each feature is independent. Team A works on Tickets, Team B on Reports—no conflicts.
- **Why lazy-load?** App loads fast (~200KB), features load on-demand.

### shared/
- **Why separate?** DRY principle (Don't Repeat Yourself). Button used 50 times? One component, shared everywhere.

### layouts/
- **Why separate?** Agents & customers have different UIs. Layouts keep structure clean.

## Migration Guide: If You Worked on Old Code

**Old structure (NgModule):**
```
app/
├── modules/
│   ├── auth/
│   ├── tickets/
│   └── ...
```

**New structure (Standalone):**
```
app/
├── features/auth/
├── features/tickets/
└── ...
```

**Changes:**
- No `@NgModule` decorators needed
- Components import dependencies directly
- Routes use `loadComponent` instead of `loadModule`
- Simpler, more modern
