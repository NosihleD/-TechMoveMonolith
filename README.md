# TechMove — Part 2 Core Prototype & Unit Testing

This is a monolithic ASP.NET Core MVC prototype implementing the Part 2 brief.

## Requirements covered
- SQL Server database with Entity Framework Core.
- Client, Contract and ServiceRequest entities and relationships.
- Signed Agreement PDF upload to the server and UI download.
- Contract status workflow: Expired and On Hold contracts cannot create Service Requests.
- LINQ contract filtering by date range and status.
- HttpClient consumption of the free open.er-api.com USD rates endpoint.
- USD→ZAR calculation stored with each ServiceRequest.
- Separate xUnit test project covering currency calculation, PDF validation and workflow rules.

## Run in Visual Studio
1. Open `TechMove.sln`.
2. Make sure SQL Server LocalDB is installed (Visual Studio normally includes it).
3. Check `appsettings.json` connection string. Change it to your SQL Server/Azure SQL connection if required.
4. Build Solution.
5. Run the `TechMove` project.
6. The prototype uses EF Core `EnsureCreatedAsync()` on startup to create `TechMoveDb` for quick demonstration.
7. The hand-written SQL Server migration script is in `Database/001_InitialCreate.sql` for submission/evidence.

## Proper EF migration option
For a formal EF migration history, run in Package Manager Console:
`Add-Migration InitialCreate`
`Update-Database`

Or CLI:
`dotnet ef migrations add InitialCreate`
`dotnet ef database update`

If you use migrations instead of the quick prototype startup creation, replace `EnsureCreatedAsync()` with `Database.MigrateAsync()` after adding the migration.

## Unit testing
In Visual Studio: Test > Test Explorer > Run All.
Expected tests: 10 passing.

CLI:
`dotnet test`

## Suggested demonstration flow
1. Open dashboard.
2. Create a Client.
3. Create an Active Contract and upload a real PDF signed agreement.
4. Show Contracts filter using Status and date range.
5. Open contract Details and download the uploaded PDF.
6. Create a Service Request; enter a USD amount and show the live rate and calculated ZAR amount.
7. Show the Service Request list and stored USD/rate/ZAR values.
8. Create an Expired or On Hold Contract and demonstrate that it is excluded from eligible contracts / rejected by the workflow.
9. Open Test Explorer and show all tests passing.

## Notes
The exchange API is external, so an internet connection is required when creating a Service Request. The application saves the exchange rate used at the time of creation so the stored ZAR amount is reproducible.
