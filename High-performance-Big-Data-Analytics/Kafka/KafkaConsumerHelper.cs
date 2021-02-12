using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using KafkaSerializationLib;
using LogToScreenLib;
using CommonInterfacesLib;
using ThreadProcessingLib;

namespace KafkaConsumerLib
{ 
    public class KafkaConsumer<T> : IStreamingConsumer
    {
        #region Variables

        private long isContinue = 1;
        private long lastOffset;
        private Consumer<string, T> consumer;
        private ThreadProcessing threadProcessing;

        #endregion // Variables

        #region Ctor

        public KafkaConsumer(Dictionary<string, object> config, string topicName, int partition, int offset,
                             MessageProcessingDelegate<T> dlgtMessageProcessing, int consumerTimeoutMs,
                             EventHandler<IError> dlgtOnError)
        {
            if (dlgtMessageProcessing == null)
                return;

            var keyDeserializer = Factory.CreateDeserializer<string>();
            var valueDeserializer = Factory.CreateDeserializer<T>();

            AssignDeserializersIfDefault(ref keyDeserializer, ref valueDeserializer);

            consumer = GetConsumer(config, topicName, partition, offset, (sender, e) => dlgtOnError(sender, (Error)e),
                                              keyDeserializer, valueDeserializer);

            consumer.OnMessage += (sender, msg) =>
                {
                    Interlocked.Exchange(ref lastOffset, msg.Offset.Value);

                    Task.Run(() =>
                    {
                        try
                        {
                            dlgtMessageProcessing(new Message<T>(msg));
                        }
                        catch (Exception e)
                        {
                            dlgtOnError(null, new Error((int)ErrorCode.Unknown, e.Message));
                            LogToScreen.Exception(e);
                        }
                    });
                };

            threadProcessing = new ThreadProcessing(() =>
                {
                    while (ToBeContinued)
                        consumer.Poll(consumerTimeoutMs);
                },
                e =>
                {
                    dlgtOnError(null, new Error((int)ErrorCode.Unknown, e.Message));
                    LogToScreen.Exception(e);
                });
        }

        #endregion // Ctor

        #region Interface Implementation

        public void StartConsuming()
        {
            threadProcessing.Execute();
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref isContinue);
            consumer.Dispose();
            threadProcessing.Dispose();
        }
        #endregion // Interface Implementation
        
        #region Public Methods

        public long LastProcessedOffset
        {
            get
            {
                return Interlocked.Exchange(ref lastOffset, lastOffset);
            }
        }

        #endregion // Public Methods

        #region Private Methods 

        private Consumer<string, T> GetConsumer(Dictionary<string, object> config, string topicName, int partition, int offset, 
                                                     EventHandler<IError> dlgtOnError,
                                                     IDeserializer<string> keyDeserializer,
                                                     IDeserializer<T> valueDeserializer)
        {
            var consumer = new Consumer<string, T>(config, keyDeserializer, valueDeserializer);
            consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(topicName, partition, offset) });
            if (dlgtOnError != null)
                consumer.OnError += (sender, e) => dlgtOnError(sender, new Error((int)e.Code, e.Reason));
            return consumer;
        }

        private bool ToBeContinued
        {
            get { return Interlocked.Read(ref isContinue) == 1; }
        }

        private void AssignDeserializersIfDefault(ref IDeserializer<string> keyDeserializer, 
                                                  ref IDeserializer<T> valueDeserializer)
        {
            if (keyDeserializer == null && typeof(string) != typeof(Null))
                keyDeserializer = new KafkaStandardSerializer<string>();

            if (valueDeserializer == null)
                valueDeserializer = new KafkaStandardSerializer<T>();
        }

        #endregion // Private MethodsHelpers
    }

    public class Message<T> : Message<string, T>, IMessage<string, T>
    {
        public Message(string topic, int partition, long offset, string key, T val, DateTime creationTime, Error error)
            : base(topic, partition, offset, key, val, new Timestamp(Timestamp.DateTimeToUnixTimestampMs(creationTime), TimestampType.NotAvailable), new Confluent.Kafka.Error(error))
        {
        }

        public Message(Message<string, T> m)
            : base(m.Topic, m.Partition, m.Offset, m.Key, m.Value, m.Timestamp, m.Error)
        {
        }

        new public string Key { get { return base.Key; } }

        new public T Value { get { return base.Value; } }
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
