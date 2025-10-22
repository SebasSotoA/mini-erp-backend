-- WARNING: This schema is for context only and is not meant to be run.
-- Table order and constraints may not be valid for execution.

CREATE TABLE public.Bodegas (
  Id uuid NOT NULL,
  Nombre character varying NOT NULL,
  Descripcion text,
  Activo boolean NOT NULL,
  FechaCreacion timestamp with time zone NOT NULL,
  CONSTRAINT Bodegas_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.CamposExtra (
  Id uuid NOT NULL,
  Nombre character varying NOT NULL,
  TipoDato character varying NOT NULL,
  EsRequerido boolean NOT NULL,
  ValorPorDefecto text,
  Activo boolean NOT NULL,
  FechaCreacion timestamp with time zone NOT NULL,
  CONSTRAINT CamposExtra_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.Categorias (
  Id uuid NOT NULL,
  Nombre character varying NOT NULL,
  Descripcion text,
  Activo boolean NOT NULL,
  FechaCreacion timestamp with time zone NOT NULL,
  ImagenCategoriaUrl text,
  CONSTRAINT Categorias_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.FacturasCompra (
  Id uuid NOT NULL,
  NumeroFactura character varying NOT NULL,
  BodegaId uuid NOT NULL,
  ProveedorId uuid NOT NULL,
  Fecha timestamp with time zone NOT NULL,
  Observaciones text,
  Estado character varying NOT NULL,
  Total numeric NOT NULL,
  CONSTRAINT FacturasCompra_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_FacturasCompra_Bodegas_BodegaId FOREIGN KEY (BodegaId) REFERENCES public.Bodegas(Id),
  CONSTRAINT FK_FacturasCompra_Proveedores_ProveedorId FOREIGN KEY (ProveedorId) REFERENCES public.Proveedores(Id)
);
CREATE TABLE public.FacturasCompraDetalle (
  Id uuid NOT NULL,
  FacturaCompraId uuid NOT NULL,
  ProductoId uuid NOT NULL,
  CostoUnitario numeric NOT NULL,
  Descuento numeric,
  Impuesto numeric,
  Cantidad integer NOT NULL,
  TotalLinea numeric NOT NULL,
  CONSTRAINT FacturasCompraDetalle_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_FacturasCompraDetalle_FacturasCompra_FacturaCompraId FOREIGN KEY (FacturaCompraId) REFERENCES public.FacturasCompra(Id),
  CONSTRAINT FK_FacturasCompraDetalle_Productos_ProductoId FOREIGN KEY (ProductoId) REFERENCES public.Productos(Id)
);
CREATE TABLE public.FacturasVenta (
  Id uuid NOT NULL,
  NumeroFactura character varying NOT NULL,
  BodegaId uuid NOT NULL,
  VendedorId uuid NOT NULL,
  Fecha timestamp with time zone NOT NULL,
  FormaPago character varying NOT NULL,
  PlazoPago integer,
  MedioPago character varying NOT NULL,
  Observaciones text,
  Estado character varying NOT NULL,
  Total numeric NOT NULL,
  CONSTRAINT FacturasVenta_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_FacturasVenta_Bodegas_BodegaId FOREIGN KEY (BodegaId) REFERENCES public.Bodegas(Id),
  CONSTRAINT FK_FacturasVenta_Vendedores_VendedorId FOREIGN KEY (VendedorId) REFERENCES public.Vendedores(Id)
);
CREATE TABLE public.FacturasVentaDetalle (
  Id uuid NOT NULL,
  FacturaVentaId uuid NOT NULL,
  ProductoId uuid NOT NULL,
  PrecioUnitario numeric NOT NULL,
  Descuento numeric,
  Impuesto numeric,
  Cantidad integer NOT NULL,
  TotalLinea numeric NOT NULL,
  CONSTRAINT FacturasVentaDetalle_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_FacturasVentaDetalle_FacturasVenta_FacturaVentaId FOREIGN KEY (FacturaVentaId) REFERENCES public.FacturasVenta(Id),
  CONSTRAINT FK_FacturasVentaDetalle_Productos_ProductoId FOREIGN KEY (ProductoId) REFERENCES public.Productos(Id)
);
CREATE TABLE public.MovimientosInventario (
  Id uuid NOT NULL,
  ProductoId uuid NOT NULL,
  BodegaId uuid NOT NULL,
  Fecha timestamp with time zone NOT NULL,
  TipoMovimiento character varying NOT NULL,
  Cantidad integer NOT NULL,
  CostoUnitario numeric,
  PrecioUnitario numeric,
  Observacion text,
  FacturaId uuid,
  CONSTRAINT MovimientosInventario_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_MovimientosInventario_Bodegas_BodegaId FOREIGN KEY (BodegaId) REFERENCES public.Bodegas(Id),
  CONSTRAINT FK_MovimientosInventario_Productos_ProductoId FOREIGN KEY (ProductoId) REFERENCES public.Productos(Id)
);
CREATE TABLE public.ProductoBodegas (
  Id uuid NOT NULL,
  ProductoId uuid NOT NULL,
  BodegaId uuid NOT NULL,
  CantidadInicial integer NOT NULL,
  CantidadMinima integer,
  CantidadMaxima integer,
  CONSTRAINT ProductoBodegas_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProductoBodegas_Bodegas_BodegaId FOREIGN KEY (BodegaId) REFERENCES public.Bodegas(Id),
  CONSTRAINT FK_ProductoBodegas_Productos_ProductoId FOREIGN KEY (ProductoId) REFERENCES public.Productos(Id)
);
CREATE TABLE public.ProductoCamposExtra (
  Id uuid NOT NULL,
  ProductoId uuid NOT NULL,
  CampoExtraId uuid NOT NULL,
  Valor text NOT NULL,
  CONSTRAINT ProductoCamposExtra_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_ProductoCamposExtra_CamposExtra_CampoExtraId FOREIGN KEY (CampoExtraId) REFERENCES public.CamposExtra(Id),
  CONSTRAINT FK_ProductoCamposExtra_Productos_ProductoId FOREIGN KEY (ProductoId) REFERENCES public.Productos(Id)
);
CREATE TABLE public.Productos (
  Id uuid NOT NULL,
  Nombre character varying NOT NULL,
  UnidadMedida character varying NOT NULL,
  PrecioBase numeric NOT NULL,
  ImpuestoPorcentaje numeric,
  CostoInicial numeric NOT NULL,
  CategoriaId uuid,
  CodigoSku text,
  Descripcion text,
  Activo boolean NOT NULL,
  FechaCreacion timestamp with time zone NOT NULL,
  ImagenProductoUrl text,
  CONSTRAINT Productos_pkey PRIMARY KEY (Id),
  CONSTRAINT FK_Productos_Categorias_CategoriaId FOREIGN KEY (CategoriaId) REFERENCES public.Categorias(Id)
);
CREATE TABLE public.Proveedores (
  Id uuid NOT NULL,
  Nombre character varying NOT NULL,
  Identificacion character varying NOT NULL,
  Correo text,
  Observaciones text,
  CONSTRAINT Proveedores_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.Vendedores (
  Id uuid NOT NULL,
  Nombre character varying NOT NULL,
  Identificacion character varying NOT NULL,
  Observaciones text,
  Correo text,
  CONSTRAINT Vendedores_pkey PRIMARY KEY (Id)
);
CREATE TABLE public.__EFMigrationsHistory (
  MigrationId character varying NOT NULL,
  ProductVersion character varying NOT NULL,
  CONSTRAINT __EFMigrationsHistory_pkey PRIMARY KEY (MigrationId)
);