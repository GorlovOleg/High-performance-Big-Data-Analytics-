/*
Author          : Sr Programmer Analyst Oleg Gorlov
Description:	: DAL to access SQL database tables. 
                : Database context is the main class that coordinates Entity Framework functionality .NET Core
Copyright       : GMedia-IT-Consulting 
email           : oleg_gorlov@yahoo.com
Date            : 22/12/2019
Release         : 1.0.0
Comment         : Implementation WebAPI2 .NET Core 3  Update - 1.1.1 Released

 */
using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Configuration;

using i4C.Models.IndicatorCategory;
using i4C.Models.IndicatorSubCategory;
using i4C.Models.Indicator;
using i4C.Models.IndicatorSegment;
using i4C.Models.DoctorIndicator;
using i4C.Models.PracticeDoctors;
using i4C.Models.IndicatorData;
using i4C.Models.IndicatorPatient;
using i4C.Models.IndicatorGraphicType;
using i4C.Models.IndicatorPatient;
using System.Configuration;

namespace i4C.DAL
{
    public partial class C3_DASHBOARDDbContext : DbContext
    {
        //private readonly ILogger<SAMSDbContext> _logger;
        //private readonly SAMSDbContext _context;

        private readonly IConfiguration _configuration;

        public C3_DASHBOARDDbContext(DbContextOptions<C3_DASHBOARDDbContext> options)  
            : base(options)
        { }

        public C3_DASHBOARDDbContext() 
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                var connectionString_ = new Startup(_configuration);
                var connectionstring = connectionString_.connectionString1;
                optionsBuilder.UseSqlServer(@connectionstring);

 //---               optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["connectionStringWebConfig1"].ConnectionString);
             }
        }

        public DbSet<IndicatorCategory> IndicatorCategorys { get; set; }
        public DbSet<IndicatorSubCategory> IndicatorSubCategorys { get; set; }
        public DbSet<Indicator> Indicators { get; set; }
        public DbSet<IndicatorSegment> IndicatorSegments  { get; set; }
        public DbSet<DoctorIndicator> DoctorIndicators { get; set; }
        public DbSet<PracticeDoctors> PracticeDoctors  { get; set; }
        public DbSet<IndicatorData> IndicatorDatas { get; set; }
        public DbSet<IndicatorPatient> IndicatorPatients { get; set; }
        public DbSet<IndicatorGraphicType> IndicatorGraphicTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.ForSqlServer().UseIdentity();

            #region IndicatorCategory
            builder.Entity<IndicatorCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            builder.Entity<IndicatorCategory>()
            .Property<int>(c => c.Id);
            #endregion

            #region IndicatorSubCategory
            builder.Entity<IndicatorSubCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            builder.Entity<IndicatorSubCategory>()
            .Property<int>(c => c.Id);
            #endregion

            #region Indicator
            builder.Entity<Indicator>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            builder.Entity<Indicator>()
            .Property<int>(c => c.Id);
            #endregion

            #region IndicatorSegment
            builder.Entity<IndicatorSegment>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            builder.Entity<IndicatorSegment>()
            .Property<int>(c => c.Id);
            #endregion

            #region DoctorIndicator
            builder.Entity<DoctorIndicator>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            builder.Entity<DoctorIndicator>()
            .Property<int>(c => c.Id);
            #endregion

            #region PracticeDoctor
            builder.Entity<PracticeDoctors>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
            builder.Entity<PracticeDoctors>()
            .Property<int>(c => c.Id);
            #endregion

...
