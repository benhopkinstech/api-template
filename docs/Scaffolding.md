# Scaffolding

## Identity

```
Scaffold-DbContext Name=ConnectionStrings:Database Npgsql.EntityFrameworkCore.PostgreSQL -NoOnConfiguring -NoPluralize -Context IdentityContext -Schema identity -ContextDir Modules/Identity/Data -OutputDir Modules/Identity/Data/Tables -Force
```