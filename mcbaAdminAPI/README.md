The documentation covers the admin authentication flow, Payee management operations (viewing, filtering, and updating Payees), and BillPay management features (blocking and unblocking scheduled payments).
Additionally, Swagger/OpenAPI has been integrated into the project to automatically generate and display interactive endpoint documentation, enabling easy testing and validation of API functionality.

This API is secured using JWT (JSON Web Token) authentication, ensuring that only authorized administrators can access sensitive endpoints.
An admin must first call the /api/Auth/login endpoint using valid credentials (default: admin/admin).
Upon successful login, a JWT token is generated and returned to the client.

The token is valid for 30 minutes.

# It must be included in the Authorization header for all subsequent API requests:
   Authorization: Bearer <your_token_here>

   Each token is digitally signed using HMAC-SHA256 and a secure secret key configured in appsettings.json.

   The token encodes the user’s identity and expiry time, which are validated on every request.

   This approach provides stateless, secure authentication between the Admin Portal frontend and the Admin API.

# Endpoints are designed following the Repository Pattern for maintainability.

Endpoint:                 /api/Auth/login
HTTP Method:              POST
Description:              Authenticates admin credentials and returns a JWT token.
Auth Required:            No

Endpoint:                 /api/Payees
HTTP Method:              GET
Description:              Retrieves all Payees in the system.
Auth Required:            Yes

Endpoint:                 /api/Payees/postcode/{postcode}
HTTP Method:              GET
Description:              Returns Payees filtered by postcode.
Auth Required:            Yes

Endpoint:                 /api/Payees/{id}
HTTP Method:              PUT
Description:              Updates the details of an existing Payee (name, address, etc.).
Auth Required:            Yes

Endpoint:                 /api/BillPay
HTTP Method:              GET
Description:              Retrieves all scheduled bill payments.
Auth Required:            Yes

Endpoint:                 /api/BillPay/block/{id}
HTTP Method:              PUT
Description:              Blocks a scheduled payment, preventing automatic execution.
Auth Required:            Yes

Endpoint:                 /api/BillPay/unblock/{id}
HTTP Method:              PUT
Description:              Unblocks a previously blocked payment.
Auth Required:            Yes


Swagger/OpenAPI is configured for automated documentation and testing.
