# LogVault

LogVault is a robust, modern RESTful API built with .NET 8 that enables users to manage their logbook activities efficiently and securely. Designed for extensibility and deployment flexibility, it features JWT-based authentication, email notifications, and is fully dockerized for seamless development and production use.

## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
- [Environment Configuration](#environment-configuration)
- [Running with Docker](#running-with-docker)
- [API Usage](#api-usage)
- [Contributing](#contributing)
- [License](#license)

## Features

- **User Authentication**: Secure login and access control using JWT tokens.
- **Activity Management**: Create, update, delete, and retrieve user logbook entries, including exporting entries as CSV.
- **Email Notifications**: Automated emails for critical account events (e.g., password resets).
- **Environment Variable Configuration**: Sensitive information is managed via environment variables.
- **Dockerized**: Rapid deployment with Docker and Docker Compose, including a built-in SQL Server database.

## Getting Started

These instructions will help you set up and run the LogBook API on your local machine for development and testing purposes.

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- [Git](https://git-scm.com/)

## Environment Configuration

The API relies on environment variables for configuration. An example file (`.env.example`) is provided to guide your setup.

1. **Clone the repository:**
    ```bash
    git clone https://github.com/Promise30/Logbook-API.git
    cd Logbook-API
    ```

2. **Set up environment variables:**
    - Copy the example file and modify it as needed:
      ```bash
      cp .env.example .env
      ```
    - Edit `.env` and fill in your credentials and configuration values (database connection strings, email service credentials, JWT secret, etc.).

## Running with Docker

You can quickly spin up the API and its dependencies using Docker Compose.

```bash
docker-compose up
```

- This command will launch the API and the SQL Server database defined in `docker-compose.yml`.
- By default, the API will be accessible at:
  - [http://localhost:8002/swagger](http://localhost:8002/swagger)
  - [https://localhost:8003/swagger](https://localhost:8003/swagger)

## API Usage

The API exposes endpoints for user authentication, logbook entry management, and exporting data. Interactive API documentation is available via Swagger UI at the URLs above.

### Example Endpoints

- `POST /api/auth/login` — Authenticate user and receive JWT
- `POST /api/logbook` — Create a new logbook entry
- `GET /api/logbook` — Retrieve logbook entries
- `PUT /api/logbook/{id}` — Update an entry
- `DELETE /api/logbook/{id}` — Delete an entry
- `GET /api/logbook/export` — Export entries as CSV

Refer to the Swagger UI for the full list and details of available endpoints.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request. For major changes, open an issue first to discuss your proposed changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

For questions or support, please open an issue in the repository.
