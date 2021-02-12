using System;
using System.Collections.Generic;
using System.Threading;
using DataStoreFactoryLib;
using ObjectsLib;
using PeriodicProcessingLib;
using LogToScreenLib;
using CommonInterfacesLib;

namespace StorageReaderLib
{
    public class StorageReader<T> : IDisposable
    {
        private IDataStore<T> dataStore;

        private string keySuffix;

        private PeriodicProcessing periodicProcessing;

        public StorageReader(Action<int, IList<DSData>> dlgtAction, int processorId, string host, int port, 
                        int firstDbNum, int periodInMs, int[] dsRange, int maxLagInMs,
                        EventHandler<IError> dlgtOnError,
                        string keySuffix)
        {
            this.keySuffix = keySuffix;

            // Init DataStore
            dataStore = DataStore.Create<T>(host, database: firstDbNum, isDurationMeasured: true, port: port);

            if (periodInMs == Timeout.Infinite)
                // Receive DataStore "INSERTED" event
                dataStore.Subscribe("ReceiverChannel",
                    s =>
                    {
                        if (string.IsNullOrEmpty(s))
                            return;

                        var ss = s.Split('|');
                        int receiverId;
                        if (ss[0] == "INSERTED" && ss[1] == "DATA_RECEIVER" && int.TryParse(ss[2], out receiverId) &&
                                processorId == receiverId &&
                                periodicProcessing != null)
                            periodicProcessing.Execute();
                    });

            periodicProcessing = new PeriodicProcessing(
                // PeriodicProc
                () =>
                {
                    for (int i = dsRange[0]; i <= dsRange[1]; i++)
                    {
                        try
                        {
                            var list = dataStore.Get(GetKeyByDsNum(i));
                            if (list.Count > 0)
                            {
                                var lstData = new List<DSData>();
                                foreach (var d in list)
                                {
                                    var item = Convert(d);
                                    if (item != null)
                                        lstData.Add(item);
                                }

                                if (lstData.Count > 0)
                                {
                                    dlgtAction?.Invoke(i, lstData);

                                    // Delay verification
                                    if (/*lag*/DateTime.UtcNow - lstData[0].Timestamp > TimeSpan.FromMilliseconds(maxLagInMs))
                                        // Lag is too high
                                        LogToScreen.Message("?");
                                }

                                // Verification of timestamps for LIFO in the list
                                for (int j = 0; j < list.Count - 1; j++)
                                {
                                    var d0 = Convert(list[j]);
                                    var d1 = Convert(list[j + 1]);
                                    if (d0.Timestamp <= d1.Timestamp)
                                        // Wrong chronologic order of data
                                        LogToScreen.Message(" ?T ");
                                }
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

        public string GetKeyByDsNum(int ds)
        {
            return $"DS#{ds}_{keySuffix}";
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

        private static DSData Convert(object d)
        {
            if (d == null)
                return null;

            var item = (dynamic)d;
            var type = item.GetType();

            if (type == typeof(DSData))
                return item;

            if (type == typeof(byte[]) || type == typeof(string))
                return new DSData(item);

            return null;
        }
    }
}

