#define _BYTES

using System;
using DataStoreFactoryLib;
using ObjectsLib;
using CommonInterfacesLib;
using StorageReaderLib;

namespace DataProcessor
{
    class Program
    {
        // Config
        const string prefixForDataProcessor = "DataProcessor#";

        static void Main(string[] args)
        {
            // Local Redis details to read configuration
            int processorId = 0;
            string host = "localhost";
            int port = 6379;
            int configDbNum = 0;

            if (args != null && args.Length > 3)
            {
                int.TryParse(args[0], out processorId);
                host = args[1];
                int.TryParse(args[2], out port);
                int.TryParse(args[3], out configDbNum);
            }

            Console.WriteLine($"DATA PROCESSOR {processorId}\n");

            // Uncomment the following line to set up breakpoint
            //Debugger.Launch();

            // Config
            var config = DataStore.GetConfig<ProcessorConfigData>(processorId, host, port, prefixForDataProcessor, configDbNum);
           
            // Creation of data storage reader and starting its periodic reading
#if _BYTES
            var dataStorageReader = new StorageReader<byte[]>(
#else
            var dataStorageReader = new StorageReader<string>(
#endif
                            dlgtAction: (ds, lstData) =>
                            {
                                // Processing of read data should be inserted here
                                var dsData = lstData[0];
                                //......
                            },
                            processorId: processorId,
                            host:        config.Host,
                            port:        config.Port,
                            firstDbNum:  config.DbNum,
                            periodInMs:  config.PeriodInMs,
                            dsRange:     config.DsRange,
                            maxLagInMs:  config.MaxLagInMs,
                            dlgtOnError: (s, e) => ProcessError(e),
                            keySuffix:   config.KeySuffix);

            Console.WriteLine("\nPress any key to get Data Storage operations avarage duration...");
            Console.ReadKey();

            var dctDuration = dataStorageReader?.GetOperationAverageDuration();
            Console.WriteLine("\nAverage Duration of Data Storage Operations");
            foreach (var key in dctDuration.Keys)
                if (dctDuration[key] != TimeSpan.Zero)
                    Console.WriteLine($"{key}:  {dctDuration[key]}");

            Console.WriteLine("\nPress any key to quit...");
            Console.ReadKey();
             
            dataStorageReader.Dispose();
        }

        static void ProcessError(IError e)
        {
            Console.WriteLine($"ERROR: {e.Code} {e.Reason}");
        }
    }
}
