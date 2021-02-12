/*
Author          : Technical  Architect / Application Developer Oleg Gorlov
Description:	: Class Model ReviewPage to access SQL database table ReviewPage. 
                : Class is  that coordinates Entity Framework functionality .NET Core to access SQL database.
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 08/28/2017
Release         : 1.0.0
Comment         : 
*/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace DAC.LPM.SQLServer
{
    [Table("ReviewPage")]
    public class ReviewPage
    {
        public ReviewPage()
        {
            LastUpdated = DateTime.UtcNow;
        }

        [Key]
        //--- 1
        public int ReviewPageId { get; set; }

        //--- 2
        //--- SourceName would be Google, Bing etc
        [StringLength(100, ErrorMessage = "SourceName  cannot be longer than 100 characters.")]
        [Column("SourceName")]
        [Display(Name = "SourceName")]
        public string SourceName { get; set; }

        //--- 3
        //--- Call Method would be Api etc
        [StringLength(100, ErrorMessage = "CallMethod  cannot be longer than 100 characters.")]
        [Column("CallMethod")]
        [Display(Name = "CallMethod")]
        public string CallMethod { get; set; }

        //--- 4
        //---  AccountId would be site Client Id
        [StringLength(100, ErrorMessage = "AccountId  cannot be longer than 100 characters.")]
        [Column("AccountId")]
        [Display(Name = "AccountId")]
        public string AccountId { get; set; }

        //--- 5
        //--- LocationId would be Site LocationId
        [StringLength(100, ErrorMessage = "LocationId  cannot be longer than 100 characters.")]
        [Column("LocationId")]
        [Display(Name = "LocationId")]
        public string LocationId { get; set; }

        //--- 6
        //--- ExternalId is the identifier of the url/location/other as was identified during
        //--- fetching/scraping. For example, with Google, it would be the CID value
        [StringLength(100, ErrorMessage = "ExternalId  cannot be longer than 100 characters.")]
        [Column("ExternalId")]
        [Display(Name = "ExternalId")]
        public string ExternalId { get; set; }

        //--- 7
        //--- It is the Review Rating
        [DataType("decimal(2 ,1")]
        [Range(0.0, 5.0, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public decimal? AverageRating { get; set; }

        //--- 8
        //--- It should be the first time scraping the page
        public DateTime? Created { get; set; }

        //--- 9
        // It should be the update time scraping the page
        public DateTime? Updated { get; set; }

        //--- 10
        /// <summary>
        /// Reviews.cs        
        /// DataContext.cs
        /// One-To-Many
        /// May have a Reviews or not
        /// ReviewPage to Reviews by ReviewId_FK
        /// </summary>
        public virtual ICollection<Review> Reviews { get; set; }

        //--- 11
        /// <summary>
        /// LastUpdated
        /// </summary>
        [Editable(false, AllowInitialValue = true)]
        public DateTime LastUpdated { get; set; }
    }
}