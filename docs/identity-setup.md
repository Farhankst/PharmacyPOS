# PharmacyPOS — Identity setup

## Requirements

- .NET 10 SDK and EF Core CLI 10.x.
- SQL Server database specified by the Web project's DefaultConnection.
- Back up the database before applying migrations. No product migration or product table is changed by the Identity migration.
- Run from the repository root, on branch Farhan. Use HTTPS for login.

## Apply the existing Identity migration

There are two DbContext classes with the same short name. Specify the full namespace.
The existing Identity migration is 00000000000000_CreateIdentitySchema in PharmacyPOS.Web.
It creates eight AspNet* tables and uses the existing default EF migration history table.
Do not create another InitialCreate or manually create these tables.

PowerShell:

```powershell
dotnet restore
dotnet build
dotnet ef migrations list --project PharmacyPOS.Web --startup-project PharmacyPOS.Web --context PharmacyPOS.Web.Data.ApplicationDbContext
dotnet ef database update --project PharmacyPOS.Web --startup-project PharmacyPOS.Web --context PharmacyPOS.Web.Data.ApplicationDbContext
```

If the EF CLI is missing, install a compatible 10.x dotnet-ef tool. If the database has partially created Identity tables or a pending-model error, stop and inspect the migration/model instead of forcing the update or deleting tables.

SQL verification (run against the configured PharmacyPOS database):

```sql
SELECT name FROM sys.tables WHERE name LIKE 'AspNet%' ORDER BY name;
SELECT MigrationId FROM dbo.__EFMigrationsHistory ORDER BY MigrationId;
```

## Create the first administrator

Run locally once after applying the migration. Do not commit credentials or put them in appsettings. Enter your administrator username (e.g. admin) and password. In Development, a minimum of six characters is sufficient; no complexity rules apply. Production retains the 12-character complexity policy. Never use a simple development password on a deployed system.

```powershell
$env:IdentityBootstrap__Enabled = 'true'
$env:IdentityBootstrap__Username = Read-Host 'Initial admin username'
$secret = Read-Host 'Initial admin password' -AsSecureString
$env:IdentityBootstrap__Password = [System.Net.NetworkCredential]::new('', $secret).Password
try {
    dotnet run --project PharmacyPOS.Web --launch-profile https
} finally {
    Remove-Item Env:IdentityBootstrap__Enabled, Env:IdentityBootstrap__Username, Env:IdentityBootstrap__Password -ErrorAction SilentlyContinue
    Remove-Variable secret -ErrorAction SilentlyContinue
}
```

After startup succeeds, stop it with Ctrl+C to clear the environment variables, then restart normally with the HTTPS launch profile. If the project uses a differently named HTTPS launch profile, select that profile instead.
Provisioning creates fixed roles Admin, Manager and Cashier and creates the first Admin only if none exists. It will not elevate an existing account or overwrite an existing password. Do not deploy development accounts with weak passwords; rotate them before deployment. Disable bootstrap after provisioning; use the admin login at /Account/Login.

## Role matrix

| Feature | Admin | Manager | Cashier |
|---|---|---|---|
| Login / own profile / password | Yes | Yes | Yes |
| View products | Yes | Yes | Yes |
| Create/edit products | Yes | Yes | No |
| Create/manage staff accounts | Yes | No | No |
| Assign Manager/Cashier role | Yes | No | No |

The initial Admin is protected from staff-management edits and account deletion. Additional administrator provisioning, arbitrary role definitions, email delivery and email-based recovery are not enabled. Staff passwords can be reset by Admin. The existing two-factor and passkey components are retained, but must be tested before production.

## Smoke tests

1. Anonymous request to /products and /Account/Users: denied or redirected to login. Anonymous API calls: 401.
2. Admin login, logout, change own password, create Manager and Cashier.
3. Manager can create/edit products; cannot access user administration.
4. Cashier can list products but cannot see Edit/Create links or invoke their routes or POST/PUT API endpoints (403).
5. Admin changes a staff role, resets password, disables and reenables staff. Verify the old session loses product access and a disabled user cannot log in.
6. Wrong password five times: temporary lockout. Verify password policy and duplicate username rejection.
7. Verify Products data and existing Product Edit functionality are preserved.
8. Verify antiforgery protection on admin forms and logout. Test passkeys and two-factor authentication separately.

Run build, migration and smoke tests on a development database before deployment. The repository changes were not compiled or executed against SQL Server by the connector.
