Name & Student Id:
Jaikanth Sellappan - s4062691
Sourav Madan - s4069038

Git hub repo url => https://github.com/rmit-wdt-s2-2025/s4062691-s4069038-a1/tree/master

### Design Pattern:

## 1. Dependency Injection (DI)

Dependency Injection provides the foundation: all services are constructed and passed explicitly, not created internally.

Purpose & Advantages in this project:

Promotes loose coupling between components.

Allows swapping implementations (e.g., mock vs real DB).

Improves testability by injecting stubs/fakes.

Centralizes object creation in Program.cs, simplifying maintenance.

Implementation in this project:

Defined service contracts: IBankingService, ILoginService, IImportService.

Added IDbConnectionFactory + SqlConnectionFactory for database connections.

Converted BankingService, LoginService, and ImportService into instance classes taking dependencies via constructors.

Program.cs manually wires up dependencies (injector role) and passes them into the console app.

Where in the code:

/Database/IDbConnectionFactory.cs

/Database/SqlConnectionFactory.cs

/Services/Abstractions.cs

/Services/BankingService.cs, /Services/LoginService.cs, /Services/ImportService.cs

/BankingConsole.cs (depends on IBankingService)

Program.cs (constructs and injects dependencies).

## 2. Adapter

The Adapter pattern integrates the external web service as a customer source in a clean, swappable way.

Purpose & Advantages in this project:

Decouples the import logic from the concrete customer data source.

Enables flexibility: the system can fetch customers from a web service now, but later a file or mock source can be injected.

Supports the Open/Closed Principle: new sources are added without modifying existing logic.

Implementation in this project:

Target: ICustomerSource – defines GetCustomersAsync().

Adaptee: existing CustomerWebService.FetchAsync(IConfiguration) (unchanged).

Adapter: CustomerWebServiceAdapter implements ICustomerSource and delegates to CustomerWebService.

In Program.cs, the client (importer) uses ICustomerSource instead of depending directly on the web client.

Where in the code:

/Services/ICustomerSource.cs (Target interface).

/Services/CustomerWebServiceAdapter.cs (Adapter implementation).

Program.cs – uses customerSource.GetCustomersAsync() when importing customers.

## 3. Proxy Pattern

The Proxy pattern adds auditing transparently, without modifying business or UI logic.

Purpose & Advantages in this project:

Adds auditing/logging of critical banking operations without modifying BankingService or UI logic.

Separation of concerns: business logic remains in BankingService, while logging resides in the proxy.

Makes it easy to change the logging destination (console, file, DB) via dependency injection.

Implementation in this project:

Real subject: BankingService : IBankingService.

Proxy: BankingServiceProxy : IBankingService, wraps the real service and logs deposits, withdrawals, and transfers.

Logger abstraction: IAuditLogger + ConsoleAuditLogger.

In Program.cs, BankingServiceProxy wraps BankingService before being passed into BankingConsole.

Where in the code:

/Infrastructure/IAuditLogger.cs (Logger abstraction + console logger).

/Services/BankingServiceProxy.cs (Proxy implementation).

Program.cs (wraps real BankingService with BankingServiceProxy).

### Class Library

A separate Class Library project s4062691_s4069038_a1.Core contains domain models, service abstractions, and cross-cutting contracts that are shared across the application. The console app references this library via a relative project reference.

Purpose & Advantage

Separation of concerns: Domain and contracts are isolated from infrastructure/console concerns.

Reusability & testability: The same contracts/models can be reused in Assignment 2 or tests without pulling in UI/ADO.NET code.

Lower coupling: The app depends on interfaces from the library, not on concrete implementations.

Implementation

Created a new project: Class Library (.NET 9) named s4062691_s4069038_a1.Core.

Added a project reference from the console app to the library (relative <ProjectReference/>).

Moved contracts and domain types into the library:

Core/Models: Customer, Account, Transaction, Login

Core/Services: IBankingService, ILoginService, IImportService, ICustomerSource

Core/Infrastructure: IAuditLogger

