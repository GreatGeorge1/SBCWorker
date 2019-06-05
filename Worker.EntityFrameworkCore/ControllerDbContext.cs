using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Worker.Core.Models;

namespace Worker.EntityFrameworkCore
{
    public class ControllerDbContext : DbContext
    {
        public ControllerDbContext(DbContextOptions<ControllerDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
            //optionsBuilder.UseSqlite("Data Source = ../filewatcher/controllerdb.db;");
            // optionsBuilder.UseInMemoryDatabase();

        }

        public DbSet<Controller> Сontrollers { get; set; }
        public DbSet<Terminal> Terminals { get; set; }
        public DbSet<ControllerConfig> ControllerConfigs { get; set; }
        public DbSet<TerminalConfig> TerminalConfigs { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Fingerprint> Fingerprints { get; set; }
        public DbSet<Employer> Employee { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //indexes
            //modelBuilder.Entity<Employer>()
            //    .HasIndex(e=>e.)

            modelBuilder
                .Entity<Employer>()
                .Property(c => c.AccessLevel)
                .HasConversion(
                    v => v.ToString(),
                    v => (AccessLevel)Enum.Parse(typeof(AccessLevel), v));


            //fk's
            modelBuilder.Entity<Card>()
                .HasOne(c => c.Employer)
                .WithMany(e => e.Cards)
                .HasForeignKey(c => c.EmployerId);
            modelBuilder.Entity<Fingerprint>()
                .HasOne(f => f.Employer)
                .WithMany(e => e.Fingerprints)
                .HasForeignKey(f => f.EmployerId);
            modelBuilder.Entity<Device>()
                .HasOne(f => f.Employer)
                .WithMany(e => e.Devices)
                .HasForeignKey(f => f.EmployerId);
            modelBuilder.Entity<Terminal>()
               .HasOne(f => f.Controller)
               .WithMany(e => e.Terminals)
               .HasForeignKey(f => f.ControllerId);

            modelBuilder.Entity<Controller>()
                .HasOne(p => p.Config)
                .WithOne(b => b.Core)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Terminal>()
                .HasOne(p => p.Config)
                .WithOne(b => b.Core)
                .OnDelete(DeleteBehavior.Cascade);


            //seeds
            //modelBuilder.Entity<Employer>()
            //    .HasData(
            //        new Employer { EmployerId = 1, Name = "Volodimyr", Surname = "Noha", LastName = "Oleksandrovych" },
            //        new Employer { EmployerId = 2, Name = "Yehor", Surname = "Illarionov", LastName = "Dmitriyevich" });
            //modelBuilder.Entity<Card>().HasData(
            //    new Card
            //    {
            //        CardId = 1,
            //        SpecialNumber = "5300A3950E6B",
            //        AccessLevel = AccessLevels.Employee1,
            //        EmployerId = 1
            //    },
            //    new Card
            //    {
            //        CardId = 2,
            //        SpecialNumber = "161810511",
            //        AccessLevel = AccessLevels.Employee1,
            //        EmployerId = 2
            //    },
            //    new Card
            //    {
            //        CardId = 3,
            //        SpecialNumber = "984635009",
            //        AccessLevel = AccessLevels.Employee1,
            //        EmployerId = 1
            //    });
            //modelBuilder.Entity<Terminal>().HasData(new Terminal { TerminalId = 1, Secret = "qwerty123" });
            // modelBuilder.Entity<Fingerprint>().HasData(
            //todo fingerprint auth
            //new Fingerprint { FingerprintId = 1, EmployerId = 1, Command = "04" },
            //new Fingerprint { FingerprintId = 2, EmployerId = 1, Command = "05" },
            //new Fingerprint { FingerprintId = 3, EmployerId = 1, Command = "06" },
            //new Fingerprint { FingerprintId = 4, EmployerId = 2, Command = "01" },
            //new Fingerprint { FingerprintId = 5, EmployerId = 2, Command = "02" },
            //new Fingerprint { FingerprintId = 6, EmployerId = 2, Command = "03" });



            base.OnModelCreating(modelBuilder);
        }
    }
}
