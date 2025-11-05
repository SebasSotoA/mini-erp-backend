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
| `bodegaId` | Guid | Filtrar por bodega específica | `?bodegaId=a1b2c3d4...` |
| `categoriaId` | Guid | Filtrar por categoría | `?categoriaId=e5f6g7h8...` |
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

# Solo productos de Bodega Principal
GET /api/inventario/resumen?bodegaId=a1b2c3d4-...

# Productos de categoría Electrónica
GET /api/inventario/resumen?categoriaId=e5f6g7h8-...

# Búsqueda por nombre o SKU
GET /api/inventario/resumen?q=laptop

# Combinación de filtros
GET /api/inventario/resumen?bodegaId=...&estado=activo&q=mouse
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

1. **Por Bodega (`bodegaId`):**
   - Filtra `ProductoBodegas` por `BodegaId`

2. **Por Categoría (`categoriaId`):**
   - Filtra `Productos` por `CategoriaId`

3. **Por Estado (`estado`):**
   - `"activo"` ? Solo productos con `Activo = true`
   - `"inactivo"` ? Solo productos con `Activo = false`
   - `"todos"` ? Incluye todos los productos

4. **Por Búsqueda (`q`):**
   - Búsqueda parcial (case-insensitive) en:
     - `Producto.Nombre`
     - `Producto.CodigoSku`

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

### **Test 2: Filtro por Bodega**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen?bodegaId={guid}"
```

**Resultado Esperado:**
- Solo productos de esa bodega
- Totales recalculados

### **Test 3: Búsqueda por SKU**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen?q=LAP"
```

**Resultado Esperado:**
- Solo productos con SKU o nombre que contenga "LAP"

### **Test 4: Exportar PDF**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen/pdf" --output reporte.pdf
```

**Resultado Esperado:**
- Archivo PDF descargado
- Contiene tabla de productos y totales

### **Test 5: Filtros Combinados**

```bash
curl -X GET "https://localhost:7262/api/inventario/resumen?bodegaId={guid}&estado=activo&q=laptop"
```

**Resultado Esperado:**
- Todos los filtros aplicados correctamente
- `filtrosAplicados` en respuesta muestra los 3 filtros

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

## ?? Soporte

Si encuentras algún error:
- Revisa que todos los servicios estén registrados en `Program.cs`
- Verifica que QuestPDF esté instalado correctamente
- Asegúrate de que el connection string de PostgreSQL sea válido

---

**¡Módulo de Inventario implementado exitosamente!** ??
