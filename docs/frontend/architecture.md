# Frontend Architecture

## Why We Built It This Way

Fresh Desk needed a **fast, scalable ticketing UI** that works for agents and customers differently. We chose this architecture to:

- **Load fast** → Lazy-load features, don't download everything upfront
- **Keep it maintainable** → Features isolated, easy to find and modify
- **Share code** → Common components in `shared/` folder, no duplication
- **Protect routes** → Only logged-in users can access agent/customer pages
- **Work on any device** → Responsive Tailwind CSS works on mobile too

## The Big Picture

```
Angular App (localhost:4200)
    ↓
Authentication Guard (JWT check)
    ├─→ Agent Layout (/agent/...) → Dashboard, Tickets, Contacts
    ├─→ Customer Layout (/customer/...) → Portal, Raise Ticket
    └─→ Auth Layout (/login, /signup) → No login required
    ↓
Lazy-Loaded Feature Components
    ↓
Shared Components (buttons, inputs, cards)
    ↓
API Interceptor adds JWT token
    ↓
.NET Backend (https://localhost:5001)
```

## Core Design Decisions

### 1. Lazy Loading (Load Features on Demand)
**What it means:** Features like "Tickets" or "Reports" only download when user visits that page.

**Why:** 
- Initial app load is ~200KB instead of 1MB
- Better UX on slow networks
- Each team can work on features independently

**How it works:**
```typescript
{
  path: 'tickets',
  loadComponent: () => import('./features/tickets/...').then(m => m.TicketListComponent)
}
```

### 2. Layout Wrappers (Different UI for Different Users)
**What it means:** Agents see a dashboard sidebar, customers see a simpler header.

**Why:**
- Agents need quick navigation (sidebar with ticket count, SLA info)
- Customers just need to view their tickets (minimal UI)
- One codebase, two user experiences

**Where:**
```
layouts/
├── agent-layout/      (Sidebar, top nav, ticket sidebar)
├── customer-layout/   (Simple header, no admin options)
└── portal-layout/     (Just auth pages, public)
```

### 3. Feature Isolation (Each Feature is Self-Contained)
**What it means:** Tickets feature has its own components, models, services.

**Why:**
- Easy to test each feature independently
- Can work on tickets without touching auth code
- Reusable pattern (copy Tickets folder, rename, repeat)

### 4. Shared Components (DRY Principle)
**What it means:** Buttons, input fields, cards live in `shared/` and are reused everywhere.

**Why:**
- Consistent UI across app
- Change button style once, updates everywhere
- Designers like this (clear component library)

## Routing Flow

1. **Root level** (`/login`, `/signup`) → Standalone, no guard
2. **Agent level** (`/agent/dashboard`) → Protected by `AuthGuard`, checks JWT
3. **Customer level** (`/customer/portal`) → Protected, different layout
4. If JWT missing → Redirect to `/login` automatically

## Service Architecture

- **AuthService** → Login, logout, token management
- **ThemeService** → Dark/light mode toggle
- **TicketService** → Fetch/create/update tickets (via API)
- **Interceptor** → Auto-adds JWT token to every API request

Why this way? Separates concerns—UI doesn't worry about API URLs, services don't know about components.
