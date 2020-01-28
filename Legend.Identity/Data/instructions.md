# Migration instructions for IdentityServer project

## Migrations using terminal

### Create migrations

dotnet ef migrations add InitialIdentityServerApplicationDbMigration -c ApplicationDbContext -o Data/Migrations

dotnet ef migrations add InitialIdentityServerPersistedGrantDbMigration -c PersistedGrantDbContext -o Data/Migrations/PersistedGrantDb
dotnet ef migrations add InitialIdentityServerConfigurationDbMigration -c ConfigurationDbContext -o Data/Migrations/ConfigurationDb

### Update database

dotnet ef database update --context ApplicationDbContext

dotnet ef database update --context ConfigurationDbContext
dotnet ef database update --context PersistedGrantDbContext


### add custom fields to ApplicationUser

dotnet ef database update --context ApplicationDbContext

dotnet ef migrations add "IsActive"


