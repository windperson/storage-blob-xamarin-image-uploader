﻿/**----------------------------------------------------------------------------------
* Microsoft Developer & Platform Evangelism
*
* Copyright (c) Microsoft Corporation. All rights reserved.
*
* THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
* EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
* OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
*----------------------------------------------------------------------------------
* The example companies, organizations, products, domain names,	
* e-mail addresses, logos, people, places, and events depicted
* herein are fictitious.  No association with any real company,
* organization, product, domain name, email address, logo, person,
* places, or events is intended or should be inferred.
*----------------------------------------------------------------------------------
**/

using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;

namespace XamarinImageUploader
{
	public class ImageManager
	{
		public ImageManager ()
		{
		}

        private static CloudBlobContainer GetContainer()
        {
            var account = CloudStorageAccount.Parse(Configuration.StorageConnectionString);
            var client = account.CreateCloudBlobClient();

            var container = client.GetContainerReference("images");

            return container;
        }

        public static async Task<string> UploadImage(Stream image)
        {
            var container = GetContainer();

            await container.CreateIfNotExistsAsync();

            var name = RandomString(10);

            var imageBlob = container.GetBlockBlobReference(name);
            await imageBlob.UploadFromStreamAsync(image);
            
            return name;
        }

        public static async Task<string[]> ListImages()
        {
            var container = GetContainer();

            var allBlobs = new List<string>();
            BlobContinuationToken token = null;

            do
            {
                var result = await container.ListBlobsSegmentedAsync(token);
                if (result.Results.Count() > 0)
                {
                    var blobs = result.Results.Cast<CloudBlockBlob>().Select(b => b.Name);
                    allBlobs.AddRange(blobs);
                }

                token = result.ContinuationToken;
            } while (token != null);

            return allBlobs.ToArray();
        }

        public static async Task<byte[]> GetImage(string name)
        {
            var container = GetContainer();

            var blob = container.GetBlobReference(name);

			if(await blob.ExistsAsync())
            {
				await blob.FetchAttributesAsync();

                byte[] blobBytes = new byte[blob.Properties.Length];

                await blob.DownloadToByteArrayAsync(blobBytes, 0);

                return blobBytes;
            }

            return null;
        }

        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

