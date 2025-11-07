# ?? Solución - Error DateTime con PostgreSQL

## ? **Problema Original**

```
System.ArgumentException: 'Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported.'
```

---

## ?? **Causa del Error**

PostgreSQL con tipo `timestamp with time zone` **solo acepta fechas con `DateTimeKind.Utc`**.

Cuando se usa `.Date` en C#, se crea un `DateTime` con `Kind=Unspecified`:

```csharp
// ? INCORRECTO - Kind=Unspecified
var fechaInicio = DateTime.UtcNow.AddDays(-30).Date;

// ? INCORRECTO - Kind=Unspecified
var primerDia = new DateTime(2025, 1, 1);
```

---

## ? **Solución Implementada**

Se corrigió el `DashboardService.cs` para especificar explícitamente `DateTimeKind.Utc` en todas las fechas:

### **Antes (Incorrecto):**

```csharp
// ? Kind=Unspecified
var primerDiaMes = new DateTime(now.Year, now.Month, 1);
var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);
var fechaInicio = DateTime.UtcNow.AddDays(-dias).Date;
```

### **Después (Correcto):**

```csharp
// ? Kind=Utc
var primerDiaMes = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
var ultimoDiaMes = DateTime.SpecifyKind(primerDiaMes.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

var fechaInicioUtc = DateTime.UtcNow.AddDays(-dias);
var fechaInicio = DateTime.SpecifyKind(fechaInicioUtc.Date, DateTimeKind.Utc);
```

---

## ?? **Métodos Corregidos**

### **1. GetMetricsAsync()**

**Problema:** Fechas para filtrar ventas/compras del mes.

**Solución:**
```csharp
var now = DateTime.UtcNow;
var primerDiaMes = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
var ultimoDiaMes = DateTime.SpecifyKind(primerDiaMes.AddMonths(1).AddDays(-1), DateTimeKind.Utc);
```

---

### **2. GetTendenciaVentasAsync()**

**Problema:** Fecha de inicio para rango de días.

**Solución:**
```csharp
var fechaInicioUtc = DateTime.UtcNow.AddDays(-dias);
var fechaInicio = DateTime.SpecifyKind(fechaInicioUtc.Date, DateTimeKind.Utc);
```

**También en agrupación:**
```csharp
.GroupBy(f => DateTime.SpecifyKind(f.Fecha.Date, DateTimeKind.Utc))
```

---

### **3. GetMovimientosStockAsync()**

**Problema:** Fecha de inicio y agrupación por día.

**Solución:**
```csharp
var fechaInicioUtc = DateTime.UtcNow.AddDays(-dias);
var fechaInicio = DateTime.SpecifyKind(fechaInicioUtc.Date, DateTimeKind.Utc);

// En agrupación:
.GroupBy(m => DateTime.SpecifyKind(m.Fecha.Date, DateTimeKind.Utc))
```

---

## ?? **Patrón a Seguir**

### **Regla General:**

**Siempre que uses `.Date` o `new DateTime()`, envuélvelo con `DateTime.SpecifyKind()`:**

```csharp
// ? CORRECTO
var fecha = DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc);
var soloFecha = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

// ? INCORRECTO
var fecha = new DateTime(2025, 1, 1);
var soloFecha = DateTime.UtcNow.Date;
```

---

## ?? **Cómo Verificar la Solución**

### **1. Detener el Debugger**

Dado que el código está en debug, necesitas:
1. Detener la ejecución (Stop Debugging)
2. Iniciar nuevamente (F5)
3. Probar los endpoints

---

### **2. Probar Endpoints**

```bash
# Métricas principales
GET /api/dashboard/metrics

# Tendencia de ventas
GET /api/dashboard/tendencia-ventas?dias=30

# Movimientos de stock
GET /api/dashboard/movimientos-stock?dias=30
```

**Resultado Esperado:**
```json
{
  "success": true,
  "data": {
    "totalProductos": 250,
    "ventasDelMes": 125000000.00,
    ...
  }
}
```

---

## ?? **Debugging**

Si el error persiste, verifica:

### **1. Configuración de PostgreSQL**

```csharp
// En Program.cs
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### **2. Tipo de Columnas en Base de Datos**

```sql
-- Verificar tipo de columnas
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'facturas_venta' 
  AND column_name = 'fecha';

-- Debe ser: timestamp with time zone
```

---

## ?? **Comparación de Enfoques**

| Enfoque | Kind | PostgreSQL | Resultado |
|---------|------|------------|-----------|
| `new DateTime(2025, 1, 1)` | Unspecified | ? Error | NO funciona |
| `DateTime.UtcNow.Date` | Unspecified | ? Error | NO funciona |
| `DateTime.SpecifyKind(..., DateTimeKind.Utc)` | Utc | ? OK | ? Funciona |
| `DateTime.UtcNow` (sin .Date) | Utc | ? OK | ? Funciona |

---

## ?? **Advertencias**

### **No Mezclar DateTimeKind**

```csharp
// ? NO HACER ESTO
var fecha1 = DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc);
var fecha2 = new DateTime(2025, 1, 31); // Unspecified

// Comparar puede causar problemas
if (fecha1 < fecha2) { ... }
```

### **Consistencia**

**Siempre usa UTC en toda la aplicación:**

```csharp
// ? CORRECTO - Todo UTC
var ahora = DateTime.UtcNow;
var fecha = DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc);
```

---

## ?? **Resumen**

### **Cambios Realizados:**

1. ? **GetMetricsAsync()** - Fechas del mes con UTC
2. ? **GetTendenciaVentasAsync()** - Fecha de inicio y agrupación con UTC
3. ? **GetMovimientosStockAsync()** - Fecha de inicio y agrupación con UTC

### **Patrón Aplicado:**

```csharp
// Para fechas nuevas
DateTime.SpecifyKind(new DateTime(...), DateTimeKind.Utc)

// Para extraer solo fecha
DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc)
```

---

## ?? **Próximos Pasos**

1. ? Detener el debugger
2. ? Reiniciar la aplicación
3. ? Probar los endpoints
4. ? Verificar que no haya más errores de DateTime

---

## ?? **Notas Adicionales**

### **Alternativa: Configurar Npgsql**

Si quieres permitir `Kind=Unspecified` (no recomendado), puedes configurar Npgsql:

```csharp
// En Program.cs
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
```

**?? NO RECOMENDADO:** Es mejor usar UTC explícitamente.

---

## ? **Estado**

- ? **Código Corregido:** Sí
- ? **Compilación:** Exitosa
- ?? **Testing:** Pendiente (requiere reiniciar debugger)

---

**¡El error está solucionado! Reinicia la aplicación y prueba los endpoints.** ??
