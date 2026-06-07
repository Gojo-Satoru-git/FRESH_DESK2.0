# Frontend Documentation

Fresh Desk 2.0 is a **modern Angular 21 HR ticketing system** built with Tailwind CSS. This guide helps you understand the codebase, make changes, and extend features.

## Quick Start (5 minutes)

```bash
cd frontend/fresh_desk_2.0
npm install
npm start
# Open http://localhost:4200
```

**Tech Stack:** Angular 21 + Tailwind CSS v4 + TypeScript 5.9 + RxJS

## What This App Does

- 🔐 **Auth Module** - Login/signup with JWT authentication
- 📊 **Agent Dashboard** - Agents view tickets, contacts, and manage workload
- 👥 **Customer Portal** - Customers raise tickets and view history
- 🎟️ **Ticket System** - Full ticket lifecycle (create, assign, resolve)
- 📚 **Knowledge Base** - Self-service articles for customers
- 📈 **Reports** - Analytics dashboards for managers
- ⚙️ **Admin Panel** - System configuration and settings

## Common Commands

```bash
npm start      # Dev server with hot reload (http://localhost:4200)
npm run build  # Production build
npm run watch  # Continuous compilation
npm test       # Run unit tests
```

## Architecture Highlights

**Why we chose this structure:**
- **Lazy-loaded features** → Fast initial load, load modules on demand
- **Standalone components** → No NgModule boilerplate, cleaner code
- **Separate layouts** → Different UI shells for agents vs customers
- **Service-based auth** → JWT tokens stored, automatic API injection via interceptor

## Quick Navigation

| Page | Purpose | Route |
|------|---------|-------|
| Login | User authentication | `/login` |
| Agent Dashboard | Main agent workspace | `/agent/dashboard` |
| Tickets | Ticket management | `/agent/tickets` |
| Customer Portal | Self-service area | `/customer/portal` |
| Admin Panel | Settings & config | `/admin` |
