//#define DAC
//#define cloudAMQP
#define AzureServiceBus
//#define AzureServiceBusAmqp

/*
Author          : Full-stack Developer Oleg Gorlov
Description:	: QueueReaderBase abstract class AzureServiceBus
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 07/07/2017
Release         : 1.0.0
Comment         : 
                : added IQueueClient class Microsoft.Azure.ServiceBus
  Storage queues and Service Bus queues - compared and contrasted
  https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted
 */

using DAC.LLM.OnDemand.Message;
using DAC.Parser.DataDomain;
using DACFramework.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.ServiceBus;
using DACFramework.Config;
using System.Diagnostics;
using System.Threading.Tasks;


namespace DAC.LLM.Queue.AzureServiceBus
{
    public class QueueReader : QueueReaderBase, IQueueReader
    {
        private List<ParserMessage> messagepool;

        public string QueueName { get; set; }
        public string message_;
        public string message__;
        //--- 1
        public QueueReader(IQueueConfig queueConfig, ILog logger) : base(queueConfig, logger)
        {
            messagepool = new List<ParserMessage>(this.queueConfig.PoolSize);
            count = 0;

            CreateConnection();
        }
        //--- 2
        public ParserMessage ReadMessage()
        {

#if DAC
            // this can be used for determining if we're flushing an ondemand queue
            var lastMessageType = RequireFunction.Unknown;
            var onDemandLocationId = string.Empty;

            try
            {
                var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
                BasicDeliverEventArgs args;
                // wait 5 seconds for a new message before flushing the backlog
                var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                if (gotMessage)
                {
                    SetQueueActivity(subscription.QueueName);
                    return MessageReceived(args, ref lastMessageType, ref onDemandLocationId);
                }

                SetQueueNonActivity(subscription.QueueName);
                return null;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                DisposeAllRabbitMqRelatedObjects();
                throw;
            }

#elif cloudAMQP

            //---
            //using (var conn = factory.CreateConnection())

            using (var channel = mqConnection.CreateModel())
            {
                // ensure that the queue exists before we access it
                //---channel.QueueDeclare("queue3", false, false, false, null);
                channel.QueueDeclare("DACgroup", false, false, false, null);
                // do a simple poll of the queue 
                var data = channel.BasicGet("DACgroup", false);
                // the message is null if the queue was empty 

                //--- if (data == null) return Json(null);

                if (data == null)
                {
                    Debug.WriteLine("message -> null ");
                }
                else
                {
                    // convert the message back from byte[] to a string
                    var message = Encoding.UTF8.GetString(data.Body);
                    // ack the message, ie. confirm that we have processed it
                    // otherwise it will be requeued a bit later
                    channel.BasicAck(data.DeliveryTag, false);
                    //return Json(message);
                    Debug.WriteLine("message -> " + message);
                }
            }
            // this can be used for determining if we're flushing an ondemand queue
            var lastMessageType = RequireFunction.Unknown;
            var onDemandLocationId = string.Empty;

            try
            {
                var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
                BasicDeliverEventArgs args;
                // wait 5 seconds for a new message before flushing the backlog
                var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                if (gotMessage)
                {
                    SetQueueActivity(subscription.QueueName);
                    return MessageReceived(args, ref lastMessageType, ref onDemandLocationId);
                }

                SetQueueNonActivity(subscription.QueueName);
                return null;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                DisposeAllRabbitMqRelatedObjects();
                throw;
            }

#elif AzureServiceBus
            // this can be used for determining if we're flushing an ondemand queue
            var lastMessageType = RequireFunction.Unknown;
            var onDemandLocationId = string.Empty;

            try
            {
                // Register a OnMessage callback
                queueClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        // Process the message

                        Debug.WriteLine($"....Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                        message__ = Encoding.UTF8.GetString(message.Body);
                        // Complete the message so that it is not received again.
                        // This can be done only if the queueClient is opened in ReceiveMode.PeekLock mode.
                        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                    },
                    new MessageHandlerOptions() { MaxConcurrentCalls = 1, AutoComplete = false });


                //var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
                //BasicDeliverEventArgs args;
                // wait 5 seconds for a new message before flushing the backlog
                //var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                //if (gotMessage)
                //{
                    //SetQueueActivity(subscription.QueueName);
                    //return MessageReceived(args, ref lastMessageType, ref onDemandLocationId);
                //    return MessageReceived(args, ref lastMessageType, ref onDemandLocationId);
                //}

                //SetQueueNonActivity(subscription.QueueName);

                return null;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                Console.WriteLine($"{DateTime.Now} > Exception: {ex.Message}");
                throw;

            }
#endif

        }

