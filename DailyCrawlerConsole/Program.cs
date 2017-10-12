using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyCrawler;
using NLogUtility;

namespace DailyCrawlerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Ed2000.Fetch();
                Mp4Ba.Fetch();
                Zimuzu.Fetch();
                Ed2Kers.Fetch();
                Verycd.Fetch();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
