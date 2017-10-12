using System;
using System.Collections.Generic;
using System.Drawing;
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
using UploadImage;

namespace Ed2000Crawler
{
    class Program
    {
        private static readonly Dictionary<string, string> UrlDictionary = new Dictionary<string, string>();
        //可根据实际保存为具体文件
        private static int _requestCount = 0;
        static void Main(string[] args)
        {
            InitialDictionary();
            Console.WriteLine("input the start page:");
            int page = Convert.ToInt32(Console.ReadLine());
            foreach (KeyValuePair<string, string> pair in UrlDictionary)
            {
                string category = pair.Key;
                string url = pair.Value;
                for (int i = page; i >= 1; i--)
                {
                    try
                    {
                        Logger.Info("Ed2000", "Start Handle:" + url + i);
                        GetListPage(url + i, category);
                        Logger.Info("Ed2000", "End Handle:" + url + i);
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
            UrlDictionary.Add("电影", "http://www.ed2000.com/FileList.asp?FileCategory=%E7%94%B5%E5%BD%B1&PageIndex=");
            UrlDictionary.Add("剧集", "http://www.ed2000.com/FileList.asp?FileCategory=%E5%89%A7%E9%9B%86&PageIndex=");
            UrlDictionary.Add("音乐", "http://www.ed2000.com/FileList.asp?FileCategory=%E9%9F%B3%E4%B9%90&PageIndex=");
            UrlDictionary.Add("游戏", "http://www.ed2000.com/FileList.asp?FileCategory=%E6%B8%B8%E6%88%8F&PageIndex=");
            UrlDictionary.Add("动漫", "http://www.ed2000.com/FileList.asp?FileCategory=%E5%8A%A8%E6%BC%AB&PageIndex=");
            UrlDictionary.Add("图书", "http://www.ed2000.com/FileList.asp?FileCategory=%E5%9B%BE%E4%B9%A6&PageIndex=");
            UrlDictionary.Add("综艺", "http://www.ed2000.com/FileList.asp?FileCategory=%E7%BB%BC%E8%89%BA&PageIndex=");
            UrlDictionary.Add("软件", "http://www.ed2000.com/FileList.asp?FileCategory=%E8%BD%AF%E4%BB%B6&PageIndex=");
            UrlDictionary.Add("资料", "http://www.ed2000.com/FileList.asp?FileCategory=%E8%B5%84%E6%96%99&PageIndex=");
            UrlDictionary.Add("教育", "http://www.ed2000.com/FileList.asp?FileCategory=%E6%95%99%E8%82%B2&PageIndex=");
        }

        private static void GetListPage(string url, string category)
        {
            string html = GetGeneralContent(url);
            if (html == "")
                return;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//table[@class='CommonListArea']");
            int tdIndex = 0;
            string detailUrl = "";
            foreach (HtmlNode node in htmlNode.Elements("tr"))
            {
                try
                {
                    if (++tdIndex <= 2)
                    {
                        continue;
                    }
                    var tdArray = node.Elements("td").ToArray();
                    if (tdArray.Length < 4)
                    {
                        continue;
                    }
                    var blog = new Blog();
                    var titleNode = tdArray[0];

                    var title = titleNode.InnerText;
                    if (title == "")
                        continue;
                    blog.Title = title.Length > 200 ? title.Substring(0, 200) : title;
                    Console.WriteLine(title);
                    var link = titleNode.Elements("a").Skip(1).FirstOrDefault();
                    if (link == null)
                        continue;
                    detailUrl = link.GetAttributeValue("href", "");
                    if (detailUrl == "")
                        continue;
                    Console.WriteLine(detailUrl);
                    string resourceId = GetResourceId(detailUrl);
                    if (CrawlerUtility.ExistRecord(resourceId, ResourceType.Ed2000))
                    {
                        var distributeDate = tdArray[1].InnerText;
                        var updateTime = tdArray[2].InnerText;
                        if (updateTime.Contains(distributeDate))
                        {
                            Console.WriteLine("resourceId:{0} exist", resourceId);
                        }
                        else
                        {
                            int blogId = CrawlerUtility.ExistContinutedRecord(resourceId, ResourceType.Ed2000);
                            Console.WriteLine("resourceId:{0} exist,to be updated blogId:{1}", resourceId, blogId);
                            if (AppendBlogLinks(resourceId, blogId))
                            {
                                Console.WriteLine("Blog Updated:" + blogId);
                            }
                            else
                            {
                                Console.WriteLine("Blog Sync Fail,resourceId:{0},blogId:{1}", resourceId, blog.Id);
                            }
                        }
                        continue;
                    }
                    string imageUrl = "";
                    var urlList = GetIntroDetail("http://www.ed2000.com" + detailUrl, blog, out imageUrl);
                    if (urlList.Count == 0)
                    {
                        continue;
                    }
                    bool syncFlag = false;
                    syncFlag = SaveBlog(blog, urlList);
                    if (!syncFlag)
                    {
                        Console.WriteLine("Blog Sync Fail,detailUrl:{0},blogId:{1}", detailUrl, blog.Id);
                        continue;
                    }
                    syncFlag = SaveRecomment(blog, imageUrl, category);
                    if (!syncFlag)
                    {
                        Console.WriteLine("Recomment Sync Fail,blogId:{0},imageUrl:{1}", blog.Id, imageUrl);
                    }
                    CrawlerUtility.AddResourceRecord(blog.Id, resourceId, ResourceType.Ed2000, true);
                    Console.WriteLine("Add success,BlogId:" + blog.Id);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, detailUrl);
                }
            }
        }

        private static bool SaveRecomment(Blog blog, string imageUrl, string category)
        {
            if (imageUrl == "")
                return true;
            string fileName = CrawlerUtility.GetFileContent(imageUrl, Const.CoverFileDirectory, GetImageReferer(imageUrl));
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
                recomment.ImageStatus = ImageStatus.Yun;
            }
            return SyncUtility.SyncRecomment(recomment);
        }

        private static string GetImageReferer(string imageUrl)
        {
            string referer = "http://www.ed2000.com/";
            if (imageUrl.Contains("http://www.btbbt.cc"))
            {
                referer = "http://www.btbbt.cc";
            }
            if (imageUrl.Contains("http://img.bttt8.com/"))
            {
                referer = "http://www.bttt8.com/";
            }
            if (imageUrl.Contains("http://bbs.btwuji.cc/"))
            {
                referer = "http://bbs.btwuji.cc/";
            }
            if (imageUrl.Contains("http://pic.yupoo.com/"))
            {
                referer = "http://www.yupoo.com/";
            }
            if (imageUrl.Contains(".doubanio.com/") || imageUrl.Contains(".douban.com/"))
            {
                referer = "http://www.douban.com/";
            }
            return referer;
        }

        private static RecommentCategory SetCategory(string category)
        {
            switch (category)
            {
                case "电影":
                    return RecommentCategory.电影;
                case "综艺":
                    return RecommentCategory.综艺;
                case "图书":
                    return RecommentCategory.图书;
                case "音乐":
                    return RecommentCategory.音乐;
                case "游戏":
                    return RecommentCategory.游戏;
                case "动漫":
                    return RecommentCategory.动漫;
                case "剧集":
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

        private static bool SaveBlog(Blog blog, List<Link> linkList)
        {
            blog.HtmlContent = "<html><body>" + blog.HtmlContent + "</body></html>";
            blog.Id = SyncUtility.SyncBlog(blog);
            if (blog.Id == 0)
                return false;
            return SyncUtility.SyncLinks(blog.Id, linkList);
        }

        private static bool AppendBlogLinks(string resourceId, int blogId)
        {
            string linkUrl = string.Format("http://www.ed2000.com/ShowFile-{0}.html", resourceId);
            string coverUrl = "";
            var linkList = GetIntroDetail(linkUrl, new Blog() {Id = blogId}, out coverUrl);
            if (linkList.Count == 0)
                return true;
            return SyncUtility.SyncLinks(blogId, linkList);
        }

        private static string GetResourceId(string detailUrl)
        {
            string id = Regex.Replace(detailUrl, @"[^\d]", "");
            return id.Trim();
        }

        private static List<Link> GetIntroDetail(string url, Blog blog,out string coverUrl)
        {
            coverUrl = "";
            List<Link> linkList = new List<Link>();
            string html = GetGeneralContent(url);
            if (html == "")
                return linkList;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            coverUrl = GetFirstImageUrl(document);
            HtmlNode contentBody;
            var sign = document.DocumentNode.SelectSingleNode("//div[@class='PannelBody']//div[@class='sign']");
            if (sign == null)
            {
                contentBody = document.DocumentNode.SelectSingleNode("//div[@class='PannelBody']");
            }
            else
            {
                contentBody = sign.ParentNode;
                contentBody.RemoveChild(sign);
            }
            if (contentBody == null)
                return linkList;

            blog.HtmlContent = contentBody.InnerHtml;
            blog.Content = contentBody.InnerText;
            var body = document.DocumentNode.SelectSingleNode("//body");
            var tableList = body.Elements("table").ToList();
            foreach (HtmlNode table in tableList)
            {
                if (table.GetAttributeValue("class", "") == "CommonListArea")
                {
                    linkList.AddRange(GetLinkList(table));
                }
            }
            return linkList;
        }

        private static List<Link> GetLinkList(HtmlNode tbody)
        {
            var list=new List<Link>();
            list.AddRange(GetEd2KLinks(tbody));
            list.AddRange(GetMagnetLinks(tbody));
            return list;
        }

        private static void FixLink(Link link)
        {
            link.Url = link.Url.Replace("(92np.com)", "");
            link.Url = link.Url.Replace("(ED2000.COM)", "");
            link.Url = link.Url.Replace("[92np.com]", "");
            link.Url = link.Url.Replace("[热门电影 www.cnhb.org]", "");
            link.Url = link.Url.Replace("[无极论坛bbs.btwuji.cc]", "");
            link.Url = link.Url.Replace("[迅雷仓www.xunleicang.cc]", "");
            link.Url = link.Url.Replace("www.mp4vod.com", "");
            link.Url = link.Url.Replace("[52来看网][www.52laikan.com]", "");
            link.Url = link.Url.Replace("www.mkvdy.com", "");
            link.Url = link.Url.Replace("[电影云filmyun.cn]", "");
            link.Url = link.Url.Replace("[电影云www.filmyun.cn]", "");
            link.Url = link.Url.Replace("【BT首发】【www.3tii.com】", "");
            link.Url = link.Url.Replace("飞鸟娱乐(www.3tii.com)", "");
            link.Url = link.Url.Replace("【且听风吟福利吧】【3tii.com】", "");
            link.Url = link.Url.Replace("[牛宝宝电影网niubaobao.cc]", "");
            link.Url = link.Url.Replace("【WWW.52CTF.COM】", "");
            link.Url = link.Url.Replace("[www.yaoku.cc]", "");
            link.Url = link.Url.Replace("[火锅电影网 www.52hgdy.cc]", "");
            link.Url = link.Url.Replace("★tv520.funbbs.me★", "");
            link.Url = link.Url.Replace("【佳片网www.jiapian.cc】", "");
            link.Url = link.Url.Replace("[迅雷下载www.poxiao001.com]", "");
            link.Url = link.Url.Replace("迅雷仓[www.xunleicang.cc]", "");
            link.Url = link.Url.Replace("[龙天论坛-www.lthack.com]", "");
            link.Url = link.Url.Replace("_龙天技术论坛培训_wWw.LtHack.Com", "");
            link.Url = link.Url.Replace("www.lthack.com", "");
            link.Url = link.Url.Replace("凤凰高清论坛[http://ptfeng.com]", "");
            link.Description = link.Description.Replace("凤凰高清论坛[http://ptfeng.com]", "");
            link.Description = link.Description.Replace("www.lthack.com", "");
            link.Description = link.Description.Replace("_龙天技术论坛培训_wWw.LtHack.Com", "");
            link.Description = link.Description.Replace("[龙天论坛-www.lthack.com]", "");
            link.Description = link.Description.Replace("迅雷仓[www.xunleicang.cc]", "");
            link.Description = link.Description.Replace("[迅雷下载www.poxiao001.com]", "");
            link.Description = link.Description.Replace("【佳片网www.jiapian.cc】", "");
            link.Description = link.Description.Replace("★tv520.funbbs.me★", "");
            link.Description = link.Description.Replace("[火锅电影网 www.52hgdy.cc]", "");
            link.Description = link.Description.Replace("[www.yaoku.cc]", "");
            link.Description = link.Description.Replace("【WWW.52CTF.COM】", "");
            link.Description = link.Description.Replace("[牛宝宝电影网niubaobao.cc]", "");
            link.Description = link.Description.Replace("【且听风吟福利吧】【3tii.com】", "");
            link.Description = link.Description.Replace("飞鸟娱乐(www.3tii.com)", "");
            link.Description = link.Description.Replace("【BT首发】【www.3tii.com】", "");
            link.Description = link.Description.Replace("[电影云www.filmyun.cn]", "");
            link.Description = link.Description.Replace("[电影云filmyun.cn]", "");
            link.Description = link.Description.Replace("www.mkvdy.com", "");
            link.Description = link.Description.Replace("[52来看网][www.52laikan.com]", "");
            link.Description = link.Description.Replace("www.mp4vod.com", "");
            link.Description = link.Description.Replace("[迅雷仓www.xunleicang.cc]", "");
            link.Description = link.Description.Replace("[无极论坛bbs.btwuji.cc]", "");
            link.Description = link.Description.Replace("[热门电影 www.cnhb.org]", "");
            link.Description = link.Description.Replace("[92np.com]", "");
            link.Description = link.Description.Replace("(92np.com)", "");
            link.Description = link.Description.Replace(".Mp4Ba", "");

            if (link.Description.Length > 500)
            {
                link.Description = link.Description.Substring(0, 500);
            }
            if (link.Url.Length > 500)
            {
                link.Url = link.Url.Substring(0, 500);
            }
            Console.WriteLine("Description:" + link.Description);
            Console.WriteLine("Url:" + link.Url);
        }

        private static List<Link> GetEd2KLinks(HtmlNode tbody)
        {
            List<Link> linkList = new List<Link>();
            foreach (HtmlNode tr in tbody.Elements("tr"))
            {
                try
                {
                    string trClass = tr.GetAttributeValue("class", "");
                    if (trClass != "CommonListCell")
                        continue;
                    var td = tr.Elements("td").ToList();
                    if (td.Count < 2)
                        continue;
                    var linkTd = td.Skip(1).FirstOrDefault();
                    if (linkTd == null)
                        continue;
                    var linkNode = linkTd.Element("a");
                    if (linkNode == null)
                        continue;
                    Link link = new Link()
                    {
                        Url = linkNode.GetAttributeValue("href", ""),
                        Description = linkNode.InnerText
                    };

                    FixLink(link);
                    if (link.Url == "")
                        continue;
                    linkList.Add(link);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return linkList;
        }
        private static List<Link> GetMagnetLinks(HtmlNode tbody)
        {
            List<Link> linkList = new List<Link>();
            var script = tbody.Element("script");
            if (script == null)
                return linkList;
            string magnetString = script.InnerText.Trim();
            string[] magnetArray = magnetString.Split(new string[] { "ShowMagnet" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string magnet in magnetArray)
            {
                try
                {
                    var link = new Link();
                    string url = magnet.Trim(';').Trim('(').Trim(')').Trim('"');
                    link.Url = url;
                    link.Description = MagnetRequest("dn", url);
                    FixLink(link);
                    if (link.Url == "")
                        continue;
                    linkList.Add(link);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            return linkList;
        }

        private static string MagnetRequest(string paras, string url)
        {
            var paraString = url.Substring(url.IndexOf("?") + 1)
                .Split("&".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var paraObj = new Dictionary<string, string>();
            for (int i = 0; i < paraString.Length; i++)
            {
                string j = paraString[i];
                if(j.IndexOf("=")<0)
                    continue;
                paraObj[j.Substring(0, j.IndexOf("=")).ToLower()] = j.Substring(j.IndexOf("=") + 1);
            }
            if (!paraObj.ContainsKey(paras.ToLower()))
                return url;
            return paraObj[paras.ToLower()];
        }

        private static string GetFirstImageUrl(HtmlDocument document)
        {
            var imgNode = document.DocumentNode.SelectSingleNode("//div[@class='PannelBody']//img");
            if (imgNode == null)
                return "";
            string imgSrc = imgNode.GetAttributeValue("src", "");
            Console.WriteLine("imgUrl:" + imgSrc);
            return imgSrc;
        }

        private static string GetGeneralContent(string strUrl)
        {
            _requestCount++;
            string referer = "http://www.ed2000.com/";
            var cookies = new List<Cookie>()
            {
                new Cookie("ASPSESSIONIDQCSQTDQD", "OPOIBEHBPMHCPNAMCBJHFPDD", "/", "www.ed2000.com"),
                new Cookie("UserID", "30602", "/", "www.ed2000.com"),
                new Cookie("UserPassword", "A9E485F632124349EDFCEC448E649702", "/", "www.ed2000.com"),
                new Cookie("CNZZDATA947842", "cnzz_eid%3D1459071056-1467470818-null%26ntime%3D1469246688", "/", "www.ed2000.com"),
                new Cookie("VisitsNumber", _requestCount.ToString(), "/", "www.ed2000.com")
            };
            string html = CrawlerUtility.GetGeneralContent(strUrl, referer, cookies);
            if (string.IsNullOrEmpty(html))
            {
                Thread.Sleep(5 * 1000);
            }
            else
            {
                Thread.Sleep(2 * 1000);
            }
            return html;
        }
    }
}