Core/Utilities: PasswordVerifier (PG task; zeroized char[]; Rfc2898DeriveBytes)

Kept implementations in the console app project: BankingService, ImportService, LoginService, BankingServiceProxy, CustomerWebServiceAdapter, SqlConnectionFactory, etc.

Where in the code

Class library: /s4062691_s4069038_a1.Core/\*

Console app: references the library and implements infrastructure and UI.

The reference is visible in the console app .csproj as a <ProjectReference>.

Justification
A dedicated class library enforces clean boundaries (domain + contracts vs. infrastructure), supports Dependency Injection (consumers depend on interfaces defined in the library), and makes the solution extensible for Assignment 2. It adheres to the assignment requirement to implement and use a custom class library (not a NuGet package).

### Use of C# async / await

## Purpose & advantage (in this project)

Non‑blocking I/O: Database calls (SqlCommand.ExecuteReaderAsync, ExecuteNonQueryAsync, ExecuteScalarAsync) and HTTP calls (HttpClient.GetStringAsync) are I/O‑bound. Using await frees the thread while the OS waits, improving responsiveness and scalability of the console loop.

Structured sequencing: await expresses “don’t continue until this completes,” which avoids race conditions (e.g., we must await the import before showing the menu; we must open the DB connection before executing commands).

Cleaner error flow: Exceptions thrown by awaited tasks are re‑thrown at the await point, so normal try/catch works around asynchronous operations.

Better UX: The login loop uses await Task.Delay(...) for brief messages without blocking the thread; the menu remains responsive between operations.

## How async/await changed & benefited the design

Method contracts: I/O methods return Task/Task<T> instead of immediate values. This allows callers to compose work (await in sequence or run tasks concurrently if ever needed).

Clear boundaries in DI: Our service interfaces (IBankingService, ILoginService, IImportService, ICustomerSource) are async‑first, so any implementation (real DB, proxy, mock) naturally supports non‑blocking behavior.

Deterministic ordering where needed: We explicitly await before using results (e.g., import, fetching accounts, statement pages), so the console never reads half‑baked data.

Avoided antipatterns: We never block on tasks (.Result / .Wait()), preventing deadlocks typical in sync‑over‑async. In a console app we also don’t need ConfigureAwait(false).

## Where it is used (files & key lines/methods)

# Program.cs

Import flow:
if (await importer.IsDatabaseEmptyAsync()) { var customers = await customerSource.GetCustomersAsync(); await importer.ImportAsync(customers); }

App loop:
var customerId = await login.SignInLoopAsync(); await console.RunAsync(customerId);

# BankingConsole.cs

Entry: public async Task RunAsync(int customerId)

Menu operations:
await \_banking.GetAccountsAsync(...), await \_banking.GetCustomerAsync(...)
await \_banking.DepositAsync(...), await \_banking.WithdrawAsync(...), await \_banking.TransferAsync(...)
Statements paging/export: await \_banking.GetStatementPageAsync(...), await \_banking.ExportAllTransactionsAsync(...)

Services/BankingService.cs (ADO.NET async everywhere)

await \_db.OpenAsync()

await cmd.ExecuteScalarAsync(), await cmd.ExecuteReaderAsync(), await cmd.ExecuteNonQueryAsync()

await tx.CommitAsync(); helper methods like GetAccountSnapshotAsync, UpdateBalanceAsync, InsertTxnAsync are all async.

Services/LoginService.cs

Loop: public async Task<int> SignInLoopAsync()

UX delay: await Task.Delay(900)

DB lookup: await \_db.OpenAsync(), await cmd.ExecuteReaderAsync(), await rdr.ReadAsync()

Services/CustomerWebService.cs

HTTP: var json = await client.GetStringAsync(url);

Services/CustomerWebServiceAdapter.cs

Adapter returns Task<List<Customer>> by delegating to the async web method.

Services/BankingServiceProxy.cs

Proxy forwards async calls and awaits inner operations before logging (e.g., await \_inner.TransferAsync(...)).
