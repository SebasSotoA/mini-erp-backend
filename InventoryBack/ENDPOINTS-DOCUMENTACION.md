# ?? Documentación de Endpoints - API Inventario

**Base URL:** `https://localhost:7262/api`  
**Formato:** JSON  
**Framework:** .NET 9

---

## ?? Índice

1. [Productos](#1??-productos-apiproductos)
2. [Bodegas](#2??-bodegas-apibodegas)
3. [Categorías](#3??-categorías-apicategorias)
4. [Campos Extra](#4??-campos-extra-apicampos-extra)
5. [Vendedores](#5??-vendedores-apivendedores)
6. [Proveedores](#6??-proveedores-apiproveedores)
7. [Movimientos de Inventario](#7??-movimientos-de-inventario-apimovimientos-inventario-read-only)
8. [Facturas de Venta](#8??-facturas-de-venta-apifacturas-venta)
9. [Facturas de Compra](#9??-facturas-de-compra-apifacturas-compra)

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

### ?? CreateProductDto (Request Body para POST)

```json
{
  "nombre": "string (requerido)",
  "unidadMedida": "string (requerido)",
  "bodegaPrincipalId": "guid (requerido)",
  "cantidadInicial": 100,
  "cantidadMinima": 10,
  "cantidadMaxima": 500,
  "precioBase": 0.00,
  "costoInicial": 0.00,
  "impuestoPorcentaje": 0.00,
  "categoriaId": "guid (opcional)",
  "codigoSku": "string (opcional, se genera automáticamente)",
  "descripcion": "string (opcional)",
  "imagenProductoUrl": "string (opcional)",
  "bodegasAdicionales": [
    {
      "bodegaId": "guid",
      "cantidadInicial": 50,
      "cantidadMinima": 5,
      "cantidadMaxima": 100
    }
  ],
  "camposExtra": [
    {
      "campoExtraId": "guid",
      "valor": "string"
    }
  ]
}
```

**Campos para Bodega Principal:**
- `bodegaPrincipalId` (**requerido**): ID de la bodega principal
- `cantidadInicial` (**requerido**): Cantidad inicial en bodega principal
- `cantidadMinima` (**opcional**): Cantidad mínima en bodega principal
- `cantidadMaxima` (**opcional**): Cantidad máxima en bodega principal

**Campos para Bodegas Adicionales:**
- `bodegasAdicionales[]` (**opcional**): Array de bodegas adicionales
  - `bodegaId` (**requerido**): ID de la bodega adicional
  - `cantidadInicial` (**requerido**): Cantidad inicial
  - `cantidadMinima` (**opcional**): Cantidad mínima
  - `cantidadMaxima` (**opcional**): Cantidad máxima

### ?? Estructura de ProductDto (Response)

```json
{
  "id": "guid",
  "nombre": "string",
  "unidadMedida": "string",
  "precioBase": 0.00,
  "impuestoPorcentaje": 0.00,
  "costoInicial": 0.00,
  "categoriaId": "guid (opcional)",
  "categoriaNombre": "string (calculado)",
  "codigoSku": "string",
  "descripcion": "string",
  "activo": true,
  "fechaCreacion": "2024-01-01T00:00:00Z",
  "imagenProductoUrl": "string",
  "stockActual": 100,
  "bodegaPrincipalId": "guid"
}
```

**Campos Calculados:**
- `stockActual`: Suma total del stock en todas las bodegas
- `categoriaNombre`: Nombre de la categoría (si tiene categoría asignada)

**Nuevo Campo:**
- ? `bodegaPrincipalId`: ID de la bodega principal del producto

### Gestión de Bodegas del Producto

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/productos/{productId}/bodegas` | Listar bodegas del producto | - |
| `POST` | `/api/productos/{productId}/bodegas` | Agregar producto a bodega | AddProductoBodegaDto |
| `PUT` | `/api/productos/{productId}/bodegas/{bodegaId}` | Actualizar cantidades | UpdateProductoBodegaDto |
| `DELETE` | `/api/productos/{productId}/bodegas/{bodegaId}` | Remover de bodega | - |

### ?? Estructura de ProductoBodegaDetailDto (Response)

```json
{
  "bodegaId": "guid",
  "bodegaNombre": "string",
  "bodegaDireccion": "string",
  "cantidadInicial": 100,
  "cantidadMinima": 10,
  "cantidadMaxima": 500,
  "esPrincipal": true
}
```

**Nuevo Campo:**
- ? `esPrincipal`: Indica si esta bodega es la principal del producto

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
| `GET` | `/api/bodegas/{id}/productos` | ? **NUEVO** - Productos de la bodega con filtros | Query params (ProductFilterDto) |

**?? Restricciones:**
- Desactivar: Solo si no tiene productos
- Eliminar: Solo si no tiene productos, facturas ni movimientos

### **? NUEVO: GET `/api/bodegas/{bodegaId}/productos`**

**Descripción:** Obtiene todos los productos de una bodega específica con filtrado avanzado y paginación.

**Usa los mismos filtros que** `GET /api/productos`

**Parámetros de Query (Todos Opcionales):**

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `q` | String | Búsqueda general (nombre, SKU, descripción) | `?q=laptop` |
| `nombre` | String | Filtrar por nombre del producto | `?nombre=dell` |
| `codigoSku` | String | Filtrar por código SKU | `?codigoSku=LAP` |
| `descripcion` | String | Filtrar por descripción | `?descripcion=gaming` |
| `precio` | String | Filtrar por precio (búsqueda parcial) | `?precio=1500` |
| **`cantidadExacta`** | Int | Filtrar por cantidad exacta en esta bodega | `?cantidadExacta=10` |
| **`cantidadMin`** | Int | Cantidad mínima (usado con operador) | `?cantidadMin=5` |
| **`cantidadMax`** | Int | Cantidad máxima (usado con operador) | `?cantidadMax=100` |
| **`cantidadOperador`** | String | Operador: `=`, `>`, `>=`, `<`, `<=`, `range` | `?cantidadOperador=range` |
| `includeInactive` | Boolean | Incluir productos inactivos | `?includeInactive=true` |
| `onlyInactive` | Boolean | Solo productos inactivos | `?onlyInactive=true` |
| `orderBy` | String | Ordenar por: `nombre`, `precio`, `sku`, `fecha` | `?orderBy=precio` |
| `orderDesc` | Boolean | Orden descendente | `?orderDesc=true` |
| `page` | Int | Número de página | `?page=2` |
| `pageSize` | Int | Tamaño de página (max: 100) | `?pageSize=50` |

**?? Nota sobre Filtros de Cantidad:**
- Los filtros de cantidad (`cantidadExacta`, `cantidadMin`, `cantidadMax`, `cantidadOperador`) se aplican **solo al stock de esta bodega específica**.
- Ejemplo: `cantidadMin=10` filtra productos que tienen **10 o más unidades en esta bodega** (no en total).

**Ejemplo de Requests con Filtros de Cantidad:**

```bash
# Productos con exactamente 50 unidades en esta bodega
GET /api/bodegas/{bodegaId}/productos?cantidadExacta=50

# Productos con más de 10 unidades en esta bodega
GET /api/bodegas/{bodegaId}/productos?cantidadMin=10&cantidadOperador=>

# Productos con 10 o más unidades en esta bodega
GET /api/bodegas/{bodegaId}/productos?cantidadMin=10&cantidadOperador=>=

# Productos con menos de 100 unidades en esta bodega
GET /api/bodegas/{bodegaId}/productos?cantidadMax=100&cantidadOperador=<

# Productos con stock entre 10 y 50 unidades en esta bodega
GET /api/bodegas/{bodegaId}/productos?cantidadMin=10&cantidadMax=50&cantidadOperador=range

# Combinado: Laptops con menos de 5 unidades (stock bajo)
GET /api/bodegas/{bodegaId}/productos?q=laptop&cantidadMax=5&cantidadOperador=<
```

**Casos de Uso:**

1. **Ver inventario de una bodega específica:**
   ```
   GET /api/bodegas/{id}/productos
   ```

2. **Buscar productos en una bodega:**
   ```
   GET /api/bodegas/{id}/productos?q=laptop
   ```

3. **Filtrar productos caros en una bodega:**
   ```
   GET /api/bodegas/{id}/productos?precio=5000000&orderBy=precio&orderDesc=true
   ```

4. **Ver productos inactivos de una bodega:**
   ```
   GET /api/bodegas/{id}/productos?onlyInactive=true
   ```

5. **? NUEVO - Productos con stock bajo en una bodega (alerta de reposición):**
   ```
   GET /api/bodegas/{id}/productos?cantidadMax=5&cantidadOperador=<&orderBy=nombre
   ```

6. **? NUEVO - Productos con stock entre rangos específicos:**
   ```
   GET /api/bodegas/{id}/productos?cantidadMin=10&cantidadMax=50&cantidadOperador=range
   ```

7. **? NUEVO - Productos con cantidad exacta (para auditoría):**
   ```
   GET /api/bodegas/{id}/productos?cantidadExacta=0
   ```

8. **? NUEVO - Productos con buen stock (más de 20 unidades):**
   ```
   GET /api/bodegas/{id}/productos?cantidadMin=20&cantidadOperador=>&orderBy=nombre
   ```

9. **? NUEVO - Combinación: Laptops con stock bajo, ordenadas por precio:**
   ```
   GET /api/bodegas/{id}/productos?q=laptop&cantidadMax=10&cantidadOperador=<&orderBy=precio
   ```

**Ejemplo de Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "nombre": "Laptop Dell XPS 15",
        "codigoSku": "LAP-DELL-001",
        "precioBase": 5500000.00,
        "costoInicial": 4000000.00,
        "stockActual": 100,
        "cantidadEnBodega": 25,
        "categoriaNombre": "Electrónica",
        "activo": true
      },
      {
        "id": "guid",
        "nombre": "Mouse Logitech MX Master",
        "codigoSku": "MOU-LOG-001",
        "precioBase": 350000.00,
        "costoInicial": 200000.00,
        "stockActual": 150,
        "cantidadEnBodega": 50,
        "categoriaNombre": "Accesorios",
        "activo": true
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 45
  },
  "message": "Productos de la bodega obtenidos correctamente."
}
```

**?? Importante - Campos de Cantidad:**

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| `stockActual` | **Stock TOTAL** del producto en **TODAS las bodegas** | Si producto está en 3 bodegas (50 + 30 + 20) = **100** |
| `cantidadEnBodega` | **Cantidad específica** en **ESTA bodega** solamente | Si estás en Bodega Principal = **50** |

**Ejemplo con datos reales:**

Producto: "Laptop Dell XPS"
- Bodega Principal: 50 unidades
- Bodega Norte: 30 unidades  
- Bodega Sur: 20 unidades

**Response de `/api/bodegas/{bodega-principal}/productos`:**
```json
{
  "nombre": "Laptop Dell XPS",
  "stockActual": 100,         // ? Total: 50 + 30 + 20
  "cantidadEnBodega": 50      // ? Solo Bodega Principal
}
```

**Response de `/api/bodegas/{bodega-norte}/productos`:**
```json
{
  "nombre": "Laptop Dell XPS",
  "stockActual": 100,         // ? Total: 50 + 30 + 20 (no cambia)
  "cantidadEnBodega": 30      // ? Solo Bodega Norte
}
```

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
| `GET` | `/api/categorias/{id}/productos` | ? **NUEVO** - Productos de la categoría con filtros | Query params (ProductFilterDto) |

**?? Restricciones:**
- Desactivar: Solo si no tiene productos
- Eliminar: Solo si no tiene productos

### **? NUEVO: GET `/api/categorias/{categoriaId}/productos`**

**Descripción:** Obtiene todos los productos de una categoría específica con filtrado avanzado y paginación.

**Usa los mismos filtros que** `GET /api/productos`

**Parámetros de Query (Todos Opcionales):**

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `q` | String | Búsqueda general (nombre, SKU, descripción) | `?q=laptop` |
| `nombre` | String | Filtrar por nombre del producto | `?nombre=dell` |
| `codigoSku` | String | Filtrar por código SKU | `?codigoSku=LAP` |
| `descripcion` | String | Filtrar por descripción | `?descripcion=gaming` |
| `precio` | String | Filtrar por precio (búsqueda parcial) | `?precio=1500` |
| **`cantidadExacta`** | Int | Filtrar por cantidad exacta (stock total) | `?cantidadExacta=100` |
| **`cantidadMin`** | Int | Cantidad mínima (usado con operador) | `?cantidadMin=50` |
| **`cantidadMax`** | Int | Cantidad máxima (usado con operador) | `?cantidadMax=500` |
| **`cantidadOperador`** | String | Operador: `=`, `>`, `>=`, `<`, `<=`, `range` | `?cantidadOperador=range` |
| `includeInactive` | Boolean | Incluir productos inactivos | `?includeInactive=true` |
| `onlyInactive` | Boolean | Solo productos inactivos | `?onlyInactive=true` |
| `orderBy` | String | Ordenar por: `nombre`, `precio`, `sku`, `fecha` | `?orderBy=precio` |
| `orderDesc` | Boolean | Orden descendente | `?orderDesc=true` |
| `page` | Int | Número de página | `?page=2` |
| `pageSize` | Int | Tamaño de página (max: 100) | `?pageSize=50` |

**?? Nota sobre Filtros de Cantidad:**
- Los filtros de cantidad (`cantidadExacta`, `cantidadMin`, `cantidadMax`, `cantidadOperador`) se aplican al **stock total del producto en todas las bodegas**.
- Ejemplo: `cantidadMin=50` filtra productos que tienen **50 o más unidades en total** (sumando todas las bodegas).

**Ejemplo de Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "nombre": "Laptop Dell XPS 15",
        "codigoSku": "LAP-DELL-001",
        "precioBase": 5500000.00,
        "costoInicial": 4000000.00,
        "stockActual": 100,
        "cantidadEnCategoria": 100,
        "categoriaNombre": "Electrónica",
        "activo": true
      },
      {
        "id": "guid",
        "nombre": "Laptop Asus Vivobook",
        "codigoSku": "LAP-ASU-016",
        "precioBase": 4200000.00,
        "costoInicial": 3000000.00,
        "stockActual": 75,
        "cantidadEnCategoria": 75,
        "categoriaNombre": "Electrónica",
        "activo": true
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 45
  },
  "message": "Productos de la categoría obtenidos correctamente."
}
```

**?? Importante - Campos de Cantidad:**

| Campo | Descripción | En Categorías |
|-------|-------------|---------------|
| `stockActual` | Stock TOTAL del producto en TODAS las bodegas | Igual para todos |
| `cantidadEnCategoria` | Cantidad total del producto (mismo valor que `stockActual`) | Siempre igual a `stockActual`* |

**\* Nota:** Dado que un producto pertenece a **UNA SOLA categoría**, `cantidadEnCategoria` siempre es igual a `stockActual`. Este campo se incluye por consistencia con el endpoint de bodegas (`cantidadEnBodega`).

**Diferencia con Bodegas:**
- En `/api/bodegas/{id}/productos`: Un producto puede estar en **MÚLTIPLES bodegas**, por lo que `cantidadEnBodega` ? `stockActual`.
- En `/api/categorias/{id}/productos`: Un producto pertenece a **UNA SOLA categoría**, por lo que `cantidadEnCategoria` = `stockActual`.

**Casos de Uso:**

1. **Ver todos los productos de una categoría:**
   ```
   GET /api/categorias/{id}/productos
   ```

2. **Buscar productos en una categoría:**
   ```
   GET /api/categorias/{id}/productos?q=laptop
   ```

3. **Filtrar productos caros en una categoría:**
   ```
   GET /api/categorias/{id}/productos?precio=5000000&orderBy=precio&orderDesc=true
   ```

4. **Ver productos inactivos de una categoría:**
   ```
   GET /api/categorias/{id}/productos?onlyInactive=true
   ```

5. **? NUEVO - Productos con stock alto en una categoría:**
   ```
   GET /api/categorias/{id}/productos?cantidadMin=100&cantidadOperador=>&orderBy=nombre
   ```

6. **? NUEVO - Productos con stock en rango específico:**
   ```
   GET /api/categorias/{id}/productos?cantidadMin=50&cantidadMax=200&cantidadOperador=range
   ```

7. **? NUEVO - Productos con stock bajo (menos de 20 unidades):**
   ```
   GET /api/categorias/{id}/productos?cantidadMax=20&cantidadOperador=<&orderBy=nombre
   ```

8. **? NUEVO - Productos agotados de una categoría:**
   ```
   GET /api/categorias/{id}/productos?cantidadExacta=0
   ```

---

## 4?? Campos Extra `/api/campos-extra`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/campos-extra` | Lista paginada con filtros | Query params (CampoExtraFilterDto) |
| `GET` | `/api/campos-extra/{id}` | Obtener campo extra por ID | - |
| `POST` | `/api/campos-extra` | Crear campo extra | CreateCampoExtraDto |
| `PUT` | `/api/campos-extra/{id}` | Actualizar campo extra | UpdateCampoExtraDto |
| `PATCH` | `/api/campos-extra/{id}/activate` | Activar campo extra | - |
| `PATCH` | `/api/campos-extra/{id}/deactivate` | Desactivar campo extra | - |
| `DELETE` | `/api/campos-extra/{id}` | Eliminar permanentemente | - |
| `GET` | `/api/campos-extra/{id}/productos` | ? **NUEVO** - Productos que tienen este campo con filtros | Query params (ProductFilterDto) |

**?? Restricciones:**
- Eliminar: Solo si no está siendo usado en productos

### **? NUEVO: GET `/api/campos-extra/{campoExtraId}/productos`**

**Descripción:** Obtiene todos los productos que tienen asignado este campo extra con filtrado avanzado y paginación.

**Usa los mismos filtros que** `GET /api/productos`

**Parámetros de Query (Todos Opcionales):**

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `q` | String | Búsqueda general (nombre, SKU, descripción) | `?q=laptop` |
| `nombre` | String | Filtrar por nombre del producto | `?nombre=dell` |
| `codigoSku` | String | Filtrar por código SKU | `?codigoSku=LAP` |
| `descripcion` | String | Filtrar por descripción | `?descripcion=gaming` |
| `precio` | String | Filtrar por precio (búsqueda parcial) | `?precio=1500` |
| **`cantidadExacta`** | Int | Filtrar por cantidad exacta (stock total) | `?cantidadExacta=100` |
| **`cantidadMin`** | Int | Cantidad mínima (usado con operador) | `?cantidadMin=50` |
| **`cantidadMax`** | Int | Cantidad máxima (usado con operador) | `?cantidadMax=500` |
| **`cantidadOperador`** | String | Operador: `=`, `>`, `>=`, `<`, `<=`, `range` | `?cantidadOperador=range` |
| `includeInactive` | Boolean | Incluir productos inactivos | `?includeInactive=true` |
| `onlyInactive` | Boolean | Solo productos inactivos | `?onlyInactive=true` |
| `orderBy` | String | Ordenar por: `nombre`, `precio`, `sku`, `fecha` | `?orderBy=precio` |
| `orderDesc` | Boolean | Orden descendente | `?orderDesc=true` |
| `page` | Int | Número de página | `?page=2` |
| `pageSize` | Int | Tamaño de página (max: 100) | `?pageSize=50` |

**?? Nota sobre Filtros de Cantidad:**
- Los filtros de cantidad se aplican al **stock total del producto en todas las bodegas**.

**Ejemplo de Response:**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "nombre": "Laptop Dell XPS 15",
        "codigoSku": "LAP-DELL-001",
        "precioBase": 5500000.00,
        "costoInicial": 4000000.00,
        "stockActual": 100,
        "cantidadEnCampo": 100,
        "valorCampoExtra": "Intel Core i7",
        "categoriaNombre": "Electrónica",
        "activo": true
      },
      {
        "id": "guid",
        "nombre": "Laptop Asus Vivobook",
        "codigoSku": "LAP-ASU-016",
        "precioBase": 4200000.00,
        "costoInicial": 3000000.00,
        "stockActual": 75,
        "cantidadEnCampo": 75,
        "valorCampoExtra": "AMD Ryzen 5",
        "categoriaNombre": "Electrónica",
        "activo": true
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 45
  },
  "message": "Productos con este campo extra obtenidos correctamente."
}
```

**?? Importante - Campos de Cantidad y Valor:**

| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| `stockActual` | Stock TOTAL del producto en TODAS las bodegas | 100 |
| `cantidadEnCampo` | Cantidad total del producto (mismo valor que `stockActual`) | 100 |
| `valorCampoExtra` | **VALOR asignado** a este campo extra para este producto | "Intel Core i7" |

**? Diferencia Clave:** A diferencia de categorías y bodegas, este endpoint incluye el campo **`valorCampoExtra`** que muestra el valor específico que tiene este campo extra para cada producto.

**Ejemplo con Campo Extra "Procesador":**

```json
{
  "nombre": "Laptop Dell XPS",
  "stockActual": 100,
  "cantidadEnCampo": 100,
  "valorCampoExtra": "Intel Core i7"  // ? Valor específico del campo
}

{
  "nombre": "Laptop Asus",
  "stockActual": 75,
  "cantidadEnCampo": 75,
  "valorCampoExtra": "AMD Ryzen 5"  // ? Diferente valor para el mismo campo
}
```

**Casos de Uso:**

1. **Ver todos los productos que tienen un campo específico:**
   ```
   GET /api/campos-extra/{id}/productos
   ```

2. **Buscar productos con un campo extra:**
   ```
   GET /api/campos-extra/{id}/productos?q=laptop
   ```

3. **Ver qué valores tiene un campo extra (ej: "Procesador"):**
   - Response muestra `valorCampoExtra` para cada producto
   - Ejemplo: "Intel Core i7", "AMD Ryzen 5", "Intel Core i5"

4. **Filtrar productos caros que tienen un campo:**
   ```
   GET /api/campos-extra/{id}/productos?precio=5000000&orderBy=precio&orderDesc=true
   ```

5. **? NUEVO - Productos con campo extra y stock alto:**
   ```
   GET /api/campos-extra/{id}/productos?cantidadMin=100&cantidadOperador=>
   ```

6. **? NUEVO - Auditoría: Ver todos los productos con campo "Color" asignado:**
   ```
   GET /api/campos-extra/{guid-campo-color}/productos
   ```
   - Response incluye `valorCampoExtra`: "Rojo", "Azul", "Negro", etc.

---

## 5?? Vendedores `/api/vendedores`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/vendedores` | Lista de vendedores | Query param: `soloActivos` (bool?) |
| `GET` | `/api/vendedores/{id}` | Obtener vendedor por ID | - |
| `POST` | `/api/vendedores` | Crear vendedor | CreateVendedorDto |
| `PUT` | `/api/vendedores/{id}` | Actualizar vendedor | UpdateVendedorDto |
| `PATCH` | `/api/vendedores/{id}/activate` | Activar vendedor | - |
| `PATCH` | `/api/vendedores/{id}/deactivate` | Desactivar vendedor | - |

### ?? CreateVendedorDto (Request Body para POST)

```json
{
  "nombre": "string (requerido)",
  "identificacion": "string (requerido)",
  "correo": "string (opcional)",
  "observaciones": "string (opcional)"
}
```

### ?? UpdateVendedorDto (Request Body para PUT)

```json
{
  "nombre": "string (requerido)",
  "identificacion": "string (requerido)",
  "correo": "string (opcional)",
  "observaciones": "string (opcional)"
}
```

### ?? VendedorDto (Response)

```json
{
  "id": "guid",
  "nombre": "string",
  "identificacion": "string",
  "correo": "string (opcional)",
  "observaciones": "string (opcional)",
  "activo": true,
  "fechaCreacion": "2024-01-01T00:00:00Z"
}
```

### Filtros de Query

**GET `/api/vendedores?soloActivos={bool?}`**

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `soloActivos` | Boolean? | Filtrar por estado activo | `?soloActivos=true` |

**Valores:**
- `null` o sin parámetro: Retorna todos (activos e inactivos)
- `true`: Solo vendedores activos
- `false`: Solo vendedores inactivos

**Ejemplos de Uso:**
```bash
# Todos los vendedores
GET /api/vendedores

# Solo vendedores activos
GET /api/vendedores?soloActivos=true

# Solo vendedores inactivos
GET /api/vendedores?soloActivos=false
```

---

## 6?? Proveedores `/api/proveedores`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/proveedores` | Lista de proveedores | Query param: `soloActivos` (bool?) |
| `GET` | `/api/proveedores/{id}` | Obtener proveedor por ID | - |
| `POST` | `/api/proveedores` | Crear proveedor | CreateProveedorDto |
| `PUT` | `/api/proveedores/{id}` | Actualizar proveedor | UpdateProveedorDto |
| `PATCH` | `/api/proveedores/{id}/activate` | Activar proveedor | - |
| `PATCH` | `/api/proveedores/{id}/deactivate` | Desactivar proveedor | - |

### ?? CreateProveedorDto (Request Body para POST)

```json
{
  "nombre": "string (requerido)",
  "identificacion": "string (requerido)",
  "correo": "string (opcional)",
  "observaciones": "string (opcional)"
}
```

### ?? UpdateProveedorDto (Request Body para PUT)

```json
{
  "nombre": "string (requerido)",
  "identificacion": "string (requerido)",
  "correo": "string (opcional)",
  "observaciones": "string (opcional)"
}
```

### ?? ProveedorDto (Response)

```json
{
  "id": "guid",
  "nombre": "string",
  "identificacion": "string",
  "correo": "string (opcional)",
  "observaciones": "string (opcional)",
  "activo": true,
  "fechaCreacion": "2024-01-01T00:00:00Z"
}
```

### Filtros de Query

**GET `/api/proveedores?soloActivos={bool?}`**

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `soloActivos` | Boolean? | Filtrar por estado activo | `?soloActivos=true` |

**Valores:**
- `null` o sin parámetro: Retorna todos (activos e inactivos)
- `true`: Solo proveedores activos
- `false`: Solo proveedores inactivos

**Ejemplos de Uso:**
```bash
# Todos los proveedores
GET /api/proveedores

# Solo proveedores activos
GET /api/proveedores?soloActivos=true

# Solo proveedores inactivos
GET /api/proveedores?soloActivos=false
```

---

## 7?? Movimientos de Inventario `/api/movimientos-inventario` (READ-ONLY)

**?? Importante:** Este endpoint es de **SOLO LECTURA**. Los movimientos se crean automáticamente al procesar facturas de venta o compra.

| Método | Endpoint | Descripción | Query Params |
|--------|----------|-------------|--------------|
| `GET` | `/api/movimientos-inventario` | Lista paginada con filtros avanzados | MovimientoInventarioFilterDto |
| `GET` | `/api/movimientos-inventario/{id}` | Obtener movimiento por ID | - |
| `GET` | `/api/movimientos-inventario/producto/{productoId}` | Kardex del producto | - |
| `GET` | `/api/movimientos-inventario/bodega/{bodegaId}` | Movimientos de una bodega | - |
| `GET` | `/api/movimientos-inventario/factura-venta/{facturaVentaId}` | Movimientos de una factura de venta | - |
| `GET` | `/api/movimientos-inventario/factura-compra/{facturaCompraId}` | Movimientos de una factura de compra | - |

### ?? MovimientoInventarioDto (Response)

```json
{
  "id": "guid",
  "productoId": "guid",
  "productoNombre": "string",
  "productoSku": "string",
  "bodegaId": "guid",
  "bodegaNombre": "string",
  "fecha": "2024-01-01T00:00:00Z",
  "tipoMovimiento": "VENTA | COMPRA",
  "cantidad": 10,
  "costoUnitario": 4000000.00,
  "precioUnitario": 5500000.00,
  "observacion": "string",
  "facturaVentaId": "guid (opcional)",
  "facturaVentaNumero": "string (opcional)",
  "facturaCompraId": "guid (opcional)",
  "facturaCompraNumero": "string (opcional)"
}
```

### Filtros de Query (GET `/api/movimientos-inventario`)

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `productoId` | Guid? | Filtrar por producto | `?productoId=guid` |
| `productoNombre` | String | Buscar por nombre de producto | `?productoNombre=laptop` |
| `productoSku` | String | Buscar por SKU | `?productoSku=LAP` |
| `bodegaId` | Guid? | Filtrar por bodega | `?bodegaId=guid` |
| `bodegaNombre` | String | Buscar por nombre de bodega | `?bodegaNombre=principal` |
| `tipoMovimiento` | String | Filtrar por tipo: `VENTA` o `COMPRA` | `?tipoMovimiento=VENTA` |
| `fechaDesde` | DateTime? | Desde esta fecha (inclusivo) | `?fechaDesde=2024-01-01` |
| `fechaHasta` | DateTime? | Hasta esta fecha (inclusivo) | `?fechaHasta=2024-12-31` |
| `cantidadMinima` | Int? | Cantidad mínima | `?cantidadMinima=10` |
| `cantidadMaxima` | Int? | Cantidad máxima | `?cantidadMaxima=100` |
| `facturaVentaId` | Guid? | Filtrar por factura de venta | `?facturaVentaId=guid` |
| `facturaCompraId` | Guid? | Filtrar por factura de compra | `?facturaCompraId=guid` |
| `orderBy` | String | Ordenar por: `fecha`, `cantidad`, `tipoMovimiento`, `productoNombre`, `bodegaNombre` | `?orderBy=fecha` |
| `orderDesc` | Boolean | Orden descendente (default: true) | `?orderDesc=false` |
| `page` | Int | Número de página | `?page=1` |
| `pageSize` | Int | Tamaño de página (max: 100) | `?pageSize=20` |

### Casos de Uso

**1. Kardex de un Producto:**
```bash
GET /api/movimientos-inventario/producto/{productoId}
```
Retorna todo el historial de movimientos del producto (entradas y salidas).

**2. Auditoría de una Bodega:**
```bash
GET /api/movimientos-inventario/bodega/{bodegaId}
```
Retorna todos los movimientos de una bodega específica.

**3. Movimientos por Rango de Fechas:**
```bash
GET /api/movimientos-inventario?fechaDesde=2024-01-01&fechaHasta=2024-01-31
```

**4. Solo Ventas del Mes:**
```bash
GET /api/movimientos-inventario?tipoMovimiento=VENTA&fechaDesde=2024-01-01&fechaHasta=2024-01-31
```

**5. Movimientos de una Factura:**
```bash
GET /api/movimientos-inventario/factura-venta/{facturaId}
```

**?? Nota sobre Cantidades:**
- **Cantidad Positiva:** Movimiento normal (venta o compra)
- **Cantidad Negativa:** Reversión (anulación de factura)

**Ejemplo de Kardex:**

| Fecha | Tipo | Cantidad | Observación |
|-------|------|----------|-------------|
| 2024-01-10 | COMPRA | +50 | Compra - FC-0001 |
| 2024-01-15 | VENTA | +5 | Venta - FV-0001 |
| 2024-01-16 | VENTA | **-5** | Anulación - FV-0001 |

---

## 8?? Facturas de Venta `/api/facturas-venta`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/facturas-venta` | Lista de todas las facturas de venta | - |
| `GET` | `/api/facturas-venta/{id}` | Obtener factura por ID | - |
| `POST` | `/api/facturas-venta` | Crear factura de venta | CreateFacturaVentaDto |
| `DELETE` | `/api/facturas-venta/{id}` | Anular factura (soft delete) | - |

**?? Importante:**
- Al crear una factura se **reduce el stock** automáticamente
- Al anular una factura se **restaura el stock**
- Se crean **movimientos de inventario** automáticos
- Solo se pueden anular facturas en estado "Completada"

### ?? CreateFacturaVentaDto (Request Body para POST)

```json
{
  "bodegaId": "guid (requerido)",
  "vendedorId": "guid (requerido)",
  "fecha": "2024-01-01T00:00:00Z",
  "formaPago": "Contado | Crédito",
  "plazoPago": 30,
  "medioPago": "Efectivo | Tarjeta | Transferencia",
  "observaciones": "string (opcional)",
  "items": [
    {
      "productoId": "guid",
      "cantidad": 5,
      "precioUnitario": 5500000.00,
      "descuento": 0.00,
      "impuesto": 0.00
    }
  ]
}
```

### ?? FacturaVentaDto (Response)

```json
{
  "id": "guid",
  "numeroFactura": "FV-0001",
  "bodegaId": "guid",
  "bodegaNombre": "Bodega Principal",
  "vendedorId": "guid",
  "vendedorNombre": "Juan Pérez",
  "fecha": "2024-01-01T00:00:00Z",
  "formaPago": "Contado",
  "plazoPago": null,
  "medioPago": "Efectivo",
  "observaciones": "string",
  "estado": "Completada | Anulada",
  "total": 27500000.00,
  "items": [
    {
      "id": "guid",
      "productoId": "guid",
      "productoNombre": "Laptop Dell XPS",
      "productoSku": "LAP-DELL-001",
      "cantidad": 5,
      "precioUnitario": 5500000.00,
      "descuento": 0.00,
      "impuesto": 0.00,
      "totalLinea": 27500000.00
    }
  ]
}
```

### Reglas de Negocio

1. **Validación de Stock:**
   - Se valida que haya stock suficiente ANTES de crear la factura
   - Si no hay stock, retorna error 400

2. **Numeración Automática:**
   - El número de factura se genera automáticamente: `FV-0001`, `FV-0002`, etc.

3. **Actualización de Stock:**
   - Se reduce el stock en la bodega especificada
   - Se crea un registro en `MovimientosInventario`

4. **Anulación:**
   - Solo se pueden anular facturas en estado "Completada"
   - Al anular, se restaura el stock
   - Se crea un movimiento de reversión (cantidad negativa)
   - El estado cambia a "Anulada" (NO se elimina físicamente)

---

## 9?? Facturas de Compra `/api/facturas-compra`

| Método | Endpoint | Descripción | Body |
|--------|----------|-------------|------|
| `GET` | `/api/facturas-compra` | Lista de todas las facturas de compra | - |
| `GET` | `/api/facturas-compra/{id}` | Obtener factura por ID | - |
| `POST` | `/api/facturas-compra` | Crear factura de compra | CreateFacturaCompraDto |
| `DELETE` | `/api/facturas-compra/{id}` | Anular factura (soft delete) | - |

**?? Importante:**
- Al crear una factura se **aumenta el stock** automáticamente
- Al anular una factura se **reduce el stock**
- Se crean **movimientos de inventario** automáticos
- Solo se pueden anular facturas en estado "Completada"

### ?? CreateFacturaCompraDto (Request Body para POST)

```json
{
  "bodegaId": "guid (requerido)",
  "proveedorId": "guid (requerido)",
  "fecha": "2024-01-01T00:00:00Z",
  "observaciones": "string (opcional)",
  "items": [
    {
      "productoId": "guid",
      "cantidad": 50,
      "costoUnitario": 4000000.00,
      "descuento": 0.00,
      "impuesto": 0.00
    }
  ]
}
```

### ?? FacturaCompraDto (Response)

```json
{
  "id": "guid",
  "numeroFactura": "FC-0001",
  "bodegaId": "guid",
  "bodegaNombre": "Bodega Principal",
  "proveedorId": "guid",
  "proveedorNombre": "Dell Colombia",
  "fecha": "2024-01-01T00:00:00Z",
  "observaciones": "string",
  "estado": "Completada | Anulada",
  "total": 200000000.00,
  "items": [
    {
      "id": "guid",
      "productoId": "guid",
      "productoNombre": "Laptop Dell XPS",
      "productoSku": "LAP-DELL-001",
      "cantidad": 50,
      "costoUnitario": 4000000.00,
      "descuento": 0.00,
      "impuesto": 0.00,
      "totalLinea": 200000000.00
    }
  ]
}
```

### Reglas de Negocio

1. **Numeración Automática:**
   - El número de factura se genera automáticamente: `FC-0001`, `FC-0002`, etc.

2. **Actualización de Stock:**
   - Se aumenta el stock en la bodega especificada
   - Se crea un registro en `MovimientosInventario`

3. **Anulación:**
   - Solo se pueden anular facturas en estado "Completada"
   - Al anular, se reduce el stock
   - Se crea un movimiento de reversión (cantidad negativa)
   - El estado cambia a "Anulada" (NO se elimina físicamente)

---

## ?? Flujo de Movimientos de Inventario

### Crear Factura de Venta

```
POST /api/facturas-venta
?
1. Validar stock suficiente
2. Crear factura (estado: "Completada")
3. Reducir stock en ProductoBodega
4. Crear MovimientoInventario (tipo: VENTA, cantidad: positiva)
```

### Anular Factura de Venta

```
DELETE /api/facturas-venta/{id}
?
1. Validar estado = "Completada"
2. Cambiar estado a "Anulada"
3. Restaurar stock en ProductoBodega
4. Crear MovimientoInventario (tipo: VENTA, cantidad: NEGATIVA)
```

### Crear Factura de Compra

```
POST /api/facturas-compra
?
1. Crear factura (estado: "Completada")
2. Aumentar stock en ProductoBodega
3. Crear MovimientoInventario (tipo: COMPRA, cantidad: positiva)
```

### Anular Factura de Compra

```
DELETE /api/facturas-compra/{id}
?
1. Validar estado = "Completada"
2. Cambiar estado a "Anulada"
3. Reducir stock en ProductoBodega
4. Crear MovimientoInventario (tipo: COMPRA, cantidad: NEGATIVA)
```

---

## ?? Resumen de Endpoints

### Endpoints CRUD Completos

| Entidad | GET Lista | GET By ID | POST | PUT | PATCH Activate | PATCH Deactivate | DELETE |
|---------|-----------|-----------|------|-----|----------------|------------------|---------|
| **Productos** | ? | ? | ? | ? | ? | ? | ? Permanente |
| **Bodegas** | ? | ? | ? | ? | ? | ? | ? Permanente |
| **Categorías** | ? | ? | ? | ? | ? | ? | ? Permanente |
| **Campos Extra** | ? | ? | ? | ? | ? | ? | ? Permanente |
| **Vendedores** | ? | ? | ? | ? | ? | ? | ? |
| **Proveedores** | ? | ? | ? | ? | ? | ? | ? |

### Endpoints de Solo Lectura

| Entidad | GET Lista | GET By ID | Endpoints Especiales |
|---------|-----------|-----------|---------------------|
| **Movimientos Inventario** | ? Filtros avanzados | ? | `/producto/{id}`, `/bodega/{id}`, `/factura-venta/{id}`, `/factura-compra/{id}` |

### Endpoints de Facturas

| Entidad | GET Lista | GET By ID | POST | DELETE (Anular) |
|---------|-----------|-----------|------|-----------------|
| **Facturas Venta** | ? | ? | ? | ? Soft Delete |
| **Facturas Compra** | ? | ? | ? | ? Soft Delete |

### Endpoints de Relaciones

| Relación | Endpoints Disponibles |
|----------|----------------------|
| **Producto ? Bodegas** | `GET /productos/{id}/bodegas`, `POST`, `PUT`, `DELETE` |
| **Producto ? Campos Extra** | `GET /productos/{id}/campos-extra`, `PUT`, `DELETE` |
| **Bodega ? Productos** | `GET /bodegas/{id}/productos` (filtros avanzados) |
| **Categoría ? Productos** | `GET /categorias/{id}/productos` (filtros avanzados) |
| **Campo Extra ? Productos** | `GET /campos-extra/{id}/productos` (filtros avanzados) |

---

## ?? Reglas de Negocio Importantes

### Productos
- ? Se puede crear con bodega principal + bodegas adicionales
- ? Se puede crear con campos extra opcionales
- ? SKU se genera automáticamente si no se proporciona
- ?? No se puede eliminar si está en facturas o movimientos
- ?? Debe estar al menos en una bodega

### Bodegas
- ?? No se puede desactivar si tiene productos
- ?? No se puede eliminar si tiene productos, facturas o movimientos

### Categorías
- ?? No se puede desactivar si tiene productos
- ?? No se puede eliminar si tiene productos

### Campos Extra
- ?? No se puede eliminar si está siendo usado en productos

### Facturas
- ? Numeración automática: `FV-0001`, `FC-0001`
- ? Crean movimientos de inventario automáticamente
- ? Actualizan stock automáticamente
- ?? Solo se pueden anular facturas "Completadas"
- ?? Al anular se restaura/reduce stock automáticamente
- ?? No se eliminan físicamente (soft delete)

### Movimientos de Inventario
- ?? **SOLO LECTURA** (no se pueden crear/editar/eliminar manualmente)
- ? Se crean automáticamente al procesar facturas
- ? Cantidad negativa indica reversión (anulación)
- ? Sirven como registro de auditoría (Kardex)

---

## ?? Casos de Uso Comunes

### 1. Crear Producto Completo
```bash
POST /api/productos
{
  "nombre": "Laptop Dell XPS 15",
  "unidadMedida": "Unidad",
  "bodegaPrincipalId": "guid",
  "cantidadInicial": 50,
  "precioBase": 5500000,
  "costoInicial": 4000000,
  "categoriaId": "guid",
  "bodegasAdicionales": [...],
  "camposExtra": [...]
}
```

### 2. Registrar Venta
```bash
POST /api/facturas-venta
{
  "bodegaId": "guid",
  "vendedorId": "guid",
  "fecha": "2024-01-15T10:00:00Z",
  "items": [
    {
      "productoId": "guid",
      "cantidad": 2,
      "precioUnitario": 5500000
    }
  ]
}
```
**Resultado:**
- ? Factura creada con número `FV-0001`
- ? Stock reducido en 2 unidades
- ? Movimiento de inventario registrado

### 3. Ver Kardex de un Producto
```bash
GET /api/movimientos-inventario/producto/{productoId}
```
**Retorna:** Historial completo de entradas y salidas

### 4. Ver Productos con Stock Bajo en una Bodega
```bash
GET /api/bodegas/{bodegaId}/productos?cantidadMax=10&cantidadOperador=<
```

### 5. Anular una Venta
```bash
DELETE /api/facturas-venta/{id}
```
**Resultado:**
- ? Estado cambia a "Anulada"
- ? Stock restaurado
- ? Movimiento de reversión registrado (cantidad negativa)

---

## ?? Notas Técnicas

### Paginación
- **Default:** `page=1`, `pageSize=20`
- **Máximo:** `pageSize=100`
- **Response:** Incluye `items`, `page`, `pageSize`, `totalCount`

### Filtros de Búsqueda
- **Texto:** Case-insensitive, búsqueda parcial
- **Precio:** Búsqueda parcial como texto (ej: "1500" encuentra 1500000, 2500000, etc.)
- **Cantidad:** Operadores: `=`, `>`, `>=`, `<`, `<=`, `range`

### Campos Calculados
- **`stockActual`:** Suma de stock en TODAS las bodegas
- **`cantidadEnBodega`:** Stock en UNA bodega específica
- **`cantidadEnCategoria`:** Igual a `stockActual` (un producto = una categoría)
- **`cantidadEnCampo`:** Igual a `stockActual` (consistencia de API)
- **`valorCampoExtra`:** Valor específico del campo para ese producto

### Soft Delete vs Hard Delete
- **Soft Delete:** Facturas (cambian estado a "Anulada")
- **Hard Delete:** Productos, Bodegas, Categorías, Campos Extra (solo si no hay dependencias)
- **No Delete:** Vendedores, Proveedores (solo activar/desactivar)

---

## ?? Endpoints por Orden de Implementación

### Fase 1: Configuración Básica
1. ? Bodegas
2. ? Categorías
3. ? Campos Extra
4. ? Vendedores
5. ? Proveedores

### Fase 2: Productos
6. ? Productos (CRUD + relaciones)
7. ? Productos ? Bodegas
8. ? Productos ? Campos Extra

### Fase 3: Facturas e Inventario
9. ? Facturas de Venta
10. ? Facturas de Compra
11. ? Movimientos de Inventario

### Fase 4: Consultas Avanzadas
12. ? Bodegas ? Productos (con filtros)
13. ? Categorías ? Productos (con filtros)
14. ? Campos Extra ? Productos (con filtros + valores)

---

## ?? Soporte

Para más información sobre el uso de estos endpoints, consulta:
- **Código fuente:** `/InventoryBack/API/Controllers/`
- **Servicios:** `/InventoryBack/Application/Services/`
- **DTOs:** `/InventoryBack/Application/DTOs/`

---

**Última actualización:** Enero 2024  
**Versión:** 1.0.0  
**Framework:** .NET 9
