/*
Author          : Application Developer Oleg Gorlov
Description:	: Unit Test ReviewManagementTests.cs to check the functionality of serve SQL database tables. 
                : Class is  that include methods to insert and update SQL database Tables and used Entity Framework functionality .NET Core.
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 08/28/2017
Release         : 1.1.0
Comment         : New ReviewPage, Review Class Model
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Data.SqlClient;
using DAC.LPM.ReviewDataDomain;
using DAC.LPM.ReviewDataDomain.SQLServer;
using DAC.LPM.SQLServer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
//using BusinessLogic;
using NLog;

namespace DAC.LPM.ReviewDataDomain.SQLServer.Tests
{
    [TestClass]
    public class ReviewManagementTests : IDisposable
    {
        #region Properties

        ReviewManagement _reviewManagement = new ReviewManagement();

        ReviewPage _reviewPage;
        List<Review> _reviews;
        Review _review;
        Response _response;
        List<Comment> _comments;
        Author _author;

        string _unique;

        #endregion

        #region Initialize

        [TestInitialize]
        public void Initialize()
        {

        }

        #endregion

        #region Logger
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        #region Cleanup

        //[TestCleanup]
        //public void Cleanup()
        //{
        //    var reviewPageBuilder = Builders<BsonDocument>.Filter;
        //    var reviewPageFilter = reviewPageBuilder.Eq("Id", _reviewPage.Id);
        //    _reviewPageCollection.DeleteMany(reviewPageFilter);

        //    var reviewBuilder = Builders<BsonDocument>.Filter;
        //    var reviewFilter = reviewBuilder.Eq("ReviewPageId",
        //        _reviews.First().ReviewPageId);
        //    _reviewCollection.DeleteMany(reviewFilter);

        //    var commentBuilder = Builders<BsonDocument>.Filter;
        //    var commentFilter = commentBuilder.Eq("Id",
        //        string.Concat(_comments.First().Id, _unique));
        //    _commentCollection.DeleteMany(commentFilter);
        //}

        #endregion

        #region Insert_to_SQL_Tables_Use_EF
        [TestMethod]
        public void Insert_to_SQL_Tables_Use_EF()
        {

            var sourceName = "Google";
            var callMethod = "Api";
            var accountId = "105294769132327896230";
            var locationId = "14880430448251263796";
            var externalId = "2366450684051732373";


            var reviewPageComp = string.Concat(sourceName, callMethod);
            reviewPageComp = string.Concat(reviewPageComp, accountId);
            reviewPageComp = string.Concat(reviewPageComp, locationId);
            reviewPageComp = string.Concat(reviewPageComp, externalId);

            try
            {
                using (DataDbContext context = new DataDbContext())
                {
                    var reviewPage = new DAC.LPM.SQLServer.ReviewPage
                    {
                        SourceName = sourceName,
                        CallMethod = callMethod,
                        AccountId = accountId,
                        LocationId = locationId,
                        ExternalId = externalId,
                        AverageRating = 0.0M,
                        //Created = DateTime.Today,
                        Updated = DateTime.Today,
                        LastUpdated = DateTime.Today,
                        Reviews = new List<DAC.LPM.SQLServer.Review>
                        {
                            new DAC.LPM.SQLServer.Review
                            {
                            ParsedId = "ParsedId",
                            AuthorName = "AuthorName",
                            AuthorProfileUrl = "AuthorProfileUrl",
                            AuthorIconUrl = "AuthorIconUrl",
                            AuthorExternalId = "AuthorExternalId",
                            ReviewPageId_FK = reviewPageComp,
                            Content = "Content for review",
                            Rating = 1,
                            Created = DateTime.Today.ToString(),
                            Updated = "Review Created 2",
                            LastUpdated = DateTime.Today,
                            Interactions = new List<Interaction>
                                {
                                    new Interaction
                                    {
                                        InteractionType = "Comment",
                                        ParentType = "Review",
                                        AuthorName = "Interaction AuthorName",
                                        AuthorProfileUrl = "AuthorProfileUrl",
                                        AuthorIconUrl = "AuthorIconUrl",
                                        AuthorExternalId = "AuthorExternalId",
                                        Content = "Content for Interaction",
                                        Created = DateTime.Today.ToString(),
                                        CreatedTime = DateTime.Today,
                                        LastUpdated = DateTime.Today
                                    }
                                }
                            }
                        }
                    };

                    context.ReviewPages.Add(reviewPage);
                    context.SaveChanges();
                }
                /*
                https://msdn.microsoft.com/en-us/library/jj573936(v=vs.113).aspx
                Finding entities using a query

                */


                // Use a separate instance of the context to verify correct data was saved to database
                using (DataDbContext context = new DataDbContext())
                {
                    //---Assert.AreEqual("GoogleApi105294769132327896230148804304482512637962366450684051732373", context.Reviews.Single().ReviewPageComp); 
                    //Assert.AreEqual("Google", context.ReviewPages.First().SourceName);
                    Assert.AreEqual("GoogleApi105294769132327896230148804304482512637962366450684051732373", context.Reviews.First().ReviewPageId_FK);
                    //---Assert.AreEqual("Interaction AuthorName", context.Interactions.First().AuthorName);

                }

            }
            finally
            {
                //  context.Database.CloseConnection();
            }
        }
        #endregion

        #region Insert_to_SQL_Tables_Use_BusinessLogic
        [TestMethod]
        public void Insert_to_SQL_Tables_Use_BusinessLogic()
        {

            try
            {
                var sourceName = "FaceBook";
                var callMethod = "Api";
                var accountId = "105294769132327896230";
                var locationId = "14880430448251263796";
                var externalId = "2366450684051732373";


                var reviewPageComp = string.Concat(sourceName, callMethod);
                reviewPageComp = string.Concat(reviewPageComp, accountId);
                reviewPageComp = string.Concat(reviewPageComp, locationId);
                reviewPageComp = string.Concat(reviewPageComp, externalId);

                // Insert seed data into the database using one instance of the context
                using (DataDbContext context = new DataDbContext())
                {
                    var reviewPage = new DAC.LPM.SQLServer.ReviewPage
                    {
                        SourceName = sourceName,
                        CallMethod = callMethod,
                        AccountId = accountId,
                        LocationId = locationId,
                        ExternalId = externalId,
                        AverageRating = 0.0M,
                        //Created = DateTime.Today,
                        Updated = DateTime.Today,
                        LastUpdated = DateTime.Today,
                        Reviews = new List<DAC.LPM.SQLServer.Review>
                        {
                            new DAC.LPM.SQLServer.Review
                            {
                            ParsedId = "ParsedId",
                            AuthorName = "AuthorName",
                            AuthorProfileUrl = "AuthorProfileUrl",
                            AuthorIconUrl = "AuthorIconUrl",
                            AuthorExternalId = "AuthorExternalId",
                            ReviewPageId_FK = reviewPageComp,
                            Content = "Content for review",
                            //ContentLength = 20,
                            Rating = 1,
                            Created = DateTime.Today.ToString(),
                            Updated = "Review Created 2",
                            LastUpdated = DateTime.Today,
                            Interactions = new List<Interaction>
                                {
                                    new Interaction
                                    {
                                        InteractionType = "Response",
                                        ParentType = "Comment",
                                        AuthorName = "AuthorName",
                                        AuthorProfileUrl = "AuthorProfileUrl",
                                        AuthorIconUrl = "AuthorIconUrl",
                                        AuthorExternalId = "AuthorExternalId",
                                        Content = "Content for Interaction",
                                        //ContentLength = 20,
                                        Created = DateTime.Today.ToString(),
                                        CreatedTime = DateTime.Today,
                                        LastUpdated = DateTime.Today
                                    }
                                }
                            }
                        }
                    };

                    context.ReviewPages.Add(reviewPage);
                    context.SaveChanges();
                }

                using (DataDbContext context = new DataDbContext())
                {
                    var service = new SQLServerServices(context);
                    var result = service.Find("FaceBook");

                    logger.Debug($"Total # of Reviews ({result.Count()}) ");

                    //---Assert.AreEqual("FaceBook", context.ReviewPages.Find().SourceName);
                    //Assert.AreEqual("FaceBook", context.ReviewPages.Find().SourceName);
                    //---Assert.AreEqual("FaceBookApi105294769132327896230148804304482512637962366450684051732373", context.Reviews.Single().ReviewPageComp);
                    Assert.AreEqual("FaceBook", result.First().SourceName);
                }
            }
            finally
            {
                //  context.Database.CloseConnection();
            }

        }
        #endregion

        #region GetReviews
        [TestMethod]
        public void Select_All_Reviews_From_ReviewPage_By_ReviewPageId()
        {

            var sourceName = "Google";
            var callMethod = "Api";
            var accountId = "105294769132327896230";
            var locationId = "14880430448251263796";
            var externalId = "2366450684051732373";

            var reviewPageComp = string.Concat(sourceName, callMethod);
            reviewPageComp = string.Concat(reviewPageComp, accountId);
            reviewPageComp = string.Concat(reviewPageComp, locationId);
            reviewPageComp = string.Concat(reviewPageComp, externalId);

            var reviews = _reviewManagement.GetReviews(reviewPageComp);

           Assert.IsTrue(reviews.Count() > 0);

        }

        [TestMethod]
        public void Select_All_ReviewPages_From_ReviewPage_By_Complex_PK_ReviewPage_()
        {
            var reviews = _reviewManagement.GetReviews(_review.ReviewPageId);

            Assert.IsTrue(reviews.Count() > 0);
        }

        #endregion

        #region GetReviewPages

        [TestMethod]
        public void Select_All_ReviewPages_From_ReviewPage_By_Complex_PK_ReviewPage() 
        {
            var SourceName = "FaceBook";
            var AccountId = "105294769132327896230";
            var LocationId = "14880430448251263796";
            var ExternalId = "2366450684051732373";
            var CallMethod = "Api";

            var reviews = _reviewManagement.GetReviewPages(
                SourceName,
                AccountId,
                LocationId,
                ExternalId,
                CallMethod);

            Assert.IsTrue(reviews.Count() > 0);
        }

        #endregion

        //---
        private void Insert_Or_Update_ReviewPage_Review_Interaction(ReviewPage reviewPage, List<Review> reviews)
        {

            try
            {
                using (DataDbContext context = new DataDbContext())
                {
                    //var reviewPage_ = context.ReviewPages.Find().ReviewPageId
                }

                var sourceName = "FaceBook";
                var callMethod = "Api";
                var accountId = "105294769132327896230";
                var locationId = "14880430448251263796";
                var externalId = "2366450684051732373";


                var reviewPageComp = string.Concat(sourceName, callMethod);
                reviewPageComp = string.Concat(reviewPageComp, accountId);
                reviewPageComp = string.Concat(reviewPageComp, locationId);
                reviewPageComp = string.Concat(reviewPageComp, externalId);

                // Insert seed data into the database using one instance of the context
                using (DataDbContext context = new DataDbContext())
                {
                    DAC.LPM.SQLServer.ReviewPage _reviewPage = new DAC.LPM.SQLServer.ReviewPage
                    {
                        SourceName = sourceName,
                        CallMethod = callMethod,
                        AccountId = accountId,
                        LocationId = locationId,
                        ExternalId = externalId,
                        AverageRating = 0.0M,
                        //Created = DateTime.Today,
                        Updated = DateTime.Today,
                        LastUpdated = DateTime.Today,

                        Reviews = new List<DAC.LPM.SQLServer.Review>
                        {
                            new DAC.LPM.SQLServer.Review
                            {
                            ParsedId = "ParsedId",
                            AuthorName = "AuthorName",
                            AuthorProfileUrl = "AuthorProfileUrl",
                            AuthorIconUrl = "AuthorIconUrl",
                            AuthorExternalId = "AuthorExternalId",
                            ReviewPageId_FK = reviewPageComp,
                            Content = "Content for review",
                            //ContentLength = 20,
                            Rating = 1,
                            Created = DateTime.Today.ToString(),
                            Updated = DateTime.Today.ToString(),
                            LastUpdated = DateTime.Today,

                Interactions = new List<Interaction>
                    {
                        new Interaction
                        {
                            InteractionType = "InteractionType",
                            ParentType = "ParentType",
                            AuthorName = "AuthorName",
                            AuthorProfileUrl = "AuthorProfileUrl",
                            AuthorIconUrl = "AuthorIconUrl",
                            AuthorExternalId = "AuthorExternalId",
                            Content = DateTime.Today.ToString(),
                            //ContentLength = 20,
                            Created = DateTime.Today.ToString(),
                            CreatedTime = DateTime.Today,
                            LastUpdated = DateTime.Today
                        }
                    }
                }
            }
                    };

                    context.ReviewPages.Add(_reviewPage);
                    context.SaveChanges();
                }



                using (DataDbContext context = new DataDbContext())
                {
                    var service = new SQLServerServices(context);
                    var result = service.Find("FaceBook");


                    //Assert.AreEqual("ReviewPage SourceName Test", context.ReviewPages.Find().SourceName);
                    //Assert.Equals("ReviewPage SourceName Test", context.ReviewPages.First().SourceName);
                    //Assert.AreEqual("Review AuthorName", context.Reviews.First().AuthorName);
                    //Assert.AreEqual("SQLServerTests ReviewPage SourceName Test", result.First().SourceName);

                    //--- Assert.AreEqual(2, result.Count());

                    logger.Debug($"Total # of Reviews ({result.Count()}) ");
                }
            }
            catch (Exception ex)
            {
                logger.Error(
                    $"Exception Error: {ex.Message} Details: {ex.StackTrace} InnerException: {ex.InnerException}");
                throw ex;
            }
            finally
            {
                //  context.Database.CloseConnection();
            }
        }

        public void Dispose()
        {
            //   context.Database.EnsureDeleted();
        }

    }
}
