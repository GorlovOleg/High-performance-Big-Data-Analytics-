using System;
using System.Threading;
using System.Collections.Generic;
using Confluent.Kafka;
using KafkaSerializationLib;
using LogToScreenLib;
using ThreadProcessingLib;
using CommonInterfacesLib;

namespace KafkaProducerLib
{
    public class KafkaProducer<T> : IStreamingProducer<T>
    {
        #region Variables

        private object locker = new object();
        private Queue<KeyValuePair<string, T>> quePair = new Queue<KeyValuePair<string, T>>();
        private long isContinue = 1;

        private Dictionary<string, object> config;
        private string topicName;
        private int partition;
        private EventHandler<IError> dlgtOnError;

        private ThreadProcessing threadProcessing;
        private Producer<string, T> producer;

        #endregion // Variables

        #region Ctor

        public KafkaProducer(Dictionary<string, object> config, string topicName, int partition, EventHandler<IError> dlgtOnError)
        {
            this.config = config;
            this.topicName = topicName;
            this.partition = partition;
            this.dlgtOnError = dlgtOnError;

            var keySerializer = Factory.CreateSerializer<string>();
            var valueSerializer = Factory.CreateSerializer<T>();

            AssignSerializersIfDefault(ref keySerializer, ref valueSerializer);

            producer = new Producer<string, T>(config, keySerializer, valueSerializer);

            threadProcessing = new ThreadProcessing(() =>
                {
                    if (ToBeContinued)
                    {
                        KeyValuePair<string, T> pair;
                        while (GetNextDataPair(out pair))
                            producer.ProduceAsync(topicName, pair.Key, pair.Value, partition, blockIfQueueFull: true);
                    }

                    // Tasks are not waited on synchronously (ContinueWith is not synchronous),
                    // so it's possible they may still in progress here.
                    //producer.Flush(TimeSpan.FromSeconds(10));
                },
                e =>
                {
                    dlgtOnError(null, new Error((int)ErrorCode.Unknown, e.Message));
                    LogToScreen.Exception(e);
                });
        }

        #endregion // Ctor

        #region Public Methods 

        public void SendDataPairs(params KeyValuePair<string, T>[] arr)
        {
            if (arr == null || arr.Length == 0)
                return;

            lock (locker)
            {
                foreach (var pair in arr)
                    quePair.Enqueue(pair);
                threadProcessing.Execute();
            }
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref isContinue);
            threadProcessing.Execute();
            threadProcessing.Dispose();

            producer.Dispose();
        }

        #endregion // Public Methods 

        #region Private Methods 

        private bool GetNextDataPair(out KeyValuePair<string, T> pair)
        {
            bool isContinue;
            lock (locker)
                pair = (isContinue = quePair.Count > 0) ? pair = quePair.Dequeue() : new KeyValuePair<string, T>();

            return isContinue;
        }

        private bool ToBeContinued
        {
            get { return Interlocked.Read(ref isContinue) == 1; }
        }

        private void AssignSerializersIfDefault(ref Confluent.Kafka.Serialization.ISerializer<string> keySerializer, 
                                                ref Confluent.Kafka.Serialization.ISerializer<T> valueSerializer)
        {
            if (keySerializer == null && typeof(string) != typeof(Null))
                keySerializer = new KafkaStandardSerializer<string>();

            if (valueSerializer == null)
                valueSerializer = new KafkaStandardSerializer<T>();
        }

        #endregion // Private Methods 
    }

    public class Error : Confluent.Kafka.Error, IError
    {
        public Error(int code)
            : base((ErrorCode)code)
        {
        }

        public Error(int code, string reason)
            : base((ErrorCode)code, reason)
        {
        }

        new public int Code { get { return (int)base.Code; } }

        new public string Reason { get { return base.Reason; } }

        public Exception ExceptionOnError { get { return null; } }
    }
}
