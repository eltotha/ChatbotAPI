# Chatbot API

API backend para un chatbot con integración a MySQL y conexión opcional con Ollama.

## Requisitos

- .NET SDK 10
- MySQL Server
- Ollama instalado y ejecutándose localmente
- Git

## Instalación de Ollama

Este proyecto usa Ollama para generar respuestas con el modelo `medgemma:4b`.

1. Descarga e instala Ollama desde su sitio oficial:
   https://ollama.com/download
2. Inicia el servicio de Ollama:

```bash
ollama serve
```

3. Descarga el modelo requerido:

```bash
ollama pull medgemma:4b
```

4. Verifica que el modelo esté disponible:

```bash
ollama list
```

> El servicio de la API consulta la URL `http://localhost:11434/api/generate`, por lo que Ollama debe estar activo antes de ejecutar la aplicación.

## Clonar y abrir el proyecto

```bash
git clone <url-del-repositorio>
cd ChatbotAPI
```

## Configuración de la base de datos

El proyecto usa la cadena de conexión `DefaultConnection` definida en `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=chatbot_db;Uid=[Tu usuario];Pwd=[Tu Password];"
  }
}
```

Asegúrate de que:

- MySQL esté corriendo en `localhost`
- La base de datos `chatbot_db` exista o pueda ser creada
- Las credenciales de acceso sean correctas

> El proyecto crea la base de datos automáticamente si no existe mediante `EnsureCreated()` al iniciar la aplicación.

## Restaurar dependencias

```bash
dotnet restore
```

## Compilar el proyecto

```bash
dotnet build
```

## Ejecutar la API

```bash
dotnet run
```

O en modo observador:

```bash
dotnet watch run
```

La API quedará disponible en el puerto configurado por `launchSettings.json` (normalmente HTTPS y HTTP locales con Kestrel).

## Swagger

Swagger está habilitado en entornos de desarrollo con estas líneas en `Program.cs`:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chatbot API v1");
        options.RoutePrefix = "swagger";
    });
}
```

Una vez ejecutada la aplicación, abre en el navegador:

```text
https://localhost:<puerto>/swagger
```

o en caso de usar HTTP:

```text
http://localhost:<puerto>/swagger
```

Esto mostrará la interfaz de Swagger con todos los endpoints disponibles de la API.

## Estructura principal

- `Controllers/` - Controladores de la API
- `Models/` - Modelos de datos
- `Data/` - Contexto de Entity Framework
- `Services/` - Lógica de negocio y servicios de integración
- `Program.cs` - Configuración principal de la aplicación

## Problemas comunes

- Si aparece un error de conexión a MySQL, revisa `DefaultConnection`.
- Si Swagger no aparece, verifica que la aplicación se esté ejecutando en `Development`.
- Si la app no arranca por el framework, asegúrate de tener instalado el SDK correcto de .NET.
