using Microsoft.EntityFrameworkCore;

namespace RPPP_WebApp.Models
{
	public partial class Rppp08Context
	{

		// DZ2
		public virtual DbSet<ViewZahtjevProjekt> vw_ZahtjevProjekt { get; set; }
		public virtual DbSet<ViewZahtjevInfo> vw_Zahtjevi { get; set; }

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
		{
            modelBuilder.Entity<ViewZahtjevProjekt>(entity =>
            {
				entity.HasNoKey();
            });

            modelBuilder.Entity<ViewZahtjevInfo>(entity =>
			{
				entity.HasNoKey();
            });
		}
	}
}
