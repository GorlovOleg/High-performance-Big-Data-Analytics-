/*
Author          : Technical  Architect / Application Developer Oleg Gorlov
Description:	: Class DataDbContext to access SQL database tables. 
                : Class is  that coordinates Entity Framework functionality .NET Core to access SQL database Tables and relations by bussines rules.
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 08/17/2017
Release         : 1.0.0
Comment         : 
*/

using System;
using System.Linq;
//using Microsoft.Data.Entity;
//using Microsoft.Data.Entity.Infrastructure;
//using OntarioRealEstate.Models.Data;
using System.ComponentModel.DataAnnotations.Schema;
using DAC.LPM.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using Microsoft.Extensions.Options;

namespace DAC.LPM.SQLServer
{
    public class DataDbContext : DbContext 
    {
        // requires using Microsoft.Extensions.Configuration;
        private  IConfiguration Configuration;

        public void TestModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public DbSet<ReviewPage> ReviewPages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Interaction> Interactions { get; set; }

        //public string getConString()
        //{
        //    string constring = ConfigurationManager.ConnectionStrings["DataDbContext"].ToString();
        //    return constring;
        //}


        public DataDbContext() : base() 
        {
 
        }

        //public IConfiguration Configuration { get; }


        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {

            //string connectionString = ConfigurationManager.ConnectionStrings["DataDbContext"].ConnectionString;
            //builder.UseSqlServer(connectionString);
            //builder.UseSqlServer(@connectionstring);
            builder.UseSqlServer(@"Server=PC-OLEG\SQLEXPRESS;Database=DACgroup;Trusted_Connection=True;");
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.ForSqlServer().UseIdentity();

            #region ReviewPage
            builder.Entity<ReviewPage>()
                .HasKey(c => c.ReviewPageId);
            builder.Entity<ReviewPage>()
                .Property<DateTime>("LastUpdated");
            #endregion

            #region Review
            builder.Entity<Review>()
                .HasKey(c => c.ReviewId);

            //--- one to zero-or-one
            //builder.Entity<Review>()
            //    .HasOne(i => i.Author)
            //    .WithOne(p => p.Review);
            #endregion

            #region Interaction
            builder.Entity<Interaction>()
                .HasKey(c => c.InteractionId);

            //builder.Entity<Interaction>()
            //    .Property(x => x.InteractionId)
            //    //.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
            //    .IsRequired()
            //    .HasColumnName("InteractionId");

            builder.Entity<Interaction>()
                .Property(x => x.InteractionType)
                .IsRequired()
                .HasMaxLength(25);

            //builder.Entity<Interaction>()
            //    .HasOne(x => x.Parent)
            //    .WithMany(x => x.Children)
            //    .HasForeignKey(x => x.ParentId);
                //.WillCascadeOnDelete(false);

            //--- one to zero-or-one
            //builder.Entity<Response>()
            //    .HasOne(i => i.Author)
            //    .WithOne(p => p.Response);

            #endregion

        }
...