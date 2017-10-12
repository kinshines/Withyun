using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CommonCrawler;
using Domain.Models;
using HtmlAgilityPack;
using NLogUtility;
using UploadImage;

namespace VerycdCrawler
{
    class Program
    {
        private static readonly Dictionary<string,string> UrlDictionary=new Dictionary<string, string>();
        //可根据实际保存为具体文件
        private static int _requestCount = 0;
        static void Main(string[] args)
        {
            //SearchService.InitContainer();
            InitialDictionary();
            Console.WriteLine("input the start page:");
            int page = Convert.ToInt32(Console.ReadLine());
            foreach (KeyValuePair<string,string> pair in UrlDictionary)
            {
                string category = pair.Key;
                string url = pair.Value;
                for (int i = page; i >= 1; i--)
                {
                    try
                    {
                        Logger.Info("Verycd", "Start Handle:" + url + i);
                        GetListPage(url + i,category);
                        Logger.Info("Verycd", "End Handle:" + url + i);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }            
        }

        private static void InitialDictionary()
        {
            UrlDictionary.Add("tv", "http://verycd.gdajie.com/tv/page");//30
            UrlDictionary.Add("game", "http://verycd.gdajie.com/game/page");//100
            UrlDictionary.Add("cartoon", "http://verycd.gdajie.com/cartoon/page");//100
            UrlDictionary.Add("software", "http://verycd.gdajie.com/software/page");//50
            UrlDictionary.Add("datum", "http://verycd.gdajie.com/datum/page");//50
            UrlDictionary.Add("edu", "http://verycd.gdajie.com/edu/page");//50
            UrlDictionary.Add("book", "http://verycd.gdajie.com/book/page");//50
            UrlDictionary.Add("music", "http://verycd.gdajie.com/music/page");//100
        }

        private static void GetListPage(string url,string category)
        {
            string html = GetGeneralContent(url);
            if(html=="")
                return;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//div[@id='main']//ul");
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
                    Console.WriteLine(detailUrl);
                    string resourceId = GetResourceId(detailUrl);
                    if (CrawlerUtility.ExistRecord(resourceId, ResourceType.Verycd))
                    {
                        Console.WriteLine("resourceId:{0} exist", resourceId);
                        continue;
                    }
                    var title = link.GetAttributeValue("title", "");
                    if (title == "")
                        continue;
                    blog.Title = title.Length > 200 ? title.Substring(0, 200) : title;
                    Console.WriteLine(title);
                    var style = link.GetAttributeValue("style", "");
                    var imageUrl = style.Replace("background-image:", "").Replace("url(", "").Replace(")", "").Trim();
                    Console.WriteLine("imageUrl:" + imageUrl);
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
                        Console.WriteLine("Blog Sync Fail,url:{0},blogId:{1}", url, blog.Id);
                        continue;
                    }
                    syncFlag=SaveRecomment(blog, imageUrl, category);
                    if (!syncFlag)
                    {
                        Console.WriteLine("Recomment Sync Fail,blogId:{0},imageUrl:{1}", blog.Id, imageUrl);
                    }
                    CrawlerUtility.AddResourceRecord(blog.Id, resourceId, ResourceType.Verycd);
                    Console.WriteLine("Add success,BlogId:" + blog.Id);
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
            //blog.UserId = 1;
            //blog.UserName = "Kinshine";
            //blog.TimeStamp=DateTime.Now;
            //blog.Status=BlogStatus.Publish;
            blog.Id = SyncUtility.SyncBlog(blog);
            if(blog.Id==0)
                return false;
            return SyncUtility.SyncLinks(blog.Id, linkList);
            //Context.Set<Blog>().Add(blog);
            //Context.SaveChanges();
            //SearchService.AddBlog(blog);
            //foreach (var link in linkList)
            //{
            //    link.BlogId = blog.Id;
            //    link.TimeStamp = DateTime.Now;
            //    Context.Set<Link>().Add(link);
            //}
            //Context.SaveChanges();
        }

        private static bool SaveRecomment(Blog blog, string imageUrl,string category)
        {
            if (imageUrl == "http://verycd.gdajie.com/img/default_cover.jpg" || imageUrl == "")
                return true;
            string fileName = CrawlerUtility.GetFileContent(imageUrl, Const.CoverFileDirectory, "http://www.gdajie.com/");
            if (string.IsNullOrEmpty(fileName))
                return true;
            string localFilePath = Const.CoverFileDirectory + fileName;
            var image = Domain.Helper.ImgHandler.ZoomPicture(Image.FromFile(localFilePath), 200, 110);
            image.Save(localFilePath);
            string yunUrl = UploadUtility.UploadLocalFile(localFilePath);
            Console.WriteLine("localFile:{0} upload to yunUrl:{1}", localFilePath, yunUrl);
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
                recomment.ImageStatus=ImageStatus.Yun;
            }
            return SyncUtility.SyncRecomment(recomment);
            //Context.Set<Recomment>().Add(recomment);
            //Context.SaveChanges();
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
                    Console.WriteLine("detailLink:" + detailLink);
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
            Console.WriteLine(link.Description + link.Url);
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
                Logger.Info("Verycd", "url:" + strUrl + ",ip被封,_requestCount:" + _requestCount);
                html= "";
            }
            if (string.IsNullOrEmpty(html))
            {
                Thread.Sleep(10*1000);
            }
            else
            {
                Thread.Sleep(5*1000);
            }
            return html;
        }

    }
}
