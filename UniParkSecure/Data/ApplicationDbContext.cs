using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniParkSecure.Models;

namespace UniParkSecure.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) 

        {
        }


        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Sector> Sectores { get; set; }
        public DbSet<Registro> Registros { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Sector>().HasData(
                new Sector { Id = 1, Nombre = "A", TotalEspacios = 50, Disponibles = 50 },
                new Sector { Id = 2, Nombre = "B", TotalEspacios = 40, Disponibles = 40 },
                new Sector { Id = 3, Nombre = "C", TotalEspacios = 30, Disponibles = 30 },
                new Sector { Id = 4, Nombre = "V", TotalEspacios = 20, Disponibles = 20 }
            );
        }
    }
}
