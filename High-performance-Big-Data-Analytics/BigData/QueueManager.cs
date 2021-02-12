using DAC.LLM.Cache;
using DAC.LLM.Cache.Redis;
using DAC.LLM.Queue;
using DAC.LLM.Queue.RabbitMQ;
using DACFramework.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAC.LLM.OnDemand
{
    public class QueueManager : IQueueManager
    {
        #region Queue Manager
        private ICacheManager cacheManager;

        private string cacheConnectionString;

        private const string CURRENT_RUNNING_QUEUE_CACHE_KEY = "Queue.CurrentRunning";

        public QueueManager(string connectionString)
        {
            CurrentConnectionString = connectionString;
        }

        public QueueManager(ICacheManager cacheManager)
        {
            this.cacheManager = cacheManager;
        }

        private string CurrentConnectionString
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    cacheConnectionString = value;
                }
                else
                {
                    cacheConnectionString = AppSettings.Get("cacheManager.connectionString");
                    if (string.IsNullOrWhiteSpace(cacheConnectionString))
                        throw new ArgumentNullException("cacheManager.connectionString");
                }
                cacheManager = new CacheManager(cacheConnectionString);
            }

            get { return cacheConnectionString; }
        }

        #endregion

        public List<string> GetQueueList(string workerId, string prefixQueueName)
        {
            return GetQueueList(workerId, prefixQueueName, string.Empty);
        }
        public List<string> GetQueueList(string workerId, string prefixQueueName, string excludeQueueName)
        {
            var cacheQueueName = string.Format("Queue.{0}", workerId);
            var currentRunningQueueNames = cacheManager.ListSortedSetMembers(CURRENT_RUNNING_QUEUE_CACHE_KEY);

            if (currentRunningQueueNames == null)
                return new List<string>();

            var queueList = GetQueueFilter(currentRunningQueueNames.ToList(), prefixQueueName, true);
            queueList = GetQueueFilter(queueList, excludeQueueName, false);           
            return queueList;
        }

        private List<string> GetQueueFilter(List<string> QueueName, string filterstring, bool isIncludedList)
        {
            List<string> queueList = new List<string>();            
            if (string.IsNullOrEmpty(filterstring))
            {
                return QueueName;
            }
            var filters = filterstring.Split(',').ToList();
            foreach (var targetfilter in filters)
            {
                foreach (var targetQueueName in QueueName)
                {
                    if (isIncludedList && targetQueueName.Contains(targetfilter.Trim()))
                    {
                        queueList.Add(targetQueueName.Trim());
                    }
                    else if (!isIncludedList && !targetQueueName.Contains(targetfilter.Trim()))
                    {
                        queueList.Add(targetQueueName.Trim());
                    }
                }
            }
            return queueList;
        }


        public bool AddTargetWorkerJob(string queueName, bool isPriority)
        {
            // Step 2 Update QueueList
            var score = 5;
            if (isPriority)
                score = 1;

            cacheManager.SortedSetAdd(CURRENT_RUNNING_QUEUE_CACHE_KEY, queueName, score);
            return true;
        }

        public bool RegisterQueueByJobId(string JobId, List<string> QueueList)
        {
            // Step 1 Register Job
            var queueJobID = string.Format("Queue.{0}", JobId);
            foreach (var queueName in QueueList)
            {
                cacheManager.SortedSetAdd(queueJobID, queueName);
            }
            return true;
        }

        public bool RemoveQueueByJobId(string jobId)
        {
            var queueJobID = string.Format("Queue.{0}", jobId);
            IQueueConfig queueInfo = new QueueConfig(queueJobID);
            using (var queueWriter = new QueueWriter(queueInfo))
            {
                var queueNamesByQueueJobId = cacheManager.ListSortedSetMembers(queueJobID);

                //Remove Queue from RabbitMq
                queueWriter.DeleteQueue(queueNamesByQueueJobId.ToList());

                //Remove Key with Queues names from Redis
                cacheManager.Remove(queueJobID);

                //Remove queues from list of Running queues
                foreach (var currentItem in queueNamesByQueueJobId)
                {
                    cacheManager.SortedSetRemove(CURRENT_RUNNING_QUEUE_CACHE_KEY, currentItem);
                }
                return true;
            }
        }

        public void Dispose()
        {
            if (cacheManager != null)
                cacheManager.Dispose();
        }
    }
}
