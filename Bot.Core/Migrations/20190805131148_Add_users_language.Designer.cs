﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StuddyBot.Core.DAL.Data;

namespace StuddyBot.Core.Migrations
{
    [DbContext(typeof(StuddyBotContext))]
    [Migration("20190805131148_Add_users_language")]
    partial class Add_users_language
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name");

                    b.Property<DateTime>("RegistrationStartDate");

                    b.Property<DateTime>("StartDate");

                    b.HasKey("Id");

                    b.ToTable("Courses");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.Dialogs", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Dialogs");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.Feedback", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Message");

                    b.HasKey("Id");

                    b.ToTable("Feedback");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.MyDialog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("DialogsId");

                    b.Property<string>("Message");

                    b.Property<string>("Sender");

                    b.Property<DateTimeOffset>("Time");

                    b.HasKey("Id");

                    b.HasIndex("DialogsId");

                    b.ToTable("Dialog");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Message");

                    b.HasKey("Id");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConversationReference");

                    b.Property<string>("Email");

                    b.Property<string>("Language");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.UserCourse", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<int>("CourseId");

                    b.HasKey("UserId", "CourseId");

                    b.HasIndex("CourseId");

                    b.ToTable("UserCourses");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.Dialogs", b =>
                {
                    b.HasOne("StuddyBot.Core.DAL.Entities.User", "User")
                        .WithMany("Dialogs")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.MyDialog", b =>
                {
                    b.HasOne("StuddyBot.Core.DAL.Entities.Dialogs", "Dialogs")
                        .WithMany("MyDialogs")
                        .HasForeignKey("DialogsId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("StuddyBot.Core.DAL.Entities.UserCourse", b =>
                {
                    b.HasOne("StuddyBot.Core.DAL.Entities.Course", "Course")
                        .WithMany("UserCourses")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("StuddyBot.Core.DAL.Entities.User", "User")
                        .WithMany("UserCourses")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
