# FleetFlow

Multi-tenant delivery management API built with .NET 10 and SQL Server.

## Local setup

Run these commands in PowerShell from the repository root. Install the .NET 10 SDK and SQL Server LocalDB first.

```powershell
dotnet restore
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))) --project FleetFlow.Api
dotnet ef database update --project FleetFlow.Infrastructure --startup-project FleetFlow.Api
dotnet run --project FleetFlow.Api
```

The development connection string targets `(localdb)\MSSQLLocalDB`, database `FleetFlowDb`. Configure `ConnectionStrings:FleetFlow` with user secrets or `ConnectionStrings__FleetFlow` to use a different server. Keep the generated JWT key stable between runs; changing it invalidates previously issued tokens. Production also needs `Jwt:Issuer`, `Jwt:Audience`, and a securely stored `Jwt:Key`.

## Register and sign in

`POST /api/auth/register` creates a company tenant and its first account atomically:

```json
{
  "companyName": "Acme Logistics",
  "email": "owner@example.com",
  "password": "Use-your-own-strong-password1!"
}
```

The response is `201 Created` with `userId`, `tenantId`, and `email`. Passwords require at least 12 characters with uppercase, lowercase, a digit, and a non-alphanumeric character. Registration never grants the platform `Administrator` role. Existing tenants are unaffected; joining an existing tenant by invitation is a separate feature.

`POST /api/auth/login` accepts `email` and `password` and returns `accessToken`, `tokenType`, `expiresAtUtc`, `userId`, and `tenantId`. Email matching is case insensitive. Use the token as `Authorization: Bearer <accessToken>` when calling protected endpoints. The token expires after 30 minutes; sign in again to obtain another token. There is no server session to sign out of: clients discard the token, and a previously issued token stays valid until expiry.

Five consecutive failed password attempts lock the account for 15 minutes. Unknown email, wrong password, and locked account responses all return `401` with the same message. Tenant and role claims come from stored account data. Tenant users can create, read, start, complete, and cancel their own jobs; platform tenant creation remains restricted to administrators.

This milestone implements account creation and password login. Email verification, password recovery, invitations, MFA, and refresh tokens are not yet implemented.

## Tests

```powershell
dotnet test FleetFlow.slnx
```

Integration tests use an isolated SQLite in-memory database and a test-only JWT key, so SQL Server and development secrets are not required for tests.
