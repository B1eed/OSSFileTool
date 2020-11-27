using System;
using System.IO;
using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace OSSFileTool
{
    class OSSDownload
    {
        public static void GetFileFromOSS(string accesskey, string secret, string endpoint, string bucketname,string objName, string filename)
        {
            var client = new OssClient(endpoint, accesskey, secret);
            try
            {
                var getObjectRequest = new GetObjectRequest(bucketname, objName);
                var fs = File.Open(filename, FileMode.OpenOrCreate);
                getObjectRequest.StreamTransferProgress += streamProgressCallback;
                var ossObject = client.GetObject(getObjectRequest);
                using (var stream = ossObject.Content)
                {
                    var buffer = new byte[1024 * 1024];
                    var bytesRead = 0;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                        // 处理读取的数据（此处代码省略）。
                    }
                    fs.Close();
                }
                Console.WriteLine($"Download File:{filename} succeeded");
            }
            catch (OssException ex)
            {
                Console.WriteLine($"Failed with error code: {ex.ErrorCode}; Error info: {ex.Message}. \nRequestID:{ex.RequestId}\tHostID:{ex.HostId}" );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed with error info: { ex.Message}");
            }
        }
        private static void streamProgressCallback(object sender, StreamTransferProgressArgs args)
        {
            Console.WriteLine($"Progress: {args.TransferredBytes * 100 / args.TotalBytes}%, TotalBytes:{args.TotalBytes}, TransferredBytes:{args.TransferredBytes}");
        }
    }
}
