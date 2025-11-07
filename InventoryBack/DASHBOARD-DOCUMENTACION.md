# ?? Documentación de API - Dashboard y Gráficas

**Base URL:** `https://localhost:7262/api/dashboard`  
**Formato:** JSON  
**Framework:** .NET 9

---

## ?? Índice

1. [Introducción](#introducción)
2. [Métricas Principales (Cards)](#1-métricas-principales-cards)
3. [Top 10 Productos Más Vendidos](#2-top-10-productos-más-vendidos)
4. [Tendencia de Ventas](#3-tendencia-de-ventas)
5. [Distribución de Inventario por Categoría](#4-distribución-de-inventario-por-categoría)
6. [Movimientos de Stock](#5-movimientos-de-stock-entradas-vs-salidas)
7. [Comparación Stock por Bodega](#6-comparación-stock-por-bodega)
8. [Gauge de Salud del Stock](#7-gauge-de-salud-del-stock)
9. [Productos con Stock Bajo](#8-productos-con-stock-bajo)
10. [Ejemplos de Uso](#ejemplos-de-uso)
11. [Integración con Frontend](#integración-con-frontend)

---

## Introducción

Este módulo proporciona endpoints especializados para **dashboards y visualizaciones de datos**. Todos los cálculos se realizan en el backend para garantizar **precisión, performance y seguridad**.

### ? Características

- **Datos agregados listos para gráficas**
- **Cálculos optimizados en backend**
- **Sin necesidad de procesamiento en frontend**
- **Respuestas rápidas y cacheables**
- **Métricas en tiempo real**

### ?? Tipos de Visualizaciones Soportadas

1. ? **Cards/KPIs** - Métricas principales
2. ? **Bar Charts** - Top productos, stock por bodega
3. ? **Line Charts** - Tendencia de ventas, movimientos
4. ? **Pie Charts** - Distribución por categoría
5. ? **Gauge Charts** - Salud del stock
6. ? **Tables** - Productos con stock bajo

---

## 1. Métricas Principales (Cards)

### **GET `/api/dashboard/metrics`**

**Descripción:** Obtiene las métricas principales para mostrar en cards del dashboard.

**Request:**
```bash
GET /api/dashboard/metrics
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalProductos": 250,
    "totalBodegas": 5,
    "ventasDelMes": 125000000.00,
    "comprasDelMes": 85000000.00,
    "productosStockBajo": 15,
    "valorTotalInventario": 450000000.00,
    "margenBruto": 40000000.00,
    "porcentajeMargen": 32.00
  },
  "message": "Métricas del dashboard obtenidas correctamente."
}
```

### **Campos del Response**

| Campo | Tipo | Descripción | Cálculo |
|-------|------|-------------|---------|
| `totalProductos` | Int | Total de productos activos | `COUNT(productos WHERE activo = true)` |
| `totalBodegas` | Int | Total de bodegas activas | `COUNT(bodegas WHERE activo = true)` |
| `ventasDelMes` | Decimal | Suma de ventas completadas del mes | `SUM(facturas_venta.total WHERE estado = 'Completada' AND mes_actual)` |
| `comprasDelMes` | Decimal | Suma de compras completadas del mes | `SUM(facturas_compra.total WHERE estado = 'Completada' AND mes_actual)` |
| `productosStockBajo` | Int | Productos con stock < mínimo | `COUNT(productos WHERE stock < stock_minimo)` |
| `valorTotalInventario` | Decimal | Valor total del inventario | `SUM(precioBase * stockActual)` |
| `margenBruto` | Decimal | Margen bruto del mes | `ventasDelMes - comprasDelMes` |
| `porcentajeMargen` | Decimal | Porcentaje de margen | `(margenBruto / ventasDelMes) * 100` |

### **Ejemplo de Uso en Frontend (React)**

```jsx
// Componente de Cards
function DashboardCards() {
  const [metrics, setMetrics] = useState(null);

  useEffect(() => {
    fetch('/api/dashboard/metrics')
      .then(res => res.json())
      .then(data => setMetrics(data.data));
  }, []);

  return (
    <div className="grid grid-cols-4 gap-4">
      <Card title="Total Productos" value={metrics?.totalProductos} />
      <Card title="Ventas del Mes" value={formatCurrency(metrics?.ventasDelMes)} />
      <Card title="Stock Bajo" value={metrics?.productosStockBajo} color="red" />
      <Card title="Margen" value={`${metrics?.porcentajeMargen}%`} />
    </div>
  );
}
```

---

## 2. Top 10 Productos Más Vendidos

### **GET `/api/dashboard/top-productos-vendidos`**

**Descripción:** Obtiene los N productos más vendidos basándose en movimientos de inventario tipo `VENTA`.

**Query Parameters:**

| Parámetro | Tipo | Default | Max | Descripción |
|-----------|------|---------|-----|-------------|
| `top` | Int | 10 | 50 | Número de productos a retornar |

**Request:**
```bash
GET /api/dashboard/top-productos-vendidos?top=10
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "productoId": "guid",
      "productoNombre": "Laptop Dell XPS 15",
      "productoSku": "LAP-DELL-001",
      "cantidadVendida": 150,
      "valorTotal": 825000000.00
    },
    {
      "productoId": "guid",
      "productoNombre": "Mouse Logitech MX Master",
      "productoSku": "MOU-LOG-001",
      "cantidadVendida": 300,
      "valorTotal": 105000000.00
    }
  ],
  "message": "Top 10 productos más vendidos obtenidos correctamente."
}
```

### **Ejemplo de Gráfica (Chart.js)**

```jsx
// Bar Chart - Top Productos
function TopProductosChart({ data }) {
  const chartData = {
    labels: data.map(p => p.productoNombre),
    datasets: [{
      label: 'Cantidad Vendida',
      data: data.map(p => p.cantidadVendida),
      backgroundColor: 'rgba(54, 162, 235, 0.5)'
    }]
  };

  return <Bar data={chartData} options={{ indexAxis: 'y' }} />;
}
```

---

## 3. Tendencia de Ventas

### **GET `/api/dashboard/tendencia-ventas`**

**Descripción:** Obtiene la tendencia de ventas agrupada por día en los últimos N días.

**Query Parameters:**

| Parámetro | Tipo | Default | Max | Descripción |
|-----------|------|---------|-----|-------------|
| `dias` | Int | 30 | 365 | Número de días a analizar |

**Request:**
```bash
GET /api/dashboard/tendencia-ventas?dias=30
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "fecha": "2025-01-01T00:00:00Z",
      "totalVentas": 5500000.00,
      "cantidadFacturas": 12
    },
    {
      "fecha": "2025-01-02T00:00:00Z",
      "totalVentas": 8200000.00,
      "cantidadFacturas": 18
    }
  ],
  "message": "Tendencia de ventas de los últimos 30 días obtenida correctamente."
}
```

### **Ejemplo de Gráfica (Line Chart)**

```jsx
// Line Chart - Tendencia de Ventas
function TendenciaVentasChart({ data }) {
  const chartData = {
    labels: data.map(d => new Date(d.fecha).toLocaleDateString()),
    datasets: [{
      label: 'Ventas Diarias',
      data: data.map(d => d.totalVentas),
      borderColor: 'rgb(75, 192, 192)',
      tension: 0.1
    }]
  };

  return <Line data={chartData} />;
}
```

---

## 4. Distribución de Inventario por Categoría

### **GET `/api/dashboard/distribucion-categorias`**

**Descripción:** Obtiene la distribución del inventario agrupado por categoría.

**Request:**
```bash
GET /api/dashboard/distribucion-categorias
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "categoriaId": "guid",
      "categoriaNombre": "Electrónica",
      "cantidadProductos": 75,
      "stockTotal": 450,
      "valorTotal": 250000000.00
    },
    {
      "categoriaId": "guid",
      "categoriaNombre": "Accesorios",
      "cantidadProductos": 120,
      "stockTotal": 850,
      "valorTotal": 85000000.00
    },
    {
      "categoriaId": null,
      "categoriaNombre": "Sin Categoría",
      "cantidadProductos": 15,
      "stockTotal": 50,
      "valorTotal": 5000000.00
    }
  ],
  "message": "Distribución de inventario por categoría obtenida correctamente."
}
```

### **Ejemplo de Gráfica (Pie Chart)**

```jsx
// Pie Chart - Distribución por Categoría
function DistribucionCategoriasChart({ data }) {
  const chartData = {
    labels: data.map(c => c.categoriaNombre),
    datasets: [{
      data: data.map(c => c.valorTotal),
      backgroundColor: [
        'rgba(255, 99, 132, 0.5)',
        'rgba(54, 162, 235, 0.5)',
        'rgba(255, 206, 86, 0.5)',
        'rgba(75, 192, 192, 0.5)'
      ]
    }]
  };

  return <Pie data={chartData} />;
}
```

---

## 5. Movimientos de Stock (Entradas vs Salidas)

### **GET `/api/dashboard/movimientos-stock`**

**Descripción:** Obtiene los movimientos de stock (entradas y salidas) agrupados por día.

**Query Parameters:**

| Parámetro | Tipo | Default | Max | Descripción |
|-----------|------|---------|-----|-------------|
| `dias` | Int | 30 | 365 | Número de días a analizar |

**Request:**
```bash
GET /api/dashboard/movimientos-stock?dias=30
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "fecha": "2025-01-01T00:00:00Z",
      "entradas": 150,
      "salidas": 85,
      "neto": 65
    },
    {
      "fecha": "2025-01-02T00:00:00Z",
      "entradas": 200,
      "salidas": 120,
      "neto": 80
    }
  ],
  "message": "Movimientos de stock de los últimos 30 días obtenidos correctamente."
}
```

**Campos:**
- `entradas`: Movimientos tipo `COMPRA` con cantidad positiva
- `salidas`: Movimientos tipo `VENTA` con cantidad positiva
- `neto`: Diferencia entre entradas y salidas

### **Ejemplo de Gráfica (Stacked Bar Chart)**

```jsx
// Stacked Bar Chart - Movimientos
function MovimientosStockChart({ data }) {
  const chartData = {
    labels: data.map(d => new Date(d.fecha).toLocaleDateString()),
    datasets: [
      {
        label: 'Entradas',
        data: data.map(d => d.entradas),
        backgroundColor: 'rgba(75, 192, 192, 0.5)'
      },
      {
        label: 'Salidas',
        data: data.map(d => d.salidas),
        backgroundColor: 'rgba(255, 99, 132, 0.5)'
      }
    ]
  };

  return <Bar data={chartData} options={{ scales: { x: { stacked: true }, y: { stacked: true } } }} />;
}
```

---

## 6. Comparación Stock por Bodega

### **GET `/api/dashboard/stock-por-bodega`**

**Descripción:** Obtiene la comparación de stock y valor por bodega.

**Request:**
```bash
GET /api/dashboard/stock-por-bodega
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "bodegaId": "guid",
      "bodegaNombre": "Bodega Principal",
      "cantidadProductos": 180,
      "stockTotal": 1250,
      "valorTotal": 350000000.00
    },
    {
      "bodegaId": "guid",
      "bodegaNombre": "Bodega Norte",
      "cantidadProductos": 95,
      "stockTotal": 580,
      "valorTotal": 125000000.00
    }
  ],
  "message": "Comparación de stock por bodega obtenida correctamente."
}
```

### **Ejemplo de Gráfica (Grouped Bar Chart)**

```jsx
// Grouped Bar Chart - Stock por Bodega
function StockPorBodegaChart({ data }) {
  const chartData = {
    labels: data.map(b => b.bodegaNombre),
    datasets: [
      {
        label: 'Stock Total',
        data: data.map(b => b.stockTotal),
        backgroundColor: 'rgba(54, 162, 235, 0.5)'
      },
      {
        label: 'Valor Total (Millones)',
        data: data.map(b => b.valorTotal / 1000000),
        backgroundColor: 'rgba(255, 206, 86, 0.5)'
      }
    ]
  };

  return <Bar data={chartData} />;
}
```

---

## 7. Gauge de Salud del Stock

### **GET `/api/dashboard/salud-stock`**

**Descripción:** Obtiene el gauge de salud del stock (porcentaje de productos en estado óptimo).

**Request:**
```bash
GET /api/dashboard/salud-stock
```

**Response:**
```json
{
  "success": true,
  "data": {
    "productosStockOptimo": 180,
    "productosStockBajo": 25,
    "productosStockAlto": 10,
    "productosAgotados": 5,
    "totalProductos": 220,
    "porcentajeStockOptimo": 81.82,
    "porcentajeStockBajo": 11.36,
    "porcentajeStockAlto": 4.55,
    "porcentajeAgotados": 2.27
  },
  "message": "Salud del stock obtenida correctamente."
}
```

**Definiciones:**

| Estado | Definición |
|--------|------------|
| **Stock Óptimo** | Stock entre `cantidadMinima` y `cantidadMaxima` |
| **Stock Bajo** | Stock < `cantidadMinima` |
| **Stock Alto** | Stock > `cantidadMaxima` |
| **Agotados** | Stock = 0 |

### **Ejemplo de Gráfica (Gauge/Doughnut Chart)**

```jsx
// Gauge Chart - Salud del Stock
function SaludStockGauge({ data }) {
  const chartData = {
    labels: ['Óptimo', 'Bajo', 'Alto', 'Agotados'],
    datasets: [{
      data: [
        data.productosStockOptimo,
        data.productosStockBajo,
        data.productosStockAlto,
        data.productosAgotados
      ],
      backgroundColor: [
        'rgba(75, 192, 192, 0.5)',   // Verde - Óptimo
        'rgba(255, 206, 86, 0.5)',   // Amarillo - Bajo
        'rgba(255, 159, 64, 0.5)',   // Naranja - Alto
        'rgba(255, 99, 132, 0.5)'    // Rojo - Agotados
      ]
    }]
  };

  return (
    <div>
      <Doughnut data={chartData} />
      <div className="text-center mt-4">
        <h3>{data.porcentajeStockOptimo.toFixed(1)}%</h3>
        <p>Stock Óptimo</p>
      </div>
    </div>
  );
}
```

---

## 8. Productos con Stock Bajo

### **GET `/api/dashboard/productos-stock-bajo`**

**Descripción:** Obtiene una lista de productos con stock por debajo del mínimo.

**Query Parameters:**

| Parámetro | Tipo | Default | Max | Descripción |
|-----------|------|---------|-----|-------------|
| `top` | Int | 20 | 100 | Número de productos a retornar |

**Request:**
```bash
GET /api/dashboard/productos-stock-bajo?top=20
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "productoId": "guid",
      "productoNombre": "Laptop Dell XPS 15",
      "productoSku": "LAP-DELL-001",
      "stockActual": 5,
      "stockMinimo": 10,
      "diferencia": -5,
      "bodegaPrincipal": "Bodega Principal"
    },
    {
      "productoId": "guid",
      "productoNombre": "Mouse Logitech",
      "productoSku": "MOU-LOG-001",
      "stockActual": 2,
      "stockMinimo": 20,
      "diferencia": -18,
      "bodegaPrincipal": "Bodega Norte"
    }
  ],
  "message": "Top 20 productos con stock bajo obtenidos correctamente."
}
```

**Nota:** Los productos están ordenados por `diferencia` ascendente (los más críticos primero).

### **Ejemplo de Tabla**

```jsx
// Table - Productos Stock Bajo
function ProductosStockBajoTable({ data }) {
  return (
    <table>
      <thead>
        <tr>
          <th>Producto</th>
          <th>SKU</th>
          <th>Stock Actual</th>
          <th>Stock Mínimo</th>
          <th>Diferencia</th>
          <th>Bodega</th>
        </tr>
      </thead>
      <tbody>
        {data.map(p => (
          <tr key={p.productoId} className={p.diferencia < -10 ? 'text-red-600' : 'text-yellow-600'}>
            <td>{p.productoNombre}</td>
            <td>{p.productoSku}</td>
            <td>{p.stockActual}</td>
            <td>{p.stockMinimo}</td>
            <td>{p.diferencia}</td>
            <td>{p.bodegaPrincipal}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
```

---

## Ejemplos de Uso

### **1. Dashboard Completo**

```jsx
function Dashboard() {
  const [metrics, setMetrics] = useState(null);
  const [topProductos, setTopProductos] = useState([]);
  const [tendenciaVentas, setTendenciaVentas] = useState([]);
  const [distribucionCategorias, setDistribucionCategorias] = useState([]);

  useEffect(() => {
    // Cargar todas las métricas en paralelo
    Promise.all([
      fetch('/api/dashboard/metrics').then(r => r.json()),
      fetch('/api/dashboard/top-productos-vendidos').then(r => r.json()),
      fetch('/api/dashboard/tendencia-ventas').then(r => r.json()),
      fetch('/api/dashboard/distribucion-categorias').then(r => r.json())
    ]).then(([metrics, top, tendencia, distribucion]) => {
      setMetrics(metrics.data);
      setTopProductos(top.data);
      setTendenciaVentas(tendencia.data);
      setDistribucionCategorias(distribucion.data);
    });
  }, []);

  return (
    <div className="dashboard">
      <MetricsCards metrics={metrics} />
      <div className="charts-grid">
        <TendenciaVentasChart data={tendenciaVentas} />
        <TopProductosChart data={topProductos} />
        <DistribucionCategoriasChart data={distribucionCategorias} />
      </div>
    </div>
  );
}
```

---

### **2. Refresh Automático**

```jsx
function DashboardWithAutoRefresh() {
  const [metrics, setMetrics] = useState(null);

  useEffect(() => {
    const loadMetrics = () => {
      fetch('/api/dashboard/metrics')
        .then(r => r.json())
        .then(data => setMetrics(data.data));
    };

    loadMetrics();
    
    // Refresh cada 5 minutos
    const interval = setInterval(loadMetrics, 5 * 60 * 1000);
    
    return () => clearInterval(interval);
  }, []);

  return <MetricsCards metrics={metrics} />;
}
```

---

## Integración con Frontend

### **Librerías Recomendadas**

1. **Chart.js** - Gráficas simples y rápidas
   ```bash
   npm install chart.js react-chartjs-2
   ```

2. **Recharts** - Gráficas React nativas
   ```bash
   npm install recharts
   ```

3. **ApexCharts** - Gráficas avanzadas e interactivas
   ```bash
   npm install apexcharts react-apexcharts
   ```

### **Ejemplo Completo con Recharts**

```jsx
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from 'recharts';

function TopProductosRecharts() {
  const [data, setData] = useState([]);

  useEffect(() => {
    fetch('/api/dashboard/top-productos-vendidos')
      .then(r => r.json())
      .then(res => setData(res.data));
  }, []);

  return (
    <BarChart width={600} height={300} data={data}>
      <CartesianGrid strokeDasharray="3 3" />
      <XAxis dataKey="productoNombre" />
      <YAxis />
      <Tooltip />
      <Legend />
      <Bar dataKey="cantidadVendida" fill="#8884d8" />
    </BarChart>
  );
}
```

---

## ?? Resumen de Endpoints

| Endpoint | Método | Descripción | Tipo de Gráfica |
|----------|--------|-------------|-----------------|
| `/api/dashboard/metrics` | GET | Métricas principales | Cards/KPIs |
| `/api/dashboard/top-productos-vendidos` | GET | Top productos vendidos | Bar Chart |
| `/api/dashboard/tendencia-ventas` | GET | Tendencia de ventas | Line Chart |
| `/api/dashboard/distribucion-categorias` | GET | Distribución por categoría | Pie Chart |
| `/api/dashboard/movimientos-stock` | GET | Entradas vs Salidas | Stacked Bar Chart |
| `/api/dashboard/stock-por-bodega` | GET | Comparación por bodega | Grouped Bar Chart |
| `/api/dashboard/salud-stock` | GET | Salud del stock | Gauge/Doughnut Chart |
| `/api/dashboard/productos-stock-bajo` | GET | Productos con stock bajo | Table |

---

## ? Performance y Optimización

### **Caching**

Los endpoints de dashboard son **ideales para caching**:

```csharp
// En Program.cs
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

// En el controller
[ResponseCache(Duration = 300)] // 5 minutos
public async Task<ActionResult> GetMetrics()
{
    // ...
}
```

### **Refresh Recomendado**

| Endpoint | Frecuencia Recomendada |
|----------|----------------------|
| Métricas | 5 minutos |
| Top Productos | 30 minutos |
| Tendencia Ventas | 1 hora |
| Distribución Categorías | 1 hora |
| Movimientos Stock | 15 minutos |
| Stock por Bodega | 30 minutos |
| Salud Stock | 30 minutos |
| Stock Bajo | 15 minutos |

---

## ?? Conclusión

Este módulo de Dashboard proporciona **todos los datos necesarios** para construir visualizaciones ricas e interactivas en el frontend, con:

? **Datos agregados y listos para usar**  
? **Cálculos complejos en backend**  
? **Performance optimizado**  
? **Respuestas estructuradas**  
? **Fácil integración con librerías de gráficas**  

**¡El frontend solo necesita consumir y renderizar!** ????
