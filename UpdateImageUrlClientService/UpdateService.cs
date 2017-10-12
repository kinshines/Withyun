using System;
using System.ServiceProcess;
using System.Threading;
using CommonCrawler;
using Domain.Models;
using NLogUtility;
using UploadImage;

namespace UpdateImageUrlClientService
{
    public partial class UpdateService : ServiceBase
    {
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
                    var blogImageList = ImageUrlDao.GetToUploadList();
                    int blogImageCount = blogImageList.Count;
                    MessageLog("updating blogImageCount:" + blogImageCount);
                    int successCount = 0;
                    foreach (ImageUrl image in blogImageList)
                    {
                        string localPath = Const.BlogFileDirectory + image.Url;
                        string yunUrl = UploadUtility.UploadLocalFile(localPath);
                        if (yunUrl == "")
                        {
                            Thread.Sleep(5 * 60 * 1000);
                            continue;
                        }
                        image.YunUrl = yunUrl;
                        image.ImageStatus = ImageStatus.Yun;
                        if (SyncUtility.SyncImageUrl(image))
                        {
                            MessageLog("sync success,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                            ImageUrlDao.DeleteRecord(image.Id);
                            successCount++;
                        }
                        else
                        {
                            MessageLog("sync fail,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                        }
                        Thread.Sleep(15 * 1000);
                    }
                    MessageLog("updated blogImageCount:" + successCount);

                    var coverImageList = RecommentDao.GetToUploadList();
                    int coverImageCount = coverImageList.Count;
                    MessageLog("updating coverImageCount:" + coverImageCount);
                    successCount = 0;
                    foreach (Recomment image in coverImageList)
                    {
                        if (image.CoverName.Trim().StartsWith("http"))
                        {
                            image.CoverName = CrawlerUtility.GetFileContent(image.CoverName.Trim(),
                                Const.CoverFileDirectory, "");
                            if(string.IsNullOrEmpty(image.CoverName))
                                continue;
                        }
                        string localPath = Const.CoverFileDirectory + image.CoverName;
                        string yunUrl = UploadUtility.UploadLocalFile(localPath);
                        if (yunUrl == "")
                        {
                            //Thread.Sleep(5 * 60 * 1000);
                            continue;
                        }
                        image.YunUrl = yunUrl;
                        image.ImageStatus = ImageStatus.Yun;
                        if (SyncUtility.SyncRecomment(image))
                        {
                            MessageLog("sync success,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                            RecommentDao.DeleteRecord(image.Id);
                            successCount++;
                        }
                        else
                        {
                            MessageLog("sync fail,localPath:{0} uploaded yunUrl:{1}", localPath, yunUrl);
                        }
                        Thread.Sleep(15 * 1000);
                    }
                    MessageLog("updated blogImageCount:" + successCount);
                    if (blogImageCount == 0 && coverImageCount == 0)
                    {
                        MessageLog("record empty,sleeping 10 minutes");
                        Thread.Sleep(10*60*1000);
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

        public static void MessageLog(string message,params object[] args)
        {
            Logger.Info("UpdateService", message, args);
        }
    }
}
