# TechMove Part 2 — Fast Submission Guide

## 1. Open and run
Open `TechMove.sln` in Visual Studio 2022.

Set **TechMove** as the startup project and press **F5**.

If LocalDB is installed, the application will create `TechMoveDb` automatically on first run. If your lecturer requires a specific SQL Server instance, change `DefaultConnection` in `appsettings.json` and run `Database/001_InitialCreate.sql` in SSMS.

## 2. Create demo data
Use this exact sequence for the video:

### Client
Create:
- Name: Global Logistics SA
- Contact Details: manager@globallogistics.example / 011 555 0100
- Region: Southern Africa

### Active Contract
- Client: Global Logistics SA
- Start Date: today
- End Date: one year from today
- Status: Active
- Service Level: Gold 24x7
- Signed Agreement: choose any genuine PDF file

Show that the PDF can be downloaded from Contract Details.

### Service Request
- Contract: Active contract
- Description: International freight monitoring
- Cost USD: 100

The application calls the external exchange-rate API, displays the current USD→ZAR rate and calculates the ZAR amount automatically. Submit it and show the stored USD, rate and ZAR values.

## 3. Demonstrate the workflow rule
Create a second contract with status **Expired** and another with **On Hold**.

Open **Service Requests > New Service Request**. Only eligible contracts appear in the selection list.

This demonstrates the rule that a ServiceRequest cannot be created for Expired or On Hold contracts. The same rule is enforced server-side in `ContractWorkflowService`, not only by the UI.

## 4. Demonstrate LINQ filtering
Open **Contracts**.

Use:
- Start from date
- End by date
- Status = Active

Click Filter and show that the table changes. Explain that `ContractsController.Index` applies the filters using Entity Framework LINQ.

## 5. Run unit tests
In Visual Studio:

**Test → Test Explorer → Run All**

Expected test count: **10 tests**.

Tests cover:
- USD→ZAR calculation using a supplied rate
- Currency rounding
- `.exe` file rejection
- incorrect PDF content type rejection
- valid PDF acceptance
- Draft/Active/Expired/On Hold workflow behaviour
- Expired contract exception
- invalid contract date validation

Take a screenshot of Test Explorer with all 10 tests passing. This screenshot must be your real local execution result; do not use a generated/mock screenshot.

## 6. GitHub submission
Create a new GitHub repository, for example:

`TechMove-Part2-Monolith`

From the project folder:

```powershell
git init
git add .
git commit -m "Complete TechMove Part 2 prototype and unit tests"
git branch -M main
git remote add origin YOUR_GITHUB_REPOSITORY_URL
git push -u origin main
```

Do not commit real connection strings, passwords, API secrets, or private signed agreements.

## 7. Video script
Keep the video focused on marking requirements:

1. **Introduction (30 sec):** State that TechMove Part 2 is an ASP.NET Core MVC monolith using EF Core and SQL Server.
2. **Database (1 min):** Show Client, Contract and ServiceRequest entities and their relationships.
3. **Client workflow (30 sec):** Create a client.
4. **Contract workflow (2 min):** Create a contract, upload a PDF, show the database/UI record and download the PDF.
5. **Search/filter (1 min):** Demonstrate date/status filtering.
6. **Currency integration (2 min):** Open Service Request creation, explain HttpClient, live USD→ZAR rate, automatic calculation and saved rate/ZAR amount.
7. **Business rule (1 min):** Demonstrate Expired/On Hold restriction.
8. **Async/await (1 min):** Point out `ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, file `CopyToAsync`, and currency `GetAsync`.
9. **Unit tests (2 min):** Open Test Explorer, Run All, show all 10 passing. Open the currency, PDF and workflow test classes.
10. **Closing (30 sec):** Show GitHub repository and briefly summarise that the prototype satisfies the Part 2 requirements.

## 8. Evidence screenshots checklist
Capture these real screenshots after running locally:

- Application dashboard
- Client list with created client
- Contract creation form showing PDF upload
- Contract list with status/date filter applied
- Contract details with Download PDF link
- Service Request creation page showing USD amount, live rate and ZAR calculation
- Service Request list showing saved USD/rate/ZAR values
- Expired/On Hold workflow demonstration
- Visual Studio Test Explorer showing 10/10 tests passed
- Solution Explorer showing the main project and `TechMove.Tests`
- GitHub repository containing the solution
