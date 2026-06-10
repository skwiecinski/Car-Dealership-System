# Salon Samochodowy — Car Dealership

A desktop application for managing a modern car dealership. Built with C# and WPF (Windows Presentation Foundation), this project uses a clear architecture and popular design patterns to manage data easily and efficiently.

> **Note:** This is a forked repository. In this group project, my primary role was **Full-Stack Developer**. 
> 
> **My personal contributions included:**
> * Implemented *Repository* and *Unit of Work* design patterns to ensure robust data architecture.
> * Engineered a comprehensive dynamic Internationalization system covering all application views, components, and PDF exports.
> * Created the PDF generation feature using *QuestPDF* for complex database summaries, including interactive charts and time-period filters.
> * Designed advanced UI/UX features, including startup animations, responsive layouts, and a cross-module Interactive Guided Tour system.
> * Built the complete authentication flow featuring SHA-256 password hashing, an Administrator CRUD dashboard, and a First-Run Configuration Wizard.
> * Resolved application bugs, optimized data caching, and implemented robust validation logic to prevent application crashes and ensure seamless view navigation.

## Design Patterns & Databases

The system uses the **MVVM (Model-View-ViewModel)** pattern to keep the user interface separate from the business logic. 

For database management, it uses **Entity Framework Core** with a SQL Server database. To keep the code clean and fast, the project uses the **Repository** and **Unit of Work** design patterns. This ensures that all database queries are grouped and saved safely, preventing data errors.

## Prerequisites

To run the application, you only need a Windows computer with:
* **.NET Desktop Runtime (6, 7, or 8)** installed.

## Installation & Releases (v1.1)

This release brings the application to a production-ready state. You do not need to build the project from source code to use it. It comes with a custom, easy-to-use installer available in two versions:

### 1. SalonSamochodowy.exe - Clean install production
Ships with an empty database. Intended for fresh production deployments. On first launch, an Initial Configuration Wizard will guide you through setting up the root administrator account and basic dealership information.

### 2. SalonSamochodowy_with_data.exe - Pre-seeded demo
Includes a fully populated database with sample cars, clients, orders, and employees. Useful for testing, demonstrations, or evaluating features without manual data entry.
*(Test accounts: admin@salon.pl, kierownik@salon.pl, sprzedawca@salon.pl — password: 123)*.

## Features, Roles & Usage

The application provides full support for **two languages (English and Polish)** and features **contextual help** across different screens. Features and menus are dynamically adjusted based on your assigned user role: Admin, Client, Salesperson, Service, or Manager.

Depending on your role, you can navigate through the sidebar to:
* **Dashboard:** View live sales charts and track business performance *(Managers & Admins)*.
* **Vehicles:** Browse the catalog of available cars, filter features, and check real-time pricing *(All Roles)*.
* **Sales & Clients:** Process new orders, review the full transaction history, and maintain the customer registry *(Salesperson & Managers)*.
* **Services:** Manage mechanic tasks, update job statuses, and handle vehicle repairs *(Service / Mechanics)*.
* **Reports:** Generate and export complex database summaries to PDF *(Managers & Admins)*.
* **Admin Panel:** Manage user accounts, assign roles, and control system settings *(Admins)*.

## Authors

Krzysztof Bieszczad
[@KBieszczad](https://github.com/KBieszczad)<br>
Mateusz Chęciński
[@perszik](https://github.com/perszik)<br>
Kamil Karwacki
[@Kamil-Karwacki](https://github.com/Kamil-Karwacki)<br>
Szymon Kwieciński
[@skwiecinski](https://github.com/skwiecinski)<br>
Marek Znamirowski
[@marekznamir](https://github.com/marekznamir)<br>
