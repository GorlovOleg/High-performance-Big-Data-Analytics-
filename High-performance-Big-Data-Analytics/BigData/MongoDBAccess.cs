/*
Author          : Technical  Architect / Application Developer Oleg Gorlov
Description:	: Class MongoDBAccess to serve MongoDB. 
                : 
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 9/08/2017
Release         : 1.0.0
Comment         : 
*/
using DACFramework.Config;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace DAC.LLM.OnDemand.MongoDB.Connection
{
    public class MongoDBAccess
    {
        private static string connectionStr = string.Empty;
        private static Dictionary<string, IMongoClient> connectionPairs = new Dictionary<string, IMongoClient>();

        public static void SetConnectionString(string appSettingKeyname)
        {
            connectionStr = AppSettings.Get(appSettingKeyname);
            if(!connectionPairs.ContainsKey(connectionStr))
            {
                try
                {
                    connectionPairs[connectionStr] = new MongoClient(connectionStr);
                }
                catch(Exception ex)
                {
                    throw new ApplicationException("Fail to connect to mongoDB database.", ex);
                }                
            }
        }

        public static string GetConnectionString()
        {
            return connectionStr;
        }

        public static IMongoCollection<BsonDocument> GetCollection(string collectionName, string databaseName)
        {
            try
            {
                IMongoClient client = connectionPairs[connectionStr];
                if(client == null)
                {
                    throw new ApplicationException("Connection String is not regisitered.");
                }
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                return collection;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Fail to get mongoDB collection.", ex);
            }
        }

        public static IMongoCollection<BsonDocument> GetCollection(string collectionName, string databaseName, string appSettingKeyname)
        {
            try
            {
                string newConnectionStr = AppSettings.Get(appSettingKeyname);
                IMongoClient client = connectionPairs[newConnectionStr];
                if (client == null)
                {
                    throw new ApplicationException("Connection String is not regisitered.");
                }
                var database = client.GetDatabase(databaseName);
                var collection = database.GetCollection<BsonDocument>(collectionName);

                return collection;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Fail to get mongoDB collection.", ex);
            }
        }

        public static IMongoDatabase GetDatabase(string databaseName)
        {
            try
            {
                IMongoClient client = connectionPairs[connectionStr];
                if (client == null)
                {
                    throw new ApplicationException("Connection String is not regisitered.");
                }
                return client.GetDatabase(databaseName);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Fail to get mongoDB database.", ex);
            }
        }

        public static IMongoDatabase GetDatabase(string databaseName, string appSettingKeyname)
        {
            try
            {
                string newConnectionStr = AppSettings.Get(appSettingKeyname);
                IMongoClient client = connectionPairs[newConnectionStr];
                if (client == null)
                {
                    throw new ApplicationException("Connection String is not regisitered.");
                }
                return client.GetDatabase(databaseName);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Fail to get mongoDB database.", ex);
            }
        }
        public static SqlConnection GetSQLConnection()
        {
            try
            {
                string ConnectionString = AppSettings.Get("sql.connection");
                SqlConnection SqlConnection = new SqlConnection(ConnectionString);

                return SqlConnection;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Fail to connect to SQL database.", ex);
            }
        }

...