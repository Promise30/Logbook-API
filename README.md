# LogBook API Project
LogBook API is a .NET 8-based RESTful API that allows users to manage their activities. The API features JWT-based authentication and email notifications, and it is dockerized for easy deployment. The project uses SQL Server for data storage and supports environment variable configuration for sensitive data.

## Features
- User Authentication: JWT-based authentication for secure login and access control.
- Activity Management: Create, update, delete, retrieve user logbook entries and exporting entries as CSV format.
- Email Notifications: Sends email notifications for important events such as password or email resets.
- Environment Variable Configuration: Uses environment variables for sensitive information.
- Dockerized: Easily deployable with Docker and Docker Compose.

## Running with Docker
Clone the repository:
   ```
   https://github.com/Promise30/Logbook-API.git
   cd logbook_api_project
   ```

## Environment Setup

To run this project, you need to set up your environment variables:

1. Copy the `.env.example` file to a new file named `.env`:
   ```
   cp .env.example .env
   ```

2. Open the `.env` file and fill in your specific values for each environment variable.
3. Run the project
   ```
   docker-compose up
   ```
This will start both the API and the SQL Server database.
The API will be available at http://localhost:8002/swagger OR https://localhost:8003/swagger
