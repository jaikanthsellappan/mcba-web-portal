### Project Structure:
```
MCBA Solution
┣ mcbaMVC                 → Customer Internet Banking Website (Presentation Layer)
┣ mcbaAdminAPI            → Secure Admin Web API (Business + Data Layer)
┣ mcbaAdminPortal         → Admin Portal Website (Consumes API)
┣ mcbaMVC/Data            → EF Core DbContext and Migrations
┣ mcbaMVC/Controllers     → Deposit, Withdraw, Transfer, Profile, Statement, BillPay
┣ mcbaMVC/ViewModels      → Strongly-typed ViewModels for MVC Views
┣ mcbaMVC/Test            → Unit test to test the backend end points
┣ mcbaAdminAPI/Repositories → Repository Pattern Implementation
┣ Trello/                 → Screenshots showing project progress
┗ README.md
```
```

**Paste the above snippet as-is in your README.md**. This preserves the indents and special characters, and ensures the project structure renders properly within a monospaced code block.


## 1. Application Overview

This project implements a three-tier ASP.NET Core MVC banking system built on .NET 9.0 and Azure SQL Server, following the assignment’s architecture:

# mcbaMVC – Customer Website

    Deposit, Withdraw, Transfer

    My Statements (paged transactions)

    My Profile (view / edit / change password)

    BillPay management with persistent scheduler

    Authentication via hashed passwords (SimpleHashing.Net)

    Login and Logout using sessions

    EF Core ORM / Data Annotations for validation

# mcbaAdminAPI – Web API for administrative operations

    Exposes endpoints for Payee and BillPay management

    Uses Repository Pattern for clean data access

    JWT Authentication 

    Swagger UI with JWT input for testing

# mcbaAdminPortal – Admin Interface

    Independent ASP.NET Core MVC site consuming the Admin API

    Manage Payees (Filter / Edit)

    Block / Unblock Scheduled Bill Payments

    Responsive Bootstrap UI with TempData feedback

## Setup Instructions

    1) Clone the repo from GitHub (https://github.com/jaikanthsellappan/mcba-web-portal).

    2) Open solution in Visual Studio 2022.

    3) Set connection string in appsettings.json

    4) Run EF migrations

    5) start projects:

        mcbaMVC → Customer site 

        mcbaAdminAPI → Swagger API 

        mcbaAdminPortal → Admin portal 
