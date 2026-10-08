# Contract Monthly Claim System (CMCS)
**PROG6212 POE | ST10438679**

An ASP.NET Core MVC (.NET 8) system for submitting and approving lecturers' monthly claims. Lecturers submit hours with supporting documents, claims move through a two-stage approval, and HR produces invoices and reports.

## Features
- **Four roles** with their own controllers and views: Lecturer, Programme Coordinator, Academic Manager and HR.
- **Two-stage approval workflow** (coordinator, then manager) with colour-coded claim statuses.
- **Encrypted document storage**: supporting documents are encrypted by `EncryptionService`, kept in `App_Data/Documents` outside the web root, and served only to authorised users.
- **PDF invoice generation** with QuestPDF (`InvoiceService`).
- **Entity Framework Core + SQL Server** with migrations and seeded demo users.
- **Unit tests** with xUnit and Moq for the lecturer controller and the claim model.

## Tech stack
C# · .NET 8 · ASP.NET Core MVC · Entity Framework Core · SQL Server · QuestPDF · xUnit · Moq · Bootstrap

## Solution structure
```
PROG6212_POE.sln
├── PROG6212_POE/          # MVC app (Controllers, Models, Services, Data, Migrations, Views)
└── PROG6212_POE.Test/     # xUnit tests
```

## Getting started
1. Open `PROG6212_POE.sln` in Visual Studio 2022.
2. Check the connection string in `PROG6212_POE/appsettings.json`.
3. In the Package Manager Console run `Update-Database` (or `dotnet ef database update --project PROG6212_POE`).
4. Run the app. Demo users for each role are seeded in `Data/CMCSDbContext.cs`.
5. Run the tests with `dotnet test`.

If a folder shows a dashed outline in Visual Studio ("excluded from project"), right-click it and choose **Include in Project**.

## Changes made after lecturer feedback
- Colour coding added to every view that shows a claim status.
- Document storage moved to `App_Data/Documents`, with protections against unauthorised file access.
- Unit tests fixed and verified by peers.
- More descriptive commit messages.

## AI usage disclosure
Claude AI was used on 19 November 2025 to speed up creating UI elements that matched the existing views. [Shared conversation](https://claude.ai/share/e21a06eb-c01c-47c3-84f6-e32eb702a338).

## Demo
[Watch on YouTube](https://youtu.be/2eQNJYsmzCc). The PowerPoint and the written report (`.docx` / `.pdf`) are in this repository.
