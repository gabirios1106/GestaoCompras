using GestaoCompras.Models.Access;
using GestaoCompras.Models.Orders;
using GestaoCompras.Models.Fruits;
using GestaoCompras.Models.Suppliers;
using GestaoCompras.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestaoCompras.Models.Stores;

namespace GestaoCompras.API.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly IConfiguration _configuration;

    public ApplicationDbContext() { }

    #region DBSets

    #region Access
    public DbSet<ApplicationUser> ApplicationUser { get; set; }
    #endregion Access

    #region Fruit
    public DbSet<Fruit> Fruit { get; set; }
    #endregion Fruit

    #region Order
    public DbSet<Order> Order { get; set; }
    #endregion Order

    #region Store
    public DbSet<Store> Store { get; set; }
    #endregion Store

    #region Supplier
    public DbSet<Supplier> Supplier { get; set; }
    #endregion Supplier

    #region Users
    public DbSet<UserData> UserData { get; set; }
    #endregion Users

    #endregion DBSets

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    // EnableSensitiveDataLogging habilitado para mostrar valores nos logs
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        bool isProduction = _configuration.GetValue<bool>("IsProduction");

        if (!isProduction)
            optionsBuilder.EnableSensitiveDataLogging();

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        #region Identity
        builder.Entity<User>(b =>
        {
            b.HasOne(u => u.UserData).WithOne(ud => ud.User).HasForeignKey<UserData>(ud => ud.UserId).OnDelete(DeleteBehavior.NoAction);

            b.ToTable("User");
        });

        builder.Entity<IdentityRole<Guid>>().ToTable("Profile");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaim");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaim");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogin");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRole");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserToken");
        #endregion Identity

        #region Access
        builder.Entity<ApplicationUser>(b =>
        {
            b.HasKey(au => au.AppId);

            b.Property(au => au.AppName).IsRequired().HasMaxLength(250);
            b.Property(au => au.AppPasswordHash).IsRequired().HasMaxLength(250);

            b.Property(au => au.CreatedAt).IsRequired();
            b.Property(au => au.Status).IsRequired();
        });
        #endregion Access

        #region Fruit
        builder.Entity<Fruit>(b =>
        {
            b.HasKey(f => f.Id);
            b.Property(f => f.Id).ValueGeneratedOnAdd();

            b.Property(f => f.Name).IsRequired().HasMaxLength(250);
            b.Property(f => f.Price).IsRequired();
            b.Property(f => f.CreatedAt).IsRequired();

            b.HasOne(f => f.UserData).WithMany().HasForeignKey(f => f.UserDataId).OnDelete(DeleteBehavior.NoAction);

        });
        #endregion Fruit

        #region Store
        builder.Entity<Store>(s =>
        {
            s.HasKey(s => s.Id);
            s.Property(s => s.Id).ValueGeneratedOnAdd();

            s.Property(s => s.Name).IsRequired().HasMaxLength(250);
        });
        #endregion Store

        #region Order
        builder.Entity<Order>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).ValueGeneratedOnAdd();

            b.Property(o => o.BackLoad).IsRequired();
            b.Property(o => o.MiddleLoad).IsRequired();
            b.Property(o => o.FrontLoad).IsRequired();
            b.Property(o => o.TotalLoad).IsRequired();
            b.Property(o => o.UnitPrice).IsRequired();
            b.Property(o => o.TotalPrice).IsRequired();
            b.Property(o => o.Observation).HasMaxLength(40);

            b.Property(o => o.CreatedAt).IsRequired();

            b.HasOne(o => o.UserData).WithMany().HasForeignKey(o => o.UserDataId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne(o => o.Store).WithMany().HasForeignKey(o => o.StoreId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne(o => o.Fruit).WithMany().HasForeignKey(o => o.FruitId).OnDelete(DeleteBehavior.NoAction);
            b.HasOne(o => o.Supplier).WithMany().HasForeignKey(o => o.SupplierId).OnDelete(DeleteBehavior.NoAction);
        });
        #endregion Order

        #region Supplier
        builder.Entity<Supplier>(b =>
        {
            b.HasKey(sp => sp.Id);
            b.Property(sp => sp.Id).ValueGeneratedOnAdd();

            b.Property(sp => sp.Name).IsRequired().HasMaxLength(250);
            b.Property(sp => sp.CreatedAt).IsRequired();

            b.HasOne(sp => sp.UserData).WithMany().HasForeignKey(f => f.UserDataId).OnDelete(DeleteBehavior.NoAction);
        });
        #endregion Supplier

        #region Users
        builder.Entity<UserData>(b =>
        {
            b.HasKey(ud => ud.Id);
            b.Property(ud => ud.Id).ValueGeneratedOnAdd();

            b.Property(ud => ud.Name).IsRequired().HasMaxLength(250);

            b.Property(ud => ud.CreatedAt).IsRequired();
        });
        #endregion Users
    }
}
