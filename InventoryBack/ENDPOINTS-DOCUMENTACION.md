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

**?? Importante:** Este endpoint es de **SOLO LECTURA**. Los movimientos se crean automáticamente al procesar facturas de venta o compra. **No existe** POST, PUT, PATCH ni DELETE para movimientos.

### ?? Propósito

Los movimientos de inventario sirven para:
- ? **Trazabilidad completa** de entradas y salidas de productos
- ? **Auditoría** de operaciones de inventario
- ? **Kardex** de productos (historial de movimientos)
- ? **Reportes** de movimientos por bodega, producto, fecha, etc.
- ? **Validación** de stock actual (sumando movimientos)

---

## ?? Endpoints Disponibles

| Método | Endpoint | Descripción | Caso de Uso |
|--------|----------|-------------|-------------|
| `GET` | `/api/movimientos-inventario` | Lista paginada con filtros avanzados | Auditoría general, reportes |
| `GET` | `/api/movimientos-inventario/{id}` | Obtener movimiento específico por ID | Ver detalles de un movimiento |
| `GET` | `/api/movimientos-inventario/producto/{productoId}` | **Kardex del producto** | Historial completo de un producto |
| `GET` | `/api/movimientos-inventario/bodega/{bodegaId}` | Movimientos de una bodega | Auditoría de bodega específica |
| `GET` | `/api/movimientos-inventario/factura-venta/{facturaVentaId}` | Movimientos de una factura de venta | Ver qué productos se vendieron |
| `GET` | `/api/movimientos-inventario/factura-compra/{facturaCompraId}` | Movimientos de una factura de compra | Ver qué productos se compraron |

---

## ?? GET `/api/movimientos-inventario` - Lista con Filtros

### **Filtros Disponibles (Query Params)**

| Parámetro | Tipo | Descripción | Ejemplo |
|-----------|------|-------------|---------|
| `page` | Int | Número de página (default: 1) | `?page=2` |
| `pageSize` | Int | Tamaño de página (default: 20, max: 100) | `?pageSize=50` |
| `productoId` | Guid? | Filtrar por producto específico | `?productoId=guid` |
| `productoNombre` | String | Buscar por nombre de producto (parcial) | `?productoNombre=laptop` |
| `productoSku` | String | Buscar por SKU (parcial) | `?productoSku=LAP` |
| `bodegaId` | Guid? | Filtrar por bodega específica | `?bodegaId=guid` |
| `bodegaNombre` | String | Buscar por nombre de bodega (parcial) | `?bodegaNombre=principal` |
| `tipoMovimiento` | String | Filtrar por tipo: `VENTA` o `COMPRA` | `?tipoMovimiento=VENTA` |
| `fechaDesde` | DateTime? | Desde esta fecha (inclusivo) | `?fechaDesde=2025-01-01` |
| `fechaHasta` | DateTime? | Hasta esta fecha (inclusivo) | `?fechaHasta=2025-01-31` |
| `cantidadMinima` | Int? | Cantidad mínima del movimiento | `?cantidadMinima=10` |
| `cantidadMaxima` | Int? | Cantidad máxima del movimiento | `?cantidadMaxima=100` |
| `facturaVentaId` | Guid? | Filtrar por factura de venta | `?facturaVentaId=guid` |
| `facturaCompraId` | Guid? | Filtrar por factura de compra | `?facturaCompraId=guid` |
| `orderBy` | String | Ordenar por: `fecha`, `cantidad`, `tipoMovimiento`, `productoNombre`, `bodegaNombre` | `?orderBy=fecha` |
| `orderDesc` | Boolean | Orden descendente (default: true - más recientes primero) | `?orderDesc=false` |

### **Ejemplos de Requests**

