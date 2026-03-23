# API Contracts

## Auth

### POST `/api/auth/register`

Request body:

```json
{
  "username": "mlopez",
  "firstName": "Maria",
  "lastName": "Lopez",
  "email": "maria.lopez@udem.edu",
  "password": "Password123!",
  "preferredLocale": "es_MX"
}
```

Successful response `201 Created`:

```json
{
  "tokenType": "Bearer",
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-03-22T18:45:00Z",
  "user": {
    "id": "00000000-0000-0000-0000-000000000000",
    "username": "mlopez",
    "statusCode": "active",
    "preferredLocale": "es_MX",
    "displayName": "Maria Lopez",
    "email": "maria.lopez@udem.edu",
    "roles": [
      "STUDENT"
    ]
  }
}
```

Possible errors:

- `400 Bad Request` for invalid payload
- `409 Conflict` when username or email already exists

### POST `/api/auth/login`

Request body:

```json
{
  "username": "mlopez",
  "password": "Password123!"
}
```

Successful response `200 OK`:

```json
{
  "tokenType": "Bearer",
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-03-22T18:45:00Z",
  "user": {
    "id": "00000000-0000-0000-0000-000000000000",
    "username": "mlopez",
    "statusCode": "active",
    "preferredLocale": "es_MX",
    "displayName": "Maria Lopez",
    "email": "maria.lopez@udem.edu",
    "roles": [
      "STUDENT"
    ]
  }
}
```

Possible errors:

- `400 Bad Request` for invalid payload
- `401 Unauthorized` for invalid credentials or locked user

## Libros

### GET `/api/libros`

Query params opcionales:

- `title`
- `author`
- `subject`
- `isbn`
- `publisher`
- `language`

Ejemplo:

```http
GET /api/libros?author=Asimov&subject=SCI_FI
```

Successful response `200 OK`:

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000000",
    "title": "Fundacion",
    "subtitle": null,
    "isbn": "9780553293357",
    "publisher": "Gnome Press",
    "publicationYear": 1951,
    "edition": "1",
    "language": "es",
    "authors": [
      {
        "id": "00000000-0000-0000-0000-000000000000",
        "fullName": "Isaac Asimov",
        "contribution": "Autor"
      }
    ],
    "subjects": [
      {
        "id": "00000000-0000-0000-0000-000000000000",
        "code": "SCI_FI",
        "name": "Ciencia Ficcion"
      }
    ],
    "createdAt": "2026-03-23T00:27:55.352Z",
    "updatedAt": "2026-03-23T00:27:55.352Z"
  }
]
```

Possible errors:

- `401 Unauthorized` when the token is missing or invalid

### POST `/api/libros`

Request body:

```json
{
  "title": "El Aleph",
  "subtitle": null,
  "isbn": "9788420674208",
  "publisher": "Emece",
  "publicationYear": 1949,
  "edition": "1",
  "language": "es",
  "summaryJson": "{\"short\":\"Coleccion de cuentos\"}",
  "metadataJson": "{\"format\":\"tapa blanda\"}",
  "authors": [
    {
      "fullName": "Jorge Luis Borges",
      "contribution": "Autor"
    }
  ],
  "subjectCodes": [
    "LIT_LATAM"
  ]
}
```

Successful response `201 Created`:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "title": "El Aleph",
  "subtitle": null,
  "isbn": "9788420674208",
  "publisher": "Emece",
  "publicationYear": 1949,
  "edition": "1",
  "language": "es",
  "summaryJson": "{\"short\":\"Coleccion de cuentos\"}",
  "metadataJson": "{\"format\":\"tapa blanda\"}",
  "authors": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "fullName": "Jorge Luis Borges",
      "contribution": "Autor"
    }
  ],
  "subjects": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "code": "LIT_LATAM",
      "name": "Literatura Latinoamericana"
    }
  ],
  "createdAt": "2026-03-23T00:27:55.352Z",
  "updatedAt": "2026-03-23T00:27:55.352Z"
}
```

Possible errors:

- `400 Bad Request` for invalid payload
- `404 Not Found` when any subject code does not exist
- `401 Unauthorized` when the token is missing or invalid
- `409 Conflict` when the ISBN already exists

### GET `/api/libros/{id}`

