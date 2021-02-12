/*
Author          : Technical  Architect Oleg Gorlov
Description:	: QueueWriter class AzureServiceBusAMQP
Copyright       : DACgroup 
email           : ogorlov@dacgroup.com
Date            : 25/10/2017
Release         : 1.0.0
Comment         : 
               
 */
using DACFramework.Core;
using System;
using Microsoft.Azure.ServiceBus;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
//using Polly;
using System.Diagnostics;
using System.Threading.Tasks;
//using Microsoft.ServiceBus;

//using Microsoft.ServiceBus.Messaging;
using System.ServiceModel;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Polly;


//---using LeadPipe.Net.Domain;

namespace DAC.LLM.Queue.AzureServiceBusAMQP 
{
    public class QueueWriter : IQueueWriter
    {
        private string connectionString;
        private readonly IQueueConfig queueConfig;
        private const int TRIES = 4;
        private string password;
        private string userName;
        private string queueName;
        private string queueNameNew;
        private string queueNameDel;
        private Uri namespaceUri;
        private TransportClientEndpointBehavior credential;
        private Message brokeredMessage = new Message();

        private Microsoft.Azure.ServiceBus.QueueClient queueClient;
        private QueueDescription queueDescription;
        private const Int16 maxTrials = 4;
        //private ConnectionFactory factory;
        protected readonly ILog logger;



       //--- 1
        public QueueWriter(IQueueConfig queueConfig)
        {
            #region AzureServiceBusAMQP CreateConnection
            /// <summary>
            /// Attempt to connect with a valid connection string
            /// </summary>

            /// <param name="UserName">AppSettings.Get("mq.user")</param>
            /// <param name="Password">AppSettings.Get("mq.pass")</param>
            /// <param name="RequestedHeartbeat">30</param>
            /// <param name="Handle<BrokerUnreachableException>()"></param>
            /// <param name="Retry(tries)">5</param>
            /// <returns></returns>

            try
            {


                connectionString = queueConfig.PrimaryConnectionString;
                password = queueConfig.Password;
                userName = queueConfig.UserName;



                queueName = queueConfig.QueueName;
                queueNameNew = queueConfig.QueueNameNew;
                queueNameDel = queueConfig.QueueNameDel;


                Uri uri = ServiceBusEnvironment.CreateServiceUri("sb", userName, string.Empty);

                // Create management credentials
                //---TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(sasKeyName, sasKeyValue);
                //---Microsoft.ServiceBus.TokenProvider tokenProvider = Microsoft.ServiceBus.TokenProvider.CreateSharedAccessSignatureTokenProvider(nameKey, password);
                Microsoft.ServiceBus.TokenProvider credentials = Microsoft.ServiceBus.TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", password);
                Microsoft.ServiceBus.TokenProvider tokenProvider = Microsoft.ServiceBus.TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", password);
                NamespaceManager namespaceManager = new NamespaceManager(uri, tokenProvider);

                // Create if not exists new one
                if (namespaceManager.QueueExists(queueName) ) 
                {
                    queueClient = new Microsoft.Azure.ServiceBus.QueueClient(connectionString, queueName, Microsoft.Azure.ServiceBus.ReceiveMode.PeekLock);
                    Debug.WriteLine($"{DateTime.Now.TimeOfDay} : Queue opened successfully.", queueConfig.QueueName);

                    //IQueueClient queueClient = new Microsoft.Azure.ServiceBus.QueueClient(connectionString, queueConfig.QueueName, Microsoft.Azure.ServiceBus.ReceiveMode.PeekLock);
                    //Debug.WriteLine(" Queue opened successfully.", queueConfig.QueueName);
                }else
                {

                    //namespaceManager.CreateQueue(queueNameNew);
                    // Configure queue settings.

                    var queueDescription = new QueueDescription(queueNameNew);
                    queueDescription.MaxSizeInMegabytes = 1024;
                    // Setting message TTL to 7 days where as default TTL is 14 days.
                    queueDescription.DefaultMessageTimeToLive = TimeSpan.FromDays(7);
                    //--- https://stackoverflow.com/questions/44720880/cant-create-queue
                    namespaceManager.CreateQueue(new QueueDescription(queueNameNew)
                    {
                        DefaultMessageTimeToLive = TimeSpan.FromDays(7),
                        LockDuration = TimeSpan.FromSeconds(30),
                        EnablePartitioning = true,
                        AutoDeleteOnIdle = TimeSpan.FromDays(1),
                        MaxSizeInMegabytes = 1024

                    });

                    Debug.WriteLine($"{DateTime.Now.TimeOfDay} : Queue Created successfully. ", queueNameNew);
                }

                //--- ReceiveMessagesAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error($"Tries happened {maxTrials} times. Throwing the exception.{ex}");
                throw;
            }
            #endregion
        }

