# BankWebApi

## Descripción
BankWebApi es una API RESTful desarrollada en .NET 8 que simula las operaciones básicas de un sistema bancario. Permite la gestión de clientes, cuentas y transacciones, incluyendo depósitos, retiros y consulta de saldos. Utiliza Entity Framework Core con una base de datos SQLite para el almacenamiento de datos.

## Estructura del Proyecto
- **BankWebApi.Api**: Proyecto principal de la API.
- **BankWebApi.Connections**: Configuración de la base de datos y modelos de datos.
- **BankWebApi.Models**: DTOs y tipos de datos compartidos.
- **BankWebApi.Services**: Lógica de negocio y servicios.
- **BankWebApi.Testing / BankWebApi.Tests**: Proyectos de pruebas.

## Requisitos Previos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Opcional) Visual Studio 2022+ o VS Code

## Pasos para correr el proyecto
1. **Clona el repositorio y navega a la carpeta del proyecto:**
   ```sh
   git clone https://github.com/blackdiamond93/BankWebApi.git
   cd BankWebApi/BankWebApi
   ```
2. **Restaura los paquetes NuGet:**
   ```sh
   dotnet restore
   ```
3. **Verifica la cadena de conexión:**
   - El archivo `appsettings.json` ya está configurado para usar SQLite en `../BankWebApi.Connections/bankapp.db`.

4. **Migraciones de la base de datos:**
   - Si es la primera vez o has realizado cambios en los modelos, debes aplicar o crear migraciones.
   - Instala la herramienta si no la tienes:
     ```sh
     dotnet tool install --global dotnet-ef
     ```
   - Para aplicar la migración existente y crear la base de datos:
     ```sh
     dotnet ef database update --project ../BankWebApi.Connections/BankWebApi.Connections.csproj
     ```
   - Para crear una nueva migración después de modificar los modelos:
     ```sh
     dotnet ef migrations add NombreDeLaMigracion --project ../BankWebApi.Connections/BankWebApi.Connections.csproj
     dotnet ef database update --project ../BankWebApi.Connections/BankWebApi.Connections.csproj
     ```

5. **Ejecuta la API:**
   ```sh
   dotnet run --project BankWebApi.Api.csproj
   ```
   Por defecto, la API estará disponible en:
   - http://localhost:5275 (HTTP)
   - https://localhost:7237 (HTTPS)

6. **Explora la documentación Swagger:**
   Al iniciar la API, accede a [http://localhost:5275/swagger](http://localhost:5275/swagger) para ver y probar los endpoints disponibles.

## Endpoints Disponibles

### Clientes
- `POST /api/customers` — Crear un nuevo cliente.
  - **Body:**
    ```json
    {
      "name": "string",
      "dateOfBirth": "yyyy-MM-dd",
      "gender": "string",
      "income": decimal
    }
    ```
  - **Respuestas:**
    - 201 Created: Cliente creado
    - 400 Bad Request: Datos inválidos
    - 409 Conflict: Cliente ya existe
- `GET /api/customers/{id}` — Obtener cliente por ID.
  - **Respuestas:**
    - 200 OK: Datos del cliente
    - 404 Not Found: No existe

### Cuentas
- `POST /api/accounts` — Crear una nueva cuenta bancaria.
  - **Body:**
    ```json
    {
      "accountNumber": "string",
      "clientId": int,
      "balance": decimal,
      "createdAt": "yyyy-MM-ddTHH:mm:ss"
    }
    ```
  - **Respuestas:**
    - 201 Created: Cuenta creada
    - 400 Bad Request: Datos inválidos
    - 409 Conflict: Número de cuenta ya existe
    - 404 Not Found: Cliente no existe
- `GET /api/accounts/balance/{accountNumber}` — Consultar el saldo de una cuenta.
  - **Respuestas:**
    - 200 OK: Saldo actual
    - 404 Not Found: Cuenta no existe
- `POST /api/accounts/apply-interest/{accountNumber}?annualRate={rate}` — Aplicar interés a una cuenta.
  - **Parámetros:**
    - `annualRate`: decimal (tasa anual)
  - **Respuestas:**
    - 200 OK: Interés aplicado
    - 404 Not Found: Cuenta no existe

### Transacciones
- `POST /api/transactions/register` — Registrar una transacción (depósito o retiro).
  - **Body:**
    ```json
    {
      "accountNumber": "string",
      "transactionType": 0, // 0=Deposit, 1=Withdrawal
      "amount": decimal
    }
    ```
  - **Respuestas:**
    - 201 Created: Transacción registrada
    - 400 Bad Request: Datos inválidos
    - 404 Not Found: Cuenta no existe
    - 422 Unprocessable Entity: Fondos insuficientes
- `GET /api/transactions/history/{accountNumber}` — Obtener el historial de transacciones de una cuenta.
  - **Respuestas:**
    - 200 OK: Lista de transacciones
    - 404 Not Found: Cuenta no existe
- `GET /api/transactions/summary/{accountNumber}` — Obtener el resumen de transacciones y balance actual de una cuenta.
  - **Respuestas:**
    - 200 OK: Resumen de movimientos y saldo
    - 404 Not Found: Cuenta no existe

## Pruebas
Para ejecutar las pruebas automatizadas:
```sh
dotnet test ../BankWebApi.Testing/BankWebApi.Testing.csproj
```

## Notas
- La base de datos SQLite se encuentra en `BankWebApi.Connections/bankapp.db`.
- Puedes modificar la cadena de conexión en `appsettings.json` según tus necesidades.
- Si tienes problemas con las migraciones, asegúrate de tener instalado el paquete `dotnet-ef` y que la base de datos no esté bloqueada por otro proceso.

---

¡Listo! Ahora puedes correr y probar la API bancaria localmente. 