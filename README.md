# 🐸 WebAPI EF FluentAPI

<div align="center">

![WebAPI EF FluentAPI — green frog logo](webapi-ef-fluentapi-logo.png)

**🌙 Dark mode · 💚 Forest green · ⚡ ASP.NET Core · 🗃️ JSON storage**

[![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C%23](https://img.shields.io/badge/C%23-API-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF_Core-Fluent_API-27783F)](https://learn.microsoft.com/ef/core/modeling/)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/)

</div>

## ✨ Overview

A dark-green ASP.NET Core 8 REST API starter with a smiling frog and crescent-moon visual identity. The application uses one shared **SVG** logo in the site header, home hero, monitor wallboard and favicon.

The logo and dashboard preview below are the PNG assets uploaded to this repository. The dashboard image is a **design mockup**, not a verified screenshot of the running application.

## 🖥️ Dashboard preview

![Dark-green WebAPI EF FluentAPI dashboard design](webapi-ef-fluentapi-dashboard.png)

*Dark-mode dashboard design concept featuring the frog identity, navigation, API panels, and green accents. Some pictured functionality is illustrative and is not implemented in this starter.*

## 🧰 Technology stack

| Icon | Technology | Role |
|---|---|---|
| 💻 | C# / .NET 8 | Application runtime |
| 🌐 | ASP.NET Core | Minimal REST API |
| 🧩 | Entity Framework Core InMemory | EF model demonstration |
| 🛠️ | Fluent API | Entity configuration via `IEntityTypeConfiguration<T>` |
| 🗂️ | JSON files | Actual persistence, separate from EF Core |
| 📖 | Swagger / OpenAPI | Interactive API documentation |
| 🌚 | HTML / CSS / JavaScript | Responsive dark-green dashboard |

**Important:** EF Core is used for model configuration and its in-memory provider; it does **not** persist the JSON data. The JSON store is implemented independently.

## 🚀 Quick start

```bash
git clone https://github.com/1x4x/20-webapi-ef-fluentapi.git
cd 20-webapi-ef-fluentapi
dotnet restore
dotnet run
```

Open the local address printed by `dotnet run`. Visit `/swagger` to explore the API.

## 🧭 API endpoints

| Method | Endpoint | Description |
|---|---|---|
| 🟢 GET | `/api/health` | Health and storage details |
| 🟢 GET | `/api/boards` | List boards |
| 🟢 GET | `/api/boards/{id}` | Get one board |
| 🔵 POST | `/api/boards` | Create board |
| 🟠 PUT | `/api/boards/{id}` | Replace board |
| 🔴 DELETE | `/api/boards/{id}` | Delete board |


## 📅 Swedish namedays API

The API includes **628 nameday records** from the supplied Swedish list. Seed data lives in `Data/namedays.seed.json`; the first modification creates `App_Data/namedays.json`. Subsequent reads and writes use that editable JSON file. The EF Core Fluent API mapping models the entity but does not persist the JSON.

| Method | Route | Action |
|---|---|---|
| 🟢 GET | `/api/namedays` | List all namedays |
| 🔎 GET | `/api/namedays?search=ann` | Search by name |
| 🟢 GET | `/api/namedays/name/Valter` | Look up an exact name |
| 📅 GET | `/api/namedays/date/8/17` | All names on 17 August |
| ☀️ GET | `/api/namedays/today` | Names for server-local current date |
| 🔵 POST | `/api/namedays` | Create a nameday |
| 🟠 PUT | `/api/namedays/name/Valter` | Update a nameday |
| 🔴 DELETE | `/api/namedays/name/Valter` | Delete a nameday |

Example response for `GET /api/namedays/name/Valter`:

```json
{
  "name": "Valter",
  "day": 17,
  "month": 8
}
```

Create a nameday:

```bash
curl -X POST http://localhost:5000/api/namedays -H "Content-Type: application/json" -d '{"name":"Example","day":15,"month":6}'
```

The date route uses `month/day` order, including February 29 validation. `/today` uses the server's local clock. CRUD routes are currently unauthenticated, as in the existing starter API; add authentication before exposing them publicly.

## 🎂 Birthdays CRUD API

Birthdays are stored in `App_Data/birthdays.json` (created on the first write). The EF Core `BirthdayConfiguration` provides Fluent API mapping, while the JSON store handles persistence independently.

| Method | Endpoint | Purpose |
|---|---|---|
| 🟢 GET | `/api/birthdays` | List birthdays, sorted by next celebration |
| 🔎 GET | `/api/birthdays?search=anna` | Search names |
| 🎉 GET | `/api/birthdays/today` | Today's birthdays |
| 📅 GET | `/api/birthdays/upcoming?days=30` | Upcoming birthdays (0–366 days) |
| 🟢 GET | `/api/birthdays/{id}` | Get a birthday |
| 🔵 POST | `/api/birthdays` | Create |
| 🟠 PUT | `/api/birthdays/{id}` | Update |
| 🔴 DELETE | `/api/birthdays/{id}` | Delete |

**Create example** (`POST /api/birthdays`):

```json
{
  "name": "Anna",
  "dateOfBirth": "1990-06-15",
  "notes": "Friend from school"
}
```

Responses include `id`, `age`, `nextBirthday`, `turningAge`, and `daysUntilBirthday`. Dates are ISO `YYYY-MM-DD`. Birthdays on February 29 are observed on **February 28** in non-leap years. Calculations use the server's local date.

⚠️ **Privacy and security:** Birthday dates and notes are personal information. This starter's endpoints are unauthenticated, so do not deploy with real personal data on a public server until authentication, authorization, and access controls are implemented.

## 🇸🇪 Flag days CRUD API

Manage recurring, fixed-date flag days in `App_Data/flagdays.json`, which is created on the first write. The API supports country filtering, today's flag days, upcoming celebrations, and complete CRUD. `FlagDayConfiguration` demonstrates EF Core Fluent API mapping; JSON storage is independent of EF Core.

| Method | Endpoint | Description |
|---|---|---|
| 🟢 GET | `/api/flagdays` | List all flag days |
| 🔎 GET | `/api/flagdays?search=national&country=SE` | Filter by name and country |
| 🇸🇪 GET | `/api/flagdays/today?country=SE` | Today's flag days |
| 📅 GET | `/api/flagdays/date/6/6` | Flag days on June 6 |
| ⏳ GET | `/api/flagdays/upcoming?days=30` | Upcoming flag days (0–366 days) |
| 🟢 GET | `/api/flagdays/{id}` | Get a flag day |
| 🔵 POST | `/api/flagdays` | Create |
| 🟠 PUT | `/api/flagdays/{id}` | Update |
| 🔴 DELETE | `/api/flagdays/{id}` | Delete |

**Example** (`POST /api/flagdays`):

```json
{
  "name": "Sveriges nationaldag",
  "month": 6,
  "day": 6,
  "countryCode": "SE",
  "description": "Swedish National Day",
  "isOfficial": true
}
```

Responses include `nextOccurrence` and `daysUntil`, calculated from the server-local date. The API currently supports **annual fixed dates**; movable holidays such as Easter and Midsummer are not calculated automatically. For February 29 entries, the anniversary falls on February 28 in non-leap years.

No official flag-day list has been seeded: create the entries you want using the API. The `isOfficial` flag is user-managed and is not independently verified. **CRUD endpoints are currently unauthenticated**; secure them before public deployment.

## 🎨 Theme days CRUD API

Create and manage annual **theme days** (temadagar), with categories such as Food, Awareness, Culture, and Technology. Entries are stored in `App_Data/themedays.json` on the first write; the `ThemeDayConfiguration` class defines the EF Core Fluent API model separately.

| Method | Endpoint | Description |
|---|---|---|
| 🟢 GET | `/api/themedays` | List all theme days |
| 🔎 GET | `/api/themedays?search=coffee&category=Food` | Search and filter by category |
| ☀️ GET | `/api/themedays/today` | Today's theme days |
| 📅 GET | `/api/themedays/date/10/1` | Theme days on October 1 |
| ⏳ GET | `/api/themedays/upcoming?days=30` | Upcoming theme days (0–366 days) |
| 🟢 GET | `/api/themedays/{id}` | Get one theme day |
| 🔵 POST | `/api/themedays` | Create a theme day |
| 🟠 PUT | `/api/themedays/{id}` | Update a theme day |
| 🔴 DELETE | `/api/themedays/{id}` | Delete a theme day |

Example `POST /api/themedays` body:

```json
{
  "name": "International Coffee Day",
  "month": 10,
  "day": 1,
  "category": "Food",
  "description": "A day to celebrate coffee"
}
```

Responses also contain `id`, `nextOccurrence`, and `daysUntil`. Dates are recurring fixed month/day values; movable observances are not calculated. February 29 falls on February 28 in non-leap years for the next-occurrence calculation. No theme days are preloaded, so you can create your own catalog through the API. The write endpoints are currently unauthenticated; add authorization before public deployment.

## 🎨 Branding

- 🐸 Smiling frog mascot
- 🌙 Glowing crescent moon
- 💚 Dark green and lighter-green accents
- 🖥️ Shared logo in the header, hero and monitor wallboard
- 🔖 Shared favicon

## 📁 Project structure

```text
WebApiEfFluentApi.csproj
Program.cs
Birthdays.cs
FlagDays.cs
ThemeDays.cs
wwwroot/
  index.html
  site.css
  site.js
  assets/
    logo.svg
App_Data/
  boards.json  # generated on first write
  namedays.json # generated after first nameday change
  birthdays.json # generated after first birthday change
  flagdays.json # generated after first flag day change
  themedays.json # generated after first theme day change
Data/
  namedays.seed.json
```

## 🔒 Production notes

This is a starter demo, not a production-ready authenticated service. JSON writes are protected by a process-local lock only; they do not provide cross-process transactions. Add authentication, validation, durable database persistence and automated tests before production use.

## 🖼️ Image assets

| Asset | File |
|---|---|
| 🐸 Approved frog logo | [webapi-ef-fluentapi-logo.png](webapi-ef-fluentapi-logo.png) |
| 🖥️ Dashboard design preview | [webapi-ef-fluentapi-dashboard.png](webapi-ef-fluentapi-dashboard.png) |

The running website currently references `wwwroot/assets/logo.svg`; the README showcases the uploaded PNG artwork.
