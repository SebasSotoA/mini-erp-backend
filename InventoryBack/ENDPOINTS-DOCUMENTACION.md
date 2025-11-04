# ?? Documentación de Endpoints - API Inventario

**Base URL:** `https://localhost:7262/api`  
**Formato:** JSON  
**Framework:** .NET 9

---

## 1?? Productos `/api/productos`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/productos` | Lista paginada con filtros | Query params (ProductFilterDto) |
| `GET` | `/api/productos/{id}` | Obtener producto por ID | - |
| `POST` | `/api/productos` | Crear producto | CreateProductDto |
| `PUT` | `/api/productos/{id}` | Actualizar producto | UpdateProductDto |
| `PATCH` | `/api/productos/{id}/activate` | Activar producto | - |
| `PATCH` | `/api/productos/{id}/deactivate` | Desactivar producto | - |
| `DELETE` | `/api/productos/{id}/permanent` | Eliminar permanentemente | - |

### Gestión de Bodegas del Producto

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/productos/{productId}/bodegas` | Listar bodegas del producto | - |
| `POST` | `/api/productos/{productId}/bodegas` | Agregar producto a bodega | AddProductoBodegaDto |
| `PUT` | `/api/productos/{productId}/bodegas/{bodegaId}` | Actualizar cantidades | UpdateProductoBodegaDto |
| `DELETE` | `/api/productos/{productId}/bodegas/{bodegaId}` | Remover de bodega | - |

### Gestión de Campos Extra del Producto

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/productos/{productId}/campos-extra` | Listar campos extra | - |
| `PUT` | `/api/productos/{productId}/campos-extra/{campoExtraId}` | Asignar/actualizar campo | SetProductoCampoExtraDto |
| `DELETE` | `/api/productos/{productId}/campos-extra/{campoExtraId}` | Remover campo extra | - |

---

## 2?? Bodegas `/api/bodegas`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/bodegas` | Lista paginada con filtros | Query params (BodegaFilterDto) |
| `GET` | `/api/bodegas/{id}` | Obtener bodega por ID | - |
| `POST` | `/api/bodegas` | Crear bodega | CreateBodegaDto |
| `PUT` | `/api/bodegas/{id}` | Actualizar bodega | UpdateBodegaDto |
| `PATCH` | `/api/bodegas/{id}/activate` | Activar bodega | - |
| `PATCH` | `/api/bodegas/{id}/deactivate` | Desactivar bodega | - |
| `DELETE` | `/api/bodegas/{id}` | Eliminar permanentemente | - |

**?? Restricciones:**
- Desactivar: Solo si no tiene productos
- Eliminar: Solo si no tiene productos, facturas ni movimientos

---

## 3?? Categorías `/api/categorias`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/categorias` | Lista paginada con filtros | Query params (CategoriaFilterDto) |
| `GET` | `/api/categorias/{id}` | Obtener categoría por ID | - |
| `POST` | `/api/categorias` | Crear categoría | CreateCategoriaDto |
| `PUT` | `/api/categorias/{id}` | Actualizar categoría | UpdateCategoriaDto |
| `PATCH` | `/api/categorias/{id}/activate` | Activar categoría | - |
| `PATCH` | `/api/categorias/{id}/deactivate` | Desactivar categoría | - |
| `DELETE` | `/api/categorias/{id}` | Eliminar permanentemente | - |

**?? Restricciones:**
- Desactivar/Eliminar: Solo si no tiene productos asignados

---

## 4?? Campos Extra `/api/campos-extra`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/campos-extra` | Lista paginada con filtros | Query params (CampoExtraFilterDto) |
| `GET` | `/api/campos-extra/{id}` | Obtener campo por ID | - |
| `POST` | `/api/campos-extra` | Crear campo extra | CreateCampoExtraDto |
| `PUT` | `/api/campos-extra/{id}` | Actualizar campo extra | UpdateCampoExtraDto |
| `PATCH` | `/api/campos-extra/{id}/activate` | Activar campo | - |
| `PATCH` | `/api/campos-extra/{id}/deactivate` | Desactivar campo | - |
| `DELETE` | `/api/campos-extra/{id}` | Eliminar permanentemente | - |

**?? Restricciones:**
- Eliminar: Solo si no está siendo usado en productos

---

## 5?? Vendedores `/api/vendedores`

| Método | Endpoint | Descripción | Body | Query Params |
|--------|----------|-------------|------|--------------|
| `GET` | `/api/vendedores` | Listar vendedores | - | `?soloActivos={bool?}` |
| `GET` | `/api/vendedores/{id}` | Obtener vendedor por ID | - | - |
| `POST` | `/api/vendedores` | Crear vendedor | CreateVendedorDto | - |
| `PUT` | `/api/vendedores/{id}` | Actualizar vendedor | UpdateVendedorDto | - |
| `PATCH` | `/api/vendedores/{id}/activate` | Activar vendedor | - | - |
| `PATCH` | `/api/vendedores/{id}/deactivate` | Desactivar vendedor | - | - |

**Query Params:**
- `soloActivos=null` ? Todos
- `soloActivos=true` ? Solo activos
- `soloActivos=false` ? Solo inactivos

---

## 6?? Proveedores `/api/proveedores`

| Método | Endpoint | Descripción | Body | Query Params |
|--------|----------|-------------|------|--------------|
| `GET` | `/api/proveedores` | Listar proveedores | - | `?soloActivos={bool?}` |
| `GET` | `/api/proveedores/{id}` | Obtener proveedor por ID | - | - |
| `POST` | `/api/proveedores` | Crear proveedor | CreateProveedorDto | - |
| `PUT` | `/api/proveedores/{id}` | Actualizar proveedor | UpdateProveedorDto | - |
| `PATCH` | `/api/proveedores/{id}/activate` | Activar proveedor | - | - |
| `PATCH` | `/api/proveedores/{id}/deactivate` | Desactivar proveedor | - | - |

