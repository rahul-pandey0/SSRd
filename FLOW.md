# SSRd Auth API — Flow

Base URL: `http://localhost:5176/api/auth`

---

## New Member Flow

```
Client                          Server
  |                               |
  |-- POST /initiate ------------>|  { phoneNo }
  |<-- { isExistingMember:false } |  OTP sent via SMS
  |                               |
  |-- POST /register ------------>|  { phoneNo, otp, name, ... }
  |<-- { memberId, membershipNo } |  Account created, MembershipNo assigned (SSRxxxxxx)
  |                               |  NO token issued here
  |                               |
  |-- POST /initiate ------------>|  { phoneNo }   (fresh OTP for login)
  |<-- { isExistingMember:true }  |
  |                               |
  |-- POST /login --------------->|  { phoneNo, otp }
  |<-- { token, memberId, ... }   |  JWT issued, valid 24h
```

---

## Existing Member Flow

```
Client                          Server
  |                               |
  |-- POST /initiate ------------>|  { phoneNo }
  |<-- { isExistingMember:true }  |  OTP sent via SMS
  |                               |
  |-- POST /login --------------->|  { phoneNo, otp }
  |<-- { token, memberId, ... }   |  JWT issued, valid 24h
```

---

## Authenticated Requests

```
Client                          Server
  |                               |
  |-- GET /me ------------------->|  Header: Authorization: Bearer <token>
  |<-- { memberId, name, ... }    |
```

---

## Endpoints

### POST /initiate
Sends OTP. Tells client whether the number is already registered.

**Request**
```json
{ "phoneNo": "9876543210" }
```

**Response**
```json
{
  "success": true,
  "message": "OTP sent. Please verify to login.",
  "data": { "isExistingMember": true, "message": "OTP sent. Please verify to login." }
}
```

---

### POST /register
Verifies OTP, creates account, assigns `MembershipNo` (`SSR` + zero-padded `MemberId`).
**Does not issue a JWT.** The member must go through `/initiate` + `/login` to get a token.

**Request**
```json
{
  "phoneNo": "9876543210",
  "otp": "482910",
  "name": "Rahul Pandey",
  "fatherName": "R. Pandey",
  "address": "123 Main St",
  "age": 30,
  "birthdate": "1995-05-08",
  "pancard": "ABCDE1234F",
  "adharCard": "1234 5678 9012"
}
```

**Response**
```json
{
  "success": true,
  "message": "Registration successful. Please login with your mobile number.",
  "data": { "memberId": 1, "membershipNo": "SSR000001", "message": "..." }
}
```

---

### POST /login
Verifies OTP for an existing member and issues a JWT.

**Request**
```json
{ "phoneNo": "9876543210", "otp": "482910" }
```

**Response**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "<jwt>",
    "memberId": 1,
    "name": "Rahul Pandey",
    "membershipNo": "SSR000001",
    "expiresAt": "2026-05-09T10:00:00Z"
  }
}
```

---

### GET /me  _(requires Bearer token)_
Returns claims from the JWT.

**Response**
```json
{
  "success": true,
  "message": "Success",
  "data": { "memberId": "1", "name": "Rahul Pandey", "membershipNo": "SSR000001", "phoneNo": "9876543210" }
}
```

---

## OTP Rules
- Valid for **5 minutes**
- Any new `/initiate` call for a number **invalidates** the previous unused OTP
- OTP is **single-use** — consumed on successful login or registration

## MembershipNo Format
`SSR` + `MemberId` zero-padded to 6 digits → `SSR000001`, `SSR000042`, etc.

## AuthStatus
`U` = Unverified (default at registration). Future admin approval flow can update this.
