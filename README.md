# InternalDocs

## Backend Auth and Roles

The API uses JWT bearer authentication. Configure these settings in `InternalDocs/InternalDocs.Api/appsettings.json` or `.env`:

- `Jwt:Secret` (min 32 chars)
- `Jwt:Issuer`
- `Jwt:Audience`
- `Jwt:ExpiryMinutes`

### Roles

- `Admin` manages setup data (document categories and document types).
- `Employee` submits, updates, and deletes their own documents.
- `Approver` creates and updates approval actions.

### Role-protected endpoints

Document catalog (Admin only):
- `POST /document-categories`
- `PUT /document-categories/{id}`
- `DELETE /document-categories/{id}`
- `POST /document-types`
- `PUT /document-types/{id}`
- `DELETE /document-types/{id}`

Documents (Employee only):
- `POST /documents`
- `PUT /documents/{id}`
- `DELETE /documents/{id}`

Approvals (Approver only):
- `POST /approvals`
- `PUT /approvals/{id}`

Notes:
- The API reads the current user id from the JWT and does not accept `CreatedByUserId` or `ApproverId` in request bodies.
- `GET /document-categories` and `GET /document-types` are authenticated but available to any role.