**Query Params:** Mismo comportamiento que vendedores

---

## 7?? Facturas de Venta `/api/facturas-venta`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/facturas-venta` | Listar todas las facturas | - |
| `GET` | `/api/facturas-venta/{id}` | Obtener factura por ID | - |
| `POST` | `/api/facturas-venta` | Crear factura de venta | CreateFacturaVentaDto |
| `DELETE` | `/api/facturas-venta/{id}` | Anular factura | - |

**?? Efectos al crear:**
- ? Reduce stock en bodega
- ? Crea movimientos de inventario (tipo: VENTA)
- ? Estado: "Completada"

**?? Efectos al anular (DELETE):**
- ? Restaura stock
- ? Cambia estado a "Anulada"
- ? Crea movimientos reversos (cantidad negativa)

**?? Restricciones:**
- Solo se pueden anular facturas con estado "Completada"

---

## 8?? Facturas de Compra `/api/facturas-compra`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/facturas-compra` | Listar todas las facturas | - |
| `GET` | `/api/facturas-compra/{id}` | Obtener factura por ID | - |
| `POST` | `/api/facturas-compra` | Crear factura de compra | CreateFacturaCompraDto |
| `DELETE` | `/api/facturas-compra/{id}` | Anular factura | - |

**?? Efectos al crear:**
- ? Aumenta stock en bodega
- ? Crea ProductoBodega si no existe
- ? Crea movimientos de inventario (tipo: COMPRA)
- ? Estado: "Completada"

**?? Efectos al anular (DELETE):**
- ? Reduce stock
- ? Cambia estado a "Anulada"
- ? Crea movimientos reversos (cantidad negativa)

**?? Restricciones:**
- Solo se pueden anular facturas con estado "Completada"

---

## 9?? Movimientos de Inventario `/api/movimientos-inventario`

> ?? **READ-ONLY** - Los movimientos se crean automáticamente al crear/anular facturas

| Método | Endpoint | Descripción | Query Params |
|--------|----------|-------------|--------------|
| `GET` | `/api/movimientos-inventario` | Lista paginada con filtros | MovimientoInventarioFilterDto |
| `GET` | `/api/movimientos-inventario/{id}` | Obtener movimiento por ID | - |
| `GET` | `/api/movimientos-inventario/producto/{productoId}` | Kardex del producto | - |
| `GET` | `/api/movimientos-inventario/bodega/{bodegaId}` | Movimientos por bodega | - |
| `GET` | `/api/movimientos-inventario/factura-venta/{facturaVentaId}` | Movimientos de factura venta | - |
| `GET` | `/api/movimientos-inventario/factura-compra/{facturaCompraId}` | Movimientos de factura compra | - |

**?? Tipos de Movimiento:**
- `VENTA` - Movimiento de venta (cantidad positiva = salida, negativa = anulación)
- `COMPRA` - Movimiento de compra (cantidad positiva = entrada, negativa = anulación)

**?? Casos de Uso:**
- **Kardex**: `GET /movimientos-inventario/producto/{id}`
- **Auditoría**: `GET /movimientos-inventario?fechaDesde=...&fechaHasta=...`
- **Trazabilidad**: `GET /movimientos-inventario/factura-venta/{id}`

---

## ?? Filtros Disponibles

### ProductFilterDto
```
?page=1
&pageSize=20
&q=laptop
&nombre=dell
&codigoSku=XPS
&descripcion=gaming
&precio=1500
&cantidadExacta=10
&cantidadMin=5
&cantidadMax=100
&cantidadOperador=>=
&includeInactive=false
&onlyInactive=false
&orderBy=nombre
&orderDesc=false
```

### BodegaFilterDto
```
?page=1
&pageSize=20
&nombre=central
&direccion=calle
&activo=true
&orderBy=nombre
&orderDesc=false
```

### CategoriaFilterDto
```
?page=1
&pageSize=20
&nombre=electro
&descripcion=productos
&activo=true
&orderBy=nombre
&orderDesc=false
```

### CampoExtraFilterDto
```
?page=1
&pageSize=20
&nombre=color
&tipoDato=Texto
&esRequerido=true
&activo=true
&orderBy=nombre
&orderDesc=false
```

### MovimientoInventarioFilterDto
```
?page=1
&pageSize=20
&productoNombre=laptop
&productoSku=DELL
&bodegaNombre=central
&tipoMovimiento=VENTA
&fechaDesde=2024-11-01
&fechaHasta=2024-11-30
&cantidadMinima=10
&cantidadMaxima=100
&orderBy=fecha
&orderDesc=true
```

---

## ? Códigos de Estado HTTP

| Código | Descripción |
|--------|-------------|
| `200` | OK - Operación exitosa |
| `201` | Created - Recurso creado |
| `400` | Bad Request - Validación fallida |
| `404` | Not Found - Recurso no encontrado |
| `500` | Internal Server Error - Error del servidor |

---

## ?? Autenticación

?? **Actualmente sin autenticación** - Endpoints abiertos para desarrollo

---

## ?? Notas Importantes

1. **Soft Delete**: La mayoría de recursos usan desactivación en lugar de eliminación física
2. **Paginación**: Default `page=1`, `pageSize=20`, máximo `pageSize=100`
3. **GUIDs**: Todos los IDs son UUID/GUID
4. **Timestamps**: Fechas en formato ISO 8601 UTC
5. **Validación**: FluentValidation automática en todos los DTOs
6. **Stock**: El campo `stockActual` en ProductDto es calculado (suma de todas las bodegas)
7. **Movimientos**: READ-ONLY, se crean/modifican solo vía facturas
