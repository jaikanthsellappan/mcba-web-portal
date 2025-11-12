# MCBA Web Portal

A three-tier ASP.NET Core MVC banking solution, built on .NET 9.0 and Azure SQL Server, designed for enterprise-grade modularity, security, and maintainability.

##  Project Structure

```
MCBA Web Portal/
├── mcbaMVC                  → Customer Internet Banking Website (Presentation Layer)
│   ├── Data                 → EF Core DbContext and Migrations
│   ├── Controllers          → Deposit, Withdraw, Transfer, Profile, Statement, BillPay
│   ├── ViewModels           → Strongly-typed ViewModels for MVC Views
│   └── Test                 → Unit tests for backend endpoints
├── mcbaAdminAPI             → Secure Admin Web API (Business + Data Layer)
│   └── Repositories         → Repository Pattern Implementation
├── mcbaAdminPortal          → Admin Portal Website (Consumes API)
├── Trello/                  → Screenshots showing project progress
└── README.md
```

##  Application Overview

**MCBA Web Portal** implements a scalable three-layered banking platform featuring:

### 1. Customer Portal (`mcbaMVC`)
- Deposit, Withdraw, and Transfer operations
- My Statements (paged transactions)
- My Profile (view/edit/change password)
- BillPay management with persistent scheduler
- Authentication via hashed passwords
- Secure login/logout using sessions
- EF Core ORM & Data Annotations for validation

### 2. Admin Web API (`mcbaAdminAPI`)
- Exposes endpoints for payee & bill payment management
- Repository Pattern for maintainable data access logic
- JWT Authentication for all admin operations
- Swagger UI with JWT input for secure API testing

### 3. Admin Portal (`mcbaAdminPortal`)
- Standalone ASP.NET Core MVC app consuming the Admin API
- Manage payees (filter/edit)
- Block/unblock scheduled bill payments
- Responsive Bootstrap UI with dynamic feedback and notifications


##  Setup Instructions

1. **Clone the repository:**
git clone https://github.com/jaikanthsellappan/mcba-web-portal.git

2. **Open the solution in Visual Studio 2022.**

3. **Set your Azure SQL connection string**  
Update `appsettings.json` for each project to point to your database.

4. **Apply Entity Framework Migrations**  
Use Package Manager Console


5. **Start the projects:**
- `mcbaMVC` → Customer site
- `mcbaAdminAPI` → Swagger API for admins
- `mcbaAdminPortal` → Admin portal

##  Technologies Used

- **Frontend:** ASP.NET Core MVC, Bootstrap 5
- **Backend:** ASP.NET Core, EF Core, JWT, Swagger, Repository Pattern
- **Database:** Azure SQL Server, EF Core Migrations
- **Testing:** xUnit (unit tests in `mcbaMVC/Test`)
- **DevOps:** Git, Visual Studio 2022


##  Security Highlights

- All passwords securely hashed with SimpleHashing.Net
- JWT Authentication for protected admin and API routes
- Validation and authorization enforced on server endpoints


##  Progress & Screenshots

Project progress screenshots are included in the `/Trello` directory.


> For any additional setup or troubleshooting details, see code comments inside each project and the inline documentation in the API.

