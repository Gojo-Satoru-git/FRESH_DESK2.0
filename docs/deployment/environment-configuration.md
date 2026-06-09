# Environment Configuration

## Overview

Fresh Desk 2.0 uses Angular's environment configuration system to manage different settings for development and production environments. This ensures your application can connect to the correct backend API and use appropriate settings based on the build mode.

## Environment Files Location

```
src/environments/
├── environment.ts              # Production environment (default)
└── environment.development.ts  # Development environment (dev mode only)
```

## File Contents

### Production (`src/environments/environment.ts`)

```typescript
export const environment = {
  production: true,
  apiUrl: "https://api.adrenalin-support.com",
};
```

**Usage**: `ng build` (production builds)
**API Base URL**: `https://api.adrenalin-support.com`

### Development (`src/environments/environment.development.ts`)

```typescript
export const environment = {
  production: false,
  apiUrl: "https://localhost:5088",
};
```

**Usage**: `ng serve` (development server)
**API Base URL**: `https://localhost:5088` (local .NET backend)

## Environment Selection

### Automatic Selection

Angular CLI automatically selects the correct environment based on the build mode:

| Command                                | Environment Used | File                       |
| -------------------------------------- | ---------------- | -------------------------- |
| `npm start` (ng serve)                 | Development      | environment.development.ts |
| `ng build`                             | Production       | environment.ts             |
| `ng build --configuration production`  | Production       | environment.ts             |
| `ng build --configuration development` | Development      | environment.development.ts |

### Configuration in `angular.json`

The file replacement is defined in the Angular build configuration:

```json
{
  "projects": {
    "fresh_desk_2.0": {
      "architect": {
        "build": {
          "configurations": {
            "development": {
              "fileReplacements": [
                {
                  "replace": "src/environments/environment.ts",
                  "with": "src/environments/environment.development.ts"
                }
              ]
            },
            "production": {
              "fileReplacements": []
            }
          }
        }
      }
    }
  }
}
```

## Using Environment Variables in Components

### Importing Environment Configuration

```typescript
import { environment } from "../../environments/environment";
```

### Accessing Values

```typescript
export class AuthService {
  private apiUrl = environment.apiUrl;

  login(credentials: LoginCredentials) {
    return this.http.post(`${environment.apiUrl}/auth/login`, credentials);
  }

  getUsers() {
    return this.http.get(`${environment.apiUrl}/api/users`);
  }
}
```

### Checking Production Mode

```typescript
if (environment.production) {
  // Production-specific logic
  enableProductionMode();
} else {
  // Development-specific logic
}
```

## Backend API Configuration

### Development Setup

- **Backend URL**: `https://localhost:5001`
- **Protocol**: HTTPS (with self-signed certificate)
- **Port**: 5001 (standard .NET development port)
- **Localhost**: Used for local development only

**Prerequisites**:

- .NET backend running locally on port 5001
- Firewall allows local connections
- HTTPS certificate configured for localhost

### Production Setup

- **Backend URL**: `https://api.adrenalin-support.com`
- **Protocol**: HTTPS (with valid SSL certificate)
- **Port**: 443 (standard HTTPS)
- **Domain**: Update to your production domain before deployment

## Adding New Environment Variables

### Step 1: Define in Both Environment Files

**development**:

```typescript
export const environment = {
  production: false,
  apiUrl: "https://localhost:5001",
  featureFlag: true,
  logLevel: "debug",
};
```

**production**:

```typescript
export const environment = {
  production: true,
  apiUrl: "https://api.adrenalin-support.com",
  featureFlag: false,
  logLevel: "error",
};
```

### Step 2: Use in Components

```typescript
import { environment } from "../../environments/environment";

export class DebugService {
  enableDebugMode = environment.logLevel === "debug";

  log(message: string) {
    if (this.enableDebugMode) {
      console.log(message);
    }
  }
}
```

### Step 3: Type Safety (Optional)

Create an interface for type checking:

```typescript
// environment.interface.ts
export interface IEnvironment {
    production: boolean;
    apiUrl: string;
    featureFlag: boolean;
    logLevel: 'debug' | 'info' | 'error';
}

// environment.ts & environment.development.ts
export const environment: IEnvironment = { ... };
```

## Common Environment Variables

| Variable        | Dev Value              | Prod Value                        | Purpose                      |
| --------------- | ---------------------- | --------------------------------- | ---------------------------- |
| `production`    | false                  | true                              | Indicates production build   |
| `apiUrl`        | https://localhost:5001 | https://api.adrenalin-support.com | Backend API base URL         |
| `logLevel`      | debug                  | error                             | Console logging verbosity    |
| `enableMocks`   | true                   | false                             | Use mock data instead of API |
| `cacheDuration` | 0                      | 3600000                           | HTTP cache timeout (ms)      |

## Debugging Environment Issues

### Verify Correct Environment is Loaded

```typescript
console.log("Current environment:", environment);
console.log("Production mode:", environment.production);
console.log("API URL:", environment.apiUrl);
```

### Browser DevTools Check

1. Open Browser DevTools (F12)
2. Go to Console tab
3. Run: `console.log(environment)` (if imported in global scope)

### Build Verification

```bash
# Check which environment was used during build
npm run build -- --verbose
```

### Common Issues

| Issue                 | Cause                           | Solution                         |
| --------------------- | ------------------------------- | -------------------------------- |
| Wrong API URL         | Environment file not selected   | Clear build cache: `rm -r dist/` |
| Environment undefined | Missing import                  | Ensure correct import path       |
| Dev API in prod       | File replacement not configured | Check angular.json configuration |

## Testing with Different Environments

### Test with Development Environment

```bash
npm start
# Automatically uses environment.development.ts
```

### Test with Production Environment

```bash
npm start -- --configuration production
# Uses environment.ts
```

### Build and Test Production Locally

```bash
npm run build
# Starts local server with production build
npx http-server dist/
# Access at http://localhost:8080
```

## Deployment Considerations

### Before Deploying to Production

1. ✅ Update `apiUrl` in `environment.ts` to production domain
2. ✅ Verify SSL certificates are valid
3. ✅ Test with `ng build --configuration production`
4. ✅ Check network requests in DevTools
5. ✅ Verify CORS settings on backend

### Post-Deployment Verification

1. Open production URL in browser
2. Open DevTools → Network tab
3. Check API calls are going to correct URL
4. Verify no CORS errors
5. Test login functionality

## Related Documentation

- [Local Setup Guide](../deployment/local-setup.md)
- [Production Deployment](../deployment/production-deployment.md)
- [Authentication Setup](./authentication-setup.md)
- [Backend API Design](../backend/api-design.md)
