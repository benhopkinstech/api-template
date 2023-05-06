# api-template

This is a great basis for starting your own project that requires an API with user accounts, making use of the following

- C# Minimal API
- Postgres Database
- Entity Framework
- JWT
- SendGrid
- Integration Tests
- Unit Tests

Microservices are unnecessary for a lot of projects so this takes a monolithic approach with the flexibility to be able to split things out in the future 

Each schema has it's own module and DbContext keeping things seperate whilst still being in the same project

The `Identity` module that comes with this can be changed as you see fit for your application

## Installation

- Create two Postgres databases, running the [scripts](scripts) required on each one
- Amend the `Database` connection string in [appsettings.json](Api/appsettings.json) to point to the database you intend to use for the application
- Amend the `Database` connection string in [integrationsettings.json](Api.Tests/integrationsettings.json) to point to the database you intend to use for testing

### Configuration

All of these are optional to run the application but we recommend changing the `Jwt:TokenSecret` right away

If you wish to send emails from the application you will need to configure SendGrid, you can leave these options as disabled but features such as email verification and password reset will not be available

<details>
  <summary>Jwt</summary>

`TokenSecret` - Your own secret for signing the token

`Issuer` - The issuer that is assigned to the token

`Audience` - The audience that is assigned to the token

`TokenExpiryMinutes` - How many minutes the token is valid for

`RefreshExpiryHours` - How many hours the refresh token is valid for

</details>

<details>
  <summary>SendGrid</summary>

`ApiKey` - API Key for your SendGrid application

`Email` - A verified sender email in your SendGrid

`Name` - The verified sender name against the email in your SendGrid

`VerificationLinkTemplateId` - A Dynamic Template Id - Add a button to the template and set the button url to {{url}}

`EmailChangedTemplateId` - A Dynamic Template Id - Place {{newEmail}} somewhere within the template

`ResetLinkTemplateId` - A Dynamic Template Id - Add a button to the template and set the button url to {{url}}

`ResetUrl` - A link to your frontend that will provide you with the code to reset

</details>

<details>
  <summary>Identity</summary>

`VerificationRequired` - A user must be verified in order to login

`VerificationExpiryHours` - How many hours the verification link is valid for

`VerificationRedirectUrlSuccess` - A link to your frontend to show that the user was verified

`VerificationRedirectUrlFail` - A link to your frontend to show that the user wasn't verified

`ResetExpiryHours` - How many hours the reset link is valid for

</details>

## Scaffolding

The project takes a database first design approach, each of the schemas that are created should have it's own script file within the [scripts](scripts) folder

The commands below can be used to generate the required files from each database schema

### Identity

```
Scaffold-DbContext Name=ConnectionStrings:Database Npgsql.EntityFrameworkCore.PostgreSQL -NoOnConfiguring -NoPluralize -Context IdentityContext -Schema identity -ContextDir Modules/Identity/Data -OutputDir Modules/Identity/Data/Tables -Force
```