Successful response `200 OK`:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "title": "El Aleph",
  "subtitle": null,
  "isbn": "9788420674208",
  "publisher": "Emece",
  "publicationYear": 1949,
  "edition": "1",
  "language": "es",
  "summaryJson": "{\"short\":\"Coleccion de cuentos\"}",
  "metadataJson": "{\"format\":\"tapa blanda\"}",
  "authors": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "fullName": "Jorge Luis Borges",
      "contribution": "Autor"
    }
  ],
  "subjects": [
    {
      "id": "00000000-0000-0000-0000-000000000000",
      "code": "LIT_LATAM",
      "name": "Literatura Latinoamericana"
    }
  ],
  "createdAt": "2026-03-23T00:27:55.352Z",
  "updatedAt": "2026-03-23T00:27:55.352Z"
}
```

Possible errors:

- `401 Unauthorized` when the token is missing or invalid
- `404 Not Found` when the book does not exist

### PUT `/api/libros/{id}`

Request body:

- usa el mismo contrato de `POST /api/libros`

Successful response `200 OK`:

- returns the same `LibroFichaDto` contract as `POST /api/libros`

Possible errors:

- `400 Bad Request` for invalid payload
- `404 Not Found` when the book or any subject code does not exist
- `401 Unauthorized` when the token is missing or invalid
- `409 Conflict` when the ISBN already exists

### DELETE `/api/libros/{id}`

Successful response `204 No Content`

Possible errors:

- `401 Unauthorized` when the token is missing or invalid
- `404 Not Found` when the book does not exist

## Usuarios

### GET `/api/usuarios`

Successful response `200 OK`:

```json
[
  {
    "id": "00000000-0000-0000-0000-000000000000",
    "username": "admin1",
    "firstName": "Admin",
    "lastName": "One",
    "displayName": "Admin One",
    "email": "admin@example.com",
    "universityId": "ADM-2024-001",
    "roleCode": "ADMIN",
    "roleLabel": "Administrador",
    "statusCode": "active",
    "statusLabel": "Activo",
    "preferredLocale": "es_MX",
    "createdAt": "2026-03-23T00:27:55.352Z",
    "updatedAt": "2026-03-23T00:27:55.352Z",
    "deletedAt": null
  }
]
```

Possible errors:

- `401 Unauthorized` when the token is missing or invalid

### POST `/api/usuarios`

Request body:

```json
{
  "username": "prof.math",
  "firstName": "Elena",
  "lastName": "Suarez",
  "displayName": "Dra. Elena Suarez",
  "email": "e.suarez@udem.edu",
  "password": "Password123!",
  "roleCode": "TEACHER",
  "statusCode": "active",
  "preferredLocale": "es_MX",
  "documentType": "employee_id",
  "documentNumber": "PRF-2026-001",
  "metadataJson": "{\"source\":\"admin-panel\"}"
}
```

Successful response `201 Created`:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "username": "prof.math",
  "firstName": "Elena",
  "lastName": "Suarez",
  "displayName": "Dra. Elena Suarez",
  "email": "e.suarez@udem.edu",
  "universityId": "PRF-2026-001",
  "roleCode": "TEACHER",
  "roleLabel": "Profesor",
  "statusCode": "active",
  "statusLabel": "Activo",
  "preferredLocale": "es_MX",
  "createdAt": "2026-03-23T00:27:55.352Z",
  "updatedAt": "2026-03-23T00:27:55.352Z",
  "deletedAt": null
}
```

Possible errors:

- `400 Bad Request` for invalid payload
- `401 Unauthorized` when the token is missing or invalid
- `409 Conflict` when username or email already exists

### PUT `/api/usuarios/{id}`

Request body:

```json
{
  "username": "prof.math",
  "firstName": "Elena",
  "lastName": "Suarez",
  "displayName": "Dra. Elena Suarez",
  "email": "e.suarez@udem.edu",
  "password": "Password123!",
  "roleCode": "TEACHER",
  "statusCode": "inactive",
  "preferredLocale": "es_MX",
  "documentType": "employee_id",
  "documentNumber": "PRF-2026-001",
  "metadataJson": "{\"source\":\"admin-panel\"}"
}
```

Successful response `200 OK`:

- returns the same `UsuarioDto` contract as `POST /api/usuarios`

Possible errors:

- `400 Bad Request` for invalid payload
- `401 Unauthorized` when the token is missing or invalid
- `404 Not Found` when the user does not exist
- `409 Conflict` when username or email already exists

### DELETE `/api/usuarios/{id}`

Successful response `204 No Content`

Possible errors:

- `401 Unauthorized` when the token is missing or invalid
- `404 Not Found` when the user does not exist
