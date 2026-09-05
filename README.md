# SupportFlow

SupportFlow is a compact IT Service Desk and ERP Access Management portfolio project. It demonstrates role-based workflows, validation, asset tracking, and an audit trail with synthetic data only.

## Features

- Employees create and follow support tickets.
- IT Support moves tickets through `Open → In Progress → Resolved → Closed` and tracks company assets.
- Employees request ERP module access; Managers approve or reject it; IT Support fulfills approved requests.
- IT Support and Admin can inspect the latest audit events.
- Dashboard summarizes active work.

## Demo accounts

All accounts use the password `Demo123!`.

| Role | Email |
| --- | --- |
| Employee | `employee@supportflow.local` |
| IT Support | `support@supportflow.local` |
| Manager | `manager@supportflow.local` |
| Admin | `admin@supportflow.local` |

## Run locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), then run:

```bash
dotnet restore
dotnet run
```

Open the local URL printed in the terminal. SQLite creates `app.db` and seeds demo data on first run.

Run the workflow check with:

```bash
dotnet run -- --self-check
```

## Technology

ASP.NET Core Razor Pages, ASP.NET Core Identity, Entity Framework Core, SQLite, and native CSS.

> This is a portfolio demo. Before production use, replace `EnsureCreated` with migrations, remove demo credentials, add secret management, and configure a real email confirmation flow.
