using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    public class Verycd
    {
        private static readonly int Page = Convert.ToInt32(ConfigurationManager.AppSettings["PageForVerycd"]);

        public static Dictionary<string, string> UrlDictionary
        {
            get
            {
                if (_urlDictionary == null)
                {
                    _urlDictionary = new Dictionary<string, string>();
                    InitialDictionary();
                }
                return _urlDictionary;
            }
        }

        private static Dictionary<string, string> _urlDictionary;
        private static int _requestCount = 0;
        private static int _fetchCount = 0;
        private static int _existResource = 0;

        public static void Fetch()
        {
            Info("Fetch Start......");
            _requestCount = 0;
            _fetchCount = 0;
            _existResource = 0;
            for (int i = Page; i >= 1; i--)
            {
                foreach (KeyValuePair<string, string> pair in UrlDictionary)
                {
                    string category = pair.Key;
                    string url = pair.Value;
                    try
                    {
                        Info("Start Handle:" + url + i);
                        GetListPage(url + i, category);
                        Info("End Handle:" + url + i);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
            Info("Fetch Count:{0},Request Count:{1},Exist Recource:{2}", _fetchCount, _requestCount, _existResource);
            Logger.Alert("Verycd Summary:Fetch Count:{0},Request Count:{1},Exist Recource:{2}", _fetchCount, _requestCount, _existResource);
        }

        private static void InitialDictionary()
        {
            _urlDictionary.Add("music", "http://verycd.gdajie.com/music/page");
            _urlDictionary.Add("game", "http://verycd.gdajie.com/game/page");
            _urlDictionary.Add("cartoon", "http://verycd.gdajie.com/cartoon/page");
            _urlDictionary.Add("book", "http://verycd.gdajie.com/book/page");
            _urlDictionary.Add("tv", "http://verycd.gdajie.com/tv/page");
            _urlDictionary.Add("software", "http://verycd.gdajie.com/software/page");
            _urlDictionary.Add("datum", "http://verycd.gdajie.com/datum/page");
            _urlDictionary.Add("edu", "http://verycd.gdajie.com/edu/page");
        }

        private static void GetListPage(string url, string category)
        {
            string html = GetGeneralContent(url);
            if (html == "")
                return;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//div[@id='main']//ul");
            if (htmlNode == null)
                return;
            foreach (HtmlNode node in htmlNode.Elements("li"))
            {
                try
                {
                    var blog = new Blog();
                    var thumb = node.Element("div");
                    if (thumb == null)
                        continue;
                    var link = thumb.Element("a");
                    if (link == null)
                        continue;
                    var detailUrl = link.GetAttributeValue("href", "");
                    if (detailUrl == "")
                        continue;
                    Trace(detailUrl);
                    string resourceId = GetResourceId(detailUrl);
                    if (CrawlerUtility.ExistRecord(resourceId, ResourceType.Verycd))
                    {
                        Info("resourceId:{0} exist", resourceId);
                        _existResource++;
                        continue;
                    }
                    var title = link.GetAttributeValue("title", "");
                    if (title == "")
                        continue;
                    blog.Title = title.Length > 200 ? title.Substring(0, 200) : title;
                    Trace(title);
                    var style = link.GetAttributeValue("style", "");
                    var imageUrl = style.Replace("background-image:", "").Replace("url(", "").Replace(")", "").Trim();
                    Trace("imageUrl:" + imageUrl);
                    var info = node.Elements("div").Skip(1).FirstOrDefault();
                    if (info == null)
                        continue;
                    var infoParas = info.Elements("p");
                    StringBuilder htmlBuilder = new StringBuilder();
                    StringBuilder textBuilder = new StringBuilder();
                    var paraArray = infoParas.Skip(1).Take(4).ToArray();
                    foreach (HtmlNode para in paraArray)
                    {
                        htmlBuilder.Append(para.OuterHtml);
                        textBuilder.Append(para.InnerText);
                    }
                    blog.HtmlContent = htmlBuilder.ToString();
                    blog.Content = textBuilder.ToString();

                    var urlList = GetIntroDetail(detailUrl, blog);
                    if (urlList.Count == 0)
                    {
                        continue;
                    }
                    bool syncFlag = SaveBlog(blog, urlList);
                    if (!syncFlag)
                    {
                        Info("Blog Sync Fail,url:{0},blogId:{1}", url, blog.Id);
                        continue;
                    }
                    syncFlag = SaveRecomment(blog, imageUrl, category);
                    if (!syncFlag)
                    {
                        Info("Recomment Sync Fail,blogId:{0},imageUrl:{1}", blog.Id, imageUrl);
                    }
                    CrawlerUtility.AddResourceRecord(blog.Id, resourceId, ResourceType.Verycd);
                    Info("Blog Added,blogId:{0},resourceId:{1}", blog.Id, resourceId);
                    _fetchCount++;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private static bool SaveBlog(Blog blog, List<Link> linkList)
        {
            blog.HtmlContent = "<html><body>" + blog.HtmlContent + "</body></html>";
            blog.UserId = 4;
            blog.UserName = "verycd";
            blog.Id = SyncUtility.SyncBlog(blog);
            if (blog.Id == 0)
                return false;
            return SyncUtility.SyncLinks(blog.Id, linkList);
        }

        private static bool SaveRecomment(Blog blog, string imageUrl, string category)
        {
            if (imageUrl == "http://verycd.gdajie.com/img/default_cover.jpg" || imageUrl == "")
                return true;
            string fileName = CrawlerUtility.GetFileContent(imageUrl, Const.CoverFileDirectory, "http://www.gdajie.com/");
            if (string.IsNullOrEmpty(fileName))
                return true;
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
            Info("localFile:{0} upload to yunUrl:{1}", localFilePath, yunUrl);
            var recomment = new Recomment()
            {
                BlogId = blog.Id,
                CoverName = fileName,
                Title = blog.Title,
                TimeStamp = DateTime.Now,
                Category = SetCategory(category)
            };
            if (!string.IsNullOrEmpty(yunUrl))
            {
                recomment.YunUrl = yunUrl;
                recomment.ImageStatus = ImageStatus.Yun;
            }
            return SyncUtility.SyncRecomment(recomment);
        }

        private static RecommentCategory SetCategory(string category)
        {
            switch (category)
            {
                case "book":
                    return RecommentCategory.图书;
                case "music":
                    return RecommentCategory.音乐;
                case "game":
                    return RecommentCategory.游戏;
                case "cartoon":
                    return RecommentCategory.动漫;
                case "tv":
                    return RecommentCategory.剧集;
                case "software":
                    return RecommentCategory.软件;
                case "datum":
                    return RecommentCategory.资料;
                case "edu":
                    return RecommentCategory.教育;
                default:
                    return RecommentCategory.资料;
            }
        }

        private static string GetResourceId(string detailUrl)
        {
            string id = Regex.Replace(detailUrl, @"[^\d]", "");
            return id.Trim();
        }

        private static List<Link> GetIntroDetail(string url, Blog blog)
        {
            List<Link> linkList = new List<Link>();
            string html = GetGeneralContent(url);
            if (html == "")
                return linkList;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var tbody = document.DocumentNode.SelectSingleNode("//div[@id='main']//table[@id='emuleFile']//tbody");
            foreach (HtmlNode tr in tbody.Elements("tr"))
            {
                try
                {
                    var td = tr.Element("td");
                    if (td == null)
                        continue;
                    var font = td.Element("font");
                    if (font == null)
                        continue;
                    var emuleLink = font.Element("a");
                    if (emuleLink == null)
                        continue;
                    string detailLink = emuleLink.GetAttributeValue("href", "");
                    Info("detailLink:" + detailLink);
                    if (detailLink == "")
                        continue;
                    Link link = GetEd2K(detailLink);
                    if (link == null || link.Url == "")
                        continue;
                    linkList.Add(link);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            var description = document.DocumentNode.SelectSingleNode("//div[@id='main']//div[@class='description']");
            blog.HtmlContent += description.InnerHtml;
            blog.Content += description.InnerText;
            return linkList;
        }

        private static Link GetEd2K(string url)
        {

            string html = GetGeneralContent(url);
            if (html == "")
                return null;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//div[@id='detail']//table//a");
            Link link = new Link
            {
                Description = htmlNode.InnerText,
                Url = htmlNode.GetAttributeValue("href", "")
            };
            if (link.Description.Length > 500)
            {
                link.Description = link.Description.Substring(0, 500);
            }
            if (link.Url.Length > 500)
            {
                link.Url = link.Url.Substring(0, 500);
            }
            Trace("Description:" + link.Description);
            Trace("Url:" + link.Url);
            return link;
        }

        private static string GetGeneralContent(string strUrl)
        {
            _requestCount++;
            string referer = "http://www.gdajie.com/";
            var cookies = new List<Cookie>()
            {
                new Cookie("AJSTAT_ok_times", "5", "/", "verycd.gdajie.com"),
                new Cookie("Hm_lpvt_9398e7331484620894c57216bea9225e", "1468695450", "/", ".gdajie.com"),
                new Cookie("JSESSIONID", "7F0463C2223BFF2E8868FC7EFA7D0167", "/", "verycd.gdajie.com"),
                new Cookie("CNZZDATA4616656",
                    "cnzz_eid%3D1953947045-1464628831-http%253A%252F%252Fwww.verycd.gdajie.com%252F%26ntime%3D1467845452",
                    "/", "verycd.gdajie.com"),
                new Cookie("CNZZDATA1254524697", "78693709-1464629094-%7C1468690207", "/", "verycd.gdajie.com")
            };
            string html = CrawlerUtility.GetGeneralContent(strUrl, referer, cookies);
            if (html.Contains("对不起，我们怀疑您正使用采集软件对我们的网站进行采集，所以我们采取了封您ip的决定"))
            {
                Console.WriteLine("url:" + strUrl + ",ip被封,_requestCount:" + _requestCount);
                Info("url:" + strUrl + ",ip被封,requestCount:" + _requestCount);
                html = "";
            }
            if (string.IsNullOrEmpty(html))
            {
                Thread.Sleep(10 * 1000);
            }
            else
            {
                Thread.Sleep(5 * 1000);
            }
            return html;
        }

        private static void Trace(string message, params object[] args)
        {
            Logger.Trace("Verycd", message, args);
        }
        private static void Info(string message, params object[] args)
        {
            Logger.Info("Verycd", message, args);
        }
    }
}
