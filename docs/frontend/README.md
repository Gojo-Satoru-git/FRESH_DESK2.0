# Frontend Documentation

## Quick Start

```bash
cd frontend/fresh_desk_2.0
npm install
npm start
# Available at http://localhost:4200
```

## Project Structure

```
src/
├── main.ts                    # Entry point
├── app.routes.ts              # Routes
├── styles.css                 # Global styles & theme
├── app/
│   ├── features/auth/         # Login page
│   ├── core/theme/            # Theme service & switcher
│   └── shared/                # Shared components
├── environments/
│   ├── environment.ts         # Production
│   └── environment.development.ts  # Development (localhost:5001)
└── assets/                    # Static files
```

## Key Features

### 1. Login Page
- Email/password validation
- Real-time error messages
- Theme switcher button (top-right)
- Location: `src/app/features/auth/login.component.ts`

### 2. Theme System
- Light/dark mode toggle
- Persists to localStorage
- CSS variables for colors
- Service: `src/app/core/theme/theme.service.ts`

### 3. Environment Config
- **Dev**: `https://localhost:5001` (local .NET API)
- **Prod**: `https://api.adrenalin-support.com`
- Auto-selected by Angular CLI based on build mode

## Available Commands

```bash
npm start              # Dev server (localhost:4200)
npm run build          # Production build
npm run watch          # Continuous build
npm test               # Run tests
```

## Environment Setup

Auto-configured based on build mode:
- `npm start` → uses `environment.development.ts`
- `npm build` → uses `environment.ts`

## Styling

Using **Tailwind CSS v4** with custom colors:
- `bg-background` - Page background
- `text-text` - Main text
- `text-text-muted` - Secondary text
- `bg-primary` - Primary color (buttons)

Colors defined in `src/styles.css` and change automatically in dark mode.

## Troubleshooting

**Port 4200 in use?**
```bash
netstat -ano | findstr :4200
taskkill /PID <PID> /F
```

**Build errors?**
```bash
npm ci
npm start
```

**API not connecting?**
- Verify .NET backend running on `https://localhost:5001`
- Check Network tab in DevTools

## Next Steps

1. Add more routes in `app.routes.ts`
2. Create components in `src/app/features/`
3. Connect login to real backend API
4. Add routing guards for authenticated routes
