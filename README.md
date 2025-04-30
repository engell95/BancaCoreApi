# BancaCore API

BancaCore API es un proyecto de API RESTful para la gestión de operaciones bancarias básicas, incluyendo clientes, cuentas bancarias y transacciones.

## Estructura del Proyecto

Arquitectura del Proyecto: Clean Architecture + CQRS
El proyecto está estructurado siguiendo los principios de Clean Architecture, que promueve la separación de responsabilidades, la independencia de tecnologías y la facilidad para realizar pruebas y mantenimientos. Además, implementa el patrón CQRS (Command Query Responsibility Segregation) para desacoplar las operaciones de lectura y escritura de datos con las siguientes capas:

- **Core**: Contiene el dominio, la lógica de aplicación y las interfaces comunes
  - Domain: Entidades y reglas de dominio
  - Application: Casos de uso y lógica de negocio
  - Common: Interfaces y utilidades compartidas
- **Infrastructure**: Implementaciones concretas de las interfaces definidas en Core
  - Persistence: Acceso a datos y configuración de Entity Framework
- **Presentation**: Capa de presentación
  - WebApi: API REST con controladores y configuración

## Requisitos Previos

- [.NET SDK](https://dotnet.microsoft.com/download) (recomendado .NET 8.0 o superior)
- Un editor de código como [Visual Studio](https://visualstudio.microsoft.com/), [Visual Studio Code](https://code.visualstudio.com/)

## Cómo Ejecutar el Proyecto

### Desde la línea de comandos

1. Clone el repositorio:
   ```bash
   git clone <url-del-repositorio>
   cd <nombre-del-directorio>
   ```

2. Navegue al directorio del proyecto WebApi:
   ```bash
   cd Src/Presentation/WebApi
   ```

3. Restaure las dependencias y compile el proyecto:
   ```bash
   dotnet restore
   dotnet build
   ```

4. Ejecute la aplicación:
   ```bash
   dotnet run
   ```

5. La API estará disponible en:
   - HTTPS: https://localhost:44370
   - Swagger UI: https://localhost:44370/swagger

### Desde Visual Studio

1. Abra la solución en Visual Studio
2. Presione F5 o haga clic en el botón "Iniciar" para ejecutar la aplicación
4. La API se abrirá automáticamente en su navegador predeterminado

## Base de Datos

El proyecto utiliza SQLite como base de datos, que se crea automáticamente en la carpeta `Data` al iniciar la aplicación. La cadena de conexión se configura en `appsettings.json`:

```json
"ConnectionStrings": {
  "Database": "Data Source=Data/banca.db"
}
```

## Cómo Ejecutar las Pruebas

### Desde la línea de comandos

1. Navegue al directorio de pruebas:
   ```bash
   cd Test/BancaCoreApi.Test
   ```

2. Ejecute las pruebas:
   ```bash
   dotnet test
   ```

### Desde Visual Studio

1. Abra el Explorador de pruebas (Test Explorer) desde el menú "Ver" > "Explorador de pruebas"
2. Haga clic en "Ejecutar todas las pruebas" o seleccione pruebas específicas para ejecutar

## Endpoints de la API

La API proporciona los siguientes endpoints principales:

### Clientes
- GET /api/clientes - Obtener todos los clientes
- GET /api/clientes/{id} - Obtener un cliente por ID
- POST /api/clientes - Crear un nuevo cliente
- PUT /api/clientes/{id} - Actualizar un cliente existente
- DELETE /api/clientes/{id} - Eliminar un cliente

### Cuentas Bancarias
- GET /api/cuentasbancarias - Obtener todas las cuentas bancarias
- GET /api/cuentasbancarias/{id} - Obtener una cuenta bancaria por ID
- POST /api/cuentasbancarias - Crear una nueva cuenta bancaria
- PUT /api/cuentasbancarias/{id} - Actualizar una cuenta bancaria existente
- DELETE /api/cuentasbancarias/{id} - Eliminar una cuenta bancaria

### Transacciones
- GET /api/transacciones - Obtener todas las transacciones
- GET /api/transacciones/{id} - Obtener una transacción por ID
- POST /api/transacciones - Crear una nueva transacción

## Documentación de la API

La documentación completa de la API está disponible a través de Swagger UI, que se puede acceder en:

```
https://localhost:44370/swagger
```

## Configuración

La configuración principal de la aplicación se encuentra en el archivo `appsettings.json`. Puede modificar la configuración según sus necesidades, incluyendo:

- Cadenas de conexión
- Configuración de CORS
- Información de la API para Swagger