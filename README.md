# TechMove GLMS Prototype

This project is a logistics management prototype built with ASP.NET Core 8 MVC.

## Structure

The solution contains two main projects.

1. TechMove.Glms.Web
This is the core web application. It handles contracts, service requests, and clients.

2. TechMove.Glms.Tests
This project contains the unit tests for the core logic.

## Configuration

The application requires a few external dependencies to run.

### Setup Database

The application connects to an instance of Microsoft SQL Server. Look at appsettings.json to configure the connection string. It points to localhost on port 1433 by default. 

Make sure your database is running before you execute the web project. The application will track schema updates using Entity Framework Core.

### Setup Currency API

The application reaches out to ExchangeRate-API to convert costs from USD to ZAR. You need a valid API key. Place it inside the appsettings.json file under the ExchangeRateApi section.

## How to execute

Open the solution file TechMove.Glms.sln in your IDE to load both projects.

Build the application.

Run the unit tests.

Start the web project to launch the frontend.