```bash
# Todos los movimientos (paginados)
GET /api/movimientos-inventario

# Movimientos del mes actual
GET /api/movimientos-inventario?fechaDesde=2025-01-01&fechaHasta=2025-01-31

# Solo ventas
GET /api/movimientos-inventario?tipoMovimiento=VENTA

# Solo compras
GET /api/movimientos-inventario?tipoMovimiento=COMPRA

# Movimientos de una bodega en un rango de fechas
GET /api/movimientos-inventario?bodegaId=guid&fechaDesde=2025-01-01&fechaHasta=2025-01-31

# Buscar movimientos de laptops
GET /api/movimientos-inventario?productoNombre=laptop

# Movimientos grandes (más de 50 unidades)
GET /api/movimientos-inventario?cantidadMinima=50

# Movimientos ordenados por cantidad descendente
GET /api/movimientos-inventario?orderBy=cantidad&orderDesc=true
```

### **Response Structure**

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "productoId": "guid",
        "productoNombre": "Laptop Dell XPS 15",
        "productoSku": "LAP-DELL-001",
        "bodegaId": "guid",
        "bodegaNombre": "Bodega Principal",
        "fecha": "2025-01-15T10:30:00Z",
        "tipoMovimiento": "VENTA",
        "cantidad": 5,
        "costoUnitario": 4000000.00,
        "precioUnitario": 5500000.00,
        "observacion": "Venta - Factura FV-202501-0001",
        "facturaVentaId": "guid",
        "facturaVentaNumero": "FV-202501-0001",
        "facturaCompraId": null,
        "facturaCompraNumero": null
      },
      {
        "id": "guid",
        "productoId": "guid",
        "productoNombre": "Mouse Logitech MX Master",
        "productoSku": "MOU-LOG-001",
        "bodegaId": "guid",
        "bodegaNombre": "Bodega Principal",
        "fecha": "2025-01-15T10:30:00Z",
        "tipoMovimiento": "VENTA",
        "cantidad": 3,
        "costoUnitario": 350000.00,
        "precioUnitario": 350000.00,
        "observacion": "Venta - Factura FV-202501-0001",
        "facturaVentaId": "guid",
        "facturaVentaNumero": "FV-202501-0001",
        "facturaCompraId": null,
        "facturaCompraNumero": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 45
  },
  "message": "Movimientos de inventario obtenidos correctamente."
}
```

---

## ?? GET `/api/movimientos-inventario/{id}` - Por ID

**Descripción:** Obtiene un movimiento específico por su ID.

**Request:**
```bash
GET /api/movimientos-inventario/{guid}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "productoId": "guid",
    "productoNombre": "Laptop Dell XPS 15",
    "productoSku": "LAP-DELL-001",
    "bodegaId": "guid",
    "bodegaNombre": "Bodega Principal",
    "fecha": "2025-01-15T10:30:00Z",
    "tipoMovimiento": "VENTA",
    "cantidad": 5,
    "costoUnitario": 4000000.00,
    "precioUnitario": 5500000.00,
    "observacion": "Venta - Factura FV-202501-0001",
    "facturaVentaId": "guid",
    "facturaVentaNumero": "FV-202501-0001",
    "facturaCompraId": null,
    "facturaCompraNumero": null
  },
  "message": "Movimiento de inventario obtenido correctamente."
}
```

---

## ?? GET `/api/movimientos-inventario/producto/{productoId}` - Kardex

**Descripción:** Obtiene el **historial completo** de movimientos de un producto (Kardex). Muestra todas las entradas y salidas del producto en todas las bodegas.

**Caso de Uso:**
- Generar reporte Kardex
- Ver historial completo de un producto
- Auditoría de movimientos de inventario
- Validar stock actual

**Request:**
```bash
GET /api/movimientos-inventario/producto/{productoId}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid-1",
      "productoNombre": "Laptop Dell XPS 15",
      "productoSku": "LAP-DELL-001",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-10T14:00:00Z",
      "tipoMovimiento": "COMPRA",
      "cantidad": 50,
      "observacion": "Compra - Factura FC-202501-0001"
    },
    {
      "id": "guid-2",
      "productoNombre": "Laptop Dell XPS 15",
      "productoSku": "LAP-DELL-001",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-15T10:30:00Z",
      "tipoMovimiento": "VENTA",
      "cantidad": 5,
      "observacion": "Venta - Factura FV-202501-0001"
    },
    {
      "id": "guid-3",
      "productoNombre": "Laptop Dell XPS 15",
      "productoSku": "LAP-DELL-001",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-16T09:00:00Z",
      "tipoMovimiento": "VENTA",
      "cantidad": -5,
      "observacion": "Anulación de venta - Factura FV-202501-0001"
    }
  ],
  "message": "Movimientos del producto obtenidos correctamente."
}
```

**?? Ejemplo de Kardex:**

| Fecha | Tipo | Cantidad | Observación | Stock Resultante |
|-------|------|----------|-------------|------------------|
| 2025-01-10 | COMPRA | +50 | Compra - FC-202501-0001 | 50 |
| 2025-01-15 | VENTA | +5 | Venta - FV-202501-0001 | 45 |
| 2025-01-16 | VENTA | **-5** | Anulación - FV-202501-0001 | 50 |

---

## ?? GET `/api/movimientos-inventario/bodega/{bodegaId}` - Por Bodega

**Descripción:** Obtiene todos los movimientos (entradas y salidas) de una bodega específica.

**Caso de Uso:**
- Auditoría de una bodega
- Ver historial de movimientos de un almacén
- Reportes de actividad por bodega

**Request:**
```bash
GET /api/movimientos-inventario/bodega/{bodegaId}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "productoNombre": "Laptop Dell XPS 15",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-10T14:00:00Z",
      "tipoMovimiento": "COMPRA",
      "cantidad": 50,
      "observacion": "Compra - FC-202501-0001"
    },
    {
      "id": "guid",
      "productoNombre": "Mouse Logitech",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-12T11:00:00Z",
      "tipoMovimiento": "COMPRA",
      "cantidad": 100,
      "observacion": "Compra - FC-202501-0002"
    }
  ],
  "message": "Movimientos de la bodega obtenidos correctamente."
}
```

---

## ?? GET `/api/movimientos-inventario/factura-venta/{facturaVentaId}` - Por Factura de Venta

**Descripción:** Obtiene todos los movimientos de inventario asociados a una factura de venta específica.

**Caso de Uso:**
- Ver qué productos se vendieron en una factura
- Auditoría de ventas
- Validar stock reducido

**Request:**
```bash
GET /api/movimientos-inventario/factura-venta/{facturaVentaId}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid-1",
      "productoNombre": "Laptop Dell XPS 15",
      "productoSku": "LAP-DELL-001",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-15T10:30:00Z",
      "tipoMovimiento": "VENTA",
      "cantidad": 2,
      "precioUnitario": 5500000.00,
      "observacion": "Venta - Factura FV-202501-0001",
      "facturaVentaNumero": "FV-202501-0001"
    },
    {
      "id": "guid-2",
      "productoNombre": "Mouse Logitech MX Master",
      "productoSku": "MOU-LOG-001",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-15T10:30:00Z",
      "tipoMovimiento": "VENTA",
      "cantidad": 3,
      "precioUnitario": 350000.00,
      "observacion": "Venta - Factura FV-202501-0001",
      "facturaVentaNumero": "FV-202501-0001"
    }
  ],
  "message": "Movimientos de la factura de venta obtenidos correctamente."
}
```

---

## ?? GET `/api/movimientos-inventario/factura-compra/{facturaCompraId}` - Por Factura de Compra

**Descripción:** Obtiene todos los movimientos de inventario asociados a una factura de compra específica.

**Caso de Uso:**
- Ver qué productos se compraron en una factura
- Auditoría de compras
- Validar stock aumentado

**Request:**
```bash
GET /api/movimientos-inventario/factura-compra/{facturaCompraId}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid-1",
      "productoNombre": "Laptop Dell XPS 15",
      "productoSku": "LAP-DELL-001",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-10T14:00:00Z",
      "tipoMovimiento": "COMPRA",
      "cantidad": 50,
      "costoUnitario": 4000000.00,
      "observacion": "Compra - Factura FC-202501-0001",
      "facturaCompraNumero": "FC-202501-0001"
    },
    {
      "id": "guid-2",
      "productoNombre": "Mouse Logitech MX Master",
      "productoSku": "MOU-LOG-001",
      "bodegaNombre": "Bodega Principal",
      "fecha": "2025-01-10T14:00:00Z",
      "tipoMovimiento": "COMPRA",
      "cantidad": 100,
      "costoUnitario": 200000.00,
      "observacion": "Compra - Factura FC-202501-0001",
      "facturaCompraNumero": "FC-202501-0001"
    }
  ],
  "message": "Movimientos de la factura de compra obtenidos correctamente."
}
```

---

## ?? Estructura Completa de MovimientoInventarioDto

```json
{
  "id": "guid",
  "productoId": "guid",
  "productoNombre": "string",
  "productoSku": "string",
  "bodegaId": "guid",
  "bodegaNombre": "string",
  "fecha": "2025-01-15T10:30:00Z",
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

**Descripción de Campos:**
| Campo | Tipo | Descripción | Ejemplo |
|-------|------|-------------|---------|
| `id` | Guid | ID único del movimiento | `guid` |
| `productoId` | Guid | ID del producto | `guid` |
| `productoNombre` | String | Nombre del producto | "Laptop Dell XPS 15" |
| `productoSku` | String | SKU del producto | "LAP-DELL-001" |
| `bodegaId` | Guid | ID de la bodega | `guid` |
| `bodegaNombre` | String | Nombre de la bodega | "Bodega Principal" |
| `fecha` | DateTime | Fecha y hora del movimiento | "2025-01-15T10:30:00Z" |
| `tipoMovimiento` | String | Tipo: `VENTA` o `COMPRA` | "VENTA" |
| `cantidad` | Int | Cantidad movida (puede ser negativa) | 10 o -5 |
| `costoUnitario` | Decimal? | Costo unitario (si es compra) | 4000000.00 |
| `precioUnitario` | Decimal? | Precio unitario (si es venta) | 5500000.00 |
| `observacion` | String | Descripción del movimiento | "Venta - Factura FV-001" |
| `facturaVentaId` | Guid? | ID de factura de venta (si aplica) | `guid` o `null` |
| `facturaVentaNumero` | String? | Número de factura de venta | "FV-202501-0001" |
| `facturaCompraId` | Guid? | ID de factura de compra (si aplica) | `guid` o `null` |
| `facturaCompraNumero` | String? | Número de factura de compra | "FC-202501-0001" |

---

## ?? Importante - Cantidades Negativas

**Cantidad Positiva:** Movimiento normal
- Venta normal: cantidad positiva (ej: +5)
- Compra normal: cantidad positiva (ej: +50)

**Cantidad Negativa:** Reversión (anulación de factura)
- Venta anulada: cantidad negativa (ej: -5)
- Compra anulada: cantidad negativa (ej: -50)

**Ejemplo:**
```json
// Venta original
{
  "cantidad": 5,
  "tipoMovimiento": "VENTA",
  "observacion": "Venta - Factura FV-202501-0001"
}

// Anulación de la venta
{
  "cantidad": -5,  // ? NEGATIVO
  "tipoMovimiento": "VENTA",
  "observacion": "Anulación de venta - Factura FV-202501-0001"
}
```

---

## ?? Casos de Uso Comunes

### **1. Generar Kardex de un Producto**

```bash
GET /api/movimientos-inventario/producto/{productoId}
```

**Uso:** Reporte completo de entradas y salidas de un producto.

---

### **2. Auditoría Mensual de Movimientos**

```bash
GET /api/movimientos-inventario?fechaDesde=2025-01-01&fechaHasta=2025-01-31
```

**Uso:** Ver todos los movimientos del mes.

---

### **3. Movimientos de Ventas del Día**

```bash
GET /api/movimientos-inventario?tipoMovimiento=VENTA&fechaDesde=2025-01-15&fechaHasta=2025-01-15
```

**Uso:** Dashboard de ventas diarias.

---

### **4. Auditoría de una Bodega Específica**

```bash
GET /api/movimientos-inventario/bodega/{bodegaId}
```

**Uso:** Ver todos los movimientos históricos de un almacén.

---

### **5. Ver Movimientos de una Factura**

```bash
GET /api/movimientos-inventario/factura-venta/{facturaId}
```

**Uso:** Ver qué productos se vendieron en una factura específica.

---

### **6. Buscar Movimientos de Laptops**

```bash
GET /api/movimientos-inventario?productoNombre=laptop&orderBy=fecha&orderDesc=true
```

**Uso:** Filtrar movimientos de un tipo de producto.

---

### **7. Movimientos Grandes (Más de 50 unidades)**

```bash
GET /api/movimientos-inventario?cantidadMinima=50&orderBy=cantidad&orderDesc=true
```

**Uso:** Detectar movimientos importantes.

---

### **8. Solo Compras del Mes**

```bash
GET /api/movimientos-inventario?tipoMovimiento=COMPRA&fechaDesde=2025-01-01&fechaHasta=2025-01-31
```

**Uso:** Reporte de compras mensuales.

---

## ?? Restricciones y Reglas

| Operación | Permitido | Razón |
|-----------|-----------|-------|
| **GET** | ? Sí | Solo lectura - auditoría |
| **POST** | ? No | Se crean automáticamente con facturas |
| **PUT** | ? No | Los movimientos son inmutables |
| **PATCH** | ? No | Los movimientos son inmutables |
| **DELETE** | ? No | No se eliminan (auditoría) |

**¿Cómo se crean los movimientos?**

Los movimientos se crean **automáticamente** cuando:

1. **Se crea una factura de venta:**
   - Se genera un movimiento tipo `VENTA` por cada item
   - Stock se reduce en la bodega

2. **Se crea una factura de compra:**
   - Se genera un movimiento tipo `COMPRA` por cada item
   - Stock se aumenta en la bodega

3. **Se anula una factura:**
   - Se genera un movimiento de reversión (cantidad negativa)
   - Stock se restaura/reduce según corresponda

---

## ?? Ejemplo Completo - Flujo de Movimientos

### **Escenario:**

1. Compra de 50 laptops
2. Venta de 5 laptops
3. Anulación de la venta

### **Movimientos Generados:**

```json
{
  "fecha": "2025-01-10T14:00:00Z",
  "tipoMovimiento": "COMPRA",
  "cantidad": 50,  // ? Positivo (entrada)
  "observacion": "Compra - Factura FC-202501-0001",
  "facturaCompraNumero": "FC-202501-0001"
},
{
  "fecha": "2025-01-15T10:30:00Z",
  "tipoMovimiento": "VENTA",
  "cantidad": 5,  // ? Positivo (salida)
  "observacion": "Venta - Factura FV-202501-0001",
  "facturaVentaNumero": "FV-202501-0001"
},
{
  "fecha": "2025-01-16T09:00:00Z",
  "tipoMovimiento": "VENTA",
  "cantidad": -5,  // ? NEGATIVO (reversión)
  "observacion": "Anulación de venta - Factura FV-202501-0001",
  "facturaVentaNumero": "FV-202501-0001"
}
```

**Stock Resultante:**
- Después de compra: 50
- Después de venta: 45
- Después de anulación: 50 (restaurado)

---

## ?? Resumen

? **Solo lectura** - No se pueden crear/editar/eliminar manualmente  
? **Generación automática** - Se crean con facturas  
? **Trazabilidad completa** - Cada movimiento queda registrado  
? **Kardex disponible** - Historial por producto  
? **Auditoría por bodega** - Ver movimientos de almacén  
? **Filtros avanzados** - Buscar por múltiples criterios  
? **Paginación** - Manejar grandes volúmenes de datos  
? **Cantidades negativas** - Indican reversiones (anulaciones)  

**Los movimientos de inventario son el corazón de la trazabilidad del sistema.** ??

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
- ? Factura creada with number `FV-0001`
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
