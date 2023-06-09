﻿using System;
using System.Collections.Generic;
using Api.Modules.Identity.Data.Tables;
using Microsoft.EntityFrameworkCore;

namespace Api.Modules.Identity.Data;

public partial class IdentityContext : DbContext
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Account { get; set; }

    public virtual DbSet<AccountAudit> AccountAudit { get; set; }

    public virtual DbSet<Login> Login { get; set; }

    public virtual DbSet<Password> Password { get; set; }

    public virtual DbSet<PasswordAudit> PasswordAudit { get; set; }

    public virtual DbSet<Provider> Provider { get; set; }

    public virtual DbSet<Refresh> Refresh { get; set; }

    public virtual DbSet<Reset> Reset { get; set; }

    public virtual DbSet<Verification> Verification { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_account");

            entity.ToTable("account", "identity");

            entity.HasIndex(e => new { e.ProviderId, e.Email }, "u_account").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_on");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.UpdatedOn).HasColumnName("updated_on");
            entity.Property(e => e.VerifiedOn).HasColumnName("verified_on");

            entity.HasOne(d => d.Provider).WithMany(p => p.Account)
                .HasForeignKey(d => d.ProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_account_provider");
        });

        modelBuilder.Entity<AccountAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_account_audit");

            entity.ToTable("account_audit", "identity");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_on");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountAudit)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_account_audit_account");
        });

        modelBuilder.Entity<Login>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_login");

            entity.ToTable("login", "identity");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_on");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.IsSuccessful).HasColumnName("is_successful");

            entity.HasOne(d => d.Account).WithMany(p => p.Login)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("fk_login_account");
        });

        modelBuilder.Entity<Password>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("pk_password");

            entity.ToTable("password", "identity");

            entity.Property(e => e.AccountId)
                .ValueGeneratedNever()
                .HasColumnName("account_id");
            entity.Property(e => e.Hash)
                .HasMaxLength(60)
                .HasColumnName("hash");
            entity.Property(e => e.UpdatedOn).HasColumnName("updated_on");

            entity.HasOne(d => d.Account).WithOne(p => p.Password)
                .HasForeignKey<Password>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_password_account");
        });

        modelBuilder.Entity<PasswordAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_password_audit");

            entity.ToTable("password_audit", "identity");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_on");
            entity.Property(e => e.Hash)
                .HasMaxLength(60)
                .HasColumnName("hash");

            entity.HasOne(d => d.Account).WithMany(p => p.PasswordAudit)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_password_audit_account");
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_provider");

            entity.ToTable("provider", "identity");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Refresh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_refresh");

            entity.ToTable("refresh", "identity");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_on");
            entity.Property(e => e.ExpiresOn).HasColumnName("expires_on");
            entity.Property(e => e.IsUsed).HasColumnName("is_used");
            entity.Property(e => e.Secret)
                .HasMaxLength(44)
                .HasColumnName("secret");

            entity.HasOne(d => d.Account).WithMany(p => p.Refresh)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_refresh_account");
        });

        modelBuilder.Entity<Reset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_reset");

            entity.ToTable("reset", "identity");

            entity.HasIndex(e => e.AccountId, "u_reset").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedOn).HasColumnName("created_on");

            entity.HasOne(d => d.Account).WithOne(p => p.Reset)
                .HasForeignKey<Reset>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reset_account");
        });

        modelBuilder.Entity<Verification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_verification");

            entity.ToTable("verification", "identity");

            entity.HasIndex(e => e.AccountId, "u_verification").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CreatedOn).HasColumnName("created_on");

            entity.HasOne(d => d.Account).WithOne(p => p.Verification)
                .HasForeignKey<Verification>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_verification_account");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
