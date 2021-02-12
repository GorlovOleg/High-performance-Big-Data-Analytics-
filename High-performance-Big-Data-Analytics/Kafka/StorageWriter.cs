using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PeriodicProcessingLib;
using LogToScreenLib;
using CommonInterfacesLib;
using DataStoreFactoryLib;

namespace StorageWriterLib
{
    public class StorageWriter<T> : IDisposable
    {
        private IDataStore<T> dataStore;

        private ConcurrentQueue<KeyValuePair<string, T>> cqueData = new ConcurrentQueue<KeyValuePair<string, T>>();
        private Dictionary<string, KeyContext> dctKeyContext = new Dictionary<string, KeyContext>();

        private string keySuffix;

        private PeriodicProcessing periodicProcessing;

        public StorageWriter(int receiverId, string host, int port, int firstDbNum, int periodInMs, int trimLength, 
                        EventHandler<IError> dlgtOnError, string keySuffix,
                        Func<string, T, KeyContext, bool> dlgtFilter = null)
        {
            this.keySuffix = keySuffix;

            // Init DataStore
            dataStore = DataStore.Create<T>(host, database: firstDbNum, isDurationMeasured: true, port: port);

            periodicProcessing = new PeriodicProcessing(
                // PeriodicProc
                () =>
                {
                    KeyValuePair<string, T> pair;
                    while (cqueData.TryDequeue(out pair))
                    {
                        try
                        {
                            if (dlgtFilter?.Invoke(pair.Key, pair.Value, GetKeyContext(pair.Key)) != false)
                            {
                                dataStore.Add(pair.Key, pair.Value, trimLength);

                                // Publish DataStore "INSERTED" event
                                dataStore.PublishAsync("ReceiverChannel", $"INSERTED|DATA_RECEIVER|{receiverId}");
                            }
                        }
                        catch (Exception e)
                        {
                            LogToScreen.Exception(e);
                        }
                    }
                },
                e => dlgtOnError(this, new Error(e)),
                periodInMs);
        }

        public T this[string key]
        {
            set
            {
                cqueData.Enqueue(new KeyValuePair<string, T>($"{key}_{keySuffix}", value));
            }
        }

        public Dictionary<string, TimeSpan> GetOperationAverageDuration()
        {
            return dataStore.OperationsAverageDuration;
        }

        public void Dispose()
        {
            periodicProcessing.Dispose();
            dataStore.Dispose();
        }

        private KeyContext GetKeyContext(string key)
        {
            KeyContext keyContext = null;
            if (!dctKeyContext.TryGetValue(key, out keyContext))
                dctKeyContext[key] = keyContext = new KeyContext();

            return keyContext;
        }       
    }

    public class KeyContext
    {
        private Dictionary<string, object> dctKeyContextProperty = new Dictionary<string, object>();

        public object this[string name]
        {
            get
            {
                object propertyValue = null;
                dctKeyContextProperty.TryGetValue(name, out propertyValue);
                return propertyValue;
            }
            set
            {
                dctKeyContextProperty[name] = value;
            }
        }
    }
}
