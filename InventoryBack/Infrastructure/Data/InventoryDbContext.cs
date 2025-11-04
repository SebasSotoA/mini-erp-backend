using InventoryBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryBack.Infrastructure.Data
{
    public class InventoryDbContext: DbContext
    {
        public InventoryDbContext(DbContextOptions dbContextOptions): base(dbContextOptions)
        {

        }
        // --- Tablas Principales ---
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Bodega> Bodegas { get; set; }
        public DbSet<ProductoBodega> ProductoBodegas => Set<ProductoBodega>();
        public DbSet<CampoExtra> CamposExtra { get; set; }
        public DbSet<ProductoCampoExtra> ProductoCamposExtra => Set<ProductoCampoExtra>();

        // --- Módulo de Ventas ---
        public DbSet<FacturaVenta> FacturasVenta => Set<FacturaVenta>();
        public DbSet<FacturaVentaDetalle> FacturasVentaDetalle => Set<FacturaVentaDetalle>();

        // --- Módulo de Compras ---
        public DbSet<FacturaCompra> FacturasCompra => Set<FacturaCompra>();
        public DbSet<FacturaCompraDetalle> FacturasCompraDetalle => Set<FacturaCompraDetalle>();

        // --- Entidades de Soporte ---
        public DbSet<Vendedor> Vendedores => Set<Vendedor>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();

        // --- Movimientos de Inventario ---
        public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === PRODUCTOS ===
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Nombre)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(p => p.UnidadMedida)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(p => p.PrecioBase)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.ImpuestoPorcentaje)
                      .HasColumnType("decimal(5,2)");

                entity.Property(p => p.CostoInicial)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Activo)
                      .IsRequired();

                entity.Property(p => p.FechaCreacion)
                      .IsRequired();

                entity.HasIndex(p => p.CodigoSku)
                      .IsUnique();

                entity.HasOne<Categoria>()
                      .WithMany()
                      .HasForeignKey(p => p.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === CATEGORIAS ===
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre)
                      .IsRequired()
                      .HasMaxLength(150);
                entity.Property(c => c.Descripcion)
                      .HasMaxLength(1000);
                entity.Property(c => c.ImagenCategoriaUrl)
                      .HasMaxLength(500);
                entity.Property(c => c.Activo).IsRequired();
                entity.Property(c => c.FechaCreacion).IsRequired();
            });

            // === BODEGAS ===
            modelBuilder.Entity<Bodega>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Nombre)
                      .IsRequired()
                      .HasMaxLength(150);
                entity.Property(b => b.Direccion)
                      .HasMaxLength(500);
                entity.Property(b => b.Descripcion)
                      .HasMaxLength(1000);
                entity.Property(b => b.Activo).IsRequired();
                entity.Property(b => b.FechaCreacion).IsRequired();
            });

            // === PRODUCTO BODEGA ===
            modelBuilder.Entity<ProductoBodega>(entity =>
            {
                entity.HasKey(pb => pb.Id);

                // Índice único compuesto para prevenir duplicados
                entity.HasIndex(pb => new { pb.ProductoId, pb.BodegaId })
                      .IsUnique()
                      .HasDatabaseName("IX_ProductoBodega_Producto_Bodega");

                entity.HasOne<Producto>()
                      .WithMany()
                      .HasForeignKey(pb => pb.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Bodega>()
                      .WithMany()
                      .HasForeignKey(pb => pb.BodegaId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // === CAMPOS EXTRA ===
            modelBuilder.Entity<CampoExtra>(entity =>
            {
                entity.HasKey(ce => ce.Id);
                entity.Property(ce => ce.Nombre)
                      .IsRequired()
                      .HasMaxLength(150);
                entity.Property(ce => ce.TipoDato)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(ce => ce.EsRequerido).IsRequired();
                entity.Property(ce => ce.Activo).IsRequired();
                entity.Property(ce => ce.FechaCreacion).IsRequired();
                entity.Property(ce => ce.Descripcion)
                      .HasMaxLength(1000);
            });

            // === PRODUCTO CAMPO EXTRA ===
            modelBuilder.Entity<ProductoCampoExtra>(entity =>
            {
                entity.HasKey(pce => pce.Id);

                entity.HasOne<Producto>()
                      .WithMany()
                      .HasForeignKey(pce => pce.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<CampoExtra>()
                      .WithMany()
                      .HasForeignKey(pce => pce.CampoExtraId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // === PROVEEDORES ===
            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Identificacion).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Correo).HasMaxLength(150);
                entity.Property(p => p.Observaciones).HasMaxLength(1000);
                entity.Property(p => p.Activo).IsRequired();
                entity.Property(p => p.FechaCreacion).IsRequired();
                entity.HasIndex(p => p.Identificacion).IsUnique();
            });

            // === VENDEDORES ===
            modelBuilder.Entity<Vendedor>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(v => v.Identificacion).IsRequired().HasMaxLength(100);
                entity.Property(v => v.Correo).HasMaxLength(150);
                entity.Property(v => v.Observaciones).HasMaxLength(1000);
                entity.Property(v => v.Activo).IsRequired();
                entity.Property(v => v.FechaCreacion).IsRequired();
                entity.HasIndex(v => v.Identificacion).IsUnique();
            });

            // === FACTURAS DE VENTA ===
            modelBuilder.Entity<FacturaVenta>(entity =>
            {
                entity.HasKey(fv => fv.Id);
                entity.Property(fv => fv.NumeroFactura)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.HasIndex(fv => fv.NumeroFactura)
                      .IsUnique();
                entity.Property(fv => fv.FormaPago).IsRequired().HasMaxLength(50);
                entity.Property(fv => fv.MedioPago).IsRequired().HasMaxLength(50);
                entity.Property(fv => fv.Estado).IsRequired().HasMaxLength(50);
                entity.Property(fv => fv.Total)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.HasOne<Bodega>()
                      .WithMany()
                      .HasForeignKey(fv => fv.BodegaId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Vendedor>()
                      .WithMany()
                      .HasForeignKey(fv => fv.VendedorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === FACTURA VENTA DETALLE ===
            modelBuilder.Entity<FacturaVentaDetalle>(entity =>
            {
                entity.HasKey(fvd => fvd.Id);

                entity.Property(fvd => fvd.PrecioUnitario)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(fvd => fvd.TotalLinea)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.HasOne<FacturaVenta>()
                      .WithMany()
                      .HasForeignKey(fvd => fvd.FacturaVentaId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Producto>()
                      .WithMany()
                      .HasForeignKey(fvd => fvd.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === FACTURAS DE COMPRA ===
            modelBuilder.Entity<FacturaCompra>(entity =>
            {
                entity.HasKey(fc => fc.Id);
                entity.Property(fc => fc.NumeroFactura)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.HasIndex(fc => fc.NumeroFactura)
                      .IsUnique();
                entity.Property(fc => fc.Estado).IsRequired().HasMaxLength(50);
                entity.Property(fc => fc.Total)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");
                entity.HasOne<Bodega>()
                      .WithMany()
                      .HasForeignKey(fc => fc.BodegaId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<Proveedor>()
                      .WithMany()
                      .HasForeignKey(fc => fc.ProveedorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === FACTURA COMPRA DETALLE ===
            modelBuilder.Entity<FacturaCompraDetalle>(entity =>
            {
                entity.HasKey(fcd => fcd.Id);

                entity.Property(fcd => fcd.CostoUnitario)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.Property(fcd => fcd.TotalLinea)
                      .IsRequired()
                      .HasColumnType("decimal(18,2)");

                entity.HasOne<FacturaCompra>()
                      .WithMany()
                      .HasForeignKey(fcd => fcd.FacturaCompraId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Producto>()
                      .WithMany()
                      .HasForeignKey(fcd => fcd.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === MOVIMIENTOS INVENTARIO ===
            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.HasKey(mi => mi.Id);

                entity.Property(mi => mi.TipoMovimiento)
                      .IsRequired()
                      .HasMaxLength(50);

                // Configurar relaciones con navigation properties explícitas
                entity.HasOne(mi => mi.Producto)
                      .WithMany()
                      .HasForeignKey(mi => mi.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(mi => mi.Bodega)
                      .WithMany()
                      .HasForeignKey(mi => mi.BodegaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(mi => mi.FacturaVenta)
                      .WithMany()
                      .HasForeignKey(mi => mi.FacturaVentaId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                entity.HasOne(mi => mi.FacturaCompra)
                      .WithMany()
                      .HasForeignKey(mi => mi.FacturaCompraId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                // Índices para mejorar rendimiento en consultas frecuentes
                entity.HasIndex(mi => mi.Fecha)
                      .HasDatabaseName("IX_MovimientosInventario_Fecha");

                entity.HasIndex(mi => mi.TipoMovimiento)
                      .HasDatabaseName("IX_MovimientosInventario_TipoMovimiento");

                entity.HasIndex(mi => mi.ProductoId)
                      .HasDatabaseName("IX_MovimientosInventario_ProductoId");

                entity.HasIndex(mi => mi.BodegaId)
                      .HasDatabaseName("IX_MovimientosInventario_BodegaId");
            });
        }

    }
}
