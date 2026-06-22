# Rate Limiting Implementation Guide

## Overview

Rate Limiting is implemented to protect the Adrenalin authentication system from:

- Brute-force login attacks
- Refresh token abuse
- OTP guessing attacks
- Password reset spam
- Email verification spam
- Denial of Service (DoS) attempts against authentication endpoints

The implementation uses the built-in ASP.NET Core Rate Limiting Middleware.

# How It Works

Every incoming request is grouped by the client's IP address.

Example:

IP Address:

192.168.1.10

Configuration:

- 5 requests
- Per 1 minute

Behavior:

Request 1 → Allowed

Request 2 → Allowed

Request 3 → Allowed

Request 4 → Allowed

Request 5 → Allowed

Request 6 → Blocked

Response:

HTTP 429 Too Many Requests

After the configured time window expires, the counter is automatically reset and requests are allowed again.

# Configuration

## Program.cs

using System.Threading.RateLimiting;  
<br/>builder.Services.AddRateLimiter(options =>  
{  
options.AddPolicy("LoginPolicy", context =>  
RateLimitPartition.GetFixedWindowLimiter(  
partitionKey:  
context.Connection.RemoteIpAddress?.ToString()  
?? "unknown",  
factory: _=> new FixedWindowRateLimiterOptions  
{  
PermitLimit = 5,  
Window = TimeSpan.FromMinutes(1),  
QueueLimit = 0  
}));  
<br/>options.AddPolicy("RefreshPolicy", context =>  
RateLimitPartition.GetFixedWindowLimiter(  
partitionKey:  
context.Connection.RemoteIpAddress?.ToString()  
?? "unknown",  
factory:_ => new FixedWindowRateLimiterOptions  
{  
PermitLimit = 20,  
Window = TimeSpan.FromMinutes(1),  
QueueLimit = 0  
}));  
<br/>options.AddPolicy("ForgotPasswordPolicy", context =>  
RateLimitPartition.GetFixedWindowLimiter(  
partitionKey:  
context.Connection.RemoteIpAddress?.ToString()  
?? "unknown",  
factory: \_ => new FixedWindowRateLimiterOptions  
{  
PermitLimit = 3,  
Window = TimeSpan.FromMinutes(15),  
QueueLimit = 0  
}));  
<br/>options.RejectionStatusCode =  
StatusCodes.Status429TooManyRequests;  
});

## Middleware Registration

Add after Authentication and Authorization middleware.

app.UseAuthentication();  
<br/>app.UseAuthorization();  
<br/>app.UseRateLimiter();

# Protected Endpoints

## Login

\[EnableRateLimiting("LoginPolicy")\]  
\[HttpPost("login")\]

Limit:

- 5 requests / minute

Reason:

- Prevent brute-force password attacks.

## Refresh Token

\[EnableRateLimiting("RefreshPolicy")\]  
\[HttpPost("refresh")\]

Limit:

- 20 requests / minute

Reason:

- Prevent refresh token abuse.

## Forgot Password

\[EnableRateLimiting("ForgotPasswordPolicy")\]  
\[HttpPost("forgot-password")\]

Limit:

- 3 requests / 15 minutes

Reason:

- Prevent password reset email flooding.

## Resend Verification

\[EnableRateLimiting("LoginPolicy")\]  
\[HttpPost("resend-verification")\]

Reason:

- Prevent email spam.

## Verify Email OTP

\[EnableRateLimiting("LoginPolicy")\]  
\[HttpPost("verify-email-otp")\]

Reason:

- Prevent OTP brute-force attacks.

## Reset Password

\[EnableRateLimiting("LoginPolicy")\]  
\[HttpPost("reset-password")\]

Reason:

- Prevent automated abuse.

# Endpoints Not Rate Limited

The following endpoints are protected by authentication and do not require rate limiting:

GET /api/auth/sessions  
DELETE /api/auth/sessions/{id}  
POST /api/auth/logout-all  
POST /api/auth/logout  
POST /api/auth/change-password

Reason:

- Already require a valid JWT.
- Lower attack surface.
- Used by authenticated users only.

# Example

Login endpoint configured:

- PermitLimit = 5
- Window = 1 minute

Timeline:

00:00 → Request 1 → 200 OK

00:10 → Request 2 → 200 OK

00:20 → Request 3 → 200 OK

00:30 → Request 4 → 200 OK

00:40 → Request 5 → 200 OK

00:45 → Request 6 → 429 Too Many Requests

01:00 → Window reset

01:01 → Request allowed again

# Security Benefits

This implementation helps mitigate:

- Credential Stuffing
- Password Brute Force Attacks
- Refresh Token Abuse
- OTP Guessing
- Email Flooding
- Basic Denial of Service Attempts

# Notes for Production

When deployed behind:

- Nginx
- IIS Reverse Proxy
- Azure Application Gateway
- Cloudflare

Configure Forwarded Headers:

app.UseForwardedHeaders();

This ensures the real client IP address is used instead of the proxy IP.

# Current Security Layers in Adrenalin

Implemented:

✓ JWT Authentication

✓ Refresh Tokens

✓ Refresh Token Rotation

✓ Refresh Token Reuse Detection

✓ Session Tracking

✓ Session Activity Middleware

✓ Logout

✓ Logout All Devices

✓ Change Password Session Invalidation

✓ Password Reset Session Invalidation

✓ Rate Limiting

These layers together provide a secure authentication architecture suitable for production use.