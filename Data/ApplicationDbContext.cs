  
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ConsejeriaEpics.Data{

    public class ApplicationDbContext : DbContext{

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ConsejeriaEpics.Models.Categoria> Categorias { get; set; }
        public DbSet<ConsejeriaEpics.Models.Requerimiento> Requerimientos { get; set; }
        public DbSet<ConsejeriaEpics.Models.Usuario> Usuarios { get; set; }
    }
}