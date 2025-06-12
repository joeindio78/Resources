# Resources Management System

A full-stack application for managing resources and their competencies, built with .NET Web API and React.

## Technology Stack

### Backend
- .NET 9 Web API
- Entity Framework Core
- SQLite Database
- xUnit for testing

### Frontend
- React 18 with TypeScript
- Vite for build tooling
- Material-UI (MUI) for components
- React Query for data fetching
- React Router for navigation
- ESLint for code quality

## Project Structure

```
├── Resources.API/          # .NET Web API project
├── Resources.API.Tests/    # API unit tests
├── resources-client/       # React frontend application
├── .vscode/               # VS Code configuration
└── package.json           # Root package.json for running both projects
```

## Features

- CRUD operations for resources
- Resource competency management
- Pagination and filtering
- Modern React UI with Material Design
- Real-time data updates
- Type-safe API integration
- Comprehensive test coverage

## Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js (v18 or later)
- npm (included with Node.js)

### Development Setup

1. Clone the repository
2. Install frontend dependencies:
   ```bash
   cd resources-client
   npm install
   ```

3. Start the backend API:
   ```bash
   cd Resources.API
   dotnet run
   ```
   The API will be available at `http://localhost:5062`

4. Start the frontend development server:
   ```bash
   cd resources-client
   npm run dev
   ```
   The frontend will be available at `http://localhost:5173`

5. For running both simultaneously, from the root directory:
   ```bash
   npm run dev
   ```

## Available Scripts

### Backend
- `dotnet run` - Run the API
- `dotnet test` - Run unit tests
- `dotnet watch run` - Run with hot reload

### Frontend
- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Run ESLint
- `npm run preview` - Preview production build

### Root
- `npm run dev` - Run both frontend and backend
- `npm run start:api` - Start API only
- `npm run start:client` - Start frontend only

## API Endpoints

### Resources
- `GET /resources` - List resources (with pagination and filtering)
- `GET /resources/{id}` - Get a specific resource
- `POST /resources` - Create a new resource
- `PUT /resources/{id}` - Update a resource
- `DELETE /resources/{id}` - Delete a resource

### Competencies
- `GET /competencies` - List all competencies
- `GET /competencies/{id}` - Get a specific competency
- `POST /competencies` - Create a new competency
- `PUT /competencies/{id}` - Update a competency

## Authentication

The application uses JWT (JSON Web Token) based authentication.

### Backend Authentication
- JWT tokens are issued upon successful login
- Tokens are valid for 7 days
- All API endpoints (except `/login`) require authentication
- Bearer token authentication scheme is used
- Rate limiting is applied to authenticated endpoints

### Frontend Authentication
- Token is stored in localStorage
- Automatic token injection for all API requests
- Protected routes redirect to login if no token is present
- Automatic handling of 401 responses

### Default Admin Credentials
```
Email: admin@admin.io
Password: admin
```

### Authentication Flow
1. Client sends login request with credentials to `/v1/login`
2. Server validates credentials and returns JWT token
3. Client stores token in localStorage
4. Client includes token in Authorization header for subsequent requests
5. Server validates token for protected endpoints

### Security Configuration
- JWT secret key is configurable via `JwtSettings:Key` in appsettings.json
- Admin credentials are configurable via `AdminUser` settings
- Token validation parameters can be customized in Program.cs
- HTTPS is enforced in production

### API Authentication Examples

Login:
```bash
POST /v1/login
Content-Type: application/json

{
    "email": "admin@admin.io",
    "password": "admin"
}
```

Using the token:
```bash
GET /v1/resources
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Rate Limiting
- Authenticated endpoints are protected by rate limiting
- Default limits: 100 requests per 10 seconds
- Configurable via `RateLimitSettings` in appsettings.json
- Returns 429 Too Many Requests when limit is exceeded

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the ISC License - see the package.json file for details. 