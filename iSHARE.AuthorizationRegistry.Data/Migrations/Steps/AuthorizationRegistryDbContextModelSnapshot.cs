﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace iSHARE.AuthorizationRegistry.Data.Migrations.Steps
{
    [DbContext(typeof(AuthorizationRegistryDbContext))]
    partial class AuthorizationRegistryDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.8-servicing-32085")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("iSHARE.AuthorizationRegistry.Core.Models.Delegation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccessSubject")
                        .IsRequired();

                    b.Property<string>("AuthorizationRegistryId")
                        .IsRequired();

                    b.Property<Guid?>("CreatedById");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Policy")
                        .IsRequired();

                    b.Property<string>("PolicyIssuer")
                        .IsRequired();

                    b.Property<Guid?>("UpdatedById");

                    b.Property<DateTime>("UpdatedDate");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("UpdatedById");

                    b.ToTable("Delegations");
                });

            modelBuilder.Entity("iSHARE.AuthorizationRegistry.Core.Models.DelegationHistory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("CreatedById");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<Guid>("DelegationId");

                    b.Property<string>("Policy");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("DelegationId");

                    b.ToTable("DelegationsHistories");
                });

            modelBuilder.Entity("iSHARE.AuthorizationRegistry.Core.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<string>("AspNetUserId");

                    b.Property<DateTime>("CreatedDate");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Name");

                    b.Property<string>("PartyId");

                    b.Property<string>("PartyName");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("iSHARE.AuthorizationRegistry.Core.Models.Delegation", b =>
                {
                    b.HasOne("iSHARE.AuthorizationRegistry.Core.Models.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("iSHARE.AuthorizationRegistry.Core.Models.User", "UpdatedBy")
                        .WithMany()
                        .HasForeignKey("UpdatedById")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("iSHARE.AuthorizationRegistry.Core.Models.DelegationHistory", b =>
                {
                    b.HasOne("iSHARE.AuthorizationRegistry.Core.Models.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById");

                    b.HasOne("iSHARE.AuthorizationRegistry.Core.Models.Delegation", "Delegation")
                        .WithMany("DelegationHistory")
                        .HasForeignKey("DelegationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}