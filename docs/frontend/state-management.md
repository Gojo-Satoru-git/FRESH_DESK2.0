# State Management

## What is State? (And Why Should You Care?)

**State** = Data that changes over time.

Examples in Fresh Desk:
- Current user (logged-in agent's name, role)
- List of tickets (changes as new tickets arrive)
- Theme preference (light/dark)
- Loading indicator (show/hide spinner)

**Why manage it?** Without good state management, your app becomes a mess:
- Components don't know what data they have
- Two components show different data
- Hard to debug (where did this value come from?)

## Current Architecture (Angular Signals)

We use **Angular Signals**, which is the modern way (since Angular 14).

### Simple Example

```typescript
// Old way (RxJS)
currentUser$ = new BehaviorSubject<User | null>(null);

// New way (Signals)
currentUser = signal<User | null>(null);
```

### Why Signals?
- ✅ Simpler syntax (no `subscribe`, no unsubscribe)
- ✅ Faster change detection (only re-render what changed)
- ✅ Smaller bundle (~20KB less)
- ✅ Modern Angular standard

## Global State Pattern

### Problem: Sharing Data Between Components

```
LoginComponent
  ↓
Sets user = {name: "John", role: "agent"}
  ↓
DashboardComponent
  ↓
Needs user, but doesn't have it!
  ↓
😡 App breaks
```

### Solution: Centralized State Service

```
LoginComponent
  ↓
Call authService.login()
  ↓
authService stores: currentUser = signal({...})
  ↓
DashboardComponent
  ↓
Inject authService
  ↓
Read: authService.currentUser()  ← Gets latest data
  ↓
✅ Works!
```

### File Structure
```
src/app/
├── core/
│   ├── auth/
│   │   └── auth.service.ts       ← Stores currentUser signal
│   ├── theme/
│   │   └── theme.service.ts      ← Stores theme signal
│   └── ...other services...
│
└── features/
    ├── tickets/
    │   ├── services/
    │   │   └── ticket.service.ts  ← Stores ticketList signal
    │   └── components/
    └── ...
```

## Authentication State

### AuthService (Central Place for User Data)

```typescript
// src/app/core/auth/auth.service.ts

@Injectable({ providedIn: 'root' })
export class AuthService {
  private currentUser = signal<User | null>(null);
  private isLoading = signal(false);
  private error = signal<string | null>(null);

  login(email: string, password: string) {
    this.isLoading.set(true);
    this.error.set(null);

    return this.http.post('/api/auth/login', { email, password })
      .pipe(
        tap(response => {
          localStorage.setItem('auth_token', response.token);
          this.currentUser.set(response.user);
        }),
        catchError(err => {
          this.error.set(err.message);
          throw err;
        }),
        finalize(() => this.isLoading.set(false))
      );
  }

  getCurrentUser() {
    return this.currentUser();  ← Read signal value
  }

  logout() {
    localStorage.removeItem('auth_token');
    this.currentUser.set(null);
  }
}
```

### Using Auth State in Components

```typescript
export class DashboardComponent {
  user = this.auth.getCurrentUser();  ← Angular auto-tracks changes

  constructor(private auth: AuthService) {}

  ngOnInit() {
    console.log(`Hello, ${this.user().name}`);
  }
}
```

Template:
```html
<h1>Welcome, {{ user().name }}!</h1>
<p>Your role: {{ user().role }}</p>
```

## Feature State (Tickets Example)

### Ticket State Service

```typescript
// src/app/features/tickets/services/ticket.service.ts

@Injectable({ providedIn: 'root' })
export class TicketService {
  // State
  private ticketList = signal<Ticket[]>([]);
  private isLoading = signal(false);
  private error = signal<string | null>(null);

  // Read state
  getTickets() {
    return this.ticketList();
  }

  getTicketsSignal() {
    return this.ticketList.asReadonly();  ← Prevent external changes
  }

  // Fetch from API
  loadTickets() {
    this.isLoading.set(true);
    
    return this.http.get<Ticket[]>('/api/tickets')
      .pipe(
        tap(tickets => this.ticketList.set(tickets)),
        finalize(() => this.isLoading.set(false))
      );
  }

  // Create new ticket
  createTicket(data: NewTicket) {
    return this.http.post<Ticket>('/api/tickets', data)
      .pipe(
        tap(newTicket => {
          this.ticketList.update(tickets => [...tickets, newTicket]);
        })
      );
  }

  // Update ticket
  updateTicket(id: string, data: TicketUpdate) {
    return this.http.patch<Ticket>(`/api/tickets/${id}`, data)
      .pipe(
        tap(updated => {
          this.ticketList.update(tickets =>
            tickets.map(t => t.id === id ? updated : t)
          );
        })
      );
  }
}
```

### Using Ticket State

```typescript
export class TicketListComponent {
  tickets = this.ticketService.getTicketsSignal();
  isLoading = this.ticketService.isLoading;

  constructor(private ticketService: TicketService) {}

  ngOnInit() {
    this.ticketService.loadTickets().subscribe();
  }

  updateStatus(ticketId: string, status: string) {
    this.ticketService.updateTicket(ticketId, { status })
      .subscribe();
  }
}
```

Template:
```html
@if (isLoading()) {
  <p>Loading...</p>
} @else {
  <div *ngFor="let ticket of tickets()">
    {{ ticket.title }}
  </div>
}
```

## Theme State

Simple example using signals:

```typescript
// src/app/core/theme/theme.service.ts

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private isDark = signal(false);

  constructor() {
    // Load saved preference
    const saved = localStorage.getItem('theme');
    if (saved === 'dark') {
      this.setDarkMode(true);
    }
  }

  toggle() {
    this.setDarkMode(!this.isDark());
  }

  private setDarkMode(dark: boolean) {
    this.isDark.set(dark);
    localStorage.setItem('theme', dark ? 'dark' : 'light');
    
    if (dark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }

  isDarkMode() {
    return this.isDark();
  }
}
```

## Best Practices

### ✅ Do This

**Centralize state:**
```typescript
// Good: All user data in one place
this.auth.getCurrentUser()
this.auth.isLoading()
```

**Make signals read-only:**
```typescript
// Good: Prevent external code from changing state
private tickets = signal<Ticket[]>([]);
getTickets() {
  return this.tickets.asReadonly();
}
```

**Use signals for async:**
```typescript
// Good: Track loading state
private isLoading = signal(false);

loadTickets() {
  this.isLoading.set(true);
  return this.api.getTickets().pipe(
    finalize(() => this.isLoading.set(false))
  );
}
```

### ❌ Don't Do This

**Pass data through HTML:**
```html
<!-- Bad: Component A has to pass data to Component B -->
<app-child [data]="someData"></app-child>
```

Better:
```typescript
// Good: Both use same service
@Component({...})
export class ComponentA {
  data = this.service.data();
}

@Component({...})
export class ComponentB {
  data = this.service.data();
}
```

**Duplicate state:**
```typescript
// Bad: Component has its own copy
ticket$ = this.api.getTicket();
```

Better:
```typescript
// Good: Use service state
ticket = this.ticketService.getTicket();
```

## Common Patterns

### Pattern 1: Load Data on Component Init
```typescript
export class MyComponent {
  items = this.service.getItems();

  constructor(private service: MyService) {}

  ngOnInit() {
    this.service.loadItems().subscribe();
  }
}
```

### Pattern 2: React to State Changes
```typescript
export class MyComponent {
  items = this.service.getItems();

  constructor(private service: MyService) {
    effect(() => {
      if (this.items().length === 0) {
        console.log('No items!');
      }
    });
  }
}
```

### Pattern 3: Combine Multiple Signals
```typescript
// Advanced: Show loading state OR empty state
isLoadingOrEmpty = computed(() =>
  this.service.isLoading() || this.service.getTickets().length === 0
);
```

## Debugging State

### Check Signal Values
```typescript
// In component
console.log(this.service.currentUser());  ← Print current value
```

### Chrome DevTools
1. Open Console (F12)
2. Inject service: `ng = ng` (Angular creates a global `ng` object)
3. Read state: `ng.getComponent($0).componentInstance.myService.currentUser()`

## When to Add New State

Ask yourself:
1. **Multiple components need this data?** → Create service with signal
2. **Data persists across navigation?** → Belongs in service state
3. **Single component only?** → Component-level signal is fine

```typescript
// Local component state (fine)
selectedTab = signal<'overview' | 'details'>('overview');

// Global service state (for sharing)
currentUser = signal<User | null>(null);
```