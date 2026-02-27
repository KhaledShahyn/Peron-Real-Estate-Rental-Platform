# 📌 Peron Real Estate Rental Platform

## 🏠 Overview

Peron Real Estate Rental Platform is a RESTful API built with ASP.NET Core for managing real estate rental operations.  
The system provides secure authentication, role-based authorization, real-time notifications, and full property management.

---

## 🚀 Features

- 🔐 JWT Authentication & Authorization  
- 👥 Role-Based Access Control (Admin / User)  
- 🏢 CRUD Operations for Properties  
- ⭐ Property Rating System  
- 🔔 Real-Time Notifications using SignalR  
- 🗄 Database First Approach  
- 🛡 Admin Full System Control  
- 📡 RESTful API Architecture  

---

## 🛠 Technologies Used

- ASP.NET Core Web API  
- Entity Framework Core (Database First)  
- SQL Server  
- JWT (JSON Web Token)  
- SignalR  
- LINQ  
- Swagger  

---

## 🔑 Authentication & Authorization

The system uses JWT for secure authentication.

### Roles:

### 👑 Admin
- Full control over the system
- Manage users
- Manage properties
- View all ratings
- Send notifications

### 👤 User
- Register / Login
- Browse properties
- Add / Update / Delete own properties
- Rate properties
- Receive notifications

---

## 🏢 Property Management

Users can:

- Create Property  
- Update Property  
- Delete Property  
- Get All Properties  
- Get Property By Id  

Admin can manage all properties in the system.

---

## ⭐ Rating System

- Users can rate properties  
- Each property displays average rating  
- Prevent duplicate ratings per user  

---

## 🔔 Real-Time Notifications

Implemented using SignalR.

- Users receive instant notifications  
- Admin can broadcast notifications  
- Real-time updates without refreshing  

---

## 🗄 Database Design

- Database First approach  
- Entities generated from SQL Server  
- Relationships configured between:
  - Users
  - Properties
  - Ratings
  - Notifications

---

## 📂 Project Structure

```
Controllers/
Services/
Repositories/
Models/
DTOs/
Data/
```

---

## ▶ How to Run

1. Clone the repository  
2. Update connection string in `appsettings.json`  
3. Apply database if needed  
4. Run the project  
5. Open Swagger to test endpoints  

---

## 📌 API Documentation

Swagger is enabled for testing all endpoints.

---

## 👨‍💻 Author

Developed by Khaled Shahyn  
Backend Developer | ASP.NET Core