# SSRd API Guide (UI Integration)

Reference for frontend developers integrating with the SSRd backend.

- **Base URL (dev):** `https://localhost:7185` or `http://localhost:5176`
- **Swagger UI (dev):** `{baseUrl}/swagger`
- **Auth scheme:** Bearer JWT — `Authorization: Bearer <token>`
- **Content-Type:** `application/json`

All responses follow this envelope:

```json
{
  "success": true,
  "message": "Success",
  "data": { /* endpoint-specific payload, may be null */ }
}
```

On error: `success: false`, `message` populated, `data: null`. HTTP status conveys category (400 validation/business, 401 unauthenticated, 403 forbidden, 404 not found, 500 server).

---

## 1. Member journey

### 1.1 Initiate (send OTP)

**`POST /api/auth/initiate`**

Request:
```json
{ "phoneNo": "9876543210" }
```

Response:
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "isExistingMember": true,
    "message": "OTP sent. Please verify to login."
  }
}
```

UI behaviour:
- `isExistingMember: true` → next call is `POST /api/auth/login`
- `isExistingMember: false` → next call is `POST /api/auth/register`

### 1.2 Register (new member only)

**`POST /api/auth/register`**

Request:
```json
{
  "phoneNo": "9876543210",
  "otp": "123456",
  "name": "Rahul Pandey",
  "fatherName": "Father Name",
  "address": "Bangalore",
  "age": 0,
  "birthdate": "1990-01-15",
  "pancard": "ABCDE1234F",
  "adharCard": "1234-5678-9012"
}
```

Response (201/200):
```json
{
  "success": true,
  "message": "Registration successful. Please login with your mobile number.",
  "data": {
    "memberId": 42,
    "membershipNo": "SSR000042",
    "message": "Registration successful. Please login with your mobile number."
  }
}
```

After register: send the user back to `/initiate` → `/login` to get a token. **Register does not return a JWT.**

### 1.3 Login (existing member)

**`POST /api/auth/login`**

Request:
```json
{ "phoneNo": "9876543210", "otp": "123456" }
```

Response:
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOi...",
    "memberId": 42,
    "name": "Rahul Pandey",
    "membershipNo": "SSR000042",
    "expiresAt": "2026-05-10T10:23:00Z"
  }
}
```

**Store `token` and `expiresAt`.** Send `Authorization: Bearer <token>` on every subsequent request. Default lifetime: 24h (`Jwt:ExpiryHours`).

### 1.4 Whoami (verify token / hydrate session)

**`GET /api/auth/me`** — requires `Authorization`

Response:
```json
{
  "success": true,
  "data": {
    "memberId": "42",
    "name": "Rahul Pandey",
    "membershipNo": "SSR000042",
    "phoneNo": "9876543210"
  }
}
```

Use on app startup to validate a stored token before showing the dashboard.

---

## 2. Membership & RD account details

> **All endpoints in this section require `Authorization: Bearer <token>`.** A member can only read their own data — supplying another member's `membershipNo` returns `403 Forbidden`. Admin tokens (`Role=Admin`) can read any member.

### 2.1 Get member profile by membership number

**`GET /api/membership/{membershipNo}`** — requires member or admin `Authorization`

Example: `GET /api/membership/SSR000042`

Response:
```json
{
  "status": true,
  "data": {
    "memberId": 42,
    "membershipNo": "SSR000042",
    "name": "Rahul Pandey",
    "phoneNo": "9876543210",
    "address": "Bangalore",
    "age": 35,
    "birthdate": "1990-01-15T00:00:00",
    "authStatus": "U"
  }
}
```

> Note: this controller uses `{ status, data }` instead of the standard envelope. UI should treat both shapes.

### 2.2 Get RD accounts of a member

**`GET /api/membership?membershipNo=SSR000042`** — requires member or admin `Authorization`
**`GET /api/membership?rdaccno=<deposit_account>`** — requires member or admin `Authorization`

At least one query param required. Members are restricted to their own `membershipNo`; if `rdaccno` is given, the result set is filtered to only deposits owned by the caller. Response:
```json
{
  "status": true,
  "data": [
    {
      "rdDepositsId": 7,
      "rdRefNumber": "RD0000007",
      "membershipNo": "SSR000042",
      "deposit_Type": "RD",
      "product_Code": null,
      "deposit_Amount": 5000,
      "tenor": 1,
      "value_Date": "2025-05-01T00:00:00",
      "book_Date": "2025-05-01T00:00:00",
      "liquidation_Date": null,
      "deposit_Account": "DA0000007",
      "authStatus": "A"
    }
  ]
}
```

Returns `404` if nothing found.

---

## 3. Create a new RD (member action)

> **DB schema note:** `tt_rddepositonline` now carries audit columns. Run once on the database:
> ```sql
> ALTER TABLE tt_rddepositonline
>   ADD COLUMN CreatedBy   VARCHAR(30) NULL AFTER CreatedDate,
>   ADD COLUMN CreatedTime TIME        NULL AFTER CreatedBy,
>   ADD COLUMN AuthBy      VARCHAR(30) NULL AFTER AuthStatus,
>   ADD COLUMN AuthTime    DATETIME    NULL AFTER AuthBy;
> ```