        //--- 2
        private void ExecuteWithRetry(Action action)
        {

            Policy
                .Handle<Exception>()
                .WaitAndRetry(TRIES, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .Execute(action);
        }

        //--- 3
        //public async Task PushMessageAsync(string queueName, List<string> messages, IDictionary<string, object> queueArgs = null)
        //{

        //    try
        //    {
        //        var index = 0;
        //        //---
        //        for (; index < messages.Count; index++)
        //        {
        //            var message_ = messages[index];
        //            var body = Encoding.UTF8.GetBytes(message_);
        //            //var consumer = new QueueingBasicConsumer(channel);
        //            //channel.BasicPublish("", queueName, null, body);

        //            //---
        //            // Create a new brokered message to send to the queue
        //            var message = new Message(Encoding.UTF8.GetBytes(message_));

        //            // Write the body of the message to the console
        //            Debug.WriteLine($"Sending message: {Encoding.UTF8.GetString(message.Body)}");

        //            // Send the message to the queue
        //            await queueClient.SendAsync(message);
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        Debug.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
        //    }

        //    // Delay by 10 milliseconds so that the console can keep up
        //    await Task.Delay(10);


        //    //--Debug.WriteLine($"{numMessagesToSend} messages sent.");
        //}

        //--- 3.1
        //--- Send List<Message> messages
        public void PushMessage(string queueName, List<Message> messages, IDictionary<string, object> queueArgs = null)
        {
            PushMessageAsync2_2(queueName, messages).GetAwaiter().GetResult(); 
        }


        public async Task PushMessageAsync(string queueName, List<string> messages, IDictionary<string, object> queueArgs = null)
        {

            try
            {
                var index = 0;
                //---
                for (; index < messages.Count; index++)
                {
                    var message_ = messages[index];
                    var body = Encoding.UTF8.GetBytes(message_);
                    //var consumer = new QueueingBasicConsumer(channel);
                    //channel.BasicPublish("", queueName, null, body);

                    //--- byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Text");
                    // Create a new brokered message to send to the queue
                    var body_ = new Message(Encoding.UTF8.GetBytes(message_));

                    // Write the body of the message to the console
                    Debug.WriteLine($"Sending message: {Encoding.UTF8.GetString(body_.Body)}");

                    // Send the message to the queue
                    await queueClient.SendAsync(body_);
                }

            }
            catch (Exception exception)
            {
                Debug.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }

            // Delay by 10 milliseconds ...
            await Task.Delay(10);

            //---Debug.WriteLine($" messages sent.");
        }

        //--- Send one message 
        public async Task PushMessageAsync2_1(string queueName, Message SendMessage)
        {
            try
            {
                await queueClient.SendAsync(SendMessage);
                //---Debug.WriteLine("--->queueClient.SendAsync(SendMessage)");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
            finally
            {
                await queueClient.CloseAsync();
            }
        }

        //--- Send List<Message> messages
        public async Task PushMessageAsync2_2(string queueName, List<Message> messagesToSend)
        {
            //---var index = 0;
            //ExecuteWithRetry(async () =>
            //{
                try
                {
                await queueClient.SendAsync(messagesToSend);


                //var receivedMaxSizeMessage = await queueClient.InnerReceiver.ReceiveAsync();

                //---Debug.WriteLine($"{DateTime.Now.TimeOfDay} :--->queueClient.SendAsync(messagesToSend)");

                for (var i = 0; i < messagesToSend.Count; i++)
                {
                    Debug.WriteLine($"{DateTime.Now.TimeOfDay} : Messages Body : {System.Text.Encoding.Default.GetString(messagesToSend[i].Body)}");
                }

                Debug.WriteLine($"{DateTime.Now.TimeOfDay} : Sent {messagesToSend.Count} messages");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
            finally
            {
                await queueClient.CloseAsync();
            }
            //});
        }

        //--- 4
        public void PushMessage(string queueName, Message message, IDictionary<string, object> queueArgs = null) 
        {

            var messagesToSend = new List<Message> { message };

            PushMessageAsync(queueName, messagesToSend, queueArgs).GetAwaiter().GetResult();
    
        }

        //--- 4
        public void PushMessage(string queueName, string message, IDictionary<string, object> queueArgs = null)
        {

            var messagesToSend = new List<string> { message };

            PushMessageAsync(queueName, messagesToSend, queueArgs).GetAwaiter().GetResult();

        }

        //--- 4.1
        //--- Push one message
        public void PushMessage2_1(string queueName, Message messageToSend)
        {
            PushMessageAsync2_1(queueName, messageToSend).GetAwaiter().GetResult();
        }

        //--- 4.2
        //--- Push List<Message> messages
        public void PushMessage2_2(string queueName, List<Message> messagesToSend)
        {
            PushMessageAsync2_2(queueName, messagesToSend);
        }

        public void PushMessage<T>(string queueName, List<T> messages, IDictionary<string, object> queueArgs = null)
        {
            //---var index = 0;
            ExecuteWithRetry(async () =>
            {
                try
                {
                    var index = 0;
                    //---
                    for (; index < messages.Count; index++)
                    {
                        var message_ = messages[index];
                        var body = ObjectToByteArray(message_);

                        //var consumer = new QueueingBasicConsumer(channel);
                        //channel.BasicPublish("", queueName, null, body);

                        // Create a new brokered message to send to the queue
                        var body_ = new Message(Encoding.UTF8.GetBytes(body.ToString()));

                        // Write the body of the message to the console
                        Debug.WriteLine($"Sending message: {Encoding.UTF8.GetString(body_.Body)}");

                        // Send the message to the queue
                        await queueClient.SendAsync(body_);
                    }

                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
                }

                // Delay by 10 milliseconds ...
                //---await Task.Delay(10);

                //---Debug.WriteLine($" messages sent.");


            });
        }

        //--- 5
        //--- Push many messages Async
        public async Task PushMessageAsync<T>(string queueName, List<T> messages, IDictionary<string, object> queueArgs = null)
        {
            try
            {
                var index = 0;
                //---
                for (; index < messages.Count; index++)
                {
                    var message_ = messages[index];
                    var body = ObjectToByteArray(message_);

                    //var consumer = new QueueingBasicConsumer(channel);
                    //channel.BasicPublish("", queueName, null, body);

                    // Create a new brokered message to send to the queue
                    var body_ = new Message(Encoding.UTF8.GetBytes(body.ToString()));

                    // Write the body of the message to the console
                    Debug.WriteLine($"Sending message: {Encoding.UTF8.GetString(body_.Body)}");

                    // Send the message to the queue
                    await queueClient.SendAsync(body_);
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
            }

            // Delay by 10 milliseconds ...
            await Task.Delay(10);

            //---Debug.WriteLine($" messages sent.");
        }

        //--- 5
        //--- Push many messages
        //public void PushMessage<T>(string queueName, List<T> messageList, IDictionary<string, object> queueArgs = null)
        //{
        //    //var messageList = new List<T> { message };

        //    PushMessageAsync(queueName, messageList, queueArgs).GetAwaiter().GetResult();
        //}

        //--- 6
        public void PushMessage<T>(string queueName, T message, IDictionary<string, object> queueArgs = null)
        {
            var messageList = new List<T> { message };
            PushMessageAsync(queueName, messageList, queueArgs).GetAwaiter().GetResult();
        }

        //--- 7
        public void DeleteQueue(List<string> queueNameList)
        {

            //var index = 0;

            //    using (var connection = factory.CreateConnection())
            //    {
            //        using (var channel = connection.CreateModel())
            //        {
            //            for (; index < queueNameList.Count; index++)
            //            {
            //                var queueName = queueNameList[index];
            //                channel.QueueDelete(queueName);
            //            }
            //        }
            //    }
            /*
        

            // Configure queue settings.            
            this.queueDescription = new QueueDescription(MyQueuePath);            
            this.queueDescription.MaxSizeInMegabytes = 1024;            
            // Setting message TTL to 5 days where as default TTL is 14 days.            
            this.queueDescription.DefaultMessageTimeToLive = TimeSpan.FromDays(7);            

            // Create management credentials.            
            this.credential = new TransportClientEndpointBehavior()            
            {            
                TokenProvider = Microsoft.ServiceBus.TokenProvider.CreateSharedSecretTokenProvider(issuerName, issuerSecret)            
                //TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(issuerName, issuerSecret)            
            };            

            // Create the URI for the queue.            
            this.namespaceUri = ServiceBusEnvironment.CreateServiceUri("sb", serviceNamespace, String.Empty);            
            Debug.WriteLine("Service Bus Namespace Uri address '{0}'", this.namespaceUri.AbsoluteUri);            

            var settings = new NamespaceManagerSettings() { TokenProvider = credential.TokenProvider };            
            var namespaceClient = new Microsoft.ServiceBus.NamespaceManager(namespaceUri, settings);            
            */

            //---
            //--- https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-create-queues
            //string name = "RootManageSharedAccessKey";

            //string  nameKey = "RootManageSharedAccessKey";
            //var password = queueConfig.Password;
            //var userName = queueConfig.UserName;
            //var queueNameDel = queueConfig.QueueNameDel;

            //Uri uri = ServiceBusEnvironment.CreateServiceUri("sb", "dacgroup-phoenix-test", string.Empty);            Uri uri = ServiceBusEnvironment.CreateServiceUri("sb", userName, string.Empty);

            // Create management credentials
            //---TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(sasKeyName, sasKeyValue);
            //---Microsoft.ServiceBus.TokenProvider tokenProvider = Microsoft.ServiceBus.TokenProvider.CreateSharedAccessSignatureTokenProvider(nameKey, password);
            Microsoft.ServiceBus.TokenProvider credentials = Microsoft.ServiceBus.TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", password);
            Microsoft.ServiceBus.TokenProvider tokenProvider = Microsoft.ServiceBus.TokenProvider.CreateSharedAccessSignatureTokenProvider("RootManageSharedAccessKey", password);
            NamespaceManager namespaceManager = new NamespaceManager(uri, tokenProvider);
            //namespaceManager.CreateQueue("DACQueue4");

            try
            {
                //--- Delete if exists
                if (namespaceManager.QueueExists(queueNameDel))
                    {
                        namespaceManager.DeleteQueue(queueNameDel);
                        Debug.WriteLine("Queue deleted successfully.", queueNameDel);
                    }
            }
            catch (FaultException e)
            {
                Debug.WriteLine("Exception when deleting queue..", e);
                logger.Trace($"Exception when deleting queue.. {0}: {e}");
                throw;
            }
        }
        //--- 8
        private byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        //--- 9

        public void Dispose()
        {
            queueClient.CloseAsync().GetAwaiter().GetResult(); ;

        }
        public async Task DisposeAsync()
        {
            await queueClient.CloseAsync();
        }

      }
}