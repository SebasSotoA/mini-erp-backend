# CONTEXT
You are an expert backend developer assistant. Build a clean, production-ready ASP.NET Core 9 Web API following Clean Architecture. The project uses EF Core with PostgreSQL (Supabase). The solution already has Entities (Domain) and InventoryDbContext configured. Use Repository + UnitOfWork patterns, DTOs, AutoMapper, FluentValidation, and async/await everywhere. Avoid heavy frameworks; keep code simple, testable, and idiomatic C#.

# GLOBAL RULES
- Language: C# / .NET 9.0 style.
- Projects: InventoryBack.API, InventoryBack.Application, InventoryBack.Domain, InventoryBack.Infrastructure.
- Namespaces should follow: InventoryBack.{Layer}.{Feature} (e.g., InventoryBack.Domain.Entities, InventoryBack.Application.Services).
- All DB interactions through repositories/UoW (no DbContext calls in controllers).
- Use DTOs for API input/output. Map with AutoMapper.
- Implement async methods only (Task/Task<T>).
- Add XML docs to public classes/methods where helpful.
- Provide small, focused code files (one class per file).
- Provide unit-test hooks: interfaces, small methods, avoid static state.
- Keep migrations intact: do not drop or recreate DB.
- Do not include secrets in code. Use configuration (User Secrets / env vars).

# PHASE 0 — VERIFY PROJECT
1. Confirm these exist and compile: Domain/Entities (all entities), Infrastructure/Data/InventoryDbContext.cs, API/Program.cs (register DbContext).
2. If missing, create minimal InventoryDbContext with DbSet properties for all entities.

# PHASE 1 — GENERIC REPOSITORY
Task A: Create IGenericRepository<T> in Application.Interfaces.
- Methods:
  - Task<T?> GetByIdAsync(Guid id);
  - Task<IEnumerable<T>> ListAsync(Expression<Func<T,bool>>? filter = null, CancellationToken ct = default);
  - Task AddAsync(T entity, CancellationToken ct = default);
  - void Update(T entity);
  - void Remove(T entity);
  - Task<int> CountAsync(Expression<Func<T,bool>>? filter = null, CancellationToken ct = default);

Task B: Implement EfGenericRepository<T> in Infrastructure/Repositories.
- Use protected readonly InventoryDbContext _db; protected readonly DbSet<T> _dbSet.
- Implement methods with EF Core best practices (AsNoTracking for reads where appropriate).
- Throw ArgumentNullException for null args.

# PHASE 2 — SCOPED REPOSITORIES (domain-specific)
For entities that need queries (Product, ProductoBodega, FacturaVenta, FacturaCompra):
- Define domain-specific interfaces (IProductRepository, IInvoiceRepository) in Application.Interfaces with methods (e.g., GetBySkuAsync, GetPagedAsync).
- Implement in Infrastructure/Repositories extending EfGenericRepository<T>.

# PHASE 3 — UNIT OF WORK
Task: Create IUnitOfWork in Application.Interfaces exposing:
- IProductRepository Products { get; }
- IGenericRepository<Categoria> Categories { get; } (and other repos)
- Task<int> SaveChangesAsync(CancellationToken ct = default);
- Task<IDisposable> BeginTransactionAsync(); // or Task<IDbContextTransaction> BeginTransactionAsync() if using EF transactions

Implement UnitOfWork in Infrastructure/UnitOfWork:
- Constructor inject InventoryDbContext and concrete repositories.
- SaveChangesAsync calls _db.SaveChangesAsync(ct).

# PHASE 4 — SERVICES / USE CASES
For each aggregate (Products, Invoices):
- Create Application.Services.IProductService with:
  - Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken ct);
  - Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct);
  - Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, string? q, CancellationToken ct);
  - Task UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct);
  - Task DeleteAsync(Guid id, CancellationToken ct);

