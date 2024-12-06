using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlobDemo;

class Program
{
    //Use your blob service endpoint here
    private const string blobServiceEndPoint = "<BLOB SERVICE_ENDPOINT>";

    //Use your Storage Account Name here
    private const string storageAccountName = "<STORAGE_ACCOUNT_NAME>";

    //use your Storage Account Key here
    private const string storageAccountKey = "<STORAGE_ACCOUNT_KEY>";

    static async Task Main(string[] args)
    {
        //The following line of code will create a new instance of StorageSharedKeyCredential by using 
        //StorageAccountName and StorageAccountKey
        StorageSharedKeyCredential accountCredential =
            new StorageSharedKeyCredential(storageAccountName, storageAccountKey);

        //The following code will create a new instance of BlobServiceClient by using
        //blobServiceEndpoint and account credentials
        BlobServiceClient serviceClient = new BlobServiceClient(new Uri(blobServiceEndPoint), accountCredential);

        //Invoke GetAccountInfoAsync() to retrieve teh account metadata from the service
        AccountInfo accountInfo = await serviceClient.GetAccountInfoAsync();

        //render a welcome message
        await Console.Out.WriteLineAsync($"Connected to Azure Storage Account");

        //render Storage Account Name
        await Console.Out.WriteLineAsync($"Account Name:\t{storageAccountName}");

        //render Storage Account Type
        await Console.Out.WriteLineAsync($"Account Type:\t{accountInfo?.AccountKind}");

        //render the currently selected stock keeping unit (SKU) for the storage account
        await Console.Out.WriteLineAsync($"Account sku:\t{accountInfo?.SkuName}");
        
        await EnumerateContainersAsync(serviceClient);

        string existingContainerName = "<CONTAINER NAME ON STORAGE ACCOUNT>";
        await EnumerateBlobsAsync(serviceClient, existingContainerName);

        string newContainerName = "<NEW CONTAINER TO BE CREATED>";
        BlobContainerClient containerClient = await GetContainerAsync(serviceClient, newContainerName);
        
        string uploadedBlobName = $"<UPLOADED BLOB NAME>";
        BlobClient blobClient = await GetBlobAsync(containerClient, uploadedBlobName);
        await Console.Out.WriteLineAsync($"Blob URL:\t{blobClient.Uri}");

        Console.ReadLine();
    }

    private static async Task EnumerateContainersAsync(BlobServiceClient serviceClient)
    {
        //create asynchronous foreach loop that iterates over the result of an invocation of the
        //GetBlobContainerAsync method of BlobServiceClient class
        await foreach (BlobContainerItem item in serviceClient.GetBlobContainersAsync())
        {
            await Console.Out.WriteLineAsync($"Container:\t{item.Name}");
        }
        
    }

    private static async Task EnumerateBlobsAsync(BlobServiceClient serviceClient, string containerName)
    {
        //get the new instance of blobContainerClient class by using the GetBlobContainerClient method 
        BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);
        await Console.Out.WriteLineAsync($"Container Name:\t{containerClient.Name}");
        
        //print name of each blob
        await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
        {
            await Console.Out.WriteLineAsync($"Blob:\t{blobItem.Name}");
        }
    }

    private static async Task<BlobContainerClient> GetContainerAsync(BlobServiceClient serviceClient, string containerName)
    {
        BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        
        await Console.Out.WriteLineAsync($"New Container Name:\t{containerClient.Name}");
        return containerClient;
    }

    private static async Task<BlobClient> GetBlobAsync(BlobContainerClient containerClient, string blobName)
    {
        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        bool exists = await blobClient.ExistsAsync();
        if (!exists)
        {
            await Console.Out.WriteLineAsync($"Blob {blobClient.Name} not found!");
        }
        else
        {
            await Console.Out.WriteLineAsync($"Blob found. URI: \t{blobClient.Uri}");
        }
        return blobClient;
    }
}