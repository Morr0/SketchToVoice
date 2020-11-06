using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Textract;
using Amazon.Textract.Model;

namespace SketchToVoice
{
    class Program
    {
        public const string FileNameToLookFor = "sample.png";
        
        public const string S3BucketName = "project-sketch-to-voice";
        public const string SketchDir = "sketch";
        public const string TextDir = "text";
        public const string AudioDir = "audio";

        private static AmazonS3Client _s3Client;

        static Program()
        {
            _s3Client = new AmazonS3Client();
        }

        static async Task Main(string[] args)
        {
            FileStream reader = GetFile();
            await CreateS3BucketIfNotExists();
            string sketchPath = await UploadToBucket(reader);
            string text = await ReadText(sketchPath);
            await StoreTextAsTxtInBucket(text);
            string voiceUrl = await StoreVoiceInS3Bucket(text);

            Cleanup();
        }
        
        private static FileStream GetFile()
        {
            return new FileStream(FileNameToLookFor, FileMode.Open, FileAccess.Read);
        }
        
        private static async Task CreateS3BucketIfNotExists()
        {
            var bucketsResponse = await _s3Client.ListBucketsAsync(new ListBucketsRequest());

            bool bucketExists = false;
            foreach (var bucket in bucketsResponse.Buckets)
            {
                if (bucket.BucketName == S3BucketName)
                {
                    bucketExists = true;
                    break;
                }
            }

            if (!bucketExists)
            {
                var request = new PutBucketRequest
                {
                    BucketName = S3BucketName,
                    UseClientRegion = true
                };

                await _s3Client.PutBucketAsync(request);
            }
        }
        
        private static async Task<string> UploadToBucket(FileStream reader)
        {
            string path = $"{SketchDir}/{FileNameToLookFor}";
            var request = new PutObjectRequest
            {
                BucketName = S3BucketName,
                Key = path,
                InputStream = reader,
            };

            await _s3Client.PutObjectAsync(request);
            return path;
        }
        
        private static async Task<string> ReadText(string textUrl)
        {
            using var client = new AmazonTextractClient();
            
            var document = new Document
            {
                S3Object = new Amazon.Textract.Model.S3Object
                {
                    Bucket = S3BucketName,
                    Name = textUrl
                }
            };
            var request = new DetectDocumentTextRequest
            {
                Document = document
            };

            var response = await client.DetectDocumentTextAsync(request);
            return GetTextFromBlocks(response.Blocks);
        }

        private static string GetTextFromBlocks(List<Block> responseBlocks)
        {
            StringBuilder sb = new StringBuilder(responseBlocks.Count);
            foreach (var block in responseBlocks)
            {
                sb.AppendLine(block.Text);
            }

            return sb.ToString();
        }
        
        private static async Task StoreTextAsTxtInBucket(string text)
        {
            var request = new PutObjectRequest
            {
                BucketName = S3BucketName,
                Key = $"{TextDir}/{FileNameToLookFor}.txt",
                ContentBody = text
            };

            await _s3Client.PutObjectAsync(request);
        }

        private static async Task<string> StoreVoiceInS3Bucket(string text)
        {
            throw new NotImplementedException();
        }
        
        private static void Cleanup()
        {
            _s3Client.Dispose();
        }
    }
}