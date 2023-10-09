using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using TestBlobStorage.Utilities;

namespace TestBlobStorage.Services
{
    public class BlobStorageManager : IStorageManager
    {
        public readonly BlobStorageOptions _storageOptions;

        public BlobStorageManager(IOptions<BlobStorageOptions> options)
        {
            _storageOptions = options.Value;
        }

        public bool DeleteFile(string fileName)
        {
            var serviceClient = new BlobServiceClient(_storageOptions.ConnectionString);
            var containerClient = serviceClient.GetBlobContainerClient(_storageOptions.ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            return blobClient.DeleteIfExists().Value;
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                var serviceClient = new BlobServiceClient(_storageOptions.ConnectionString);
                var containerClient = serviceClient.GetBlobContainerClient(_storageOptions.ContainerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                var response = await blobClient.DeleteAsync();

                return response.Status == 202; 
            }
            catch (RequestFailedException ex)
            {
                return false;
            }
        }

        public string GetSignedUrl(string fileName)
        {
            var serviceClient = new BlobServiceClient(_storageOptions.ConnectionString);
            var contaionerClient = serviceClient.GetBlobContainerClient(_storageOptions.ContainerName);
            var blobClient = contaionerClient.GetBlobClient(fileName);

            var signedUrl = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTime.Now.AddHours(2)).AbsoluteUri;
            
            return signedUrl;
        }

        public async Task<string> GetSignedUrlAsync(string fileName)
        {
            var serviceClient = new BlobServiceClient(_storageOptions.ConnectionString);
            var contaionerClient = serviceClient.GetBlobContainerClient(_storageOptions.ContainerName);
            var blobClient = contaionerClient.GetBlobClient(fileName);

            var signedUrl = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTime.Now.AddHours(2)).AbsoluteUri;

            return await Task.FromResult(signedUrl);
        }

        public bool UploadFile(Stream stream, string fileName, string contentType)
        {
            var serviceClient = new BlobServiceClient(_storageOptions.ConnectionString);
            var containerClient = serviceClient.GetBlobContainerClient(_storageOptions.ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = blobClient.Upload(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });

            return true;
        }

        public async Task<bool> UploadFileAsync(Stream stream, string fileName, string contentType)
        {
            var serviceClient = new BlobServiceClient(_storageOptions.ConnectionString);
            var containerClient = serviceClient.GetBlobContainerClient(_storageOptions.ContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            });

            return response.Value.ContentHash == (await blobClient.GetPropertiesAsync()).Value.ContentHash;
        }
    }
}
