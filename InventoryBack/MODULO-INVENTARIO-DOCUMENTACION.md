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

**Descripción:** Obtiene un resumen del inventario con totales y detalles por producto.

**Parámetros de Query (Todos Opcionales):**

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `bodegaIds` | Guid[] | Filtrar por una o más bodegas | `?bodegaIds=guid1&bodegaIds=guid2` |
| `categoriaIds` | Guid[] | Filtrar por una o más categorías | `?categoriaIds=guid1&categoriaIds=guid2` |
| `estado` | String | `"activo"`, `"inactivo"`, `"todos"` | `?estado=activo` |
| `q` | String | Búsqueda por nombre o SKU | `?q=laptop` |

**Respuesta de Ejemplo:**

```json
{
  "success": true,
  "data": {
    "valorTotal": 244189.00,
    "stockTotal": 1610,
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
# Todos los productos activos
GET /api/inventario/resumen

# ? NUEVO: Múltiples bodegas (Bodega 1 Y Bodega 2)
GET /api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2

# ? NUEVO: Múltiples categorías (Electrónica Y Accesorios)
GET /api/inventario/resumen?categoriaIds=guid1&categoriaIds=guid2

# ? NUEVO: Combinación de múltiples bodegas y categorías
GET /api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2&categoriaIds=guid3&estado=activo

# Búsqueda por nombre o SKU
GET /api/inventario/resumen?q=laptop

# ? NUEVO: Filtros complejos - 2 bodegas, 1 categoría, solo activos
GET /api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2&categoriaIds=guid3&estado=activo&q=laptop
```

---

### **2?? GET `/api/inventario/resumen/pdf`**

**Descripción:** Exporta el resumen de inventario como archivo PDF.

**Parámetros:** Los mismos que `/resumen` (ver tabla arriba)

**Respuesta:**
- **Content-Type:** `application/pdf`
- **Descarga automática:** `inventario_resumen_yyyyMMdd_HHmmss.pdf`

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
// Valor Total del Inventario
valorTotal = SUM(cantidadActual * costoInicial)

// Stock Total
stockTotal = SUM(cantidadActual)

// Valor por Producto
valorProducto = cantidadActual * costoInicial
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

---

## ?? Notas Importantes

### **Performance**

- ?? El servicio actual carga todos los `ProductoBodegas` y luego filtra en memoria
- ?? **Para Producción:** Considera optimizar con queries LINQ más eficientes usando `IQueryable`

### **Mejoras Futuras (Opcional)**

1. **Paginación en `/resumen`:**
   ```csharp
   public class InventarioFilterDto
   {
       public int Page { get; set; } = 1;
       public int PageSize { get; set; } = 20;
       // ...otros filtros
   }
   ```

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
- [x] Compilación exitosa

---

## ?? Próximos Pasos

1. **Probar los endpoints:**
   ```bash
   dotnet run
   # Abrir Scalar: https://localhost:7262/scalar/v1
   ```

2. **Verificar respuestas:**
   - GET `/api/inventario/resumen`
   - GET `/api/inventario/resumen/pdf`

3. **Ajustar filtros** según necesidades del frontend

4. **Optimizar queries** si el dataset es muy grande

5. **Agregar logs** para troubleshooting

---

## ?? Casos de Uso Prácticos con Múltiples Filtros

### **Caso 1: Reporte Consolidado de Dos Sucursales**

**Escenario:** Necesitas el inventario total de "Bodega Principal" y "Bodega Norte".

```bash
GET /api/inventario/resumen?bodegaIds=guid-principal&bodegaIds=guid-norte
```

**Response:**
```json
{
  "valorTotal": 450000.00,
  "stockTotal": 250,
  "filtrosAplicados": {
    "Bodegas": "Principal, Norte"
  },
  "productos": [
    { "nombre": "Laptop Dell", "bodega": "Principal", ... },
    { "nombre": "Mouse Logitech", "bodega": "Norte", ... }
  ]
}
```

---

