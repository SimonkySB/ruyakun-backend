using Microsoft.EntityFrameworkCore;

namespace Models.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Adopcion> Adopcion { get; set; }
    public DbSet<AdopcionEstado> AdopcionEstado { get; set; }
    public DbSet<Animal> Animal { get; set; }
    public DbSet<AnimalImagen> AnimalImagen { get; set; }
    public DbSet<Comuna> Comuna { get; set; }
    public DbSet<Especie> Especie { get; set; }
    public DbSet<NivelActividad> NivelActividad { get; set; }
    public DbSet<Organizacion> Organizacion { get; set; }
    public DbSet<OrganizacionUsuario> OrganizacionUsuario { get; set; }
    public DbSet<Seguimiento> Seguimiento { get; set; }
    public DbSet<SeguimientoEstado> SeguimientoEstado { get; set; }
    public DbSet<SeguimientoTipo> SeguimientoTipo { get; set; }
    public DbSet<Sexo> Sexo { get; set; }
    public DbSet<Tamano> Tamano { get; set; }
    public DbSet<Usuario> Usuario { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comuna>(opt =>
        {
            opt.ToTable("COMUNA");
    
            opt.Property(p => p.comunaId).HasColumnName("COMUNAID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<Usuario>(opt =>
        {
            opt.ToTable("USUARIO");
    
            opt.Property(p => p.usuarioId).HasColumnName("USUARIOID");
            opt.Property(p => p.username).HasColumnName("USERNAME");
            opt.Property(p => p.nombres).HasColumnName("NOMBRES");
            opt.Property(p => p.apellidos).HasColumnName("APELLIDOS");
            opt.Property(p => p.activo).HasColumnName("ACTIVO").HasColumnType("NUMBER(1)");;
            opt.Property(p => p.fechaCreacion).HasColumnName("FECHACREACION");
            opt.Property(p => p.direccion).HasColumnName("DIRECCION");
            opt.Property(p => p.telefono).HasColumnName("TELEFONO");
            opt.Property(p => p.telefono2).HasColumnName("TELEFONO2");
            opt.Property(p => p.comunaId).HasColumnName("COMUNAID");
    
            opt.HasOne(t => t.comuna).WithMany().HasForeignKey(t => t.comunaId);
        });
        
        modelBuilder.Entity<Organizacion>(opt =>
        {
            opt.ToTable("ORGANIZACION");
    
            opt.Property(p => p.organizacionId).HasColumnName("ORGANIZACIONID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
            opt.Property(p => p.nombreContacto).HasColumnName("NOMBRECONTACTO");
            opt.Property(p => p.telefonoContacto).HasColumnName("TELEFONOCONTACTO");
            opt.Property(p => p.emailContacto).HasColumnName("EMAILCONTACTO");
            opt.Property(p => p.direccion).HasColumnName("DIRECCION");
            opt.Property(p => p.comunaId).HasColumnName("COMUNAID");
            opt.Property(p => p.fechaEliminacion).HasColumnName("FECHAELIMINACION");
            
            opt.HasOne(t => t.comuna).WithMany().HasForeignKey(t => t.comunaId);
        });

        modelBuilder.Entity<OrganizacionUsuario>(opt =>
        {
            opt.ToTable("ORGANIZACIONUSUARIO");

            opt.HasKey(ou => new { ou.organizacionId, ou.usuarioId });
            
            opt.Property(p => p.organizacionId).HasColumnName("ORGANIZACIONID");
            opt.Property(p => p.usuarioId).HasColumnName("USUARIOID");
            
            opt.HasOne(t => t.organizacion).WithMany(t => t.organizacionUsuarios).HasForeignKey(t => t.organizacionId);
            opt.HasOne(t => t.usuario).WithMany(t => t.organizacionUsuarios).HasForeignKey(t => t.usuarioId);
        });
        
        modelBuilder.Entity<Sexo>(opt =>
        {
            opt.ToTable("SEXO");
    
            opt.Property(p => p.sexoId).HasColumnName("SEXOID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<Especie>(opt =>
        {
            opt.ToTable("ESPECIE");
    
            opt.Property(p => p.especieId).HasColumnName("ESPECIEID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<Tamano>(opt =>
        {
            opt.ToTable("TAMANO");
    
            opt.Property(p => p.tamanoId).HasColumnName("TAMANOID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<NivelActividad>(opt =>
        {
            opt.ToTable("NIVELACTIVIDAD");
    
            opt.Property(p => p.nivelActividadId).HasColumnName("NIVELACTIVIDADID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<Animal>(opt =>
        {
            opt.ToTable("ANIMAL");
    
            opt.Property(p => p.animalId).HasColumnName("ANIMALID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
            opt.Property(p => p.peso).HasColumnName("PESO").HasColumnType("NUMBER(10,1)");;
            opt.Property(p => p.fechaRegistro).HasColumnName("FECHAREGISTRO");
            opt.Property(p => p.fechaNacimiento).HasColumnName("FECHANACIMIENTO");
            opt.Property(p => p.publicado).HasColumnName("PUBLICADO").HasColumnType("NUMBER(1)");
            opt.Property(p => p.descripcion).HasColumnName("DESCRIPCION").HasColumnType("CLOB");;
            opt.Property(p => p.especieId).HasColumnName("ESPECIEID");
            opt.Property(p => p.sexoId).HasColumnName("SEXOID");
            opt.Property(p => p.organizacionId).HasColumnName("ORGANIZACIONID");
            opt.Property(p => p.fechaEliminacion).HasColumnName("FECHAELIMINACION");
            opt.Property(p => p.tamanoId).HasColumnName("TAMANOID");
            opt.Property(p => p.nivelActividadId).HasColumnName("NIVELACTIVIDADID");
            
            opt.HasOne(t => t.especie).WithMany().HasForeignKey(t => t.especieId);
            opt.HasOne(t => t.sexo).WithMany().HasForeignKey(t => t.sexoId);
            opt.HasOne(t => t.organizacion).WithMany().HasForeignKey(t => t.organizacionId);
            opt.HasOne(t => t.tamano).WithMany().HasForeignKey(t => t.tamanoId);
            opt.HasOne(t => t.nivelActividad).WithMany().HasForeignKey(t => t.nivelActividadId);
        });
        
        modelBuilder.Entity<AnimalImagen>(opt =>
        {
            opt.ToTable("ANIMALIMAGEN");
    
            opt.Property(p => p.animalImagenId).HasColumnName("ANIMALIMAGENID");
            opt.Property(p => p.url).HasColumnName("URL");
            opt.Property(p => p.refCode).HasColumnName("REFCODE");
            opt.Property(p => p.animalId).HasColumnName("ANIMALID");
            
            opt.HasOne(t => t.animal).WithMany(t => t.animalImagenes).HasForeignKey(t => t.animalId);
        });
        
        modelBuilder.Entity<AdopcionEstado>(opt =>
        {
            opt.ToTable("ADOPCIONESTADO");
    
            opt.Property(p => p.adopcionEstadoId).HasColumnName("ADOPCIONESTADOID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<SeguimientoTipo>(opt =>
        {
            opt.ToTable("SEGUIMIENTOTIPO");
    
            opt.Property(p => p.seguimientoTipoId).HasColumnName("SEGUIMIENTOTIPOID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<SeguimientoEstado>(opt =>
        {
            opt.ToTable("SEGUIMIENTOESTADO");
    
            opt.Property(p => p.seguimientoEstadoId).HasColumnName("SEGUIMIENTOESTADOID");
            opt.Property(p => p.nombre).HasColumnName("NOMBRE");
        });
        
        modelBuilder.Entity<Adopcion>(opt =>
        {
            opt.ToTable("ADOPCION");
    
            opt.Property(p => p.adopcionId).HasColumnName("ADOPCIONID");
            opt.Property(p => p.usuarioId).HasColumnName("USUARIOID");
            opt.Property(p => p.adopcionEstadoId).HasColumnName("ADOPCIONESTADOID");
            opt.Property(p => p.fechaCreacion).HasColumnName("FECHACREACION");
            opt.Property(p => p.fechaActualizacion).HasColumnName("FECHAACTUALIZACION");
            opt.Property(p => p.descripcionFamilia ).HasColumnName("DESCRIPCIONFAMILIA");
            
            opt.HasOne(t => t.usuario).WithMany().HasForeignKey(t => t.usuarioId);
            opt.HasOne(t => t.adopcionEstado).WithMany().HasForeignKey(t => t.adopcionEstadoId);
        });
        
        
        modelBuilder.Entity<Seguimiento>(opt =>
        {
            opt.ToTable("SEGUIMIENTO");
    
            opt.Property(p => p.seguimientoId).HasColumnName("SEGUIMIENTOID");
            opt.Property(p => p.adopcionId).HasColumnName("ADOPCIONID");
            opt.Property(p => p.seguimientoTipoId).HasColumnName("SEGUIMIENTOTIPOID");
            opt.Property(p => p.fechaCreacion).HasColumnName("FECHACREACION");
            opt.Property(p => p.descripcion).HasColumnName("DESCRIPCION");
            opt.Property(p => p.fechaEntrevista).HasColumnName("FECHAENTREVISTA");
            opt.Property(p => p.seguimientoEstadoId).HasColumnName("SEGUIMIENTOESTADOID");
            opt.Property(p => p.observacion).HasColumnName("OBSERVACION");
            opt.Property(p => p.fechaActualizacion).HasColumnName("FECHAACTUALIZACION");
            opt.Property(p => p.fechaCierre).HasColumnName("FECHACIERRE");
            
            opt.HasOne(t => t.adopcion).WithMany(t => t.seguimientos).HasForeignKey(t => t.adopcionId);
            opt.HasOne(t => t.seguimientoTipo).WithMany().HasForeignKey(t => t.seguimientoTipoId);
            opt.HasOne(t => t.seguimientoEstado).WithMany().HasForeignKey(t => t.seguimientoEstadoId);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}