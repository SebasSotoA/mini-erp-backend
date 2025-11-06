# ?? Módulo de Inventario - Documentación

## ? Implementación Completa

Se ha creado exitosamente el módulo de **Inventario** con endpoints REST para obtener resumen de inventario y exportar reportes en PDF.

---

## ?? Archivos Creados

### **Application Layer**

1. **`Application/DTOs/InventarioFilterDto.cs`**
   - Filtros opcionales para consultas de inventario

2. **`Application/DTOs/InventarioProductoDto.cs`**
   - DTO para cada producto en el resumen

3. **`Application/DTOs/InventarioResumenDto.cs`**
   - DTO de respuesta con totales y lista de productos

4. **`Application/Interfaces/IInventarioService.cs`**
   - Interfaz del servicio de inventario

5. **`Application/Services/InventarioService.cs`**
   - Implementación del servicio con lógica de negocio

### **Infrastructure Layer**

6. **`Infrastructure/Interfaces/IPdfGenerator.cs`**
   - Interfaz para generación de PDFs

7. **`Infrastructure/Services/PdfGeneratorService.cs`**
   - Implementación del generador de PDFs usando QuestPDF

### **API Layer**

8. **`API/Controllers/InventarioController.cs`**
   - Controlador REST con 2 endpoints

9. **`API/Program.cs`** (Actualizado)
   - Registro de servicios en DI container
   - Configuración de licencia QuestPDF

---

## ?? Endpoints Implementados

### **1?? GET `/api/inventario/resumen`**

**Descripción:** Obtiene un resumen del inventario con totales y detalles por producto (paginado).

**Parámetros de Query (Todos Opcionales):**

| Parámetro | Tipo | Descripción | Ejemplo | Default |
|-----------|------|-------------|---------|---------|
| `bodegaIds` | Guid[] | Filtrar por una o más bodegas | `?bodegaIds=guid1&bodegaIds=guid2` | - |
| `categoriaIds` | Guid[] | Filtrar por una o más categorías | `?categoriaIds=guid1&categoriaIds=guid2` | - |
| `estado` | String | `"activo"`, `"inactivo"`, `"todos"` | `?estado=activo` | `"activo"` |
| `q` | String | Búsqueda por nombre o SKU | `?q=laptop` | - |
| `page` | Int | Número de página | `?page=2` | `1` |
| `pageSize` | Int | Tamaño de página (max: 1000) | `?pageSize=100` | `50` |

**Respuesta de Ejemplo:**

```json
{
  "success": true,
  "data": {
    "valorTotal": 244189.00,
    "stockTotal": 1610,
    "page": 1,
    "pageSize": 50,
    "totalCount": 150,
    "totalPages": 3,
    "productos": [
      {
        "nombre": "Laptop Asus Vivobook 15",
        "codigoSku": "LAP-ASU-016",
        "bodega": "Principal",
        "cantidad": 10,
        "costoUnitario": 24000.00,
        "valorTotal": 240000.00,
        "categoria": "Electrónica"
      },
      {
        "nombre": "Mouse Logitech",
        "codigoSku": "MOU-LOG-001",
        "bodega": "Principal",
        "cantidad": 50,
        "costoUnitario": 15.50,
        "valorTotal": 775.00,
        "categoria": "Accesorios"
      }
      // ...48 productos más en la página 1
    ],
    "filtrosAplicados": {
      "Bodegas": "Principal, Sucursal Norte",
      "Categorías": "Electrónica, Accesorios",
      "Estado": "activo"
    },
    "fechaGeneracion": "2024-11-05T20:30:00Z"
  },
  "message": "Resumen de inventario obtenido correctamente."
}
```

**Ejemplos de Uso:**

