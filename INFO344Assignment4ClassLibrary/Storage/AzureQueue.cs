using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace INFO344Assignment4ClassLibrary.Storage
{
    public class AzureQueue
    {
        private CloudQueue _queue;

        public AzureQueue(string connString, string queueName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();

            this._queue = queue;
        }

        public CloudQueue GetQueue()
        {

            return _queue;
        }
    }
}
