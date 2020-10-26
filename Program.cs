using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace BeginnerCSharp
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter Storage Account Name: ");
            string accountName = Console.ReadLine();
            Console.WriteLine("\nEnter Storage Account Key: ");
            string accountKey = Console.ReadLine();

            string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + accountKey;
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
            ExecuteChoice(account);
        }

        public static void ExecuteChoice(CloudStorageAccount account)
        {
            Console.WriteLine("\nWhat type of transfer would you like to execute?\n1. Local file --> Azure Blob\n2. Local directory --> Azure Blob directory\n3. URL (e.g. Amazon S3 file) --> Azure Blob\n4. Azure Blob --> Azure Blob");
            int choice = int.Parse(Console.ReadLine());

            if (choice == 1)
            {
                TransferLocalFileToAzureBlob(account).Wait();
            }
            else if (choice == 2)
            {
                TransferLocalDirectoryToAzureBlobDirectory(account).Wait();
            }
            else if (choice == 3)
            {
                TransferUrlToAzureBlob(account).Wait();
            }
            else if (choice == 4)
            {
                TransferAzureBlobToAzureBlob(account).Wait();
            }
        }

        public static async Task TransferLocalFileToAzureBlob(CloudStorageAccount account)
        { 
            string localFilePath = Upload.GetSourcePath();
            CloudBlockBlob blob = Upload.GetBlob(account); 
            TransferCheckpoint checkpoint = null;
            SingleTransferContext context = Perf.GetSingleTransferContext(checkpoint); 
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            Console.WriteLine("\nTransfer started...\nPress 'c' to temporarily cancel your transfer...\n");

            Stopwatch stopWatch = Stopwatch.StartNew();
            Task task;
            ConsoleKeyInfo keyinfo;
            try
            {
                task = TransferManager.UploadAsync(localFilePath, blob, null, context, cancellationSource.Token);
                while(!task.IsCompleted)
                {
                    if(Console.KeyAvailable)
                    {
                        keyinfo = Console.ReadKey(true);
                        if(keyinfo.Key == ConsoleKey.C)
                        {
                            cancellationSource.Cancel();
                        }
                    }
                }
                await task;
            }
            catch(Exception e)
            {
                Console.WriteLine("\nThe transfer is canceled: {0}", e.Message);  
            }

            if(cancellationSource.IsCancellationRequested)
            {
                Console.WriteLine("\nTransfer will resume in 3 seconds...");
                Thread.Sleep(3000);
                checkpoint = context.LastCheckpoint;
                context = Perf.GetSingleTransferContext(checkpoint);
                Console.WriteLine("\nResuming transfer...\n");
                await TransferManager.UploadAsync(localFilePath, blob, null, context);
            }

            stopWatch.Stop();
            Console.WriteLine("\nTransfer operation completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");
            ExecuteChoice(account);
        }

        public static async Task TransferLocalDirectoryToAzureBlobDirectory(CloudStorageAccount account)
        { 
            string localDirectoryPath = Upload.GetSourcePath();
            CloudBlobDirectory blobDirectory = Upload.GetBlobDirectory(account); 
            TransferCheckpoint checkpoint = null;
            DirectoryTransferContext context = Perf.GetDirectoryTransferContext(checkpoint); 
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            Console.WriteLine("\nTransfer started...\nPress 'c' to temporarily cancel your transfer...\n");

            Stopwatch stopWatch = Stopwatch.StartNew();
            Task task;
            ConsoleKeyInfo keyinfo;
            UploadDirectoryOptions options = new UploadDirectoryOptions()
            {
                Recursive = true
            };

            try
            {
                task = TransferManager.UploadDirectoryAsync(localDirectoryPath, blobDirectory, options, context, cancellationSource.Token);
                while(!task.IsCompleted)
                {
                    if(Console.KeyAvailable)
                    {
                        keyinfo = Console.ReadKey(true);
                        if(keyinfo.Key == ConsoleKey.C)
                        {
                            cancellationSource.Cancel();
                        }
                    }
                }
                await task;
            }
            catch(Exception e)
            {
                Console.WriteLine("\nThe transfer is canceled: {0}", e.Message);  
            }

            if(cancellationSource.IsCancellationRequested)
            {
                Console.WriteLine("\nTransfer will resume in 3 seconds...");
                Thread.Sleep(3000);
                checkpoint = context.LastCheckpoint;
                context = Perf.GetDirectoryTransferContext(checkpoint);
                Console.WriteLine("\nResuming transfer...\n");
                await TransferManager.UploadDirectoryAsync(localDirectoryPath, blobDirectory, options, context);
            }

            stopWatch.Stop();
            Console.WriteLine("\nTransfer operation completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");
            ExecuteChoice(account);
        }

        public static async Task TransferUrlToAzureBlob(CloudStorageAccount account)
        {
            Uri uri = new Uri(Upload.GetSourcePath());
            CloudBlockBlob blob = Upload.GetBlob(account); 
            TransferCheckpoint checkpoint = null;
            SingleTransferContext context = Perf.GetSingleTransferContext(checkpoint); 
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            Console.WriteLine("\nTransfer started...\nPress 'c' to temporarily cancel your transfer...\n");

            Stopwatch stopWatch = Stopwatch.StartNew();
            Task task;
            ConsoleKeyInfo keyinfo;
            try
            {
                task = TransferManager.CopyAsync(uri, blob, true, null, context, cancellationSource.Token);
                while(!task.IsCompleted)
                {
                    if(Console.KeyAvailable)
                    {
                        keyinfo = Console.ReadKey(true);
                        if(keyinfo.Key == ConsoleKey.C)
                        {
                            cancellationSource.Cancel();
                        }
                    }
                }
                await task;
            }
            catch(Exception e)
            {
                Console.WriteLine("\nThe transfer is canceled: {0}", e.Message);  
            }

            if(cancellationSource.IsCancellationRequested)
            {
                Console.WriteLine("\nTransfer will resume in 3 seconds...");
                Thread.Sleep(3000);
                checkpoint = context.LastCheckpoint;
                context = Perf.GetSingleTransferContext(checkpoint);
                Console.WriteLine("\nResuming transfer...\n");
                await TransferManager.CopyAsync(uri, blob, true, null, context, cancellationSource.Token);
            }

            stopWatch.Stop();
            Console.WriteLine("\nTransfer operation completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");
            ExecuteChoice(account);
        }

        public static async Task TransferAzureBlobToAzureBlob(CloudStorageAccount account)
        {
            CloudBlockBlob sourceBlob = Upload.GetBlob(account);
            CloudBlockBlob destinationBlob = Upload.GetBlob(account); 
            TransferCheckpoint checkpoint = null;
            SingleTransferContext context = Perf.GetSingleTransferContext(checkpoint); 
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            Console.WriteLine("\nTransfer started...\nPress 'c' to temporarily cancel your transfer...\n");

            Stopwatch stopWatch = Stopwatch.StartNew();
            Task task;
            ConsoleKeyInfo keyinfo;
            try
            {
                task = TransferManager.CopyAsync(sourceBlob, destinationBlob, true, null, context, cancellationSource.Token);
                while(!task.IsCompleted)
                {
                    if(Console.KeyAvailable)
                    {
                        keyinfo = Console.ReadKey(true);
                        if(keyinfo.Key == ConsoleKey.C)
                        {
                            cancellationSource.Cancel();
                        }
                    }
                }
                await task;
            }
            catch(Exception e)
            {
                Console.WriteLine("\nThe transfer is canceled: {0}", e.Message);  
            }

            if(cancellationSource.IsCancellationRequested)
            {
                Console.WriteLine("\nTransfer will resume in 3 seconds...");
                Thread.Sleep(3000);
                checkpoint = context.LastCheckpoint;
                context = Perf.GetSingleTransferContext(checkpoint);
                Console.WriteLine("\nResuming transfer...\n");
                await TransferManager.CopyAsync(sourceBlob, destinationBlob, false, null, context, cancellationSource.Token);
            }

            stopWatch.Stop();
            Console.WriteLine("\nTransfer operation completed in " + stopWatch.Elapsed.TotalSeconds + " seconds.");
            ExecuteChoice(account);
        }
    }
}
