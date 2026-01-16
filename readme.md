# Authentication & Authorization System

## Overview
This is a make-shift authentication & Authorization system with minimal CRUD ops  built with **.NET 10** that I developed In my .NET speedrun just out of curiosity. The project combines **Vertical Slice Architecture** with Clean Architecture (Still working on that)

---

## Tech Stack

- **Core**: .NET 10 (Preview/LTS features) & ASP.NET Core Web API
- **Database**: PostgreSQL with Entity Framework Core 10
- **Caching & performance**: StackExchange.Redis for distributed caching and rate limiting
- **Security**: 
  - JWT (JSON Web Tokens) for stateless authentication
  - BCrypt.Net-Next for adaptive password hashing
- **Validation**: FluentValidation for strong-typed input validation
- **Utilities**: MailKit for email delivery services

---

## Key Features Implemented

### 1. Security Architecture
I implemented a complete authentication flow using **JWTs**. Understanding the importance of proper security, I avoided simple implementations in favor of a robust system:
- **Stateless Authentication**: Fully decoupled authentication suitable for microservices.
- **Secure Password Storage**: Used BCrypt with appropriate work factors to defeat rainbow table attacks.
- **Token Management**: Implemented refresh token rotation to balance security and user experience.

##

### 2. Rate Limiting
Implemented the standard fixed window at first, but then shifted to **Token bucket**, because it's so cool.

The idea is simple but powerful: each user gets a bucket of tokens. Every request consumes a token. If the bucket is empty, you wait. The cool part is the bucket refills at a constant rate, allowing for short bursts of traffic while still enforcing a long-term limit. Since it runs atomically in Redis, we don't worry about race conditions. It just works.

- **Lua Scripting**: I wrote custom Lua scripts to ensure atomicity of rate-limit counters, preventing race conditions in high-concurrency scenarios.

- **Granular Control**: configured different limits for public endpoints (signup/login) vs. authenticated endpoints to protect critical resources from abuse/DDoS.

##

### 3. Repository → Service → Controller (Hybrid Architecture) 

Each feature is implemented as a **self-contained vertical slice** using a  
**Repository → Service → Controller** structure.

The architecture is **Vertical Slice–first**, while:
- borrowing the **Controller** concept from MVC for HTTP handling
- applying **tactical DDD** patterns (application services and repositories)

This keeps feature boundaries explicit without enforcing heavy architectural ceremony.
##

#### Controller
- HTTP entry point for the feature
- Handles routing, authorization, rate limiting, and DTO binding
- Contains no business logic
- Delegates execution to the Service

##

#### Service
- Represents a single **application use-case** (e.g. Register, Verify, Reset Password)
- Contains business rules and orchestration logic
- Handles hashing, cooldowns, token lifecycle, and invariants
- Coordinates one or more repositories

##

#### Repository
- Encapsulates persistence concerns (DB, Redis, token storage, etc.)
- No business rules
- No HTTP or application flow logic


##


**Guiding Rule:**  
A change affecting a single application use-case is expected to remain within one slice.  
Changes to shared domain rules or cross-cutting concerns may affect multiple slices.

##

### 4. Error Handling
I implemented a **Global Exception Handling** middleware to standardize API responses.
- **Problem Details**: returns compliant error responses.
- **Custom Exceptions**: Defines domain-specific exceptions (e.g., `BadRequestException`, `UnauthorizedException`) which the middleware intercepts and translates into appropriate HTTP 4xx/5xx status codes.

---

# Endpoints


### Auth (`api/auth`)

| Method | Route | Description |
| :--- | :--- | :--- |
| POST | `/login` | Authenticates a user and returns a JWT. |
| POST | `/signup` | Creates a new user account. |
| POST | `/refresh-token` | Rotates tokens using a valid refresh token. |
| POST | `/logout` | Invalidates the current session. |
| POST | `/forgot-password` | Initiates the password recovery process. |
| POST | `/reset-password` | Resets the password using a recovery token. |
| POST | `/verify-email` | Verifies the user's email address. |
| POST | `/resend-verification` | Re-sends the email verification code. |

### User (`api/users/me`)

| Method | Route | Description |
| :--- | :--- | :--- |
| GET | `/profile` | Retrieves the current user's profile details. |
| POST | `/onboard` | Completes the user onboarding process. |
| PUT | `/profile` | Updates user profile information. |
| DELETE | `/delete-account` | Permanently removes the user's account. |




