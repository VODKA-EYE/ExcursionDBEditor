using System;
using System.Collections.Generic;
using ExcursionHelperDBEditor.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcursionHelperDBEditor.Context;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Checkpoint> Checkpoints { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Dbversion> Dbversions { get; set; }

    public virtual DbSet<Excursion> Excursions { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseLazyLoadingProxies().UseNpgsql("Host=vodka.chemirproject.net:82;Database=postgres;Username=vodka;Password=uWGw5eTR;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Checkpoint>(entity =>
        {
            entity.HasKey(e => e.CheckpointId).HasName("excursioncheckpoin_pk");

            entity.ToTable("checkpoints");

            entity.Property(e => e.CheckpointId)
                .HasDefaultValueSql("nextval('excursioncheckpoin_checkpoint_id_seq'::regclass)")
                .HasColumnName("checkpoint_id");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasMany(d => d.Images).WithMany(p => p.Checkpoints)
                .UsingEntity<Dictionary<string, object>>(
                    "Checkpointimage",
                    r => r.HasOne<Image>().WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("checkpointimages_images_fk"),
                    l => l.HasOne<Checkpoint>().WithMany()
                        .HasForeignKey("CheckpointId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("checkpointimages_checkpoints_fk"),
                    j =>
                    {
                        j.HasKey("CheckpointId", "ImageId").HasName("checkpointimages_pk");
                        j.ToTable("checkpointimages");
                        j.IndexerProperty<int>("CheckpointId").HasColumnName("checkpoint_id");
                        j.IndexerProperty<int>("ImageId").HasColumnName("image_id");
                    });
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("comments_pk");

            entity.ToTable("comments");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.Commentary).HasColumnName("commentary");
            entity.Property(e => e.Commentator).HasColumnName("commentator");
            entity.Property(e => e.ExcursionId).HasColumnName("excursion_id");
            entity.Property(e => e.CommentDate).HasColumnName("comment_date");

            entity.HasOne(d => d.Excursion).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ExcursionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comments_excursions_fk");
        });

        modelBuilder.Entity<Dbversion>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("dbversion_pk");

            entity.ToTable("dbversion");

            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("timestamp");
        });

        modelBuilder.Entity<Excursion>(entity =>
        {
            entity.HasKey(e => e.ExcursionId).HasName("excursions_pk");

            entity.ToTable("excursions");

            entity.Property(e => e.ExcursionId).HasColumnName("excursion_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.MapUrl).HasColumnName("map_url");
            entity.Property(e => e.MapImageUrl).HasColumnName("map_image_url");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasMany(d => d.Checkpoints).WithMany(p => p.Excursions)
                .UsingEntity<Dictionary<string, object>>(
                    "Excursionscheckpoint",
                    r => r.HasOne<Checkpoint>().WithMany()
                        .HasForeignKey("CheckpointId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("excursionscheckpoints_checkpoints_fk"),
                    l => l.HasOne<Excursion>().WithMany()
                        .HasForeignKey("ExcursionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("excursionscheckpoints_excursions_fk"),
                    j =>
                    {
                        j.HasKey("ExcursionId", "CheckpointId").HasName("excursionscheckpoints_pk");
                        j.ToTable("excursionscheckpoints");
                        j.IndexerProperty<int>("ExcursionId").HasColumnName("excursion_id");
                        j.IndexerProperty<int>("CheckpointId").HasColumnName("checkpoint_id");
                    });
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("images_pk");

            entity.ToTable("images");

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ImageDescription).HasColumnName("image_description");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
