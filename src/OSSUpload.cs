using System;
using System.Collections.Generic;
using System.IO;
using Aliyun.OSS;
using Aliyun.OSS.Common;
namespace OSSFileTool
{
    class OSSUpload
    {
        public static void PutFile2OSS(string accesskey,string secret,string endpoint,string bucketname,string filename)
        {
            string objName = Path.GetFileName(filename);
            var client = new OssClient(endpoint, accesskey,secret);
            // 带进度条的上传。
            try
            {
                using (var fs = File.Open(filename, FileMode.Open))
                {
                    var putObjectRequest = new PutObjectRequest(bucketname, objName, fs);
                    putObjectRequest.StreamTransferProgress += streamProgressCallback;
                    client.PutObject(putObjectRequest);
                }
                Console.WriteLine($"Put File:{objName} succeeded");
            }
            catch (OssException ex)
            {
                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID: {2}\tHostID: {3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
            }
        }
        // 获取上传进度。
        private static void streamProgressCallback(object sender, StreamTransferProgressArgs args)
        {
            Console.WriteLine($"Progress: {args.TransferredBytes * 100 / args.TotalBytes}%, TotalBytes:{args.TotalBytes}, TransferredBytes:{args.TransferredBytes}");
        }
        public static void UploadBigFile(string accesskey,string secret,string endpoint,string bucketname,string filename) {
            string objName = Path.GetFileName(filename);
            // 创建OssClient实例。
            var client = new OssClient(endpoint, accesskey, secret);
            // 初始化分片上传。
            var uploadId = "";
            try
            {
                // 定义上传的文件及所属存储空间的名称。您可以在InitiateMultipartUploadRequest中设置ObjectMeta，但不必指定其中的ContentLength。
                var request = new InitiateMultipartUploadRequest(bucketname, objName);
                var result = client.InitiateMultipartUpload(request);
                uploadId = result.UploadId;
                // 打印UploadId。
                Console.WriteLine("Upload Id:{0}", result.UploadId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Init multipart upload failed:{ex.Message}" );
            }
            // 计算分片总数。
            var partSize = 100 * 1024;
            var fi = new FileInfo(filename);
            var fileSize = fi.Length;
            var partCount = fileSize / partSize;
            if (fileSize % partSize != 0)
            {
                partCount++;
            }
            // 开始分片上传。PartETags是保存PartETag的列表，OSS收到用户提交的分片列表后，会逐一验证每个分片数据的有效性。当所有的数据分片通过验证后，OSS会将这些分片组合成一个完整的文件。
            var partETags = new List<PartETag>();
            try
            {
                using (var fs = File.Open(filename, FileMode.Open))
                {
                    for (var i = 0; i < partCount; i++)
                    {
                        var skipBytes = (long)partSize * i;
                        fs.Seek(skipBytes, 0);
                        var size = (partSize < fileSize - skipBytes) ? partSize : (fileSize - skipBytes);
                        var request = new UploadPartRequest(bucketname, objName, uploadId)
                        {
                            InputStream = fs,
                            PartSize = size,
                            PartNumber = i + 1
                        };
                        request.StreamTransferProgress += streamProgressCallback;
                        var result = client.UploadPart(request);
                        partETags.Add(result.PartETag);
                        Console.WriteLine($"finish {partETags.Count}/{partCount}");
                    }
                    Console.WriteLine("MultipartUpload succeeded");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MultipartUpload failed:{ex.Message}");
            }
            // 完成分片上传。
            try
            {
                var completeMultipartUploadRequest = new CompleteMultipartUploadRequest(bucketname, objName, uploadId);
                foreach (var partETag in partETags)
                {
                    completeMultipartUploadRequest.PartETags.Add(partETag);
                }
                var result = client.CompleteMultipartUpload(completeMultipartUploadRequest);
                Console.WriteLine("complete Multipart succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"complete Multipart failed, {ex.Message}");
            }
        }
    }
}
