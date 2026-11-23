# 🐸 WebAPI EF FluentAPI

<div align="center">

![WebAPI EF FluentAPI frog identity](wwwroot/assets/logo.svg)

**🌙 Dark mode · 💚 Forest green · ⚡ ASP.NET Core · 🗃️ JSON storage**

[![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C%23](https://img.shields.io/badge/C%23-API-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF_Core-Fluent_API-27783F)](https://learn.microsoft.com/ef/core/modeling/)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger&logoColor=black)](https://swagger.io/)

</div>

## ✨ Overview

A dark-green ASP.NET Core 8 REST API starter with a smiling frog and crescent-moon visual identity. The application uses one shared **SVG** logo in the site header, home hero, monitor wallboard and favicon.

> 🎨 **Design previews:** The approved high-resolution frog PNG and dashboard screenshot from the chat are **not yet committed** to this repository. The image above is the currently deployed SVG variant, not the final PNG. The dashboard screenshot is a design mockup, not a verified running-app screenshot.

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
wwwroot/
  index.html
  site.css
  site.js
  assets/
    logo.svg
App_Data/
  boards.json  # generated on first write
```

## 🔒 Production notes

This is a starter demo, not a production-ready authenticated service. JSON writes are protected by a process-local lock only; they do not provide cross-process transactions. Add authentication, validation, durable database persistence and automated tests before production use.

## 🖼️ Screenshots

The approved chat dashboard mockup and green frog PNG still need to be uploaded as binary image assets. No substitute screenshot is presented here as if it were a real capture.
