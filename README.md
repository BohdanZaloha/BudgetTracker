# BudgetTracker

A clean, test-covered ASP.NET Core API for tracking personal finances. Create accounts, categorize expenses/income, and add transactions — all secured with JWT auth.

## Features
- **JWT auth**: register/login and fetch your profile (`/api/authentication/*`)
- **Accounts**: list + create personal accounts in specific currencies
- **Categories**: hierarchical income/expense categories
- **Transactions**: add/query paged transactions with filters (date/type/account/category)
- **Validation & Problem Details**: consistent errors via FluentValidation + RFC7807 responses
- **Logging & Swagger**: Serilog JSON logs and built-in OpenAPI/Swagger

## Architecture
```
BudgetTracker.Domain         // Entities & enums
BudgetTracker.Application    // DTOs, services, validators
BudgetTracker.Infrastructure // EF Core, Identity, JWT, DI
BudgetTracker.Api            // Controllers, middleware, Swagger, ProblemDetails
BudgetTracker.Tests          // xUnit + FluentAssertions + Moq
```

## Authentication
- **Register**: `POST /api/authentication/register`
- **Login**: `POST /api/authentication/login` → returns `accessToken` & `expiresInSeconds`
- **Me**: `GET /api/authentication/me` (requires `Authorization: Bearer <token>`)

**Sample login request**
```http
POST /api/authentication/login
Content-Type: application/json

{ "email": "you@example.com", "password": "P@ssw0rd!" }
```

**Sample login response**
```json
{
  "accessToken": "<jwt>",
  "expiresInSeconds": 3600,
  "userId": "abc123",
  "email": "you@example.com",
  "userName": "You",
  "roles": []
}
```
## API Endpoints

### Accounts (auth required)
- `GET /api/accounts` → list non-archived accounts (sorted by name)
- `POST /api/accounts`
  ```json
  { "name": "Wallet", "currency": "USD" }
  ```

### Categories (auth required)
- `GET /api/categories` → list non-archived (ordered by Type then Name)
- `POST /api/categories`
  ```jsonc
  { "name": "Groceries", "type": 0, "parentId": null } // 0=Expense, 1=Income
  ```

### Transactions (auth required)
- `GET /api/transactions?fromUtc=...&toUtc=...&type=...&accountId=...&categoryId=...&page=1&pageSize=20`
  - Sorted by `OccurredAtUtc` desc, then `CreatedAtUtc` desc; returns paged result
- `POST /api/transactions`
  ```jsonc
  {
    "accountId": "<guid>",
    "type": 0,                 // 0=Expense, 1=Income
    "amount": 42.50,
    "currency": "USD",
    "categoryId": "<guid-or-null>",
    "occurredAtUtc": null,     // null -> uses server UTC now
    "note": "Groceries"
  }
  ```

## Tests
- Unit tests cover auth, accounts, categories, and transactions (including validation, currency/account/category checks, and paging).

## ⚙️ Getting Started (local)
1. **Prereqs**: .NET SDK (current LTS), SQL Server (or change provider), PowerShell/Bash.
2. **Config**: set **`appsettings.json`**:
   - `"Jwt": { "Issuer", "Audience", "Key", "AccessTokenMinutes" }`
   - Connection string in Infrastructure `AddInfrastructure` (switch to your DB or use `Default` from config).
3. **Database**: create/migrate the DB (EF Core Migrations).  
   _If you don’t have migrations yet, add them_:
   ```bash
   dotnet ef migrations add Init --project BudgetTracker.Infrastructure --startup-project BudgetTracker.Api
   dotnet ef database update --project BudgetTracker.Infrastructure --startup-project BudgetTracker.Api
   ```
4. **Run**:
   ```bash
   dotnet run --project BudgetTracker.Api
   ```
   Browse Swagger UI at `/swagger`.
5. **Test**:
   ```bash
   dotnet test
   ```

## Tech stack
- ASP.NET Core, EF Core, Identity, JWT Bearer
- FluentValidation, ProblemDetails
- Serilog (console + rolling files)
- Swagger/OpenAPI
- xUnit, FluentAssertions, Moq

## Conventions & Behavior
- Duplicate account names per user are rejected.
- Category parent must match child type (Expense/Income).
- Transaction currency must equal the account currency.
- Consistent error payloads via ProblemDetails; validation errors are grouped by property.
