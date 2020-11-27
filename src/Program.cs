using System;
namespace OSSFileTool
{
    class Program
    {
        static void Main(string[] args)
        {
            //bucketName accessKeyId accessKeySecret endpoint UploadFilePath
            bool help = false;
            bool isBig = false;
            string accesskey = string.Empty;
            string secret = string.Empty;
            string endpoint = string.Empty;
            string bucketname = string.Empty;
            string filename = string.Empty;
            string downpath = string.Empty;
            OptionSet argsparse = new OptionSet()
               .Add<string>("k=|key=", "Aliyun AccessKeyId", v => accesskey = v)
               .Add<string>("s=|secret=", "Aliyun AccessKeySecret", v => secret = v)
               .Add<string>("endpoint=", "Aliyun OSS Endpoint", v => endpoint = v)
               .Add<string>("bucket=", "Aliyun OSS Bucket Name", v => bucketname = v)
               .Add<string>("f=|file=", "Upload File Name Or Download Filename", v => filename = v)
               .Add<string>("d=|down=", "Download File Path", v => downpath = v)
               .Add("b|big", "When File is Big", v => isBig = v != null)
               .Add("h|help", "Get Usage", v => help = v != null);
            try
            {
                argsparse.Parse(args);
                if (help)
                {
                    Console.WriteLine(@"
   ____   _____ _____ ______ _ _   _______          _ 
  / __ \ / ____/ ____|  ____(_) | |__   __|        | |
 | |  | | (___| (___ | |__   _| | ___| | ___   ___ | |
 | |  | |\___ \\___ \|  __| | | |/ _ \ |/ _ \ / _ \| |
 | |__| |____) |___) | |    | | |  __/ | (_) | (_) | |
  \____/|_____/_____/|_|    |_|_|\___|_|\___/ \___/|_|
  https://github.com/B1eed  @1x2Bytes
");
                    argsparse.WriteOptionDescriptions(Console.Out);
                }
                else if (!string.IsNullOrEmpty(accesskey) && !string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(bucketname) && !string.IsNullOrEmpty(filename))
                {
                    if (string.IsNullOrEmpty(downpath))
                    {
                        if (isBig)
                        {
                            OSSUpload.UploadBigFile(accesskey,secret,endpoint,bucketname,filename);
                        }
                        else {
                            OSSUpload.PutFile2OSS(accesskey,secret,endpoint,bucketname,filename);
                        }
                    }
                    else
                    {
                        OSSDownload.GetFileFromOSS(accesskey,secret,endpoint,bucketname,filename,downpath);
                    }
                }

            }
            catch
            {
                Console.WriteLine("[!]Parser Args Error!");
            }
        }
    }
}