**`POST /api/rdcreation/create`** — requires member `Authorization`

Request:
```json
{
  "MEMBERSHIP_NO": "SSR000042",
  "name": "Rahul Pandey",
  "Deposit_Type": "RD",
  "Deposit_Amount": 5000,
  "Tenor": 1
}
```

Field rules:

| Field            | Required | Constraints                                     |
|------------------|----------|-------------------------------------------------|
| `MEMBERSHIP_NO`  | yes      | Must match the JWT's `membershipNo` (or omit and the server fills it from the token) |
| `name`           | yes      | Member name                                     |
| `Deposit_Type`   | yes      | One of `RD`, `RDS`, `CD`                        |
| `Deposit_Amount` | yes      | > 0                                             |
| `Tenor`          | yes      | Years — currently only `1` or `2` accepted      |

Interest rate is derived server-side from tenor (1y → 9%, 2y → 10%).

Response (200) — auto-auth OFF (default):
```json
{
  "success": true,
  "message": "RD created. Pending admin authorization.",
  "data": {
    "rdDepositsId": 18,
    "MEMBERSHIP_NO": "SSR000042",
    "deposit_Type": "RD",
    "deposit_Amount": 5000,
    "tenor": 1,
    "interestRate": 9.0,
    "authStatus": "U",
    "createdBy": "SSR000042",
    "createdDate": "2026-05-09T00:00:00",
    "createdTime": "11:42:17",
    "authBy": null,
    "authTime": null,
    "message": "RD created. Pending admin authorization."
  }
}
```

Response (200) — auto-auth ON:
```json
{
  "success": true,
  "message": "RD created and auto-authorized.",
  "data": {
    "rdDepositsId": 18,
    "MEMBERSHIP_NO": "SSR000042",
    "deposit_Type": "RD",
    "deposit_Amount": 5000,
    "tenor": 1,
    "interestRate": 9.0,
    "authStatus": "A",
    "createdBy": "SSR000042",
    "createdDate": "2026-05-09T00:00:00",
    "createdTime": "11:42:17",
    "authBy": "SSR000042",
    "authTime": "2026-05-09T11:42:17",
    "message": "RD created and auto-authorized."
  }
}
```

Audit fields:

| Field         | Meaning                                                                 |
|---------------|-------------------------------------------------------------------------|
| `createdBy`   | `MEMBERSHIP_NO` of the member who submitted the RD                      |
| `createdDate` | Date of submission                                                      |
| `createdTime` | Time of submission                                                      |
| `authStatus`  | `U` = unauthorized (pending) · `A` = authorized                         |
| `authBy`      | Who authorized — member's `MEMBERSHIP_NO` (auto-auth) or admin username |
| `authTime`    | When it was authorized (`null` while pending)                           |

`authStatus` is `"A"` (auto-authorized, with `authBy = createdBy`) when `RdSettings:AutoAuthorize=true`, otherwise `"U"` until an admin authorizes via §4.3. Show this state on the UI so the user knows whether their RD is active or awaiting admin review.

Validation errors (400):
```json
{ "success": false, "message": "Invalid Deposit_Type. Allowed: RD, RDS, CD." }
```

`403 Forbidden` is returned if the body's `MEMBERSHIP_NO` does not match the logged-in member.

---

## 4. Admin journey

### 4.1 Admin login

**`POST /api/admin/auth/login`**

Request:
```json
{ "userName": "admin", "password": "secret" }
```

Response:
```json
{
  "success": true,
  "message": "Admin login successful.",
  "data": {
    "token": "eyJhbGciOi...",
    "userId": 225,
    "userName": "Manu",
    "branch": "BR001",
    "expiresAt": "2026-05-10T10:23:00Z",
    "loginAuditId": 901
  }
}
```

Store `token` (carries `Role=Admin`) and `loginAuditId` (needed for logout). All admin endpoints require this token.

> **Production note:** password verification is currently a placeholder; do not deploy to production until the legacy `s_Password` cipher is wired up.

### 4.2 List pending RDs

**`GET /api/rdcreation/pending`** — requires admin `Authorization`

Response:
```json
{
  "success": true,
  "data": [
    {
      "rdDepositsId": 18,
      "MEMBERSHIP_NO": "SSR000042",
      "name": "Rahul Pandey",
      "deposit_Type": "RD",
      "deposit_Amount": 5000,
      "tenor": 1,
      "createdDate": "2026-05-09T00:00:00",
      "createdBy": "SSR000042",
      "createdTime": "11:42:17",
      "authStatus": "U",
      "authBy": null,
      "authTime": null
    }
  ]
}
```

### 4.3 Authorize an RD

**`POST /api/rdcreation/{id}/authorize`** — requires admin `Authorization`

Example: `POST /api/rdcreation/18/authorize` (no body)

