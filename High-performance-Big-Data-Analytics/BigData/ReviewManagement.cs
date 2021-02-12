/*
Author          : Technical  Architect / Application Developer Oleg Gorlov
Description:	: Class ReviewManagement to serve MongoDB. 
                : 
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 02/09/2017
Release         : 1.0.0
Comment         : 
*/
using DAC.LLM.OnDemand.MongoDB.Connection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DAC.LPM.ReviewDataDomain.Mongo
{
    public class ReviewManagement : IReviewManagement
    {
        #region ConstantValue

        public const string MONGO_DATABASE = "parrot3";
        public const string MONGO_COLLECTION_REVIEWPAGE = "parrot.scrapeReviewPage";
        public const string MONGO_COLLECTION_REVIEW = "parrot.scrapeReview";
        public const string MONGO_COLLECTION_COMMENT = "parrot.scrapeComment";

        #endregion

        #region Property
        private string _environment;
        #endregion

        #region Construct
        public ReviewManagement()
        {
            _environment = "dev";
        }
        public ReviewManagement(string Env)
        {
            _environment = Env;
        }
        #endregion

        #region Implement Interface Method
        public void Save(ReviewPage reviewPage, List<Review> reviews, int expectedReviewCount = -1)
        {
            UpsertReviewPageSet(reviewPage, reviews);
        }

        public List<Review> GetReviews(string reviewPageId)
        {
            List<Review> reviews = new List<Review>();

            try
            {
                SetUpMongoConnection();
                var reviewCollection = MongoDBAccess.GetCollection(MONGO_COLLECTION_REVIEW, MONGO_DATABASE);
                var reviewBuilder = Builders<BsonDocument>.Filter;
                var reviewFilter = reviewBuilder.Eq("ReviewPageId", reviewPageId);
                var reviewBsonDocuments = reviewCollection.Find(reviewFilter).ToList();

                if (reviewBsonDocuments.Any())
                {
                    foreach (var reviewBsonDocument in reviewBsonDocuments)
                    {
                        var id = reviewBsonDocument.GetValue("Id", null).ToString();

                        var collection = MongoDBAccess.GetCollection(
                            MONGO_COLLECTION_COMMENT, MONGO_DATABASE);
                        var builder = Builders<BsonDocument>.Filter;
                        var filter = builder.Eq("ReviewId", id);


                        //var commentBsonDocuments = collection.Find(filter).ToList();
                        List<Comment> comments = new List<Comment>();

                        //if (commentBsonDocuments.Any())
                        //{
                        //    comments = commentBsonDocuments.
                        //        Select(x => new Comment
                        //        {
                        //            Id = x.GetValue("Id", null).ToString(),
                        //            ReviewId = x.GetValue("ReviewId", null).ToString(),
                        //            Author = (reviewBsonDocument.GetValue("Author", null) != null) ?
                        //            JsonConvert.DeserializeObject<Author>(
                        //                reviewBsonDocument["Author"].ToJson()) : new Author(),
                        //            Content = reviewBsonDocument.GetValue("Content", null).ToString(),
                        //            Created = reviewBsonDocument.GetValue("Created", null).ToString(),
                        //            Replies = (reviewBsonDocument.GetValue("Replies", null) != null) ?
                        //            JsonConvert.DeserializeObject<List<Comment>>(
                        //                reviewBsonDocument["Replies"].ToJson()) : null
                        //        })
                        //        .ToList();
                        //}

                        var currReview = new Review
                        {
                            ParsedId = (reviewBsonDocument.GetValue("ParsedId", null) != null) ? reviewBsonDocument.GetValue("ParsedId", null).ToString():string.Empty,
                            ReviewPageId = reviewBsonDocument.GetValue("ReviewPageId", null).ToString(),
                            Author = (reviewBsonDocument.GetValue("Author", null) != null) ?
                                    JsonConvert.DeserializeObject<Author>(
                                        reviewBsonDocument["Author"].ToJson()) : new Author(),
                            Rating = (reviewBsonDocument.GetValue("Rating", null) != null) ? reviewBsonDocument.GetValue("Rating", null).ToInt32() : 0,
                            Content = (reviewBsonDocument.GetValue("Content", null) != null) ? reviewBsonDocument.GetValue("Content", null).ToString() : null,
                            Created = reviewBsonDocument.GetValue("Created", null).ToString(),
                            Updated = reviewBsonDocument.GetValue("Updated", null).ToString(),
                            Response = (reviewBsonDocument.GetValue("Response", null) != null) ?
                                    JsonConvert.DeserializeObject<Response>(
                                        reviewBsonDocument["Response"].ToJson()) : new Response(),
                            Children = (reviewBsonDocument.GetValue("Children", null) != null) ?
                                    JsonConvert.DeserializeObject<List<Review>>(
                                        reviewBsonDocument["Children"].ToJson()) : null,
                            Comments = comments,
                            Metadata = (reviewBsonDocument.GetValue("Metadata", null) != null) ?
                                JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                reviewBsonDocument["Metadata"].ToJson()) : null
                        };

                        reviews.Add(currReview);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return reviews;
        }

        public List<ReviewPage> GetReviewPages(string vendor, string accountId, string locationId, string externalId, string CallMethod)
        {
            List<ReviewPage> reviewPages = new List<ReviewPage>();
            var reviewPageId = $"{vendor}|{CallMethod}|{accountId}|{locationId}|{externalId}";

            try
            {
                SetUpMongoConnection();
                var reviewCollection = MongoDBAccess.GetCollection(
                    MONGO_COLLECTION_REVIEWPAGE, MONGO_DATABASE);
                var reviewBuilder = Builders<BsonDocument>.Filter;
                FilterDefinition<BsonDocument> reviewFilter = null;
                AddReviewFilter("SourceName", vendor, ref reviewFilter);
                AddReviewFilter("AccountId", accountId, ref reviewFilter);
                AddReviewFilter("LocationId", locationId, ref reviewFilter);
                AddReviewFilter("ExternalId", externalId, ref reviewFilter);
                AddReviewFilter("CallMethod", CallMethod, ref reviewFilter);
                if (reviewFilter == null)
                {
                    throw new Exception("You can not select Everything");
                }
                //var reviewFilter = reviewBuilder.Eq("Id", reviewPageId);
                var policy = Policy.Handle<Exception>().Retry(3);
                policy.Execute(
              () => reviewPages = GetReviewPage(reviewFilter, reviewCollection));


            }
            catch (Exception ex)
            {
                throw ex;
            }

            return reviewPages;
        }

        #endregion

        #region Private method
        private void SetUpMongoConnection()
        {
            if (!String.IsNullOrEmpty(_environment))
            {
                switch (_environment.ToLower())
                {
                    case "prod": // Production
                        MongoDBAccess.SetConnectionString("mongoDB.connection");
                        break;
                    case "dev":
                        MongoDBAccess.SetConnectionString("mongoDB.connectionDev");
                        break;
                    default:
                        MongoDBAccess.SetConnectionString("mongoDB.connectionDev");
                        break;
                }
            }
            else
            {
                MongoDBAccess.SetConnectionString("mongoDB.connectionDev");
            }
        }

        private void UpsertReviewPageSet(ReviewPage reviewPage, List<Review> reviews)
        {
            SetUpMongoConnection();
            var policy = Policy.Handle<Exception>().Retry(3);

            var reviewPageFilter = Builders<BsonDocument>.Filter.Eq("Id", reviewPage.Id);
            var reviewPageCollection = MongoDBAccess.GetCollection(
                MONGO_COLLECTION_REVIEWPAGE, MONGO_DATABASE);
            var reviewBsonDocument = reviewPageCollection.
                Find(reviewPageFilter).Limit(1).FirstOrDefault();

            policy.Execute(
                () => reviewPageCollection.ReplaceOne(reviewPageFilter,
                new BsonDocument
                {
                    { "SourceName", (!String.IsNullOrEmpty(reviewPage.SourceName))?reviewPage.SourceName:string.Empty },
                    { "CallMethod", (!String.IsNullOrEmpty(reviewPage.CallMethod))?reviewPage.CallMethod:string.Empty },
                    { "AccountId", (!String.IsNullOrEmpty(reviewPage.AccountId))?reviewPage.AccountId:string.Empty },
                    { "LocationId", (!String.IsNullOrEmpty(reviewPage.LocationId))?reviewPage.LocationId:string.Empty },
                    { "ExternalId", (!String.IsNullOrEmpty(reviewPage.ExternalId))?reviewPage.ExternalId:string.Empty },
                    { "Metadata", ObjectToBsonDocument(reviewPage.Metadata) },
                    { "Created", (reviewBsonDocument != null) ?
                        reviewBsonDocument.GetValue("Created", null).ToUniversalTime() :
                        DateTime.Now.ToUniversalTime() },
                    { "Updated", DateTime.Now.ToUniversalTime() },
                    { "Id", reviewPage.Id }
                },
                new UpdateOptions { IsUpsert = true }));

            var reviewWriteModel = new List<WriteModel<BsonDocument>>();
            var reviewCollection = MongoDBAccess.GetCollection(
                MONGO_COLLECTION_REVIEW, MONGO_DATABASE);
            var commentWriteModel = new List<WriteModel<BsonDocument>>();
            var commentCollection = MongoDBAccess.GetCollection(
                MONGO_COLLECTION_COMMENT, MONGO_DATABASE);

            reviewWriteModel.Add(new DeleteManyModel<BsonDocument>(
                Builders<BsonDocument>.Filter.Eq("ReviewPageId", reviewPage.Id)));

            foreach (Review review in reviews)
            {
                BsonDocument reviewBson = new BsonDocument();
                this.AddBsonDocument("Id", review.Id, reviewBson);
                this.AddBsonDocument("ParsedId", review.ParsedId, reviewBson);
                this.AddBsonDocument("ReviewPageId", review.ReviewPageId, reviewBson);
                this.AddBsonDocument("Author", ObjectToBsonDocument(review.Author), reviewBson);
                this.AddBsonDocument("Rating", review.Rating, reviewBson);
                this.AddBsonDocument("Content", review.Content, reviewBson);
                this.AddBsonDocument("Created", review.Created, reviewBson);
                this.AddBsonDocument("Updated", review.Updated, reviewBson);

                this.AddBsonDocument("Children", IEnumerableToBsonArray(review.Children), reviewBson);
                this.AddBsonDocument("Metadata", ObjectToBsonDocument(review.Metadata), reviewBson);
                this.AddBsonDocument("Response", ObjectToBsonDocument(review.Response), reviewBson);


                reviewWriteModel.Add(new InsertOneModel<BsonDocument>(
                                    reviewBson));



                commentWriteModel.Add(new DeleteManyModel<BsonDocument>(
                    Builders<BsonDocument>.Filter.Eq("ReviewId", review.Id)));
                if (review.Comments != null)
                {
                    foreach (Comment comment in review.Comments)
                    {
                        commentWriteModel.Add(new InsertOneModel<BsonDocument>(
                            new BsonDocument
                            {
                            { "Id", comment.Id },
                            { "ReviewId", comment.ReviewId },
                            { "Author", ObjectToBsonDocument(comment.Author) },
                            { "Content", comment.Content },
                            { "Created", comment.Created },
                            { "Replies", IEnumerableToBsonArray(comment.Replies) }
                            }));
                    }
                }

            }
            if (reviewWriteModel.Count > 0)
            {
                policy.Execute(
                              () => reviewCollection.BulkWrite(reviewWriteModel,
                              new BulkWriteOptions { IsOrdered = false }));
            }

            if (commentWriteModel.Count > 0)
            {
                policy.Execute(
                                () => commentCollection.BulkWrite(commentWriteModel,
                                new BulkWriteOptions { IsOrdered = true }));
            }

        }

        private void AddBsonDocument(string key, BsonValue value, BsonDocument targetBson)
        {
            if (value != null)
            {
                targetBson.Add(key, value);
            }
        }

        private List<ReviewPage> GetReviewPage(FilterDefinition<BsonDocument> reviewFilter, IMongoCollection<BsonDocument> reviewCollection)
        {
            List<ReviewPage> ret = new List<ReviewPage>();
            var reviewBsonDocuments = reviewCollection.Find(reviewFilter).ToList();

            if (reviewBsonDocuments.Any())
            {
                foreach (var reviewBsonDocument in reviewBsonDocuments)
                {
                    ret.Add(new ReviewPage
                    {
                        SourceName = reviewBsonDocument.GetValue("SourceName", null).ToString(),
                        CallMethod = reviewBsonDocument.GetValue("CallMethod", null).ToString(),
                        AccountId = reviewBsonDocument.GetValue("AccountId", null).ToString(),
                        LocationId = reviewBsonDocument.GetValue("LocationId", null).ToString(),
                        ExternalId = reviewBsonDocument.GetValue("ExternalId", null).ToString(),
                        Metadata = (reviewBsonDocument.GetValue("Metadata", null) != null) ?
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(
                            reviewBsonDocument["Metadata"].ToJson()) : null,
                        Updated = reviewBsonDocument.GetValue("Updated", null).ToUniversalTime()
                    });
                }
            }
            return ret;
        }

        private BsonArray IEnumerableToBsonArray(IEnumerable list)
        {
            if (list != null)
            {
                var array = new BsonArray();

                foreach (var item in list)
                {
                    array.Add(BsonSerializer.Deserialize<BsonDocument>(
                        JsonConvert.SerializeObject(item)));
                }
                return array;
            }
            else
            {
                return null;
            }


        }

        private void AddReviewFilter(string key, string value, ref FilterDefinition<BsonDocument> target)
        {
            var reviewBuilder = Builders<BsonDocument>.Filter;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                if (target != null)
                {
                    target = reviewBuilder.And(reviewBuilder.Eq(key, value), target);
                }
                else
                {
                    target = reviewBuilder.Eq(key, value);
                }
            }
        }

        /*  Avoid a bug where the "Id" field value is reassigned 
            to the auto-generated "_id" field when using the
            standard "ToBsonDocument" method. */
        private BsonDocument ObjectToBsonDocument<T>(T doc)
        {
            if (doc != null)
            {
                return BsonSerializer.Deserialize<BsonDocument>(
                                  JsonConvert.SerializeObject(doc));
            }
            else
            {
                return null;
            }
        }

        private DateTime GetDateTime(BsonValue currValue)
        {
            if (currValue != null)
            {
                if (currValue.IsBsonDateTime)
                {
                    return currValue.AsUniversalTime;
                }
            }
            return DateTime.MinValue;
        }

        #endregion
    }
}