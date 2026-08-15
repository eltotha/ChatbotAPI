using ChatbotAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatbotAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Conversacion> Conversaciones => Set<Conversacion>();
    public DbSet<Mensaje> Mensajes => Set<Mensaje>();
    public DbSet<MensajeAdjunto> MensajesAdjuntos => Set<MensajeAdjunto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255);
            entity.Property(e => e.FechaRegistro)
                .HasColumnName("fecha_registro")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.Username)
                .IsUnique();
        });

        modelBuilder.Entity<Conversacion>(entity =>
        {
            entity.ToTable("conversaciones");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UsuarioId)
                .HasColumnName("usuario_id")
                .IsRequired();
            entity.Property(e => e.Titulo)
                .HasColumnName("titulo")
                .HasMaxLength(255);
            entity.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaActualizacion)
                .HasColumnName("fecha_actualizacion")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Conversaciones)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UsuarioId)
                .HasDatabaseName("idx_conversaciones_usuario");
            entity.HasIndex(e => e.FechaActualizacion)
                .HasDatabaseName("idx_conversaciones_fecha");
        });

        modelBuilder.Entity<Mensaje>(entity =>
        {
            entity.ToTable("mensajes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConversacionId)
                .HasColumnName("conversacion_id")
                .IsRequired();
            entity.Property(e => e.TipoEmisor)
                .HasColumnName("tipo_emisor")
                .HasConversion<string>()
                .IsRequired();
            entity.Property(e => e.Contenido)
                .HasColumnName("contenido")
                .HasColumnType("TEXT")
                .IsRequired();
            entity.Property(e => e.FechaEnvio)
                .HasColumnName("fecha_envio")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Conversacion)
                .WithMany(c => c.Mensajes)
                .HasForeignKey(e => e.ConversacionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Adjuntos)
                .WithOne(a => a.Mensaje)
                .HasForeignKey(a => a.MensajeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ConversacionId)
                .HasDatabaseName("idx_mensajes_conversacion");
            entity.HasIndex(e => e.FechaEnvio)
                .HasDatabaseName("idx_mensajes_fecha");
        });

        modelBuilder.Entity<MensajeAdjunto>(entity =>
        {
            entity.ToTable("mensajes_adjuntos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MensajeId)
                .HasColumnName("mensaje_id")
                .IsRequired();
            entity.Property(e => e.ConversacionId)
                .HasColumnName("conversacion_id")
                .IsRequired();
            entity.Property(e => e.TipoArchivo)
                .HasColumnName("tipo_archivo")
                .HasMaxLength(50)
                .IsRequired();
            entity.Property(e => e.MimeType)
                .HasColumnName("mime_type")
                .HasMaxLength(100);
            entity.Property(e => e.NombreArchivo)
                .HasColumnName("nombre_archivo")
                .HasMaxLength(255);
            entity.Property(e => e.Base64)
                .HasColumnName("base64")
                .HasColumnType("LONGTEXT");
            entity.Property(e => e.FechaCreacion)
                .HasColumnName("fecha_creacion")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Mensaje)
                .WithMany(m => m.Adjuntos)
                .HasForeignKey(e => e.MensajeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Conversacion)
                .WithMany()
                .HasForeignKey(e => e.ConversacionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.MensajeId)
                .HasDatabaseName("idx_adjuntos_mensaje");
            entity.HasIndex(e => e.ConversacionId)
                .HasDatabaseName("idx_adjuntos_conversacion");
        });
    }
}
