using Microsoft.EntityFrameworkCore;
using PROFESICIPTA.Models;

namespace PROFESICIPTA.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<SOOrder> SO_ORDER { get; set; }
        public DbSet<COM_CUSTOMER> COM_CUSTOMER { get; set; }
        public DbSet<SO_ITEM> SO_ITEM { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-FRE6547\\SQLEXPRESS;Initial Catalog=PROFESICIPTA;MultipleActiveResultSets=true;Integrated Security=True;TrustServerCertificate=True;User ID=sa;Password=admin123");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SOOrder>().HasKey(p => p.SO_ORDER_ID);
            modelBuilder.Entity<COM_CUSTOMER>().HasKey(p => p.COM_CUSTOMER_ID);
            modelBuilder.Entity<SO_ITEM>().HasKey(p => p.SO_ITEM_ID);
			modelBuilder.Entity<SOOrder>()
				.HasOne(o => o.COM_CUSTOMER) 
				.WithMany(c => c.SOOrders)   
				.HasForeignKey(o => o.COM_CUSTOMER_ID) 
				.OnDelete(DeleteBehavior.SetNull);

			modelBuilder.Entity<SO_ITEM>()
	            .HasOne<SOOrder>()  
	            .WithMany(o => o.SOItems)  
	            .HasForeignKey(item => item.SO_ORDER_ID)  
	            .OnDelete(DeleteBehavior.Cascade);  


			base.OnModelCreating(modelBuilder);
		}
    }
}
