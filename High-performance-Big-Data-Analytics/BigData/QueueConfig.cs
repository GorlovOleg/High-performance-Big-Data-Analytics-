//#define DAC
//#define cloudAMQP
#define AzureServiceBus

/*
Author          : Technical  Architect Oleg Gorlov
Description:	: Queue AzureServiceBus Config file
Copyright       : DAC group
email           : ogorlov@dacgroup.com
Date            : 07/07/2017
Release         : 1.0.0
Comment         : 
				: Queue pre-config :
                : Max size - 1 Gb
                : Message time - to live 14 day
                : Lock duration - 30 seconds
                : Enable sessions - true 
                : Enable partition - true
                : Enable dublicate detection - false 
                : Move expired message to the dead-letter - false 
 */
using DACFramework.Config;

namespace DAC.LLM.Queue.AzureServiceBusAMQP
{
    public class QueueConfig : IQueueConfig
    {
#if DAC
        #region RabbitMQ public variable
        /// <summary>
        /// </summary>

        public string Host { get; set; }
        public string QueueName { get; set; }
        public int PoolSize { get; set; }
        public ushort PoolPrefetchSize { get; set; }
        public string ReviewContentQueue { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PrimaryConnectionString { get; set; }
        #endregion
#elif cloudAMQP
        #region cloudAMQP public variable
        /// <summary>
        /// </summary>
        public string Host { get; set; }
        public string QueueName { get; set; }
        public int    PoolSize { get; set; }
        public ushort PoolPrefetchSize { get; set; }
        public string ReviewContentQueue { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PrimaryConnectionString { get; set; }
        #endregion
#elif AzureServiceBus
        #region AzureServiceBus public variable
        /// <summary>
        /// </summary>
        public string Host { get; set; }
        public string QueueName { get; set; }
        public int    PoolSize { get; set; }
        public ushort PoolPrefetchSize { get; set; }
        public string ReviewContentQueue { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PrimaryConnectionString { get; set; }
        public string QueueNameNew { get; set; } 
        public string QueueNameDel { get; set; } 
        #endregion
#endif
        public QueueConfig(string queueName)
        {

#if DAC
            
            #region RabbitMQ  AppSettings 
        /// <summary>
        /// Attempt to connect with a valid connection string
        /// </summary>

            Host = AppSettings.Get("xxx.url", "dacrabbitmq1.cloudapp.net");
            PoolSize = AppSettings.Get("app.pool_size", 100);
            PoolPrefetchSize = (ushort)AppSettings.Get("mq.prefetch", 500);
            ReviewContentQueue = AppSettings.Get("mq.queue.reviews", "StandardQueue");
            UserName = AppSettings.Get("mq.user");
            Password = AppSettings.Get("mq.pass");

            QueueName = queueName;
            #endregion
#elif cloudAMQP
            #region cloudAMQP AppSettings

            /// <summary>
            /// Configuration AzureServiceBus 
            /// </summary>
            /// 

            Host = AppSettings.Get("xxx.url", "xxx.rmq.cloudamqp.com");
            PoolSize = AppSettings.Get("app.pool_size", 100);
            PoolPrefetchSize = (ushort)AppSettings.Get("mq.prefetch", 500);
            ReviewContentQueue = AppSettings.Get("mq.queue.reviews", "StandardQueue");
            UserName = AppSettings.Get("xxx.user");
            Password = AppSettings.Get("xxx.pass");

            QueueName = queueName;

            #endregion
#elif  AzureServiceBus
            #region AzureServiceBus AppSettings

            /// <summary>
            /// Configuration AzureServiceBus 
            /// </summary>
            /// 
            //PrimaryConnectionString = AppSettings.Get("DAC.ServiceBus.ConnectionString");
            //Host = AppSettings.Get("smartsystemsservicebus.servicebus.windows.net");
            //Host = "dacgroup-phoenix-test.servicebus.windows.net";
            //UserName = AppSettings.Get("smartsystemsservicebus");
            //Password = AppSettings.Get("SmartSystems.ServiceBus.pass");
            //QueueName = queueName;

            PrimaryConnectionString = AppSettings.Get("DAC.ServiceBus.ConnectionString", "Endpoint=");
            Host = AppSettings.Get("xxx.url", "xxx.servicebus.windows.net");
            PoolSize = AppSettings.Get("app.pool_size", 100);
            PoolPrefetchSize = (ushort)AppSettings.Get("mq.prefetch", 500);
            ReviewContentQueue = AppSettings.Get("mq.queue.reviews", "StandardQueue");
            UserName = AppSettings.Get("DAC.ServiceBus.user");
            Password = AppSettings.Get("DAC.ServiceBus.pass");
            QueueNameNew = AppSettings.Get("DAC.ServiceBus.QueueNameNew");
            QueueNameDel = AppSettings.Get("DAC.ServiceBus.QueueNameDel");
            
            QueueName = queueName;

            #endregion
#endif
        }

    }
}
