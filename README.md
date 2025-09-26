# 🎬 MovieManager API

MovieManager is a simple **ASP.NET Core Web API** built for managing movies and user authentication.  
It supports **JWT authentication**, **SQLite persistence**, and a minimal set of endpoints for users and movies.  

---

## 🚀 Features
- User registration and login with **JWT token generation**  
- Movie CRUD (Create, Read, Update, Delete)  
- Protected endpoints secured with **Bearer authentication**  
- Runs locally with **SQLite** (`movie.db`)  
- Ready for deployment to **Railway** or any containerized environment  

---

## 🛠 Tech Stack
- **.NET 8.0**  
- **Entity Framework Core** with SQLite  
- **JWT Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)  
- **Swagger / OpenAPI**  

---

## 🔧 Setup & Run Locally

### 1. Clone repository
```bash
git clone https://github.com/your-username/MovieManager.git
cd MovieManager
```

### 2. Install Dependencies - Create DB - Run project
```bash
dotnet restore
dotnet ef database update --project MovieManager
dotnet run --project MovieManager
```

### 🎥 Endpoints
(Protected: requires Authorization: Bearer {token})

- **GET /api/movies** → Get all movies

- **GET /api/movies/{id}** → Get movie by ID

- **POST /api/movies** → Add new movie

- **PUT /api/movies/{id}** → Update movie

- **DELETE /api/movies/{id}** → Delete movie