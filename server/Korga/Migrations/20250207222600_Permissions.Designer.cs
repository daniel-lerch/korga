﻿// <auto-generated />
using System;
using Korga;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Korga.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20250207222600_Permissions")]
    partial class Permissions
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Korga.ChurchTools.Entities.Department", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("DeletionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.DepartmentMember", b =>
                {
                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.Property<int>("DepartmentId")
                        .HasColumnType("int");

                    b.HasKey("PersonId", "DepartmentId");

                    b.HasIndex("DepartmentId");

                    b.ToTable("DepartmentMembers");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Group", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("DeletionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("GroupStatusId")
                        .HasColumnType("int");

                    b.Property<int>("GroupTypeId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("GroupStatusId");

                    b.HasIndex("GroupTypeId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupMember", b =>
                {
                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int>("GroupMemberStatus")
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

                    b.Property<DateTime>("DeletionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("GroupTypeId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("GroupTypeId");

                    b.ToTable("GroupRoles");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("DeletionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("GroupStatuses");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.GroupType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("DeletionTime")
                        .HasColumnType("datetime(6)");

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

                    b.Property<DateTime>("DeletionTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("StatusId");

                    b.ToTable("People");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Status", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("DeletionTime")
                        .HasColumnType("datetime(6)");

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

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<byte[]>("Content")
                        .IsRequired()
                        .HasColumnType("longblob");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<long?>("InboxEmailId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("InboxEmailId");

                    b.ToTable("OutboxEmails");
                });

            modelBuilder.Entity("Korga.EmailDelivery.Entities.SentEmail", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<int>("ContentSize")
                        .HasColumnType("int");

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

                    b.HasIndex("DeliveryTime");

                    b.HasIndex("InboxEmailId");

                    b.ToTable("SentEmails");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.DistributionList", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<int>("Flags")
                        .HasColumnType("int");

                    b.Property<long?>("PermittedRecipientsId")
                        .HasColumnType("bigint");

                    b.Property<long?>("PermittedSendersId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasAlternateKey("Alias");

                    b.HasIndex("PermittedRecipientsId");

                    b.HasIndex("PermittedSendersId");

                    b.ToTable("DistributionLists");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.InboxEmail", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<byte[]>("Body")
                        .HasColumnType("longblob");

                    b.Property<long?>("DistributionListId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DownloadTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                    b.Property<string>("From")
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
                        .HasColumnType("longtext");

                    b.Property<string>("To")
                        .HasColumnType("longtext");

                    b.Property<uint?>("UniqueId")
                        .HasColumnType("int unsigned");

                    b.HasKey("Id");

                    b.HasIndex("DistributionListId");

                    b.HasIndex("ProcessingCompletedTime");

                    b.HasIndex("UniqueId")
                        .IsUnique();

                    b.ToTable("InboxEmails");
                });

            modelBuilder.Entity("Korga.Filters.Entities.Permission", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("varchar(255)");

                    b.Property<long>("PersonFilterListId")
                        .HasColumnType("bigint");

                    b.HasKey("Key");

                    b.HasIndex("PersonFilterListId");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Korga.Filters.Entities.PersonFilter", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("varchar(21)");

                    b.Property<string>("EqualityKey")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("varchar(255)")
                        .HasComputedColumnSql("\r\nCONCAT(\r\n    LPAD(HEX(IFNULL(`GroupId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`GroupRoleId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`GroupTypeId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`PersonId`, 0)), 8, '0'),\r\n    LPAD(HEX(IFNULL(`StatusId`, 0)), 8, '0'))\r\n");

                    b.Property<long>("PersonFilterListId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("PersonFilterListId", "Discriminator", "EqualityKey")
                        .IsUnique();

                    b.ToTable("PersonFilters");

                    b.HasDiscriminator().HasValue("PersonFilter");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Korga.Filters.Entities.PersonFilterList", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.HasKey("Id");

                    b.ToTable("PersonFilterLists");
                });

            modelBuilder.Entity("Korga.Filters.Entities.GroupFilter", b =>
                {
                    b.HasBaseType("Korga.Filters.Entities.PersonFilter");

                    b.Property<int>("GroupId")
                        .HasColumnType("int");

                    b.Property<int?>("GroupRoleId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int")
                        .HasColumnName("GroupRoleId");

                    b.HasIndex("GroupId");

                    b.HasIndex("GroupRoleId");

                    b.HasDiscriminator().HasValue("GroupFilter");
                });

            modelBuilder.Entity("Korga.Filters.Entities.GroupTypeFilter", b =>
                {
                    b.HasBaseType("Korga.Filters.Entities.PersonFilter");

                    b.Property<int?>("GroupRoleId")
                        .ValueGeneratedOnUpdateSometimes()
                        .HasColumnType("int")
                        .HasColumnName("GroupRoleId");

                    b.Property<int>("GroupTypeId")
                        .HasColumnType("int");

                    b.HasIndex("GroupRoleId");

                    b.HasIndex("GroupTypeId");

                    b.HasDiscriminator().HasValue("GroupTypeFilter");
                });

            modelBuilder.Entity("Korga.Filters.Entities.SinglePerson", b =>
                {
                    b.HasBaseType("Korga.Filters.Entities.PersonFilter");

                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.HasIndex("PersonId");

                    b.HasDiscriminator().HasValue("SinglePerson");
                });

            modelBuilder.Entity("Korga.Filters.Entities.StatusFilter", b =>
                {
                    b.HasBaseType("Korga.Filters.Entities.PersonFilter");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.HasIndex("StatusId");

                    b.HasDiscriminator().HasValue("StatusFilter");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.DepartmentMember", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Korga.ChurchTools.Entities.Person", "Person")
                        .WithMany("Departments")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Department");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Group", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.GroupStatus", "GroupStatus")
                        .WithMany()
                        .HasForeignKey("GroupStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Korga.ChurchTools.Entities.GroupType", "GroupType")
                        .WithMany()
                        .HasForeignKey("GroupTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupStatus");

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
                        .WithMany()
                        .HasForeignKey("InboxEmailId");

                    b.Navigation("InboxEmail");
                });

            modelBuilder.Entity("Korga.EmailDelivery.Entities.SentEmail", b =>
                {
                    b.HasOne("Korga.EmailRelay.Entities.InboxEmail", "InboxEmail")
                        .WithMany()
                        .HasForeignKey("InboxEmailId");

                    b.Navigation("InboxEmail");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.DistributionList", b =>
                {
                    b.HasOne("Korga.Filters.Entities.PersonFilterList", "PermittedRecipients")
                        .WithMany()
                        .HasForeignKey("PermittedRecipientsId");

                    b.HasOne("Korga.Filters.Entities.PersonFilterList", "PermittedSenders")
                        .WithMany()
                        .HasForeignKey("PermittedSendersId");

                    b.Navigation("PermittedRecipients");

                    b.Navigation("PermittedSenders");
                });

            modelBuilder.Entity("Korga.EmailRelay.Entities.InboxEmail", b =>
                {
                    b.HasOne("Korga.EmailRelay.Entities.DistributionList", "DistributionList")
                        .WithMany()
                        .HasForeignKey("DistributionListId");

                    b.Navigation("DistributionList");
                });

            modelBuilder.Entity("Korga.Filters.Entities.Permission", b =>
                {
                    b.HasOne("Korga.Filters.Entities.PersonFilterList", "PersonFilterList")
                        .WithMany()
                        .HasForeignKey("PersonFilterListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PersonFilterList");
                });

            modelBuilder.Entity("Korga.Filters.Entities.PersonFilter", b =>
                {
                    b.HasOne("Korga.Filters.Entities.PersonFilterList", "PersonFilterList")
                        .WithMany("Filters")
                        .HasForeignKey("PersonFilterListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PersonFilterList");
                });

            modelBuilder.Entity("Korga.Filters.Entities.GroupFilter", b =>
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

            modelBuilder.Entity("Korga.Filters.Entities.GroupTypeFilter", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.GroupRole", "GroupRole")
                        .WithMany()
                        .HasForeignKey("GroupRoleId");

                    b.HasOne("Korga.ChurchTools.Entities.GroupType", "GroupType")
                        .WithMany()
                        .HasForeignKey("GroupTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("GroupRole");

                    b.Navigation("GroupType");
                });

            modelBuilder.Entity("Korga.Filters.Entities.SinglePerson", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");
                });

            modelBuilder.Entity("Korga.Filters.Entities.StatusFilter", b =>
                {
                    b.HasOne("Korga.ChurchTools.Entities.Status", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Status");
                });

            modelBuilder.Entity("Korga.ChurchTools.Entities.Person", b =>
                {
                    b.Navigation("Departments");
                });

            modelBuilder.Entity("Korga.Filters.Entities.PersonFilterList", b =>
                {
                    b.Navigation("Filters");
                });
#pragma warning restore 612, 618
        }
    }
}
