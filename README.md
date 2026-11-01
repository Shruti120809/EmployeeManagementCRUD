<<<<<<< HEAD
# Employee Management System

A simple employee directory with a .NET 8 Web API, ADO.NET repository, SQL Server stored procedure, and a separate Bootstrap/jQuery UI. The UI is static HTML and does not use MVC or server-rendered views.

## Project layout

- `EmployeeManagement.API/` - ASP.NET Core API, validation, dependency injection, CORS, and ADO.NET repository.
- `EmployeeManagement.UI/` - static HTML, Bootstrap, CSS, and jQuery AJAX client.
- `EmployeeManagement.Database/EmployeeManagementDB.sql` - database, table, active-email index, stored procedure, and sample calls.

## Run locally

1. In SQL Server Management Studio or `sqlcmd`, execute `EmployeeManagement.Database/EmployeeManagementDB.sql` using an account allowed to create databases. The script creates `EmployeeManagementDB` and `dbo.p_EmployeeCRUD`. Its sample insert can be run again only after removing or changing its sample email.
2. Set the `EmployeeManagement` connection string in `EmployeeManagement.API/appsettings.json` to the SQL Server instance where `EmployeeManagementDB` was created. The included Windows trusted-connection example is:

   ```text
   Server=localhost;Database=EmployeeManagementDB;Trusted_Connection=True;TrustServerCertificate=True;
   ```

3. From the repository root, start the API:

   ```powershell
   dotnet run --project .\EmployeeManagement.API\EmployeeManagement.API.csproj
   ```

   The configured development URLs are `https://localhost:7039` and `http://localhost:5239`. The first run may require trusting the ASP.NET Core HTTPS development certificate (`dotnet dev-certs https --trust`).
4. Open another PowerShell window at the repository root and serve the UI over HTTP:

   ```powershell
   py -m http.server 5500 --directory .\EmployeeManagement.UI
   ```

   If `py` is not available, use another static file server on port 5500. Open `http://localhost:5500/login.html`. The API allows this UI origin through its CORS policy.
5. Register an account, sign in, and then manage employees from the dashboard. The frontend API base URLs are configured at the beginning of `EmployeeManagement.UI/js/auth.js` and `EmployeeManagement.UI/js/employee.js`; update them if you change the API HTTPS port. Bootstrap and jQuery are loaded from CDNs, so the browser needs internet access to load those libraries.

## Authentication

The database script also creates `dbo.Users`, a unique email index, and the `p_UserRegister`, `p_UserGetByEmail`, and `p_UserGetById` stored procedures. Registration stores an ASP.NET Core Identity password hash; login verifies it and issues an HMAC-SHA256 signed JWT. Employee endpoints require a valid Bearer token. Registration and login remain anonymous.

The checked-in JWT key in `appsettings.json` is only a local-development placeholder. For local overrides, initialize User Secrets in the API project and set `Jwt:Key` to a private random value of at least 32 bytes:

```powershell
dotnet user-secrets init --project .\EmployeeManagement.API\EmployeeManagement.API.csproj
dotnet user-secrets set "Jwt:Key" "replace-with-a-private-random-secret-at-least-32-bytes" --project .\EmployeeManagement.API\EmployeeManagement.API.csproj
```

For deployed environments, provide secrets through an environment variable or a managed secret store (for example, `Jwt__Key`) and never use the checked-in development key. The browser keeps the JWT in `sessionStorage` only for this local assignment; JavaScript-accessible storage remains vulnerable to XSS. The disabled “Remember me” option does not persist a login. Production deployments should prefer an HttpOnly, Secure, SameSite cookie through a backend-for-frontend design.

Password requirements are at least 8 characters with uppercase, lowercase, a number, and a special character. The backend enforces these requirements regardless of frontend validation.

## API and Swagger

When the API is running in Development, open `https://localhost:7039/swagger`. To test protected employee endpoints, first call `POST /api/Auth/login` with a registered account. Copy the `data.token` value from the response, select **Authorize** at the top of Swagger, paste the token without adding `Bearer `, and select **Authorize**. Swagger adds the Bearer prefix to protected requests. Registration and login remain public and do not require authorization. Select **Logout** in the Swagger authorization dialog when finished.

The endpoints are:

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/Employee` | List active employees |
| GET | `/api/Employee?search=alex` | Search name or email |
| GET | `/api/Employee/{id}` | Get an active employee |
| POST | `/api/Employee` | Create an employee |
| PUT | `/api/Employee/{id}` | Update an employee |
| DELETE | `/api/Employee/{id}` | Soft-delete an employee |

Use this JSON body for POST and PUT (replace values as needed):

```json
{
  "name": "Alex Morgan",
  "email": "alex.morgan@example.com",
  "mobileNo": "9876543210",
  "department": "Engineering",
  "designation": "Software Engineer"
}
```

In Swagger, expand an endpoint, select **Try it out**, enter the path/query/body as applicable, and select **Execute**. A successful POST returns `201 Created` with the created record; GET returns `200 OK`; PUT and DELETE return `200 OK`. Missing records return `404`, invalid requests return `400`, and duplicate active email addresses return `409 Conflict`. DELETE sets `Status` to `0`, so the record is excluded from active list/search and get-by-ID results.

## Architecture

The controller handles HTTP routes, model validation, status codes, and safe error responses. It delegates employee operations to `IEmployeeRepository`; `EmployeeRepository` executes the single flag-based stored procedure using typed SQL parameters and asynchronous ADO.NET calls. `DbConnectionFactory` reads the connection string and creates SQL connections. The static UI calls the API with jQuery AJAX, with server-side validation remaining authoritative.
=======
# EmployeeManagementCRUD
>>>>>>> 71b7140f7f2b5964cb65e60edefdca497af18f87
