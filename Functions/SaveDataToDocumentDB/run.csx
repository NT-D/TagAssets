using System;

public static void Run(string myQueueItem, TraceWriter log, out string outputDocument)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");   
    outputDocument = myQueueItem;
}