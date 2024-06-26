﻿// <auto-generated />
using System;
using Korga;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Korga.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20230427101434_InboxOutbox")]
    partial class InboxOutbox
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Korga.ChurchTools.Entities.Group", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("GroupTypeId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("GroupTypeId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupMember", b =>
                {
                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("GroupRoleId")
                        .HasColumnType("int");

                    b.HasKey("PersonId", "GroupId");

                    b.HasIndex("GroupId");

                    b.HasIndex("GroupRoleId");

                    b.ToTable("GroupMembers");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupRole", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("GroupTypeId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("GroupTypeId");

                    b.ToTable("GroupRoles");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("GroupTypes");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Person", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("StatusId");

                    b.ToTable("People");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Status", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Status");
                });

            modelBuilder.Entity("Korga.EmailDelivery.Entities.OutboxEmail", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<DateTime>("DeliveryTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("longtext");

                    b.Property<long?>("InboxEmailId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("InboxEmailId");

                    b.ToTable("OutboxEmails");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.DistributionList", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasAlternateKey("Alias");

                    b.ToTable("DistributionLists");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.InboxEmail", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<byte[]>("Body")
                        .HasColumnType("longblob");

                    b.Property<long?>("DistributionListId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DownloadTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<string>("From")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<byte[]>("Header")
                        .HasColumnType("longblob");

                    b.Property<DateTime>("ProcessingCompletedTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Receiver")
                        .HasColumnType("longtext");

                    b.Property<string>("ReplyTo")
                        .HasColumnType("longtext");

                    b.Property<string>("Sender")
                        .HasColumnType("longtext");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("To")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<uint>("UniqueId")
                        .HasColumnType("int unsigned");

                    b.HasKey("Id");

                    b.HasIndex("DistributionListId");

                    b.ToTable("InboxEmails");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.PersonFilter", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long>("DistributionListId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("DistributionListId");

                    b.ToTable("PersonFilters");

                    b.HasDiscriminator<string>("Discriminator").HasValue("PersonFilter");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Korga.EventRegistration.Entities.Event", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("Korga.EventRegistration.Entities.EventParticipant", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("FamilyName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("GivenName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long>("ProgramId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("ProgramId");

                    b.ToTable("EventParticipants");
                });

            modelBuilder.Entity("Korga.EventRegistration.Entities.EventProgram", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long>("EventId")
                        .HasColumnType("bigint");

                    b.Property<int>("Limit")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.ToTable("EventPrograms");
                });

            modelBuilder.Entity("Korga.Ldap.Entities.PasswordReset", b =>
                {
                    b.Property<Guid>("Token")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Expiry")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Token");

                    b.ToTable("PasswordResets");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.GroupFilter", b =>
                {
                    b.HasBaseType("Korga.EmailRelay.Entities.PersonFilter");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int?>("GroupRoleId")
                        .HasColumnType("int");

                    b.HasIndex("GroupId");

                    b.HasIndex("GroupRoleId");

                    b.HasDiscriminator().HasValue("GroupFilter");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.SinglePerson", b =>
                {
                    b.HasBaseType("Korga.EmailRelay.Entities.PersonFilter");

                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.HasIndex("PersonId");

                    b.HasDiscriminator().HasValue("SinglePerson");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.StatusFilter", b =>
                {
                    b.HasBaseType("Korga.EmailRelay.Entities.PersonFilter");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.HasIndex("StatusId");

                    b.HasDiscriminator().HasValue("StatusFilter");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Group", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.GroupType", "GroupType")
                        .WithMany()
                        .HasForeignKey("GroupTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupType");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupMember", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Korga.ChurchTools.Entities.GroupRole", "GroupRole")
                        .WithMany()
                        .HasForeignKey("GroupRoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Korga.ChurchTools.Entities.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("GroupRole");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupRole", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.GroupType", "GroupType")
                        .WithMany()
                        .HasForeignKey("GroupTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupType");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Person", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Status", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Korga.EmailDelivery.Entities.OutboxEmail", b =>
                {
                    b.HasOne("Korga.EmailRelay.Entities.InboxEmail", "InboxEmail")
                        .WithMany("Recipients")
                        .HasForeignKey("InboxEmailId");

                    b.Navigation("InboxEmail");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.InboxEmail", b =>
                {
                    b.HasOne("Korga.EmailRelay.Entities.DistributionList", "DistributionList")
                        .WithMany()
                        .HasForeignKey("DistributionListId");

                    b.Navigation("DistributionList");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.PersonFilter", b =>
                {
                    b.HasOne("Korga.EmailRelay.Entities.DistributionList", "DistributionList")
                        .WithMany("Filters")
                        .HasForeignKey("DistributionListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DistributionList");
                });

            modelBuilder.Entity("Korga.EventRegistration.Entities.EventParticipant", b =>
                {
                    b.HasOne("Korga.EventRegistration.Entities.EventProgram", "Program")
                        .WithMany("Participants")
                        .HasForeignKey("ProgramId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Program");
                });

            modelBuilder.Entity("Korga.EventRegistration.Entities.EventProgram", b =>
                {
                    b.HasOne("Korga.EventRegistration.Entities.Event", "Event")
                        .WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.GroupFilter", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Korga.ChurchTools.Entities.GroupRole", "GroupRole")
                        .WithMany()
                        .HasForeignKey("GroupRoleId");

                    b.Navigation("Group");

                    b.Navigation("GroupRole");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.SinglePerson", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.StatusFilter", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Status", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.DistributionList", b =>
                {
                    b.Navigation("Filters");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.InboxEmail", b =>
                {
                    b.Navigation("Recipients");
                });

            modelBuilder.Entity("Korga.EventRegistration.Entities.EventProgram", b =>
                {
                    b.Navigation("Participants");
                });
#pragma warning restore 612, 618
        }
    }
}