```bash
# Primera página (50 productos por defecto)
GET /api/inventario/resumen

# Segunda página
GET /api/inventario/resumen?page=2

# Personalizar tamaño de página (100 productos)
GET /api/inventario/resumen?pageSize=100

# ? NUEVO: Paginación + Múltiples bodegas
GET /api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2&page=2&pageSize=20

# ? NUEVO: Paginación + Múltiples categorías
GET /api/inventario/resumen?categoriaIds=guid1&categoriaIds=guid2&page=1&pageSize=100

# ? NUEVO: Paginación + Filtros complejos
GET /api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2&categoriaIds=guid3&estado=activo&page=3&pageSize=25

# Búsqueda paginada
GET /api/inventario/resumen?q=laptop&page=1&pageSize=20
```

**Información de Paginación en la Respuesta:**

```json
{
  "page": 2,           // Página actual
  "pageSize": 50,      // Productos por página
  "totalCount": 150,   // Total de productos encontrados
  "totalPages": 3      // Total de páginas disponibles
}
```

---

### **2?? GET `/api/inventario/resumen/pdf`**

**Descripción:** Exporta el resumen de inventario como archivo PDF.

**Parámetros:** Los mismos que `/resumen` (ver tabla arriba)

**Respuesta:**
- **Content-Type:** `application/pdf`
- **Descarga automática:** `inventario_resumen_yyyyMMdd_HHmms.pdf`

**Contenido del PDF:**
- ? Encabezado con título y fecha de generación
- ? Filtros aplicados (si hay)
- ? Resumen con Stock Total y Valor Total destacados
- ? Tabla con todos los productos
- ? Numeración de páginas en el footer

**Ejemplos de Uso:**

```bash
# Descargar PDF de todos los productos activos
GET /api/inventario/resumen/pdf

# PDF filtrado por bodega
GET /api/inventario/resumen/pdf?bodegaId=a1b2c3d4-...

# PDF de productos inactivos
GET /api/inventario/resumen/pdf?estado=inactivo
```

---

## ?? Configuración Técnica

### **1. Paquete NuGet Instalado**

```sh
dotnet add package QuestPDF --version 2024.10.3
```

### **2. Servicios Registrados en DI (`Program.cs`)**

```csharp
// Application Services
builder.Services.AddScoped<IInventarioService, InventarioService>();

// Infrastructure Services
builder.Services.AddScoped<IPdfGenerator, PdfGeneratorService>();
```

### **3. Licencia QuestPDF Configurada**

```csharp
// Configure QuestPDF license (Community license for non-commercial use)
QuestPDF.Settings.License = LicenseType.Community;
```

---

## ?? Lógica de Negocio

### **Cálculos Implementados**

```csharp
// Valor Total del Inventario (SIN paginar - total de todos los productos filtrados)
valorTotal = SUM(cantidadActual * costoInicial)

// Stock Total (SIN paginar - total de todos los productos filtrados)
stockTotal = SUM(cantidadActual)

// Valor por Producto
valorProducto = cantidadActual * costoInicial
```

**?? Importante sobre Paginación:**
- Los campos `valorTotal` y `stockTotal` representan el **total de TODOS** los productos filtrados, **NO solo de la página actual**.
- Los `productos[]` son **solo los de la página solicitada** (según `page` y `pageSize`).
- `totalCount` indica cuántos productos hay en total (antes de paginar).
- `totalPages` = `Math.Ceiling(totalCount / pageSize)`

**Ejemplo:**
```json
{
  "valorTotal": 1500000.00,    // Suma de TODOS los 150 productos
  "stockTotal": 5000,           // Suma de TODOS los 150 productos
  "page": 2,                    // Página actual
  "pageSize": 50,               // Productos por página
  "totalCount": 150,            // Total de productos filtrados
  "totalPages": 3,              // 150 / 50 = 3 páginas
  "productos": [ /* Solo 50 productos de la página 2 */ ]
}
```

### **Filtros Aplicados**

1. **Por Múltiples Bodegas (`bodegaIds`):**
   - Filtra `ProductoBodegas` donde `BodegaId` está en la lista de IDs
   - Ejemplo: `?bodegaIds=guid1&bodegaIds=guid2` ? Productos en Bodega 1 **O** Bodega 2