### **Caso 2: Inventario de Electrónica y Computadoras en Todas las Bodegas**

**Escenario:** Ver todos los productos de "Electrónica" y "Computadoras" sin importar la bodega.

```bash
GET /api/inventario/resumen?categoriaIds=guid-electronica&categoriaIds=guid-computadoras
```

**Response:**
```json
{
  "filtrosAplicados": {
    "Categorías": "Electrónica, Computadoras"
  },
  "productos": [
    { "nombre": "Laptop Dell", "categoria": "Computadoras", ... },
    { "nombre": "TV Samsung", "categoria": "Electrónica", ... }
  ]
}
```

---

### **Caso 3: Productos Activos de Categorías Específicas en Bodegas Específicas**

**Escenario:** Auditoría de productos activos de "Electrónica" y "Accesorios" en "Principal" y "Norte".

```bash
GET /api/inventario/resumen?bodegaIds=guid1&bodegaIds=guid2&categoriaIds=guid3&categoriaIds=guid4&estado=activo
```

**Lógica Aplicada:**
```
(bodega = Principal OR bodega = Norte) 
AND 
(categoria = Electrónica OR categoria = Accesorios)
AND
(estado = activo)
```

---

### **Caso 4: Búsqueda de "Laptop" en Bodegas Específicas**

**Escenario:** Buscar todas las laptops solo en "Principal" y "Sur".

```bash
GET /api/inventario/resumen?bodegaIds=guid-principal&bodegaIds=guid-sur&q=laptop
```

---

### **Caso 5: Reporte PDF de Productos Inactivos**

**Escenario:** Generar PDF con productos marcados como inactivos en todas las bodegas.

```bash
GET /api/inventario/resumen/pdf?estado=inactivo
```

**PDF generado:**
- Filtros aplicados: "Estado: inactivo"
- Lista de todos los productos inactivos
- Útil para planificar limpieza de inventario

---

### **Caso 6: Inventario Completo (Sin Filtros)**

**Escenario:** Ver absolutamente todo el inventario (activos e inactivos).

```bash
GET /api/inventario/resumen?estado=todos
```

---

## ?? Tabla Comparativa: Antes vs Después

| Funcionalidad | Antes | ? Después |
|---------------|-------|----------|
| Filtrar por 1 bodega | ? | ? |
| Filtrar por **múltiples** bodegas | ? | ? |
| Filtrar por 1 categoría | ? | ? |
| Filtrar por **múltiples** categorías | ? | ? |
| Combinar bodegas + categorías | ? | ? |
| Búsqueda por texto | ? | ? |
| Filtro por estado | ? | ? |
| Exportar a PDF con filtros | ? | ? |

---

## ?? Formato de Query Strings

### **ASP.NET Core maneja automáticamente arrays en query strings:**

**Múltiples valores con el mismo nombre:**
```
?bodegaIds=guid1&bodegaIds=guid2&bodegaIds=guid3
```

**Se convierte en:**
```csharp
filters.BodegaIds = new List<Guid> { guid1, guid2, guid3 }
```

**Desde JavaScript/TypeScript:**
```typescript
// Opción 1: URLSearchParams
const params = new URLSearchParams();
params.append('bodegaIds', 'guid1');
params.append('bodegaIds', 'guid2');
params.append('estado', 'activo');

fetch(`/api/inventario/resumen?${params}`);

// Opción 2: Construcción manual
const bodegaIds = ['guid1', 'guid2'];
const queryString = bodegaIds.map(id => `bodegaIds=${id}`).join('&');
fetch(`/api/inventario/resumen?${queryString}&estado=activo`);
```

---

## ?? Soporte

Si encuentras algún error:
- Revisa que todos los servicios estén registrados en `Program.cs`
- Verifica que QuestPDF esté instalado correctamente
- Asegúrate de que el connection string de PostgreSQL sea válido
- Para filtros múltiples, verifica que los GUIDs sean válidos

---

**¡Módulo de Inventario con Filtros Múltiples implementado exitosamente!** ??
