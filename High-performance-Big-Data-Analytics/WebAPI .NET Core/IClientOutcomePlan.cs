/*
Author          : System Analyst Oleg Gorlov
Description:	: Interface OutcomePlan. 
Copyright       : 
email           : 
Date            : 06/08/2019
Release         : 1.0.0
Comment         : Implementation MVC6, .NET C# 7, .NET Core 3
*/
using System;
using System.Collections.Generic;

namespace i4C.Models.CASA.ClientOutcomePlan
{
    public interface IClientOutcomePlan
    {
        int ClientOutcomePlanId { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        string Name { get; set; }
        DateTime NextReviewDueDate { get; set; }
        string ServiceOffice { get; set; }
        string EmployabilityRating { get; set; }
        bool ParticipationRequired { get; set; }
        string ReferredTo { get; set; }
        string JobGoal { get; set; }
        string JobCategory { get; set; }
        DateTime ResumeUpdate { get; set; }
        DateTime MedicalRestrictionDueStart { get; set; }
        DateTime MedicalRestrictionDueDate { get; set; }   
        int AddMoreMonths { get; set; }  
...