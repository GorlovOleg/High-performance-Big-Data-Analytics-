#define AzureServiceBus
/*
Author          : Technical  Architec Oleg Gorlov
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

using DACFramework.Config;
using System.Diagnostics;
using System.Threading.Tasks;
//using Microsoft.ServiceBus;
//using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

//using Microsoft.Azure.ServiceBus;

//using Polly;

//---using Microsoft.Azure.ServiceBus;
//using AzureServiceBus.Client;
//using AzureServiceBus.Client.Exceptions;
//using AzureServiceBus.Client.MessagePatterns;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.Amqp;
using System.Threading;

namespace DAC.LLM.Queue.AzureServiceBusAMQP
{

    public class QueueReader : QueueReaderBase, IQueueReader
    {
        public static string ConnectionString = "Endpoint=xxx";
        public const string NonPartitionedQueueName = "non-partitioned-queue";
        public int numberOfMessages = 10;
        public string QueueName = "dacgroupqueue1";
        static IQueueClient queueClient;

        //--- 1
        public QueueReader(IQueueConfig queueConfig, ILog logger) /*: base(queueConfig, logger)*/
        {

        }

        public string ReadMessage_()
        {
            ReadMessageTest().GetAwaiter().GetResult();
            //---var result = ReadMessageTest2();
            //---ReadMessageTest3().GetAwaiter().GetResult();
            //--- var result = ReadMessageTest3();
            //--- Multiple_plugins_should_run_in_order().GetAwaiter().GetResult();

            //---var result2 = Multiple_plugins_should_run_in_order();

            return "true";
        }


        static async Task ReadMessageTest3()
        {
            const int numberOfMessages = 10;
            queueClient = new QueueClient(ConnectionString, NonPartitionedQueueName, ReceiveMode.PeekLock);

            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();

            // Send Messages
            //---await SendMessagesAsync(numberOfMessages);

            //---Console.ReadKey();

            await queueClient.CloseAsync();
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that will process messages
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            
            Debug.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        // Use this Handler to look at the exceptions received on the MessagePump
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        async Task<bool> ReadMessageTest2()
        {

            IQueueClient queueClient;

            queueClient = new QueueClient(ConnectionString, NonPartitionedQueueName);

            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages();

            // Send Messages
            await SendMessagesAsync(numberOfMessages);

            await queueClient.CloseAsync();

            return true;
        }



        async Task<bool> ReadMessageTest()
        {

            string ConnectionString = "Endpoint=sb://dacgroup-phoenix-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=i84yvck3SujQeMaA5oG6NmaqQoG9kSqJKK5mdhMb1GI=;TransportType=Amqp";
            //--var messageReceiver = new MessageReceiver(TestUtility.NamespaceConnectionString, TestConstants.NonPartitionedQueueName, ReceiveMode.ReceiveAndDelete);

            //var messageSender = new MessageSender(TestUtility.NamespaceConnectionString, TestConstants.NonPartitionedQueueName);
            //var messageReceiver = new MessageReceiver(TestUtility.NamespaceConnectionString, TestConstants.NonPartitionedQueueName, ReceiveMode.ReceiveAndDelete);

            var messageSender = new MessageSender(ConnectionString, NonPartitionedQueueName);
            var messageReceiver = new MessageReceiver(ConnectionString, NonPartitionedQueueName, ReceiveMode.PeekLock);

            try
            {
                //var firstPlugin = new FirstSendPlugin();
                //var secondPlugin = new SecondSendPlugin();

                //messageSender.RegisterPlugin(firstPlugin);
                //messageSender.RegisterPlugin(secondPlugin);

                //var sendMessage = new Message(Encoding.UTF8.GetBytes("Test message..."));
                //await messageSender.SendAsync(sendMessage);

                var receivedMessage = await messageReceiver.ReceiveAsync(1, TimeSpan.FromMinutes(1));
                var _receivedMessage = receivedMessage;
                //var firstSendPluginUserProperty = receivedMessage.First().UserProperties["FirstSendPlugin"];
                //var secondSendPluginUserProperty = receivedMessage.First().UserProperties["SecondSendPlugin"];

                //Assert.True((bool)firstSendPluginUserProperty);
                //Assert.True((bool)secondSendPluginUserProperty);
            }
            finally
            {
                await messageSender.CloseAsync();
                await messageReceiver.CloseAsync();
            }
            return true;
        }

        async Task<bool> Multiple_plugins_should_run_in_order()
        {

            string ConnectionString = "Endpoint=sb://dacgroup-phoenix-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=i84yvck3SujQeMaA5oG6NmaqQoG9kSqJKK5mdhMb1GI=;TransportType=Amqp";
            //--var messageReceiver = new MessageReceiver(TestUtility.NamespaceConnectionString, TestConstants.NonPartitionedQueueName, ReceiveMode.ReceiveAndDelete);

            //var messageSender = new MessageSender(TestUtility.NamespaceConnectionString, TestConstants.NonPartitionedQueueName);
            //var messageReceiver = new MessageReceiver(TestUtility.NamespaceConnectionString, TestConstants.NonPartitionedQueueName, ReceiveMode.ReceiveAndDelete);

            var messageSender = new MessageSender(ConnectionString, NonPartitionedQueueName);
            var messageReceiver = new MessageReceiver(ConnectionString, NonPartitionedQueueName, ReceiveMode.PeekLock);

            try
            {
                //---
                await SendMessagesAsync(messageSender, 10);
                //---



                var firstPlugin = new FirstSendPlugin();
                var secondPlugin = new SecondSendPlugin();

                messageSender.RegisterPlugin(firstPlugin);
                messageSender.RegisterPlugin(secondPlugin);

                var sendMessage = new Message(Encoding.UTF8.GetBytes("Test message..."));
                await messageSender.SendAsync(sendMessage);

                //var receivedMessage = await messageReceiver.ReceiveAsync(1, TimeSpan.FromMinutes(1));
                //var firstSendPluginUserProperty = receivedMessage.First().UserProperties["FirstSendPlugin"];
                //var secondSendPluginUserProperty = receivedMessage.First().UserProperties["SecondSendPlugin"];

                //Assert.True((bool)firstSendPluginUserProperty);
                //Assert.True((bool)secondSendPluginUserProperty);
            }
            finally
            {
                await messageSender.CloseAsync();
                await messageReceiver.CloseAsync();
            }

            return true;
        }



        public async Task<bool> ReadMessageTest1()
        {
            var messageReceiver = new MessageReceiver(ConnectionString, "non-partitioned-queue", ReceiveMode.PeekLock);
            var messageSender = new MessageSender(ConnectionString, "non-partitioned-queue");

            try
            {


                var firstPlugin = new FirstSendPlugin();
                var secondPlugin = new SecondSendPlugin();

                messageSender.RegisterPlugin(firstPlugin);
                messageSender.RegisterPlugin(secondPlugin);

                var sendMessage = new Message(Encoding.UTF8.GetBytes("Test message>>>>>>"));
                await messageSender.SendAsync(sendMessage);

                var receivedMessage = await messageReceiver.ReceiveAsync(TimeSpan.FromMinutes(1));
                var receivedMessage_body = receivedMessage.Body.ToString();

                //var firstSendPluginUserProperty = receivedMessage.First().UserProperties["FirstSendPlugin"];
                //var secondSendPluginUserProperty = receivedMessage.First().UserProperties["SecondSendPlugin"];

                //Assert.True((bool)firstSendPluginUserProperty);
                //Assert.True((bool)secondSendPluginUserProperty);
            }
            finally
            {
                await messageSender.CloseAsync();
                await messageReceiver.CloseAsync();
            }

            return true;
        }

        public class FirstSendPlugin : ServiceBusPlugin
        {
            public override string Name => nameof(SendReceivePlugin);

            public override Task<Message> BeforeMessageSend(Message message)
            {
                message.UserProperties.Add("FirstSendPlugin", true);
                return Task.FromResult(message);
            }
        }

        public class SecondSendPlugin : ServiceBusPlugin
        {
            public override string Name => nameof(SendReceivePlugin);

            public override Task<Message> BeforeMessageSend(Message message)
            {
                // Ensure that the first plugin actually ran first
                //Assert.True((bool)message.UserProperties["FirstSendPlugin"]);
                message.UserProperties.Add("SecondSendPlugin", true);
                return Task.FromResult(message);
            }
        }

        public class SendReceivePlugin : ServiceBusPlugin
        {
            // Null the body on send, and replace it when received.
            public Dictionary<string, byte[]> MessageBodies = new Dictionary<string, byte[]>();

            public override string Name => nameof(SendReceivePlugin);

            public override Task<Message> BeforeMessageSend(Message message)
            {
                this.MessageBodies.Add(message.MessageId, message.Body);
                var clonedMessage = message.Clone();
                clonedMessage.Body = null;
                return Task.FromResult(clonedMessage);
            }

            public override Task<Message> AfterMessageReceive(Message message)
            {
                //Assert.Null(message.Body);
                message.Body = this.MessageBodies[message.MessageId];
                return Task.FromResult(message);
            }
        }

        public class ExceptionPlugin : ServiceBusPlugin
        {
            public override string Name => nameof(ExceptionPlugin);

            public override Task<Message> BeforeMessageSend(Message message)
            {
                throw new NotImplementedException();
            }
        }


        public static async Task SendMessagesAsync(IMessageSender messageSender, int messageCount)
        {
            if (messageCount == 0)
            {
                await Task.FromResult(false);
            }

            var messagesToSend = new List<Message>();
            for (var i = 0; i < messageCount; i++)
            {
                var message = new Message(Encoding.UTF8.GetBytes("test" + i));
                message.Label = "test" + i;
                messagesToSend.Add(message);
            }

            await messageSender.SendAsync(messagesToSend);
            //---Log($"Sent {messageCount} messages");
        }

        public static async Task<IList<Message>> ReceiveMessagesAsync(IMessageReceiver messageReceiver, int messageCount)
        {
            var receiveAttempts = 0;
            var messagesToReturn = new List<Message>();

            while (receiveAttempts++ < 10 && messagesToReturn.Count < messageCount)
            {
                var messages = await messageReceiver.ReceiveAsync(messageCount - messagesToReturn.Count);
                if (messages != null)
                {
                    messagesToReturn.AddRange(messages);
                }
            }

            //VerifyUniqueMessages(messagesToReturn);
            //Log($"Received {messagesToReturn.Count} messages");
            return messagesToReturn;
        }


        //--- 13
        public void Dispose()
        {
            //DisposeInactiveChannelsAndSubscriptions(string.Empty);
        }
    }
}