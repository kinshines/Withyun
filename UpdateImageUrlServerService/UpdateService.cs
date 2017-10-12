using System;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Domain.Helper;
using Domain.Models;
using Domain.Services;
using NLogUtility;
using UploadImage;

namespace UpdateImageUrlServerService
{
    public partial class UpdateService : ServiceBase
    {
        const string BlogFileDirectory = @"C:\Withyun\images\blog";
        const string CoverFileDirectory = @"C:\Withyun\images\cover";
        private Thread _workerThread = null;
        public UpdateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_workerThread == null ||
                (_workerThread.ThreadState &
                  (System.Threading.ThreadState.Unstarted | System.Threading.ThreadState.Stopped)) != 0)
            {
                _workerThread = new Thread(new ThreadStart(UpdateServiceProcess));
                _workerThread.Start();
                MessageLog("图片更新服务启动");
            }
        }

        private void UpdateServiceProcess()
        {
            while (true)
            {
                try
                {
                    ImageUrlService imageService = new ImageUrlService();
                    var blogImageList = imageService.GetLocalList();
                    int blogImageCount = blogImageList.Count;
                    MessageLog("updating blogImageCount:" + blogImageCount);
                    int successCount = 0;
                    foreach (ImageUrl image in blogImageList)
                    {
                        string localPath = BlogFileDirectory + image.Url;
                        string yunUrl = UploadUtility.UploadLocalFile(localPath);
                        if (yunUrl == "")
                        {
                            Thread.Sleep(5 * 60 * 1000);
                            continue;
                        }
                        image.YunUrl = yunUrl;
                        image.ImageStatus = ImageStatus.Yun;
                        if (imageService.Update(image))
                        {
                            MessageLog("update success,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                            successCount++;
                        }
                        else
                        {
                            MessageLog("update fail,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                        }
                        Thread.Sleep(15 * 1000);
                    }
                    MessageLog("updated blogImageCount:" + successCount);
                    imageService.Dispose();
                    RecommentService recommentService = new RecommentService();
                    var coverImageList = recommentService.GetLocalList();
                    int coverImageCount = coverImageList.Count;
                    MessageLog("updating coverImageCount:" + coverImageCount);
                    successCount = 0;
                    foreach (Recomment image in coverImageList)
                    {
                        string localPath = CoverFileDirectory + image.CoverName;
                        string yunUrl = UploadUtility.UploadLocalFile(localPath);
                        if (yunUrl == "")
                        {
                            Thread.Sleep(5 * 60 * 1000);
                            continue;
                        }
                        image.YunUrl = yunUrl;
                        image.ImageStatus = ImageStatus.Yun;
                        if (recommentService.Update(image))
                        {
                            MessageLog("update success,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                            successCount++;
                        }
                        else
                        {
                            MessageLog("update fail,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                        }
                        Thread.Sleep(15 * 1000);
                    }
                    MessageLog("updated blogImageCount:" + successCount);
                    recommentService.Dispose();
                    if (blogImageCount == 0 && coverImageCount == 0)
                    {
                        MessageLog("record empty,sleeping 10 minutes");
                        Thread.Sleep(10 * 60 * 1000);
                    }
                }
                catch (ThreadAbortException threadAbortException)
                {
                    Logger.Error(threadAbortException);
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    Thread.Sleep(10 * 60 * 1000);
                }
            }
        }

        protected override void OnStop()
        {
            this.RequestAdditionalTime(10000);
            if (_workerThread != null && _workerThread.IsAlive)
            {
                _workerThread.Abort();
                _workerThread.Join();
                MessageLog("图片更新服务结束");
            }
            this.ExitCode = 0;
        }

        public static void MessageLog(string message, params object[] args)
        {
            Logger.Info("UpdateService", message, args);
        }
    }
}
