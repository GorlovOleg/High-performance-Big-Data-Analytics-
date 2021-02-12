/*
Author          : Application Developer Oleg Gorlov
Description:	: Class ReviewManagement to serve SQL database tables. 
                : Class is  that include methods to insert and update SQL database Tables and used Entity Framework functionality .NET Core.
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 08/28/2017
Release         : 1.1.0
Comment         : New ReviewPage, Review Class Model
*/

#define SQL
//using Polly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Newtonsoft.Json;

using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using DAC.LPM.SQLServer;
using DAC.LPM.ReviewDataDomain;

using DAC.LPM.ReviewDataDomain.SQLServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DAC.LPM.ReviewDataDomain.SQLServer
{

    public class ReviewManagement : IReviewManagement
    {
        #region Logger
 //       private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        #endregion


        #region property SQL
        public List<DAC.LPM.SQLServer.ReviewPage> _reviewPages = new List<DAC.LPM.SQLServer.ReviewPage>();
        public List<DAC.LPM.SQLServer.Review> _reviews = new List<DAC.LPM.SQLServer.Review>();
        public List<DAC.LPM.SQLServer.Interaction> _interactions = new List<DAC.LPM.SQLServer.Interaction>();
        #endregion

        //--- 1
        #region Implement Interface Method
        public bool  Save(ReviewPage reviewPage, List<Review> reviews, int expectedReviewCount = -1)
        {
            //--return UpsertReviewPageSet(reviewPage, reviews);
            #region SQL

            try
            {

                // Insert seed data into the database using one instance of the context
                using (DataDbContext context = new DataDbContext())
                {

                    var sourceName = !String.IsNullOrEmpty(reviewPage.SourceName) ? reviewPage.SourceName : string.Empty;
                    var callMethod = !String.IsNullOrEmpty(reviewPage.CallMethod) ? reviewPage.CallMethod : string.Empty;
                    var accountId = !String.IsNullOrEmpty(reviewPage.AccountId) ? reviewPage.AccountId : string.Empty;
                    var locationId = !String.IsNullOrEmpty(reviewPage.LocationId) ? reviewPage.LocationId : string.Empty;
                    var externalId = !String.IsNullOrEmpty(reviewPage.ExternalId) ? reviewPage.ExternalId : string.Empty;
                    var created = (reviewPage.Created != null) ? reviewPage.Created : DateTime.Now.ToUniversalTime();

                    var reviewPageComp = string.Concat(sourceName, callMethod);
                    reviewPageComp = string.Concat(reviewPageComp, accountId);
                    reviewPageComp = string.Concat(reviewPageComp, locationId);
                    reviewPageComp = string.Concat(reviewPageComp, externalId);


                    //--- Create a lists  of all from Review.
                    foreach (Review review in reviews)
                    {

                       //if (review.Comments != null)
                        //{
                        //foreach (Comment comment in review.Comments)
                        //{
                        //_interactions.Add(new DAC.LPM.SQLServer.Interaction()
                        //{
                        //    InteractionType = "Comment.."
                        ////ParentId = ,
                        //ParentType = "Review..",
                        //AuthorName = comment.Author.Name,
                        //AuthorProfileUrl = comment.Author.ProfileUrl,
                        //AuthorIconUrl = comment.Author.IconUrl,
                        //AuthorExternalId = "AuthorExternalId",
                        //Content = comment.Content,
                        //Created = comment.Created,
                        //    CreatedTime = comment.CreatedTime
                        //});
                        //}
                        //}
                        //else if (review.Response != null)
                        //{
                        //foreach (Response response in review.Response)
                        //{
                        //    _interactions.Add(new DAC.LPM.SQLServer.Interaction()
                        //    {
                        //        InteractionType = "Response.."
                        //        //ParentId = ,
                        //ParentType = "Review.."
                        //        AuthorName = comment.Author.Name,
                        //        AuthorProfileUrl = comment.Author.ProfileUrl,
                        //        AuthorIconUrl = comment.Author.IconUrl,
                        //        AuthorExternalId = "AuthorExternalId",
                        //        Content = comment.Content,
                        //        Created = comment.Created,
                        //        //        CreatedTime = comment.CreatedTime
                        //    });
                        //}
                        if (review.Comments != null)
                        {
                            foreach (Comment comment in review.Comments)
                            {
                                _interactions.Add(new DAC.LPM.SQLServer.Interaction()
                                {
                                    InteractionType = "Comment..",
                                //ParentId = ,
                                    ParentType = "Review..",
                                    AuthorName = comment.Author.Name,
                                    AuthorProfileUrl = comment.Author.ProfileUrl,
                                    AuthorIconUrl = comment.Author.IconUrl,
                                    //AuthorExternalId = comment.Author.ExternalId,
                                    Content = comment.Content,
                                    //ContentLength = 20,
                                    Created = comment.Created
                                    //CreatedTime = comment.CreatedTime
                                });
                            }
                        }
                        //else if (review.Response != null)
                        //{
                        //    foreach (Response response in review.Response)
                        //    {
                        //        _interactions.Add(new DAC.LPM.SQLServer.Interaction()
                        //        {
                        //            InteractionType = "Response.."
                        //            //ParentId = ,
                        //            ParentType = "Review.."
                        //            AuthorName = comment.Author.Name,
                        //            AuthorProfileUrl = comment.Author.ProfileUrl,
                        //            AuthorIconUrl = comment.Author.IconUrl,
                        //            AuthorExternalId = comment.Author.ExternalId,
                        //            Content = comment.Content,
                        //            Created = comment.Created,
                        //            //        CreatedTime = comment.CreatedTime
                        //        });
                        //    }
                        //}


                        _reviews.Add(new DAC.LPM.SQLServer.Review()
                        {
                            //ParsedId = review.ParsedId,
                            //AuthorName = review.Author.Name,
                            //AuthorProfileUrl = review.Author.ProfileUrl,
                            //AuthorIconUrl = review.Author.IconUrl,
                            //AuthorExternalId = "AuthorExternalId..",
                            //ReviewPageId_FK = reviewPageComp,
                            //Content = review.Content,
                            //Rating = review.Rating,
                            //Created = review.Created,
                            //Updated = review.Updated,
                            //LastUpdated = DateTime.Now.ToUniversalTime(),

                            ParsedId = review.ParsedId,
                            AuthorName = review.Author.Name,
                            AuthorProfileUrl = review.Author.ProfileUrl,
                            AuthorIconUrl = review.Author.IconUrl,
                            //AuthorExternalId = review.Author.ExternalId,
                            ReviewPageId_FK = reviewPageComp,
                            Content = review.Content,
                            Rating = 1,
                            Created = DateTime.Today.ToString(),
                            Updated = review.Updated,
                            LastUpdated = DateTime.Today,
                            Interactions = _interactions
                        });
                    }

                    DAC.LPM.SQLServer.ReviewPage __reviewPage = new DAC.LPM.SQLServer.ReviewPage
                    {
                        SourceName = sourceName,
                        CallMethod = callMethod,
                        AccountId = accountId,
                        LocationId = locationId,
                        ExternalId = externalId,
                        AverageRating = 4.1M,
                        Created = DateTime.Today,
                        Updated = DateTime.Today,
                        LastUpdated = DateTime.Today,
                        Reviews = _reviews
                    };

                    context.ReviewPages.Add(__reviewPage);
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                //---logger.Error($"Exception Error: {ex.Message} Details: {ex.StackTrace} InnerException: {ex.InnerException}");
                return false;
                throw ex;
            }

            #endregion

            return true;
        }

        //--- 2
        private bool  UpsertReviewPageSet(ReviewPage reviewPage, List<Review> reviews)
        {
            #region SQL

                try
                {
                    // Insert seed data into the database using one instance of the context
                    using (DataDbContext context = new DataDbContext())
                    {

                        var sourceName = !String.IsNullOrEmpty(reviewPage.SourceName) ? reviewPage.SourceName : string.Empty;
                        var callMethod = !String.IsNullOrEmpty(reviewPage.CallMethod) ? reviewPage.CallMethod : string.Empty;
                        var accountId = !String.IsNullOrEmpty(reviewPage.AccountId) ? reviewPage.AccountId : string.Empty;
                        var locationId = !String.IsNullOrEmpty(reviewPage.LocationId) ? reviewPage.LocationId : string.Empty;
                        var externalId = !String.IsNullOrEmpty(reviewPage.ExternalId) ? reviewPage.ExternalId : string.Empty;
                        var created = (reviewPage.Created != null) ? reviewPage.Created : DateTime.Now.ToUniversalTime();

                        var reviewPageComp = string.Concat(sourceName, callMethod);
                        reviewPageComp = string.Concat(reviewPageComp, accountId);
                        reviewPageComp = string.Concat(reviewPageComp, locationId);
                        reviewPageComp = string.Concat(reviewPageComp, externalId);


                        //--- Create a lists  of all from Review.
                        foreach (Review review in reviews)
                        {

                            if (review.Comments != null)
                            {
                                _interactions.Add(new DAC.LPM.SQLServer.Interaction()
                                {
                                    InteractionType = "Comment",
                                    //ParentId = ,
                                    ParentType = "Review",
                                    AuthorName = review.Author.Name,
                                    AuthorProfileUrl = review.Author.ProfileUrl,
                                    AuthorIconUrl = review.Author.IconUrl,
                                    AuthorExternalId = "review.Author.ExternalId",
                                    Content = "comment.Content",
                                    Created = DateTime.Today.ToString(),
                                    CreatedTime = DateTime.Today
                                });
                            }

                            if (review.Response != null)
                            {
                                _interactions.Add(new DAC.LPM.SQLServer.Interaction()
                                {
                                    InteractionType = "Response",
                                    //ParentId = ,
                                    ParentType = "Review",
                                    AuthorName = review.Author.Name,
                                    AuthorProfileUrl = review.Author.ProfileUrl,
                                    AuthorIconUrl = review.Author.IconUrl,
                                    //AuthorExternalId = review.Author.ExternalId,
                                    Content = "comment.Content",
                                    Created = DateTime.Today.ToString(),
                                    CreatedTime = DateTime.Today
                                });
                            }

                        }

                        //-- Create a lists  of all from Review.
                        foreach (Review review in reviews)
                        {

                            //if (review.Comments != null)
                            //{
                                //foreach (Comment comment in review.Comments)
                                //{
                                    //_interactions.Add(new DAC.LPM.SQLServer.Interaction()
                                    //{
                                    //    InteractionType = "Comment.."
                                        ////ParentId = ,
                                        //ParentType = "Review..",
                                        //AuthorName = comment.Author.Name,
                                        //AuthorProfileUrl = comment.Author.ProfileUrl,
                                        //AuthorIconUrl = comment.Author.IconUrl,
                                        //AuthorExternalId = "AuthorExternalId",
                                        //Content = comment.Content,
                                        //Created = comment.Created,
                                        //    CreatedTime = comment.CreatedTime
                                    //});
                                //}
                            //}
                            //else if (review.Response != null)
                            //{
                                //foreach (Response response in review.Response)
                                //{
                                //    _interactions.Add(new DAC.LPM.SQLServer.Interaction()
                                //    {
                                //        InteractionType = "Response.."
                                        //        //ParentId = ,
                                        //ParentType = "Review.."
                                        //        AuthorName = comment.Author.Name,
                                        //        AuthorProfileUrl = comment.Author.ProfileUrl,
                                        //        AuthorIconUrl = comment.Author.IconUrl,
                                        //        AuthorExternalId = "AuthorExternalId",
                                        //        Content = comment.Content,
                                        //        Created = comment.Created,
                                //        //        CreatedTime = comment.CreatedTime
                                //    });
                                //}
                            //}


                            _reviews.Add(new DAC.LPM.SQLServer.Review()
                            {
                                //ParsedId = review.ParsedId,
                                //AuthorName = review.Author.Name,
                                //AuthorProfileUrl = review.Author.ProfileUrl,
                                //AuthorIconUrl = review.Author.IconUrl,
                                //AuthorExternalId = "AuthorExternalId..",
                                //ReviewPageId_FK = reviewPageComp,
                                //Content = review.Content,
                                //Rating = review.Rating,
                                //Created = review.Created,
                                //Updated = review.Updated,
                                //LastUpdated = DateTime.Now.ToUniversalTime(),

                                ParsedId = review.ParsedId,
                                AuthorName = "review.Author.Name",
                                //AuthorProfileUrl = review.Author.ProfileUrl,
                                //AuthorIconUrl = review.Author.IconUrl,
                                //AuthorExternalId = "AuthorExternalId..",
                                ReviewPageId_FK = reviewPageComp,
                                //Content = review.Content,
                                Rating = 1,
                                //Created = review.Created,
                                //Updated = review.Updated,
                                LastUpdated = DateTime.Now,
                                Interactions = _interactions
                            });
                        }

                        DAC.LPM.SQLServer.ReviewPage __reviewPage = new DAC.LPM.SQLServer.ReviewPage
                        {
                            //SourceName = sourceName,
                            //CallMethod = callMethod,
                            //AccountId = accountId,
                            //LocationId = locationId,
                            //ExternalId = externalId,
                            //AverageRating = 4.1M,
                            //Created = created,
                            //Updated = DateTime.Now.ToUniversalTime(),
                            LastUpdated = DateTime.Now,
                            Reviews = _reviews
                        };

                        context.ReviewPages.Add(__reviewPage);
                        context.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    //---logger.Error($"Exception Error: {ex.Message} Details: {ex.StackTrace} InnerException: {ex.InnerException}");
                    return false;
                    throw ex;
                }

            #endregion

            return true;
        }

        public List<Review> GetReviews(string reviewPageId)
        {
            //--- SQL Server
            using (DataDbContext context = new DataDbContext())
            { 
                _reviews = context.Reviews
                        .Where(p => p.ReviewPageId_FK == reviewPageId)
                        .ToList();
            }
            //return _reviews;

            //--- MongoDB
            List<Review> __reviews = new List<Review>();  

            foreach (DAC.LPM.SQLServer.Review review in _reviews)
            {

               var author = new Author
                {
                    Name = review.AuthorName,
                    ProfileUrl = review.AuthorProfileUrl,
                    IconUrl = review.AuthorIconUrl
                    //ExternalId = review.AuthorExternalId
               };

                __reviews.Add(new Review()
                {
                    ParsedId = review.ParsedId,
                    Author = author,
                    ReviewPageId = review.ReviewPageId_FK,
                    Content = review.Content,
                    Rating = review.Rating,
                    Created = review.Created,
                    Updated = review.Updated

                });
            }

            //---return new List<Review>();
            return __reviews;
        }

        public List<ReviewPage> GetReviewPages(string vendor, string accountId, string locationId, string externalId, string CallMethod)
        {
            var reviewPageId = $"{vendor}|{CallMethod}|{accountId}|{locationId}|{externalId}";

            //--- SQL Server
            using (DataDbContext context = new DataDbContext())
            {
                _reviewPages = context.ReviewPages
                    .Where
                    (p => p.SourceName == vendor &&
                     p.CallMethod == CallMethod &&
                     p.AccountId == accountId &&
                     p.LocationId == locationId &&
                     p.ExternalId == externalId
                    )
                    .ToList();
            }

            //--- MongoDB
            List<ReviewPage> __reviewpages = new List<ReviewPage>();  

            foreach (DAC.LPM.SQLServer.ReviewPage reviewPage in _reviewPages) 
            {
                __reviewpages.Add(new ReviewPage()
                {
                    SourceName = reviewPage.SourceName,
                    CallMethod = reviewPage.CallMethod,
                    AccountId = reviewPage.AccountId,
                    LocationId = reviewPage.LocationId,
                    ExternalId = reviewPage.ExternalId
                });
            }

            //---return new List<ReviewPage>();
            return __reviewpages;
        }

        #endregion
    }
}