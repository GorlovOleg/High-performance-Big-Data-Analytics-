/*
Author          : Sr Programmer Analyst Oleg Gorlov
Description:	: Model Class OutcomePlan. 
Copyright       :  
email           : 
Date            : 08/19/2019
Release         : 1.0.0
Comment         : Implementation MVC6, .NET C#, .NET Core 3
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using i4C.Models.CASA.Comment_Log;

namespace i4C.Models.CASA.ClientOutcomePlan
{
    [Table("ClientOutcomePlan")]
    public class ClientOutcomePlan : IClientOutcomePlan
    {
        public ClientOutcomePlan()
        {
        }

        [Key]
        //--- 1
        public int ClientOutcomePlanId { get; set; }

        //---
        //--- 2
        [StringLength(30, ErrorMessage = "Field CreatedBy cannot be longer than 30 characters.")]
        [Column("CreatedBy")]
        [Display(Name = "CreatedBy")]
        public string CreatedBy { get; set; }

        //--- 3  
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please select Date Created")]
        public DateTime CreatedDate { get; set; }

        //--- 4
        [StringLength(30, ErrorMessage = "Name cannot be longer than 30 characters.")]
        [Column("Name")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        //--- 5
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please select Date NextReviewDueDate")]
        public DateTime NextReviewDueDate { get; set; }

        //--- 6
        [StringLength(30, ErrorMessage = "Field EmployabilityRating cannot be longer than 30 characters.")]
        [Column("EmployabilityRating")]
        [Display(Name = "Employability Rating ")]
        public string EmployabilityRating { get; set; }

        //--- 7
        [Column("ParticipationRequired")]
        [Display(Name = "ParticipationRequired")]
        public bool ParticipationRequired { get; set; }

        //--- 8
        [StringLength(30, ErrorMessage = "ReferredTo cannot be longer than 30 characters.")]
        [Column("ReferredTo")]
        [Display(Name = "ReferredTo")]
        public string ReferredTo { get; set; }

        //--- 9
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please select Date MedicalRestrictionDueStart")]
        public DateTime MedicalRestrictionDueStart { get; set; }

        //--- 10
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please select  Date MedicalRestrictionDueDate")] 
        public DateTime MedicalRestrictionDueDate { get; set; } 

...