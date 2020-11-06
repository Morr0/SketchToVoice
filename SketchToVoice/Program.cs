using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

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
            string voiceUrl = await GetVoice(text);
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
            throw new NotImplementedException();
        }
        
        private static async Task<string> GetVoice(string text)
        {
            throw new NotImplementedException();
        }
    }
}