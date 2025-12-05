# Multi-Tenancy Support

## Overview
This application now supports multi-team (tenant) separation. Each key data model (`Membership`, `User`, `Payment`, `Transaction`) includes a `TenantId` property. All relevant API routes and Razor pages use `{tenantId}` in their paths and filter data accordingly.

## Admin User
The admin user is tenant-independent and can manage all tenants. The admin is created without a `TenantId`.

## Routing
- API endpoints: `/api/{tenantId}/...`
- Razor pages: `/{tenantId}/players`, etc.

## Database Migration
A migration (`AddTenantIdToModels`) adds the `TenantId` column to all relevant tables. Run the migration after pulling these changes:

```
dotnet ef database update
```

## User Registration
Regular users should be assigned a `TenantId` on registration. Admin user creation does not set a `TenantId`.

## Example Usage
- To view players for a team: `/{tenantId}/players`
- To query transactions for a team: `/api/{tenantId}/transactions`

## Next Steps
- Update any custom queries or UI logic to respect tenant boundaries.
- Ensure tenant selection is available in the UI for admins.
