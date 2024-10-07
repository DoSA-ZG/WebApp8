using Microsoft.EntityFrameworkCore;

namespace RPPP_WebApp.Models;

public partial class Rppp08Context : DbContext
{
    public Rppp08Context()
    {
    }

    public Rppp08Context(DbContextOptions<Rppp08Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Dokumentacija> Dokumentacijas { get; set; }

    public virtual DbSet<KarticaProjektum> KarticaProjekta { get; set; }

    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<PartnerProjekt> PartnerProjekts { get; set; }

    public virtual DbSet<Posao> Posaos { get; set; }

    public virtual DbSet<Projekt> Projekts { get; set; }

    public virtual DbSet<StatusZadatka> StatusZadatkas { get; set; }

    public virtual DbSet<Suradnik> Suradniks { get; set; }

    public virtual DbSet<SuradnikProjekt> SuradnikProjekts { get; set; }

    public virtual DbSet<Transakcija> Transakcijas { get; set; }

    public virtual DbSet<VrstaDokumentacije> VrstaDokumentacijes { get; set; }

    public virtual DbSet<VrstaPosla> VrstaPoslas { get; set; }

    public virtual DbSet<VrstaProjektum> VrstaProjekta { get; set; }

    public virtual DbSet<VrstaTransakcije> VrstaTransakcijes { get; set; }

    public virtual DbSet<VrstaZahtjeva> VrstaZahtjevas { get; set; }

    public virtual DbSet<Zadatak> Zadataks { get; set; }

    public virtual DbSet<ZadatakSuradnik> ZadatakSuradniks { get; set; }

    public virtual DbSet<Zahtjev> Zahtjevs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {}
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dokumentacija>(entity =>
        {
            entity.HasKey(e => e.DokumentacijaId).HasName("PK__Dokument__25CA66D497FCD953");

            entity.ToTable("Dokumentacija");

            entity.Property(e => e.DokumentacijaId).HasColumnName("DokumentacijaID");
            entity.Property(e => e.NazivDokumentacije)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProjektId).HasColumnName("ProjektID");
            entity.Property(e => e.VrstaDokumentacijeId).HasColumnName("VrstaDokumentacijeID");

            entity.HasOne(d => d.Projekt).WithMany(p => p.Dokumentacijas)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__Dokumenta__Proje__70DDC3D8");

            entity.HasOne(d => d.VrstaDokumentacije).WithMany(p => p.Dokumentacijas)
                .HasForeignKey(d => d.VrstaDokumentacijeId)
                .HasConstraintName("FK__Dokumenta__Vrsta__71D1E811");
        });