2. **Por Múltiples Categorías (`categoriaIds`):**
   - Filtra `Productos` donde `CategoriaId` está en la lista de IDs
   - Ejemplo: `?categoriaIds=guid1&categoriaIds=guid2` ? Productos de Categoría 1 **O** Categoría 2

3. **Por Estado (`estado`):**
   - `"activo"` ? Solo productos con `Activo = true`
   - `"inactivo"` ? Solo productos con `Activo = false`
   - `"todos"` ? Incluye todos los productos

4. **Por Búsqueda (`q`):**
   - Búsqueda parcial (case-insensitive) en:
     - `Producto.Nombre`
     - `Producto.CodigoSku`

**Nota sobre la Lógica de Filtrado:**
- **Bodegas**: Usa lógica **OR** (productos en bodega 1 O bodega 2)
- **Categorías**: Usa lógica **OR** (productos de categoría A O categoría B)
- **Entre filtros diferentes**: Usa lógica **AND** (bodega 1 O 2) **Y** (categoría A O B) **Y** (estado activo)

---

## ? Checklist de Implementación

- [x] DTOs creados (`InventarioFilterDto`, `InventarioProductoDto`, `InventarioResumenDto`)
- [x] Interfaz `IInventarioService` definida
- [x] Servicio `InventarioService` implementado
- [x] Interfaz `IPdfGenerator` definida
- [x] Servicio `PdfGeneratorService` implementado con QuestPDF
- [x] Controlador `InventarioController` con 2 endpoints
- [x] Paquete QuestPDF instalado
- [x] Servicios registrados en DI
- [x] Licencia QuestPDF configurada
- [x] **Filtros múltiples** (bodegaIds[], categoriaIds[]) implementados
- [x] **Paginación** (page, pageSize, totalCount, totalPages) implementada
- [x] Compilación exitosa

---

## ?? Estructura del PDF

```
???????????????????????????????????????????????
?  Reporte de Inventario                      ?
?  Fecha: 05/11/2024 20:30                    ?
?                                             ?
?  Filtros aplicados:                         ?
?  • Bodega: Principal                        ?
?  • Estado: activo                           ?
???????????????????????????????????????????????
?                                             ?
?  Stock Total:           Valor Total:        ?
?  1,610 unidades        $244,189.00          ?
?                                             ?
???????????????????????????????????????????????
? Producto   ? SKU  ? Bodega  ? Cant.  ? Val. ?
???????????????????????????????????????????????
? Laptop...  ? LAP..? Princip.?   10   ?$240k ?
? Mouse Log..? MOU..? Princip.?   50   ?$775  ?
? ...        ? ...  ? ...     ?  ...   ? ...  ?
???????????????????????????????????????????????
                 Página 1 de 2
```

---

## ?? Pruebas Recomendadas

### **Test 1: Resumen Sin Filtros**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen"
```

**Resultado Esperado:**
- Status: `200 OK`
- Todos los productos activos listados
- `valorTotal` y `stockTotal` calculados correctamente

### **Test 2: Filtro por UNA Bodega**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen?bodegaIds={guid}"
```

**Resultado Esperado:**
- Solo productos de esa bodega
- Totales recalculados

### **Test 3: ? NUEVO - Filtro por MÚLTIPLES Bodegas**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2&bodegaIds=guid3"
```

**Resultado Esperado:**
- Productos de bodega 1 **O** bodega 2 **O** bodega 3
- `filtrosAplicados.Bodegas` = "Principal, Sucursal Norte, Sucursal Sur"
- Totales de todas las bodegas seleccionadas

### **Test 4: ? NUEVO - Filtro por MÚLTIPLES Categorías**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen?categoriaIds=guid1&categoriaIds=guid2"
```

**Resultado Esperado:**
- Productos de categoría 1 **O** categoría 2
- `filtrosAplicados.Categorías` = "Electrónica, Accesorios"

### **Test 5: Búsqueda por SKU**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen?q=LAP"
```

**Resultado Esperado:**
- Solo productos con SKU o nombre que contenga "LAP"

### **Test 6: Exportar PDF**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen/pdf" --output reporte.pdf
```

