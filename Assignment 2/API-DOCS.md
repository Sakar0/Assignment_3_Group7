# Library Catalogue REST API (Read-Only)

A secure, read-only REST API exposing the public library catalogue. Part of the
Group 7 Library Management System (Aryan's feature: kiosk + read-only API).

**Base URL (Development):** `http://localhost:5108/api/catalogue`
**Format:** `application/json`
**Write operations:** none — the API is strictly GET-only.

---

## Quick start

```powershell
# 1. Start the app (web mode)
dotnet run --project "Assignment 2/Assignment 2.csproj" -- web

# 2. Call the API
Invoke-WebRequest http://localhost:5108/api/catalogue | Select-Object -Expand Content
```

Or open `Assignment 2/KioskApi.http` with the VS Code **REST Client** extension
and click *Send Request* above any entry.

A self-describing summary is also available at `GET /api/catalogue/docs`.

---

## Endpoints

### 1. List items — `GET /api/catalogue`

Returns a paged list of catalogue items.

**Query parameters**

| Name | Type | Required | Default | Notes |
|---|---|---|---|---|
| `name` | string | no | — | Partial, case-insensitive name filter |
| `type` | string | no | — | One of `Book`, `Music`, `Toy` |
| `status` | string | no | — | One of `Available`, `Borrowed`, `Damaged`, `Destroy` |
| `page` | int | no | `1` | Must be ≥ 1 |
| `pageSize` | int | no | `12` | Must be between 1 and 50 |

**Example request**

```
GET /api/catalogue?type=Book&status=Available&page=1&pageSize=2
```

**Example 200 response**

```json
{
  "items": [
    {
      "libraryCode": "BOOK001",
      "name": "The Great Gatsby",
      "description": "A classic American novel",
      "type": "Book",
      "status": "Available",
      "isAvailable": true,
      "author": "F. Scott Fitzgerald",
      "genre": "Fiction",
      "artist": null,
      "year": null,
      "toyType": null,
      "ageGroup": null
    }
  ],
  "page": 1,
  "pageSize": 2,
  "totalItems": 2,
  "totalPages": 1
}
```

---

### 2. Get one item — `GET /api/catalogue/{libraryCode}`

Returns a single catalogue item.

**Example request**

```
GET /api/catalogue/BOOK001
```

**Example 200 response**

```json
{
  "libraryCode": "BOOK001",
  "name": "The Great Gatsby",
  "description": "A classic American novel",
  "type": "Book",
  "status": "Available",
  "isAvailable": true,
  "author": "F. Scott Fitzgerald",
  "genre": "Fiction",
  "artist": null,
  "year": null,
  "toyType": null,
  "ageGroup": null
}
```

**404 example**

```json
{
  "title": "Item not found.",
  "detail": "No catalogue item exists with library code 'NOPE999'.",
  "status": 404
}
```

---

### 3. API summary — `GET /api/catalogue/docs`

Machine-readable description of the endpoints, parameters, limits and error
codes above. Read-only.

---

## Response model (`ItemDto`)

Only these fields are ever returned. Internal entities, borrower records, fines
and any persistence details are never exposed.

| Field | Type | Notes |
|---|---|---|
| `libraryCode` | string | Unique item identifier |
| `name` | string | Item title/name |
| `description` | string | Short description |
| `type` | string | `Book` / `Music` / `Toy` |
| `status` | string | `Available` / `Borrowed` / `Damaged` / `Destroy` |
| `isAvailable` | bool | Convenience flag (`status == "Available"`) |
| `author`, `genre` | string? | Book only, otherwise `null` |
| `artist`, `year` | string?, int? | Music only, otherwise `null` |
| `toyType`, `ageGroup` | string? | Toy only, otherwise `null` |

---

## Status codes

| Code | Meaning |
|---|---|
| `200 OK` | Successful read |
| `400 Bad Request` | Invalid query/route parameter |
| `404 Not Found` | Item does not exist |
| `405 Method Not Allowed` | Non-GET method (API is read-only) |
| `500 Internal Server Error` | Unexpected failure (no internal details leaked) |

Errors use **RFC 7807 ProblemDetails** (`title`, `detail`, `status`). Stack
traces, secrets and internal implementation details are never returned.

---

## Security

- **Read-only:** only `GET` actions exist; `POST`/`PUT`/`DELETE` return `405`.
  No endpoint can create, update or delete items, borrowers or borrow records.
- **No sensitive data:** the API exposes only the same public catalogue fields
  already shown on the existing `/Search` page. No borrower PII, passwords,
  tokens or configuration values are present.
- **Authentication:** the application has no authentication infrastructure; the
  catalogue is intentionally public (consistent with the existing search page).
- **Result limits:** collection responses are always paged (`pageSize` ≤ 50) so
  responses cannot be unbounded.