        //--- 3
        public T ReadMessage<T>() where T : IAcknowledgeable
        {
#if DAC

            try
            {
                var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
                BasicDeliverEventArgs args;
                // wait 5 seconds for a new message before flushing the backlog
                var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                if (gotMessage)
                {
                    SetQueueActivity(subscription.QueueName);
                    return MessageReceived<T>(args);
                }

                SetQueueNonActivity(subscription.QueueName);

                return default(T);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                DisposeAllRabbitMqRelatedObjects();
                return default(T);
            }
#elif cloudAMQP
            try
            {
                var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
                BasicDeliverEventArgs args;
                // wait 5 seconds for a new message before flushing the backlog
                var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                if (gotMessage)
                {
                    SetQueueActivity(subscription.QueueName);
                    return MessageReceived<T>(args);
                }

                SetQueueNonActivity(subscription.QueueName);

                return default(T);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                DisposeAllRabbitMqRelatedObjects();
                return default(T);
            }
#elif AzureServiceBus
            
            
            /*
            var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
            BasicDeliverEventArgs args;
            // wait 5 seconds for a new message before flushing the backlog
            var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

            if (gotMessage)
            {
                SetQueueActivity(subscription.QueueName);
                return MessageReceived<T>(args);
            }
//---
            SetQueueNonActivity(subscription.QueueName);
*/

            try
            {
                // Register a OnMessage callback
                queueClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        // Process the message
                        Debug.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                        message__ = Encoding.UTF8.GetString(message.Body);
                        // Complete the message so that it is not received again.
                        // This can be done only if the queueClient is opened in ReceiveMode.PeekLock mode.
                        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                    },
                    new MessageHandlerOptions() { MaxConcurrentCalls = 1, AutoComplete = false });


                /*
                                var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
                                BasicDeliverEventArgs args;
                                // wait 5 seconds for a new message before flushing the backlog
                               var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                                //---
                                if (message_ != null)
                                {
                                    //SetQueueActivity(subscription.QueueName);
                                    //return MessageReceived<T>(args);
                                    var messageString = Encoding.UTF8.GetString(args.Body);
                                    count++;

                                    //Creates the object for the desired type.
                                    var message = (T)Activator.CreateInstance(typeof(T), new object[] { messageString });
                                    message.MessageEventArgs = args;
                                    return message;
                                }
                */
                //SetQueueNonActivity(subscription.QueueName);
                //---
                ReceiveMessages();

                return default(T);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                Console.WriteLine($"{DateTime.Now} > Exception: {ex.Message}");
                throw;

            }

            //message__ = message_;
