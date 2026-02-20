# Sistema de Login (Angular + .NET 9)

## Comandos de Ejecución

### .NET 9

```bash
cd backendapi
dotnet run
```

### Angular

```bash
cd frontend
npm install
npm start
```

## Variables de Entorno (.env en backendapi)

```env
DB_CONNECTION=Server=.;Database=bdcookie;User Id=sa;Password=123456;TrustServerCertificate=True;
JWT_KEY=TuClaveMaestraSuperSecretaDeAlMenos32Caracteres!
JWT_ISSUER=MiAppBackend
JWT_AUDIENCE=MiAppFrontend
SEED_USERNAME=admin
SEED_PASSWORD=admin123
```

## Postman / .http URLs

### 1. Login (POST)

**URL:** `http://localhost:50637/api/Auth/login` 
**Body (JSON):**

```json
{
  "username": "admin",
  "password": "admin123"
}
```

### 2. Cerrar sesión (POST)

**URL:** `https://localhost:5000/api/Auth/logout`
**Body:** Vacío.

### 3. Perfil protegido (GET)

**URL:** `https://localhost:5000/api/Auth/perfil`
Requiere enviar Cookie `AuthToken`.


<img width="1343" height="620" alt="image" src="https://github.com/user-attachments/assets/639bbca1-345a-48a6-bfb1-2f20bab9abdd" />
<img width="1357" height="634" alt="image" src="https://github.com/user-attachments/assets/ba11ff64-5b4f-4d90-a57e-5e9849b79e53" />
<img width="554" height="637" alt="image" src="https://github.com/user-attachments/assets/abcd55db-53e3-4b3b-a8df-353a6d901f4b" />

