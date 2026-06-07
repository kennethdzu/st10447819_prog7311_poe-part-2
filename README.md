# TechMove GLMS Prototype

This project is a logistics management system built with ASP.NET Core 8, refactored into a Service-Oriented Architecture with a decoupled Web API backend and an MVC frontend.

## Structure

The solution contains four projects.

1. TechMove.Glms.Core
This is the shared class library. It holds all domain models, the database context, repository interfaces, and business services. Both the API and the Web app reference this project.

2. TechMove.Glms.Api
This is the backend Web API. It is the only component that communicates with the database. It exposes REST endpoints for contracts, clients, and service requests, and it is secured with JWT authentication. Swagger is enabled for testing.

3. TechMove.Glms.Web
This is the MVC frontend. It no longer connects to the database directly. All data is fetched and submitted through the API using HttpClient.

4. TechMove.Glms.Tests
This project contains unit tests and automated API integration tests.

## Configuration

The application requires a few external dependencies to run.

### Setup Database

The API is the only project that connects to a database. Locally, it uses a SQLite file located at TechMove.Glms.Web/glms.db. The connection string in TechMove.Glms.Api/appsettings.json is pre-configured for this.

When running via Docker Compose, the API connects to a SQL Server container instead. The compose file passes the correct SQL Server connection string automatically via environment variables.

### Setup Currency API

The application reaches out to ExchangeRate-API to convert costs from USD to ZAR. You need a valid API key. Place it inside the appsettings.json file under the ExchangeRateApi section.

## How to execute

Open the solution file TechMove.Glms.sln in your IDE to load all four projects.

Build the application.

Run the unit tests.

Start both the API project and the Web project simultaneously. In Rider, configure a Compound run configuration that launches TechMove.Glms.Api and TechMove.Glms.Web together.

The web frontend will be available on http://localhost:5255 and the API with Swagger will be available on http://localhost:5269/swagger.

## How to run with Docker

Make sure Docker and Docker Compose are installed.

Run the following command from the root of the solution.

docker-compose up --build

This will start three containers: sql-server-db, glms-backend-api, and glms-frontend-web. The web frontend maps to port 5255 and the API maps to port 5269.