#endif
        }
        // Receives messages from the queue in a loop
        public  void ReceiveMessages()
        {
            try
            {
                // Register a OnMessage callback
                queueClient.RegisterMessageHandler(
                    async (message, token) =>
                    {
                        // Process the message
                        Debug.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
                        // Process the message
                        message__ = Encoding.UTF8.GetString(message.Body);

                        // Complete the message so that it is not received again.
                        // This can be done only if the queueClient is opened in ReceiveMode.PeekLock mode.
                        await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                    },
                    new MessageHandlerOptions() { MaxConcurrentCalls = 5, AutoComplete = false });
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }
        }

        //--- 4
        public bool AckMessage<T>(T message) where T : IAcknowledgeable
        {
            var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
            subscription.Ack((BasicDeliverEventArgs)message.MessageEventArgs);
            //SetQueueActivity(subscription.QueueName);
            return true;
        }
        //--- 5
        public void ReleaseSubscription()
        {
            //CleanMemoryForcibly(queueConfig.QueueName);
        }
        //--- 6
        public bool NackMessage<T>(T message, bool requeue = false) where T : IAcknowledgeable
        {
            var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
            subscription.Nack((BasicDeliverEventArgs)message.MessageEventArgs, false, requeue);
            //SetQueueActivity(subscription.QueueName);
            return true;
        }
        //--- 7
        public List<T> ReadMessages<T>() where T : IAcknowledgeable
        {
            var localMessagepool = new List<T>();
            try
            {
                var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);

                BasicDeliverEventArgs args;
                //It means that there is no queue with the defined name.
                if (subscription == null)
                    return localMessagepool;

                // wait x milliseconds for a new message before flushing the backlog
                var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                if (gotMessage)
                {
                    //SetQueueActivity(subscription.QueueName);

                    if (args == null)
                    {
                        //This means the connection is closed.
                        //DisposeAllRabbitMqRelatedObjects();
                        CreateConnection();
                        return localMessagepool;
                    }

                    var message = MessageReceived<T>(args);
                    if (message != null)
                    {
                        localMessagepool.Add(message);
                    }

                    // NOTE this is only applicable for non-ondemand jobs
                    if (count % queueConfig.PoolSize == 0)
                        return localMessagepool;
                }
                else
                {
                    //SetQueueNonActivity(subscription.QueueName);
                    //DisposeInactiveChannelsAndSubscriptions(subscription.QueueName);
                    return localMessagepool;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                //DisposeAllRabbitMqRelatedObjects();
                throw;
            }
            return localMessagepool;
        }
        //--- 8
        public List<ParserMessage> ReadMessages()
        {
#if AzureServiceBus

            var messageString = "ReadMessages";
            Debug.WriteLine("List<ParserMessage> ReadMessages()" + messageString);

#endif

            // this can be used for determining if we're flushing an ondemand queue
            var lastMessageType = RequireFunction.Unknown;
            var onDemandLocationId = string.Empty;

            try
            {
                var subscription = SetupSubscription(queueConfig.QueueName, queueConfig.PoolPrefetchSize);
                BasicDeliverEventArgs args;

                // wait 5 seconds for a new message before flushing the backlog
                var gotMessage = subscription.Next(READ_FROM_QUEUE_TIMEOUT, out args);

                if (gotMessage)
                {
                    //SetQueueActivity(subscription.QueueName);
                    if (args == null)
                    {
                        //This means the connection is closed.
                        //DisposeAllRabbitMqRelatedObjects();
                        CreateConnection();
                        return messagepool;
                    }

                    var message = MessageReceived(args, ref lastMessageType, ref onDemandLocationId);
                    if (message != null)
                    {
                        messagepool.Add(message);
                    }

                    // if we have a pool size set then flush the backlog if the count mods to the pool size
                    // NOTE this is only applicable for non-ondemand jobs
                    if (count % queueConfig.PoolSize == 0 && lastMessageType != RequireFunction.OnDemand)
                        return messagepool;
                }
                else
                {
                    //SetQueueNonActivity(subscription.QueueName);
                    return messagepool;
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex.ToString());
                //DisposeAllRabbitMqRelatedObjects();
                throw;
            }
            return messagepool;
        }
        //--- 9
        private ParserMessage MessageReceived(BasicDeliverEventArgs args, ref RequireFunction lastMessageType, ref string onDemandLocationId)
        {
            var messageString = Encoding.UTF8.GetString(args.Body);
            count++;

            //Creates the object for the desired type.
            var parserMessageTemp = new ParserMessage(messageString);

            if (!string.IsNullOrEmpty(parserMessageTemp.ErrorMessage))
            {
                logger.Error("code: {0},{1}. message: {2}", parserMessageTemp.GetMessageId(), (int)Errors.ScraperMessageCanotbeAnalysed, parserMessageTemp.ErrorMessage);
                return new ParserMessage();
            }

            lastMessageType = parserMessageTemp.Parameters.Function;
            onDemandLocationId = parserMessageTemp.Parameters.LocationId;

            parserMessageTemp.MessageEventArgs = args;
            return parserMessageTemp;
        }
        //--- 10
        private T MessageReceived<T>(BasicDeliverEventArgs args) where T : IAcknowledgeable
        {
            var messageString = Encoding.UTF8.GetString(args.Body);
            count++;

            //Creates the object for the desired type.
            var message = (T)Activator.CreateInstance(typeof(T), new object[] { messageString });
            message.MessageEventArgs = args;
            return message;
        }
        //--- 11
        public void SetPool(int size)
        {
            queueConfig.PoolSize = size;
        }
        //--- 12
...