**Resultado Esperado:**
- Archivo PDF descargado
- Contiene tabla de productos y totales

### **Test 7: ? NUEVO - Filtros Combinados Avanzados**

```bash
# Productos de Bodega 1 O 2, Categoría Electrónica O Accesorios, solo activos
curl -X GET "https://localhost:7262/api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2&categoriaIds=guid3&categoriaIds=guid4&estado=activo"
```

**Resultado Esperado:**
- Productos que cumplan TODAS las condiciones:
  - Están en bodega 1 **O** bodega 2 **Y**
  - Son de categoría 3 **O** categoría 4 **Y**
  - Están activos
- `filtrosAplicados` muestra todos los filtros aplicados

### **Test 8: ? NUEVO - PDF con Múltiples Filtros**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen/pdf?bodegaIds=guid1&bodegaIds=guid2&categoriaIds=guid3&estado=activo" --output reporte_filtrado.pdf
```

**Resultado Esperado:**
- PDF con encabezado mostrando:
  - "Bodegas: Principal, Sucursal Norte"
  - "Categorías: Electrónica"
  - "Estado: activo"
- Tabla con productos filtrados

### **Test 9: ? NUEVO - Paginación Básica**

```bash
# Primera página (50 productos por defecto)
curl -X GET "https://localhost:7262/api/inventario/resumen"

# Segunda página
curl -X GET "https://localhost:7262/api/inventario/resumen?page=2"

# Página 3 con 20 productos por página
curl -X GET "https://localhost:7262/api/inventario/resumen?page=3&pageSize=20"
```

**Resultado Esperado:**
```json
{
  "page": 2,
  "pageSize": 50,
  "totalCount": 150,
  "totalPages": 3,
  "productos": [ /* 50 productos de la página 2 */ ]
}
```

### **Test 10: ? NUEVO - Paginación con Filtros**

```bash
# Productos de Electrónica, página 2, 25 por página
curl -X GET "https://localhost:7262/api/inventario/resumen?categoriaIds=guid-electronica&page=2&pageSize=25"
```

**Resultado Esperado:**
- Solo productos de categoría Electrónica
- Página 2 de resultados
- Máximo 25 productos en la respuesta
- `totalCount` muestra el total de productos de Electrónica (sin paginar)

---

## ?? Notas Importantes

### **Performance**

- ?? El servicio actual carga todos los `ProductoBodegas` y luego filtra en memoria
- ?? **Para Producción:** Considera optimizar con queries LINQ más eficientes usando `IQueryable`
- ? **Paginación Implementada:** El endpoint ahora devuelve solo `pageSize` productos por request (default: 50, max: 1000)
- ?? **Totales:** `valorTotal` y `stockTotal` siempre muestran el total de TODOS los productos filtrados, no solo de la página actual

### **Comportamiento de Paginación**

1. **Sin parámetros:** Devuelve página 1 con 50 productos
2. **Page fuera de rango:** Si `page > totalPages`, devuelve array vacío `productos: []`
3. **PageSize máximo:** Limitado a 1000 productos por página
4. **Totales:** Siempre se calculan sobre TODOS los productos filtrados

### **Mejoras Futuras (Opcional)**

1. ? **Paginación** - **IMPLEMENTADO**

2. **Ordenamiento:**
   ```csharp
   public string? OrderBy { get; set; } = "nombre"; // "nombre", "cantidad", "valor"
   public bool OrderDesc { get; set; } = false;
   ```

3. **Exportar a Excel (CSV):**
   ```csharp
   [HttpGet("resumen/csv")]
   public async Task<IActionResult> GetResumenCsv(...)
   ```

4. **Agrupación por Categoría en PDF:**
   - Mostrar subtotales por categoría

5. **Gráficos en PDF:**
   - Usar QuestPDF para agregar charts de distribución

6. **Optimización de Queries:**
   - Usar `IQueryable` para que el filtrado se haga en la base de datos
   - Evitar cargar todos los productos en memoria
