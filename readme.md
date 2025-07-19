# Notes API.net:

API REST para gestionar notas o apuntes con autenticación de usuarios.

## Tecnologías utilizadas:

- C#
- ASP.Net WebAPI
- Entity Framework Core (EFCore)
- PostgreSQL
- .Net 8
- BCrypt.net
- JSONWebToken (jwt)

## Instalación:

- Clona el proyecto: 
```bash
  git clone https://github.com/JosePerezHuanca/NotesAPI.net
  cd NotesAPI.net/NotesAPI
```
- Instala las dependencias: ``` dotnet restore ```
- Aplica las migraciones: ``` dotnet ef database update ```
- Configura las variables de entorno (ver .env.example como referencia)
- Ejecutar el proyecto: ``` dotnet run ```

El servidor está disponible en el puerto 5172 y la documentación en swagger en [localhost:5172/swagger](localhost:5172/swagger)

## Endpoints:

### /auth:

#### /register:

Método: POST.
Datos a proporcionar:
```json
{
  "username": "string",
  "email": "user@example.com",
  "password": "string"
}
```

Respuestas: 201, 400, 409, 500.

#### /login:

Método: POST.
Datos a proporcionar:
```json
{
  "loginIdentifier": "string", //Puede ser tanto el correo como el nombre de usuario registrado
  "password": "string"
}
```

Respuestas: 200, 400, 500

Ejemplo de respuesta exitosa:
```json
{
  "success": true,
  "status": 200,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjMiLCJleHAiOjE3NTI2OTc2NzAsImlzcyI6Im5vdGVzQVBJIiwiYXVkIjoiYW5kcm9pZCJ9.qiYIaqqsW14PN1HRuMTbhWE__2GFQoPJq-3nyt-v6u4"
}
```

#### /notes:

El endpoint /notes requiere autenticación y los siguientes datos en el header:
```
Authorization: Bearer token
```

Métodos: 

- GET /notes: Lista de todas las notas
- GET /notes/id: Obtiene una nota por id
- POST /notes: Crea una nueva nota
- PUT /notes/id: Actualiza una nota existente
- DELETE /notes/id: Elimina una nota

Respuestas: 200, 201, 204, 400, 401, 500.

##### POST:

Datos a proporcionar:
```json
{
  "title": "string",
  "content": "string"
}
```

