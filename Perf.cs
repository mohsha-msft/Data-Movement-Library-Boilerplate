using System;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;

namespace BeginnerCSharp
{
    class Perf
    {
        public static void SetNumberOfParallelOperations()
        {
            Console.WriteLine("\nHow many parallel operations would you like to use?");
            string parallelOperations = Console.ReadLine();
            TransferManager.Configurations.ParallelOperations = int.Parse(parallelOperations);
        }

        public static SingleTransferContext GetSingleTransferContext(TransferCheckpoint checkpoint)
        {
            SingleTransferContext context = new SingleTransferContext(checkpoint);

            context.ProgressHandler = new Progress<TransferStatus>((progress) =>
            {
                Console.Write("\rBytes transferred: {0}", progress.BytesTransferred);
            });

            return context;
        }

        public static DirectoryTransferContext GetDirectoryTransferContext(TransferCheckpoint checkpoint)
        {
            DirectoryTransferContext context = new DirectoryTransferContext(checkpoint);

            context.ProgressHandler = new Progress<TransferStatus>((progress) =>
            {
                Console.Write("\rBytes transferred: {0}", progress.BytesTransferred);
            });

            return context;
        }
    }
}