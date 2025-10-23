# Agri-Energy Connect - Farm Management System

## Project Overview
Agri-Energy Connect is a comprehensive web application designed to connect farmers with agricultural enterprises. This system provides role-based access for employees and farmers to manage farm profiles and agricultural products efficiently.

## Features

### Employee Features
- Add new farmer profiles to the system
- View all registered farmers
- Filter and search products by date range and category
- View comprehensive product listings from all farmers

### Farmer Features
- Add new products to their profile
- View their own product listings
- Manage product inventory

### Security Features
- Role-based authentication (Employee/Farmer)
- Secure password policies
- Session management
- Data validation and error handling

## Technology Stack
- **Backend**: ASP.NET Core 6.0 MVC
- **Database**: MySQL with Entity Framework Core
- **Frontend**: Bootstrap 5, Font Awesome
- **Authentication**: ASP.NET Core Identity

## Prerequisites
- Visual Studio 2022
- .NET 6.0 SDK
- MySQL Server 8.0+
- MySQL Workbench

## Installation Instructions

### Step 1: Database Setup
1. Install MySQL Server and MySQL Workbench
2. Create a new database named `AgriEnergyConnect`
3. Update the connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=AgriEnergyConnect;Uid=root;Pwd=yourpassword;"
}