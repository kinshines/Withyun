using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DailyCrawler;
using NLogUtility;

namespace DailyCrawlerService
{
    public partial class CrawlerService : ServiceBase
    {
        private Thread _workerThread = null;
        private static DateTime _runTime = DateTime.MinValue;

        public CrawlerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_workerThread == null ||
                (_workerThread.ThreadState &
                  (System.Threading.ThreadState.Unstarted | System.Threading.ThreadState.Stopped)) != 0)
            {
                _workerThread = new Thread(new ThreadStart(CrawlerServiceProcess));
                _workerThread.Start();
                MessageLog("每日抓取服务启动");
            }
        }

        private void CrawlerServiceProcess()
        {
            while (true)
            {
                try
                {
                    if (6 < DateTime.Now.Hour && DateTime.Now.Hour < 20)
                    {
                        MessageLog("The program won't executed between 6 and 20");
                        Thread.Sleep(1*60*60*1000);
                        continue;
                    }
                    if (_runTime.Date == DateTime.Now.Date)
                    {
                        MessageLog("The program has been executed today");
                        Thread.Sleep(1*60*60*1000);
                        continue;
                    }
                    if ((DateTime.Now - _runTime).Hours < 20)
                    {
                        MessageLog("The program has been executed in recent 20 hours");
                        Thread.Sleep(1*60*60*1000);
                        continue;
                    }
                    Ed2000.Fetch();
                    Mp4Ba.Fetch();
                    Zimuzu.Fetch();
                    Ed2Kers.Fetch();
                    Verycd.Fetch();
                    _runTime = DateTime.Now;
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
                MessageLog("每日抓取服务结束");
            }
            this.ExitCode = 0;
        }

        public static void MessageLog(string message, params object[] args)
        {
            Logger.Info("CrawlerService", message, args);
        }
    }
}
