using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StangeCastle.Data;

// O IdentityDbContext já traz todas as tabelas prontas para usuários, senhas e permissões (RBAC)
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // No futuro, é aqui que será colocado as tabelas do seu Drive e do TTRPG
    // public DbSet<Arquivo> Arquivos { get; set; }
    // public DbSet<DocumentoRPG> Documentos { get; set; }
}