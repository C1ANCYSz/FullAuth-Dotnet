# Authentication & Authorization Project

## Overview
This is a make-shift authentication & Authorization system with minimal CRUD ops  built with **.NET 10** that I developed In my .NET speedrun. The project combines **Vertical Slice Architecture** with Clean Architecture (Still working on that)

## Tech Stack

### Core Frameworks
- **.NET 10**: Using the latest LTS release
- **ASP.NET Core Web API**: For building RESTful services

### Data & Infrastructure
- **Entity Framework Core with Npgsql**: PostgreSQL integration
- **StackExchange.Redis**: For distributed caching and rate limiting
- **FluentValidation**: Request validation

### Security
- **JWT (JSON Web Tokens)**: Stateless authentication via `Microsoft.AspNetCore.Authentication.JwtBearer`
- **BCrypt**: Password hashing with `BCrypt.Net-Next`

## Architecture

The solution uses a traditional layered approach that keeps concerns separated while maintaining feature cohesion.

### Structure
- **Controllers** (`AuthController`, `UserController`): Handle HTTP requests and responses
- **Services** (`AuthService`, `UserService`): Implement business logic and orchestration
- **Repositories** (`UserRepository`): Manage data access and abstract database operations

### Middleware Pipeline
I've implemented some custom middleware components:

1. **GlobalExceptionMiddleware**: Centralized exception handling that maps domain exceptions to appropriate HTTP status codes (400, 401, 500)
2. **RedisRateLimitMiddleware**: Distributed rate limiting using Redis and Lua scripts for atomic operations
3. **Authentication/Authorization**: Standard ASP.NET Core middleware with custom policies

## Key Features

### Distributed Rate Limiting
Instead of in-memory rate limiting, I implemented a Redis-based solution:
- Uses a **Fixed Window** algorithm with Lua scripts for atomic operations
- Different rate limit policies for various endpoints (stricter limits on login vs signup)
- Returns 429 status codes when limits are exceeded

### Domain Design
- **Custom Exceptions**: Typed exceptions like `BadRequestException` and `UnauthorizedException` for cleaner business logic
- **DTOs**: Clear separation between database entities and API contracts
- **Guid-based IDs**: For user identification

## API Endpoints (All rate limited)

### Authentication (`/api/auth`)
- `POST /login`: User authentication 
- `POST /signup`: User registration 
- `POST /refresh-token`: Token refresh
- `POST /logout`: Session revocation

### User Management (`/api/users/me`)
- `GET /profile`: Get current user details
- `POST /onboard`: Complete user profile
- `PUT /profile`: Update user information
- `DELETE /delete-account`: Delete user account

## Infrastructure
- **Redis**: Powers the rate limiting system and Verification Tokens for password reset with short-lived TTL
- **PostgreSQL**: Primary data store