- Implement ProductService using IUnitOfWork.
- Business rules:
  - Validate SKU uniqueness before creating.
  - Set FechaCreacion automatically.
  - Use transactions when multiple repos updated (BeginTransactionAsync / Commit).

# PHASE 5 — DTOs, AutoMapper, Validation
- Create DTOs in Application.DTOs: CreateProductDto, UpdateProductDto, ProductDto, PagedResult<T>.
- Create AutoMapper Profile in Application/Mapping: map entity <-> DTOs, ignore DB-only fields when appropriate.
- Add FluentValidation validators for Create/Update DTOs and register in DI.

# PHASE 6 — API CONTROLLERS
- Create ProductsController in API/Controllers:
  - Use [ApiController], route "api/products".
  - Endpoints: GET /, GET /{id}, POST /, PUT /{id}, DELETE /{id}.
  - Use IProductService injected.
  - Return ActionResult<T>, proper status codes (201 on create, 204 on delete).
  - Use ModelState and return ProblemDetails on validation errors (global validation middleware optional).

# PHASE 7 — DI & Program.cs
- In API Program.cs register:
  - builder.Services.AddDbContext<InventoryDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
  - builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));
  - builder.Services.AddScoped<IProductRepository, ProductRepository>();
  - builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
  - builder.Services.AddScoped<IProductService, ProductService>();
  - builder.Services.AddAutoMapper(typeof(MappingProfile));
  - Register FluentValidation validators.
  - Add exception handling middleware (global), logging, and swagger.

# PHASE 8 — MIGRATIONS & DB
- Do NOT modify existing DB schema.
- After implementing baseline service/repository code, run:
  - dotnet ef migrations add Feature_ProductService (only if schema changes)
  - dotnet ef database update
- Use User Secrets for connection string locally.

# PHASE 9 — TESTS
- Add Application.Tests and Infrastructure.Tests projects.
- Mock IUnitOfWork and repositories for service tests.
- Use EF Core InMemory or SQLite for integration tests with Infrastructure.

# PHASE 10 — CI / CD (brief)
- Add GitHub Actions workflow:
  - Steps: checkout, setup dotnet, restore, build, test, run migrations (if desired in staging), publish.
- Store production connection secrets in GitHub Secrets or cloud secret manager.

# CODE STYLE & CONVENTIONS
- Use cancellation tokens in public async methods.
- Exceptions: throw domain-specific exceptions (e.g., NotFoundException) from services; middleware maps to HTTP codes.
- Keep controllers thin—no business logic.
- Methods: small and single responsibility.

# DELIVERABLES (per iteration)
For the first iteration, produce these files (one per file) with full code:
1. Application/Interfaces/IGenericRepository.cs
2. Infrastructure/Repositories/EfGenericRepository.cs
3. Application/Interfaces/IUnitOfWork.cs
4. Infrastructure/UnitOfWork/UnitOfWork.cs
5. Application/Interfaces/IProductRepository.cs
6. Infrastructure/Repositories/ProductRepository.cs
7. Application/Services/IProductService.cs
8. Application/Services/ProductService.cs
9. Application/DTOs/CreateProductDto.cs, UpdateProductDto.cs, ProductDto.cs, PagedResult.cs
10. Application/Mapping/MappingProfile.cs
11. API/Controllers/ProductsController.cs
12. Program.cs DI block snippet (ready to paste)

# TEST CASES / VALIDATION (for you to run)
- Create product with unique SKU ? returns 201.
- Create product with duplicate SKU ? service returns 400 with explanation.
- Get product list with paging and search ? returns results and total.
- Update product fields ? persisted.
- Delete product referenced in invoice ? service must prevent if business rule disallows.

# FINAL NOTES TO AGENT
- Provide only the files requested in each iteration; do not scaffold unrelated features.
- Keep code compilable; include necessary using statements.
- Show brief instructions to wire up DI and how to run migrations locally.
- After delivering iteration 1, wait for further prompts to implement invoices, purchases, and stock flows.
- Reports in the next sprints will be handled later.