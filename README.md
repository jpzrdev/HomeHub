# HomeHub
A home management App with different modules to help manage a personal home.

## 🚀 Objective

The goal of **HomeHub** is to provide a centralized app for common household needs.  
It starts as a simple inventory tracker but is designed to grow into a **multi-module platform** for managing various aspects of home life, such as:

- Kitchen stock & grocery planning  
- Household chores & reminders  
- Basic automation & notifications  

The long-term vision is to make HomeHub a **personal “smart home hub”** that you can self-host, integrate with other services, or extend with custom modules.

---

## ✨ Current Features

### Backend API (.NET 9)

**Inventory Management**
- ✅ Create inventory items with name, quantity, minimum threshold, and notification settings
- ✅ Get all inventory items with pagination support
- ✅ Get inventory item by ID
- ✅ Delete inventory items
- ✅ Track stock levels and identify items below minimum quantity

**Shopping List**
- ✅ Generate shopping lists automatically from items below minimum stock
- ✅ Get all shopping lists with pagination support
- ✅ Get shopping list by ID with items
- ✅ Track shopping list completion status and item purchase status

### Frontend (Angular 21)

**Pages**
- ✅ Home page with navigation
- ✅ Inventory page displaying items with low stock indicators
- ✅ Shopping List page showing lists with progress tracking

> **Note:** Frontend currently uses mock data. API integration is in progress.

---

## 🗺️ Roadmap

| Phase | Focus | Key Features | Status |
|-------|-------|--------------|--------|
| **0.1 (MVP)** | Kitchen Inventory | CRUD for items, stock tracking, threshold alerts | ✅ Complete |
| **0.2** | Grocery List | Generate shopping list from items below minimum stock | ✅ Complete |
| **0.3** | Frontend Integration | Connect Angular frontend to backend API | 🚧 In Progress |
| **0.4** | Notifications | Basic notifications (log, email, or webhook) | 📋 Planned |
| **0.5** | Expansion Modules | Chore tracking, expense logging, pantry analytics | 📋 Planned |
| **0.6+** | Integrations | Mobile app, smart-home APIs | 📋 Planned |

---

## 🛠️ Tech Stack

### Backend
- **.NET 9** Web API (Controller-based)  
- **Clean Architecture** (Domain, Application, Infrastructure, API layers)
- **CQRS** pattern with **MediatR**
- **Entity Framework Core** for data persistence  
- **SQL Server** database
- **Swagger / OpenAPI** for API documentation  
- **xUnit** with **Moq** for unit testing

### Frontend
- **Angular 21** with standalone components
- **TypeScript**
- **Angular Router** for navigation
- **Vitest** for testing (configured)

---

## 📦 Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (or SQL Server Express)
- Node.js and npm (for frontend)
- Angular CLI (for frontend development)

### Backend Setup
1. Navigate to `homehub-backend/src/HomeHub.Api`
2. Update connection string in `appsettings.json` or `appsettings.Development.json`
3. Run migrations:
   ```bash
   dotnet ef database update --project ../HomeHub.Infrastructure --startup-project .
   ```
4. Run the API:
   ```bash
   dotnet run
   ```
5. Access Swagger UI at `https://localhost:5001/swagger` (or the configured port)

### Frontend Setup
1. Navigate to `homehub-frontend`
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start the development server:
   ```bash
   npm start
   ```
4. Access the app at `http://localhost:4200`

---

## 📜 License

MIT (to be decided)

---

## 🤝 Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements and new modules.