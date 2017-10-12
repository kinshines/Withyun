using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonCrawler;
using Domain.Helper;
using Domain.Models;
using HtmlAgilityPack;
using NLogUtility;
using UploadImage;

namespace DailyCrawler
{
    public class Mp4Ba
    {
        private static readonly int Page = Convert.ToInt32(ConfigurationManager.AppSettings["PageForMp4Ba"]);
        private static int _requestCount = 0;
        private static int _fetchCount = 0;
        private static int _existResource = 0;
        public static void Fetch()
        {
            Info("Fetch Start......");
            _requestCount = 0;
            _fetchCount = 0;
            _existResource = 0;
            string url = "http://www.mp4ba.com/index.php?page=";
            for (int i = Page; i >= 1; i--)
            {
                try
                {
                    Info("Start Handle:" + url + i);
                    GetListPage(url + i);
                    Info("End Handle:" + url + i);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            Info("Fetch Count:{0},Request Count:{1},Exist Resource:{2}", _fetchCount, _requestCount, _existResource);
            Logger.Alert("Mp4Ba Summary:Fetch Count:{0},Request Count:{1},Exist Recource:{2}", _fetchCount, _requestCount, _existResource);
        }

        private static void GetListPage(string url)
        {
            string html = GetGeneralContent(url);
            if(html=="")
                return;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//tbody[@id='data_list']");
            if(htmlNode==null)
                return;
            foreach (HtmlNode node in htmlNode.Elements("tr"))
            {
                try
                {
                    var elements = node.Elements("td").ToArray();
                    if (elements.Length < 3)
                        continue;
                    var category = elements[1].InnerText.Trim();
                    Trace(category);
                    var detailTd = elements[2];
                    var detailLink = detailTd.Element("a");
                    var detailUrl = detailLink.GetAttributeValue("href", "").Trim();
                    Trace(detailUrl);
                    var title = detailLink.InnerText.Trim().Replace(".Mp4Ba", "");
                    Trace(title);
                    string resourceId = GetResourceId(detailUrl);
                    if (CrawlerUtility.ExistRecord(resourceId, ResourceType.Mp4Ba))
                    {
                        Info("Resource Existed,resourceId:{0}", resourceId);
                        _existResource++;
                        continue;
                    }
                    var blog = new Blog { Title = title.Length > 200 ? title.Substring(0, 200) : title };
                    string coverUrl = "";
                    var link = GetIntroDetail("http://www.mp4ba.com/" + detailUrl, blog, out coverUrl);
                    bool syncFlag = false;
                    syncFlag = SaveBlog(blog, link);
                    if (!syncFlag)
                    {
                        Info("Blog Sync Fail,detailUrl:{0},blogId:{1}", detailUrl, blog.Id);
                        continue;
                    }
                    syncFlag = SaveRecomment(blog, coverUrl, category);
                    if (!syncFlag)
                    {
                        Info("Recomment Sync Fail,blogId:{0},imageUrl:{1}", blog.Id, coverUrl);
                    }
                    CrawlerUtility.AddResourceRecord(blog.Id, resourceId, ResourceType.Mp4Ba);
                    Info("Blog Added,blogId:{0},resourceId:{1}", blog.Id, resourceId);
                    _fetchCount++;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private static bool SaveBlog(Blog blog, Link link)
        {
            blog.HtmlContent = "<html><body>" + blog.HtmlContent + "</body></html>";
            blog.UserId = 2;
            blog.UserName = "mp4ba";
            blog.Id = SyncUtility.SyncBlog(blog);
            if (blog.Id == 0)
                return false;
            SyncUtility.SyncLinks(blog.Id, new List<Link> { link });
            return true;
        }

        private static bool SaveRecomment(Blog blog, string imageUrl, string category)
        {
            if (imageUrl == "")
                return true;
            string fileName = CrawlerUtility.GetFileContent(imageUrl, Const.CoverFileDirectory, "http://www.mp4ba.com/");
            if (fileName == "")
            {
                return true;
            }

            var recomment = new Recomment()
            {
                BlogId = blog.Id,
                Title = blog.Title,
                CoverName = fileName,
                TimeStamp = DateTime.Now
            };
            if (category.Contains("电影"))
            {
                recomment.Category = RecommentCategory.电影;
            }
            if (category.Contains("电视剧"))
            {
                recomment.Category = RecommentCategory.剧集;
            }
            if (category == "欧美电视剧")
            {
                recomment.Category = RecommentCategory.美剧;
            }
            if (category == "日韩电视剧")
            {
                recomment.Category = RecommentCategory.韩剧;
            }
            if (category == "综艺娱乐")
            {
                recomment.Category = RecommentCategory.综艺;
            }
            string localFilePath = Const.CoverFileDirectory + fileName;
            try
            {
                using (var localImage = Image.FromFile(localFilePath))
                {
                    var image = ImgHandler.ZoomPicture(localImage, 200, 110);
                    image.Save(localFilePath);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "localFilePath:{0}", localFilePath);
                return true;
            }
            string yunUrl = UploadUtility.UploadLocalFile(localFilePath);
            Trace("localFile:{0} upload to yunUrl:{1}", localFilePath, yunUrl);
            if (!string.IsNullOrEmpty(yunUrl))
            {
                recomment.YunUrl = yunUrl;
                recomment.ImageStatus = ImageStatus.Yun;
            }
            return SyncUtility.SyncRecomment(recomment);
        }

        public static Link GetIntroDetail(string url, Blog blog, out string coverUrl)
        {
            string html = GetGeneralContent(url);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var introDiv = document.DocumentNode.SelectSingleNode("//div[@id='btm']//div[@class='intro']");
            coverUrl = GetFirstImageUrl(document);
            RemoveMp4BaInfo(introDiv);
            
            blog.HtmlContent = introDiv.InnerHtml.Replace("MP4吧", "").Trim();
            blog.Content =
                introDiv.InnerText.Replace("\r\n", "")
                    .Replace("&hellip;", "")
                    .Replace("&middot;", "")
                    .Replace("&nbsp;", "")
                    .Replace("MP4吧","")
                    .Trim();
            Trace(blog.Content);
            Trace(blog.HtmlContent);
            var linkDiv = document.DocumentNode.SelectSingleNode("//div[@id='btm']//div[@class='slayout']//div[@class='box']//p[@class='original magnet']//a");
            var managet = linkDiv.GetAttributeValue("href", "");
            Trace("managet:" + managet);
            if (managet.Length > 500)
            {
                Trace("managet too long");
                managet = managet.Substring(0, 500);
            }
            var link = new Link()
            {
                Description = managet,
                Url = managet
            };
            return link;
        }

        private static void RemoveMp4BaInfo(HtmlNode parentNode)
        {
            var strongNodes = parentNode.Elements("strong").ToArray();
            foreach (HtmlNode node in strongNodes)
            {
                string text = node.InnerText;
                if (text.Contains("MP4吧") || text.Contains("左上角") || text.Contains("主下载") ||
                    text.Contains("mp4ba") || text.Contains("请勿被骗"))
                {
                    parentNode.RemoveChild(node);
                }
            }
            var spanNodes = parentNode.Elements("span").ToArray();
            foreach (HtmlNode node in spanNodes)
            {
                string text = node.InnerText;
                if (text.Contains("MP4吧") || text.Contains("左上角") || text.Contains("主下载") ||
                    text.Contains("mp4ba") || text.Contains("请勿被骗"))
                {
                    parentNode.RemoveChild(node);
                }
            }
            var aNodes = parentNode.Elements("a").ToArray();
            foreach (HtmlNode node in aNodes)
            {
                string text = node.InnerText;
                if (text.Contains("MP4吧") || text.Contains("左上角") || text.Contains("主下载") ||
                    text.Contains("mp4ba") || text.Contains("请勿被骗"))
                {
                    parentNode.RemoveChild(node);
                }
            }

            var childNodes = parentNode.Elements("p").ToArray();
            foreach (HtmlNode childNode in childNodes)
            {
                RemoveMp4BaInfo(childNode);
            }
        }

        private static string GetFirstImageUrl(HtmlDocument document)
        {
            var imgNode = document.DocumentNode.SelectSingleNode("//div[@id='btm']//div[@class='intro']//img");
            if (imgNode == null)
                return "";
            string imgSrc = imgNode.GetAttributeValue("src", "");
            Trace("imgUrl:" + imgSrc);
            return imgSrc;
        }

        public static string GetResourceId(string detailUrl)
        {
            return detailUrl.Replace("show.php?hash=", "");
        }

        private static string GetGeneralContent(string strUrl)
        {
            _requestCount++;
            string referer = "http://www.mp4ba.com/";
            var cookies = new List<Cookie>()
            {
                new Cookie("CNZZDATA5925857", "cnzz_eid%3D921376316-1462025230-%26ntime%3D1467044423", "/",
                    "www.mp4ba.com")
            };

            string html = CrawlerUtility.GetGeneralContent(strUrl, referer, cookies);
            if (html.Contains("服务器维护中，请稍后访问"))
            {
                Info("服务器维护中，请稍后访问,60秒后继续操作");
                html = "";
            }
            if (string.IsNullOrEmpty(html))
            {
                Thread.Sleep(60 * 1000);
            }
            else
            {
                Thread.Sleep(5 * 1000);
            }
            return html;

        }

        private static void Trace(string message, params object[] args)
        {
            Logger.Trace("Mp4Ba", message, args);
        }
        private static void Info(string message, params object[] args)
        {
            Logger.Info("Mp4Ba", message, args);
        }

    }
}