        modelBuilder.Entity<KarticaProjektum>(entity =>
        {
            entity.HasKey(e => e.KarticaProjektaId).HasName("PK__KarticaP__70C2D5B3E4BB05B8");

            entity.Property(e => e.KarticaProjektaId).HasColumnName("KarticaProjektaID");
            entity.Property(e => e.ProjektId).HasColumnName("ProjektID");
            entity.Property(e => e.VirtualniIban)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("VirtualniIBAN");

            entity.HasOne(d => d.Projekt).WithMany(p => p.KarticaProjekta)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__KarticaPr__Proje__66603565");

                
             entity.HasOne(t => t.Projekt) // Navigation property in Transakcija
                .WithMany(k => k.KarticaProjekta) // Collection property in KarticaProjektum
                .HasForeignKey(t => t.ProjektId) // Foreign key property in Transakcija
                .OnDelete(DeleteBehavior.Cascade); // Or use another delete behavior as needed
        
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.HasKey(e => e.PartnerId).HasName("PK__Partner__39FD6332863685EB");

            entity.ToTable("Partner");

            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Iban)
                .HasMaxLength(21)
                .IsUnicode(false)
                .HasColumnName("IBAN");
            entity.Property(e => e.Ime)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Naziv)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Oib)
                .HasMaxLength(11)
                .IsUnicode(false)
                .HasColumnName("OIB");
            entity.Property(e => e.Prezime)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("URL");
        });

        modelBuilder.Entity<PartnerProjekt>(entity =>
        {
            entity.HasKey(e => e.PartnerProjektId).HasName("PK__PartnerP__0A609246F69EC838");

            entity.ToTable("PartnerProjekt");

            entity.Property(e => e.PartnerProjektId).HasColumnName("PartnerProjektID");
            entity.Property(e => e.PartnerId).HasColumnName("PartnerID");
            entity.Property(e => e.ProjektId).HasColumnName("ProjektID");
            entity.Property(e => e.UlogaPartnera)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Partner).WithMany(p => p.PartnerProjekts)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK__PartnerPr__Partn__534D60F1");

            entity.HasOne(d => d.Projekt).WithMany(p => p.PartnerProjekts)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__PartnerPr__Proje__5441852A");
        });

        modelBuilder.Entity<Posao>(entity =>
        {
            entity.HasKey(e => e.PosaoId).HasName("PK__Posao__7F5EF59A8A47AFC0");

            entity.ToTable("Posao");

            entity.Property(e => e.PosaoId).HasColumnName("PosaoID");
            entity.Property(e => e.Opis).HasColumnType("text");
            entity.Property(e => e.ProjektId).HasColumnName("ProjektID");
            entity.Property(e => e.VrstaPoslaId).HasColumnName("VrstaPoslaID");

            entity.HasOne(d => d.Projekt).WithMany(p => p.Posaos)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__Posao__ProjektID__5EBF139D");

            entity.HasOne(d => d.VrstaPosla).WithMany(p => p.Posaos)
                .HasForeignKey(d => d.VrstaPoslaId)
                .HasConstraintName("FK__Posao__VrstaPosl__5FB337D6");

            entity.HasMany(d => d.Suradniks).WithMany(p => p.Posaos)
                .UsingEntity<Dictionary<string, object>>(
                    "PosaoSuradnik",
                    r => r.HasOne<Suradnik>().WithMany()
                        .HasForeignKey("SuradnikId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PosaoSura__Surad__6383C8BA"),
                    l => l.HasOne<Posao>().WithMany()
                        .HasForeignKey("PosaoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__PosaoSura__Posao__628FA481"),
                    j =>
                    {
                        j.HasKey("PosaoId", "SuradnikId").HasName("PK__PosaoSur__7969E14D582BE6EF");
                        j.ToTable("PosaoSuradnik");
                        j.IndexerProperty<int>("PosaoId").HasColumnName("PosaoID");
                        j.IndexerProperty<int>("SuradnikId").HasColumnName("SuradnikID");
                    });
        });

        modelBuilder.Entity<Projekt>(entity =>
        {
            entity.HasKey(e => e.ProjektId).HasName("PK__Projekt__D93388D10BE8F79D");

            entity.ToTable("Projekt");

            entity.Property(e => e.ProjektId).HasColumnName("ProjektID");
            entity.Property(e => e.KraticaProjekta)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NazivProjekta)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.OpisProjekta).HasColumnType("text");
            entity.Property(e => e.PlaniraniPocetak).HasColumnType("date");
            entity.Property(e => e.PlaniraniZavrsetak).HasColumnType("date");
            entity.Property(e => e.StvarniPocetak).HasColumnType("date");
            entity.Property(e => e.StvarniZavrsetak).HasColumnType("date");
            entity.Property(e => e.VrstaProjektaId).HasColumnName("VrstaProjektaID");

            entity.HasOne(d => d.VrstaProjekta).WithMany(p => p.Projekts)
                .HasForeignKey(d => d.VrstaProjektaId)
                .HasConstraintName("FK__Projekt__VrstaPr__4E88ABD4");
        });

        modelBuilder.Entity<StatusZadatka>(entity =>
        {
            entity.HasKey(e => e.StatusZadatkaId).HasName("PK__StatusZa__4D9147148A5A0665");

            entity.ToTable("StatusZadatka");

            entity.Property(e => e.StatusZadatkaId).HasColumnName("StatusZadatkaID");
            entity.Property(e => e.NazivStatusaZadatka)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Suradnik>(entity =>
        {
            entity.HasKey(e => e.SuradnikId).HasName("PK__Suradnik__63714D71728924BC");

            entity.ToTable("Suradnik");

            entity.Property(e => e.SuradnikId).HasColumnName("SuradnikID");
            entity.Property(e => e.BrojMobitela)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Ime)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Prezime)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SuradnikProjekt>(entity =>
        {
            entity.HasKey(e => e.SuradnikProjektId).HasName("PK__Suradnik__7860F45AAD2ABAC8");

            entity.ToTable("SuradnikProjekt");

            entity.Property(e => e.SuradnikProjektId).HasColumnName("SuradnikProjektID");
            entity.Property(e => e.ProjektId).HasColumnName("ProjektID");
            entity.Property(e => e.SuradnikId).HasColumnName("SuradnikID");
            entity.Property(e => e.UlogaSuradnika)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Projekt).WithMany(p => p.SuradnikProjekts)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__SuradnikP__Proje__59FA5E80");

            entity.HasOne(d => d.Suradnik).WithMany(p => p.SuradnikProjekts)
                .HasForeignKey(d => d.SuradnikId)
                .HasConstraintName("FK__SuradnikP__Surad__59063A47");
        });

        modelBuilder.Entity<Transakcija>(entity =>
        {
            entity.HasKey(e => e.TransakcijaId).HasName("PK__Transakc__BED360B394A99D67");

            entity.ToTable("Transakcija");

            entity.Property(e => e.TransakcijaId).HasColumnName("TransakcijaID");
            entity.Property(e => e.DatumTransakcije).HasColumnType("date");
            entity.Property(e => e.Iznos).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.KarticaProjektaId).HasColumnName("KarticaProjektaID");
            entity.Property(e => e.Opis)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PrimateljIban)
                .HasMaxLength(21)
                .IsUnicode(false)
                .HasColumnName("PrimateljIBAN");
            entity.Property(e => e.SubjektIban)
                .HasMaxLength(21)
                .IsUnicode(false)
                .HasColumnName("SubjektIBAN");
            entity.Property(e => e.VrstaTransakcijeId).HasColumnName("VrstaTransakcijeID");

            entity.HasOne(d => d.KarticaProjekta).WithMany(p => p.Transakcijas)
                .HasForeignKey(d => d.KarticaProjektaId)
                .HasConstraintName("FK__Transakci__Karti__6D0D32F4");

            entity.HasOne(d => d.VrstaTransakcije).WithMany(p => p.Transakcijas)
                .HasForeignKey(d => d.VrstaTransakcijeId)
                .HasConstraintName("FK__Transakci__Vrsta__6E01572D");

            entity.HasOne(t => t.KarticaProjekta) // Navigation property in Transakcija
            .WithMany(k => k.Transakcijas) // Collection property in KarticaProjektum
            .HasForeignKey(t => t.KarticaProjektaId) // Foreign key property in Transakcija
            .OnDelete(DeleteBehavior.Cascade); // Or use another delete behavior as needed
            
        });

        modelBuilder.Entity<VrstaDokumentacije>(entity =>
        {
            entity.HasKey(e => e.VrstaDokumentacijeId).HasName("PK__VrstaDok__05A8C8D82DD611F3");

            entity.ToTable("VrstaDokumentacije");

            entity.Property(e => e.VrstaDokumentacijeId).HasColumnName("VrstaDokumentacijeID");
            entity.Property(e => e.NazivVrsteDokumentacije)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VrstaPosla>(entity =>
        {
            entity.HasKey(e => e.VrstaPoslaId).HasName("PK__VrstaPos__F7F16555FC065883");

            entity.ToTable("VrstaPosla");

            entity.Property(e => e.VrstaPoslaId).HasColumnName("VrstaPoslaID");
            entity.Property(e => e.NazivVrste)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VrstaProjektum>(entity =>
        {
            entity.HasKey(e => e.VrstaProjektaId).HasName("PK__VrstaPro__C8F8709498048E26");

            entity.Property(e => e.VrstaProjektaId).HasColumnName("VrstaProjektaID");
            entity.Property(e => e.NazivVrsteProjekta)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VrstaTransakcije>(entity =>
        {
            entity.HasKey(e => e.VrstaTransakcijeId).HasName("PK__VrstaTra__E6764D9734F22117");

            entity.ToTable("VrstaTransakcije");

            entity.Property(e => e.VrstaTransakcijeId).HasColumnName("VrstaTransakcijeID");
            entity.Property(e => e.NazivVrste)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VrstaZahtjeva>(entity =>
        {
            entity.HasKey(e => e.VrstaZahtjevaId).HasName("PK__VrstaZah__BFD2481FF45E08C0");

            entity.ToTable("VrstaZahtjeva");

            entity.Property(e => e.VrstaZahtjevaId).HasColumnName("VrstaZahtjevaID");
            entity.Property(e => e.NazivVrsteZahtjeva)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Zadatak>(entity =>
        {
            entity.HasKey(e => e.ZadatakId).HasName("PK__Zadatak__EA89E6658D80116E");

            entity.ToTable("Zadatak");

            entity.Property(e => e.ZadatakId).HasColumnName("ZadatakID");
            entity.Property(e => e.Opis).HasColumnType("text");
            entity.Property(e => e.StatusZadatkaId).HasColumnName("StatusZadatkaID");
            entity.Property(e => e.ZahtjevId).HasColumnName("ZahtjevID");

            entity.HasOne(d => d.StatusZadatka).WithMany(p => p.Zadataks)
                .HasForeignKey(d => d.StatusZadatkaId)
                .HasConstraintName("FK__Zadatak__StatusZ__7C4F7684");

            entity.HasOne(d => d.Zahtjev).WithMany(p => p.Zadataks)
                .HasForeignKey(d => d.ZahtjevId)
                .HasConstraintName("FK__Zadatak__Zahtjev__7D439ABD");
        });

        modelBuilder.Entity<ZadatakSuradnik>(entity =>
        {
            entity.HasKey(e => new { e.ZadatakId, e.SuradnikId }).HasName("PK__ZadatakS__ECBEF2B20C01BBD4");

            entity.ToTable("ZadatakSuradnik");

            entity.Property(e => e.ZadatakId).HasColumnName("ZadatakID");
            entity.Property(e => e.SuradnikId).HasColumnName("SuradnikID");

            entity.HasOne(d => d.Suradnik).WithMany(p => p.ZadatakSuradniks)
                .HasForeignKey(d => d.SuradnikId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ZadatakSu__Surad__01142BA1");

            entity.HasOne(d => d.Zadatak).WithMany(p => p.ZadatakSuradniks)
                .HasForeignKey(d => d.ZadatakId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ZadatakSu__Zadat__00200768");
        });

        modelBuilder.Entity<Zahtjev>(entity =>
        {
            entity.HasKey(e => e.ZahtjevId).HasName("PK__Zahtjev__62CC7BBD38795618");

            entity.ToTable("Zahtjev");

            entity.Property(e => e.ZahtjevId).HasColumnName("ZahtjevID");
            entity.Property(e => e.NazivZahtjeva)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Oznaka)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Prioritet)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ProjektId).HasColumnName("ProjektID");
            entity.Property(e => e.VrstaZahtjevaId).HasColumnName("VrstaZahtjevaID");

            entity.HasOne(d => d.Projekt).WithMany(p => p.Zahtjevs)
                .HasForeignKey(d => d.ProjektId)
                .HasConstraintName("FK__Zahtjev__Projekt__797309D9");

            entity.HasOne(d => d.VrstaZahtjeva).WithMany(p => p.Zahtjevs)
                .HasForeignKey(d => d.VrstaZahtjevaId)
                .HasConstraintName("FK__Zahtjev__VrstaZa__787EE5A0");
        });



		OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

}
