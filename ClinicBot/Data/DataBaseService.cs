﻿using ClinicBot.Common.Models.MedicalAppointment;
using ClinicBot.Common.Models.Qualification;
using ClinicBot.Common.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicBot.Data
{
    public class DataBaseService: DbContext, IDataBaseService
    {
        public DataBaseService(DbContextOptions options) : base(options)
        {
            Database.EnsureCreatedAsync();
        }
        public DataBaseService()
        {
            Database.EnsureCreatedAsync();
        }
        public DbSet<UserModel> User { get; set; }
        public DbSet<QualificationModel> Qualification { get; set; }
        public DbSet<MedicalAppointmentModel> MedicalAppointment { get; set; }

        public async Task<bool> SaveAsync()
        {
            return (await SaveChangesAsync() > 0);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>().ToContainer("User").HasPartitionKey("channel").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<QualificationModel>().ToContainer("Qualification").HasPartitionKey("idUser").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<QualificationModel>().ToContainer("MedicalAppointment").HasPartitionKey("idUser").HasNoDiscriminator().HasKey("id");

        }
    }
}
