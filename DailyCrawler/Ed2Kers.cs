using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommonCrawler;
using Domain.Models;
using HtmlAgilityPack;
using NLogUtility;

namespace DailyCrawler
{
    public class Ed2Kers
    {
        private static readonly int Page = Convert.ToInt32(ConfigurationManager.AppSettings["PageForEd2kers"]);

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
                        Info("Start Handle:" + string.Format(url, i));
                        GetListPage(string.Format(url, i), category);
                        Info("End Handle:" + string.Format(url, i));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
            Info("Fetch Count:{0},Request Count:{1},Resource Record:{2}", _fetchCount, _requestCount, _existResource);
            Logger.Alert("Ed2Kers Summary:Fetch Count:{0},Request Count:{1},Exist Recource:{2}", _fetchCount,
                _requestCount, _existResource);
        }

        private static void InitialDictionary()
        {
            _urlDictionary.Add("图书", "http://www.ed2kers.com/%E5%9B%BE%E4%B9%A6/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("资料", "http://www.ed2kers.com/%E8%B5%84%E6%96%99/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("音乐", "http://www.ed2kers.com/%E9%9F%B3%E4%B9%90/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("电影", "http://www.ed2kers.com/%E7%94%B5%E5%BD%B1/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("软件", "http://www.ed2kers.com/%E8%BD%AF%E4%BB%B6/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("教育", "http://www.ed2kers.com/%E6%95%99%E8%82%B2/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("游戏", "http://www.ed2kers.com/%E6%B8%B8%E6%88%8F/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("综艺", "http://www.ed2kers.com/%E7%BB%BC%E8%89%BA/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("电视剧", "http://www.ed2kers.com/%E7%94%B5%E8%A7%86%E5%89%A7/%E7%AC%AC{0}%E9%A1%B5");
            _urlDictionary.Add("动漫", "http://www.ed2kers.com/%E5%8A%A8%E6%BC%AB/%E7%AC%AC{0}%E9%A1%B5");
        }

        private static void GetListPage(string url, string category)
        {
            string html = GetGeneralContent(url);
            if (html == "")
                return;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//div[@id='tech']");
            if (htmlNode == null)
                return;
            foreach (HtmlNode node in htmlNode.Elements("div"))
            {
                try
                {
                    var childrenDiv = node.Elements("div").ToArray();
                    if (childrenDiv == null || childrenDiv.Length < 3)
                        continue;
                    var blog = new Blog();
                    var thumb = childrenDiv.FirstOrDefault();
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
                    if (CrawlerUtility.ExistRecord(resourceId, ResourceType.Ed2Kers))
                    {
                        Info("resourceId:{0} exist", resourceId);
                        _existResource++;
                        continue;
                    }
                    var imageNode = link.Element("img");
                    string imageUrl = "";
                    if (imageNode != null)
                    {
                        imageUrl = imageNode.GetAttributeValue("data-original", "");
                        Trace("imageUrl:" + imageUrl);
                    }

                    var titleNode = childrenDiv[1].Element("ul").Element("li").Elements("a").LastOrDefault();
                    if (titleNode == null)
                        continue;
                    var title = titleNode.InnerText;
                    if (title == "")
                        continue;
                    blog.Title = title.Length > 200 ? title.Substring(0, 200) : title;
                    Trace(title);

                    var urlList = GetIntroDetail("http://www.ed2kers.com/" + detailUrl, blog);
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
                    CrawlerUtility.AddResourceRecord(blog.Id, resourceId, ResourceType.Ed2Kers);
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
            blog.UserId = 6;
            blog.UserName = "ed2kers";
            blog.Id = SyncUtility.SyncBlog(blog);
            if (blog.Id == 0)
                return false;
            return SyncUtility.SyncLinks(blog.Id, linkList);
        }

        private static bool SaveRecomment(Blog blog, string imageUrl, string category)
        {
            if (imageUrl == "")
                return true;
            string fileName = CrawlerUtility.GetFileContent(imageUrl, Const.CoverFileDirectory, "http://www.ed2kers.com/");
            if (string.IsNullOrEmpty(fileName))
                fileName = imageUrl;
            string localFilePath = Const.CoverFileDirectory + fileName;

            string yunUrl = "";
            //UploadUtility.UploadLocalFile(localFilePath);
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
                case "图书":
                    return RecommentCategory.图书;
                case "音乐":
                    return RecommentCategory.音乐;
                case "电影":
                    return RecommentCategory.电影;
                case "游戏":
                    return RecommentCategory.游戏;
                case "动漫":
                    return RecommentCategory.动漫;
                case "电视剧":
                    return RecommentCategory.剧集;
                case "软件":
                    return RecommentCategory.软件;
                case "资料":
                    return RecommentCategory.资料;
                case "教育":
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
            var tbody = document.DocumentNode.SelectSingleNode("//table[@id='ed2k']//tbody");
            if (tbody == null)
                return linkList;
            foreach (HtmlNode tr in tbody.Elements("tr"))
            {
                try
                {
                    var td = tr.Element("td");
                    if (td == null)
                        continue;
                    var emuleLink = td.Element("a");
                    if (emuleLink == null)
                        continue;
                    Link link = new Link
                    {
                        Description = emuleLink.InnerText.Trim(),
                        Url = emuleLink.GetAttributeValue("href", "").Replace("[www.ed2kers.com]", "").Trim()
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
                    if (string.IsNullOrEmpty(link.Url))
                        continue;
                    linkList.Add(link);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            var arcbody = document.DocumentNode.SelectSingleNode("//div[@class='arcbody']");
            GetDescription(arcbody, blog);
            return linkList;
        }

        private static void GetDescription(HtmlNode arcbody, Blog blog)
        {
            var destextList = new List<HtmlNode>();
            foreach (HtmlNode childDiv in arcbody.Elements("div"))
            {
                if (childDiv.GetAttributeValue("class", "") == "destext")
                {
                    destextList.Add(childDiv);
                }
            }
            var description = destextList.LastOrDefault();
            if (description == null)
                return;
            blog.HtmlContent += description.InnerHtml.Trim();
            blog.Content += description.InnerText.Replace("&#160;", "").Replace("&nbsp;", "").Trim();
            Trace("HtmlContent:" + blog.HtmlContent);
            Trace("Content:" + blog.Content);
            if (string.IsNullOrEmpty(blog.Content))
            {
                blog.HtmlContent = blog.Title;
                blog.Content = blog.Title;
            }
        }

        private static string GetGeneralContent(string strUrl)
        {
            _requestCount++;
            string referer = "http://www.gdajie.com/";
            var cookies = new List<Cookie>()
                {
                    new Cookie("AJSTAT_ok_pages", _requestCount.ToString(), "/", "www.ed2kers.com"),
                    new Cookie("AJSTAT_ok_times", "1", "/", "www.ed2kers.com"),
                    new Cookie("Hm_lpvt_9398e7331484620894c57216bea9225e", "1468695450", "/", ".gdajie.com"),
                    new Cookie("PHPSESSID", "0hbum6a055q5sfg5oi03a7ug35", "/", "www.ed2kers.com"),
                    new Cookie("Hm_lvt_9ad7d25789ce8adb7b225bc58a1b8525", "1470154733", "/", "ed2kers.com"),
                    new Cookie("Hm_lpvt_9ad7d25789ce8adb7b225bc58a1b8525", "1470156513", "/", "ed2kers.com")
                };
            string html = CrawlerUtility.GetGeneralContent(strUrl, referer, cookies);
            if (string.IsNullOrEmpty(html))
            {
                Thread.Sleep(10 * 1000);
            }
            else
            {
                Thread.Sleep(2 * 1000);
            }
            return html;
        }

        private static void Trace(string message, params object[] args)
        {
            Logger.Trace("Ed2Kers", message, args);
        }
        private static void Info(string message, params object[] args)
        {
            Logger.Info("Ed2Kers", message, args);
        }
    }
}