On success this endpoint does two things atomically:
1. Inserts a booked-RD row into `tt_rddeposits` (the live RD ledger) — generates `RdRefNumber` (`RD0000019`) and `Deposit_Account` (`DA0000019`), sets `value_Date` / `Book_Date` to today and `Liquidation_Date` to today + tenor years, copies `InterestRate` from the rate table, stamps `BranchCode` / `CreatedBy` / `AuthorisedBy` from the admin's JWT, sets `AuthStatus='A'`, `RecordStatus='O'`.
2. Updates the request row in `tt_rddepositonline`: `AuthStatus='A'`, `AuthBy = adminUserName`, `AuthTime = now`.

If either insert fails, the whole authorization is rolled back.

Response:
```json
{
  "success": true,
  "message": "Authorized by Manu (id 225). Booked as RD0000019 / DA0000019.",
  "data": {
    "rdDepositsId": 18,
    "MEMBERSHIP_NO": "SSR000042",
    "deposit_Type": "RD",
    "deposit_Amount": 5000,
    "tenor": 1,
    "interestRate": 9.0,
    "authStatus": "A",
    "createdBy": "SSR000042",
    "createdDate": "2026-05-09T00:00:00",
    "createdTime": "11:42:17",
    "authBy": "Manu",
    "authTime": "2026-05-09T12:05:33",
    "message": "Authorized by Manu (id 225). Booked as RD0000019 / DA0000019."
  }
}
```

> The `rdDepositsId` in the response is the **online request id** (from `tt_rddepositonline`). The newly created booked row in `tt_rddeposits` has its own auto-increment id; it's referenced indirectly via the `RdRefNumber` shown in the message. UI can fetch the booked details via `GET /api/membership?membershipNo=...` (which reads `tt_rddeposits`).

Returns 400 if the deposit was already authorized or doesn't exist.

### 4.4 Admin logout (closes audit row)

**`POST /api/admin/auth/logout/{auditId}`** — requires admin `Authorization`

Stamps `LogoutTime` on the row created at login. Response:
```json
{ "success": true, "message": "Logged out.", "data": { "auditId": 901 } }
```

---

## 5. Auth & error handling — UI checklist

- **Token storage:** keep the JWT in `sessionStorage` or an httpOnly cookie (proxied). Do not put it in `localStorage` if you can avoid it (XSS risk).
- **Attach token:** `Authorization: Bearer <token>` on every authenticated call.
- **401 handling:** if any call returns 401, drop the token and route the user back to the login screen.
- **403 handling:** the user is logged in but acting on someone else's data — show an "access denied" screen, do not log them out.
- **Token expiry:** check `expiresAt` before calling protected endpoints; force re-login when expired.
- **Response envelope:** member/admin auth + RD creation use `{ success, message, data }`. The Membership controller uses `{ status, data, message }`. Handle both.
- **OTP flow:** OTPs are 6 digits, sent via SMS (mock provider in dev — check server logs for the code). Each OTP is single-use; resend by hitting `/initiate` again.

---

## 6. Quick reference

| Method | Path                                  | Auth   | Purpose                          |
|--------|---------------------------------------|--------|----------------------------------|
| POST   | `/api/auth/initiate`                  | none   | Send OTP, check member exists    |
| POST   | `/api/auth/register`                  | none   | Create member (verifies OTP)     |
| POST   | `/api/auth/login`                     | none   | Verify OTP → JWT                 |
| GET    | `/api/auth/me`                        | member | Validate token, get claims       |
| GET    | `/api/membership/{membershipNo}`      | member/admin | Member profile (owner only) |
| GET    | `/api/membership?membershipNo=&rdaccno=` | member/admin | List RD accounts (owner only) |
| POST   | `/api/rdcreation/create`              | member | Submit new RD                    |
| GET    | `/api/rdcreation/pending`             | admin  | List unauthorized RDs            |
| POST   | `/api/rdcreation/{id}/authorize`      | admin  | Authorize an RD                  |
| POST   | `/api/admin/auth/login`               | none   | Admin login → JWT (Role=Admin)   |
| POST   | `/api/admin/auth/logout/{auditId}`    | admin  | Stamp logout time on audit row   |

All endpoints marked `member` / `admin` require `Authorization: Bearer <token>`. Members are scoped to their own data; admins are unrestricted.

---

## 7. Sample frontend flow (pseudocode)

```ts
// 1. User enters phone
await api.post('/api/auth/initiate', { phoneNo });

// 2a. Existing member → enter OTP → login
const { data } = await api.post('/api/auth/login', { phoneNo, otp });
saveToken(data.token, data.expiresAt);

// 2b. New member → fill form → register → back to step 1
await api.post('/api/auth/register', { phoneNo, otp, name, ... });

// 3. Dashboard load
const me  = await api.get('/api/auth/me');
const acc = await api.get(`/api/membership?membershipNo=${me.data.membershipNo}`);

// 4. Create new RD
await api.post('/api/rdcreation/create', {
  MEMBERSHIP_NO: me.data.membershipNo,
  name: me.data.name,
  Deposit_Type: 'RD',
  Deposit_Amount: 5000,
  Tenor: 1
});
```

Admin app uses the same axios client but stores a separate token from `/api/admin/auth/login` and renders the `/api/rdcreation/pending` queue.
