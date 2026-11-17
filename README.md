# SpendAudit 

SaaS multi-empresa que audita comprobantes de gastos contra la política de cada empresa (semáforo Verde / Amarillo / Rojo).
.NET 10 · Clean Architecture · EF Core (Code-First) · PostgreSQL · JWT · FluentValidation.

## Estructura

```
SpendAudit/
├── infra/                       # PostgreSQL local en Docker (+ .env)
└── backend/
    ├── SpendAudit.Domain/       # Entidades, enums. Sin referencias a EF Core ni a paquetes
    ├── SpendAudit.Application/  # Features/<Feature>/{DTOs,Validators,Services}, interfaces de repositorios
    ├── SpendAudit.Infrastructure/  # DbContext, repositorios, JWT, BCrypt, PdfPig, almacenamiento
    ├── SpendAudit.API/          # Controllers, middleware global, Program.cs
    ├── SpendAudit.Tests/        # xUnit: motor de auditoría y parser
    └── SpendAudit.http          # Pruebas manuales (2 empresas, aislamiento, errores)
```

Regla de dependencia: `API → Application → Domain` e `Infrastructure → Application → Domain`.

## Puesta en marcha

```bash
# 1. Base de datos
cd infra
cp .env.example .env            # cambia POSTGRES_PASSWORD
docker compose up -d

# 2. Configuración de la API
cd ../backend/SpendAudit.API
cp .env.example .env            # misma clave de BD; genera Jwt__Key con: openssl rand -base64 48

# 3. Migración inicial (Code-First)
cd ..
dotnet tool install --global dotnet-ef        # una sola vez
dotnet ef migrations add Inicial --project SpendAudit.Infrastructure --startup-project SpendAudit.API --output-dir Persistence/Migrations
dotnet ef database update      --project SpendAudit.Infrastructure --startup-project SpendAudit.API

# 4. Pruebas y ejecución
dotnet test
dotnet run --project SpendAudit.API       # http://localhost:5080  ·  documentación: /scalar/v1
```

Luego abre `backend/SpendAudit.http` y ejecútalo de arriba hacia abajo.

## Cómo se garantiza el aislamiento entre empresas

1. El **login** emite un JWT firmado con los Claims `sub`, `empresaId` y `role`.
2. `HttpCurrentTenantService` lee `empresaId` del token validado. Ningún endpoint lo recibe del body, la ruta o el query.
3. `SpendAuditDbContext` aplica `HasQueryFilter(e => e.EmpresaId == EmpresaIdActual)` a **todas** las entidades con `EmpresaId` (vía `IEmpresaScoped`). Sin sesión no se ve ninguna fila.
4. Al guardar, el DbContext asigna `EmpresaId` a lo nuevo y lanza excepción si alguien intenta escribir en otra empresa.
5. Un recurso de otra empresa responde **404** (no 403): para el usuario no existe.
6. Solo hay `IgnoreQueryFilters()` en tres consultas marcadas como "global": login, email único y RTN único.

## Auditoría (HU-09)

| Resultado | Regla |
|---|---|
| Rojo | Factura duplicada (mismo RTN de proveedor + número) · contiene un término restringido |
| Amarillo | Antigüedad mayor que la vigencia máxima · total del día del usuario supera el tope de su puesto |
| Verde | Sin hallazgos |

El motor (`MotorAuditoria`) es una clase pura, sin base de datos; el servicio le entrega el contexto. Amarillo y Rojo son **resultados de negocio**, no excepciones: FluentValidation valida la forma de las peticiones y el middleware global convierte sus errores en `400`.

## Decisiones que conviene conocer

- **Unidad de trabajo:** los repositorios `Add/Remove` solo marcan el cambio y el servicio llama una vez a `SaveChangesAsync()`. Así registrar una empresa (empresa + admin + política) o liquidar un lote es atómico. Es la única diferencia con los repositorios de la clase, que guardan en cada `AddAsync`.
- **Servicios de aplicación:** los controladores son delgados; la lógica vive en `Features/<X>/Services`.
- **Cambios respecto al contrato de la Fase 1:** (a) los endpoints personales de gastos admiten `Empleado` y `Admin`; (b) se agrega `GET /gastos/{id}/archivo` (descarga autenticada, no hay archivos estáticos); (c) `ComprobanteResponseDto` incluye `Items`; (d) el multipart se recibe en `CreateBorradorForm` (API) y se traduce al DTO de Application.
- **Formato del 400:** lista `[{ propertyName, errorMessage }]`, igual a `validationResult.Errors` de la Clase 8. Verifícalo contra el extractor de errores de la Clase 16 antes de la Fase 3.
- **Extracción de PDF:** son heurísticas con expresiones regulares para facturas hondureñas (CAI, RTN, número `000-000-00-00000000`, fechas `dd/MM/yyyy`). Ajústalas con facturas reales de tu temática (`ParserComprobante`). Imágenes y PDFs escaneados no tienen OCR: el empleado completa los datos (HU-06).
- **Archivos:** se guardan en disco (`Storage__RootPath`). En Render (plan gratuito) el disco es efímero; para la Fase 5 hay que guardarlos en un servicio de objetos o en la base de datos.
- **Zona horaria:** "hoy" y los días de vigencia se calculan en UTC−6 (Honduras no tiene horario de verano).
- **JWT:** va firmado (HS256), no cifrado; el contenido es legible pero no alterable.
- **Swagger/Scalar:** solo en Development (o `Docs__Enabled=true`). Para probar endpoints protegidos usa `SpendAudit.http`.

## Pendiente para las siguientes fases

Dockerfile multi-stage y compose completo (Fase 4); variables de entorno en Render/Neon y `Database__MigrateOnStartup=true` (Fase 5); frontend (Fase 3).
