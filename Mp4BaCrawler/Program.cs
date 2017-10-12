using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading;
using CommonCrawler;
using Domain.Models;
using HtmlAgilityPack;
using NLogUtility;
using UploadImage;

namespace Mp4BaCrawler
{
    class Program
    {
        //可根据实际保存为具体文件
        static void Main(string[] args)
        {
            //SearchService.InitContainer();
            string url = "http://www.mp4ba.com/index.php?page=";
            for (int i = 116; i >= 1; i--)
            {
                try
                {
                    Logger.Info("Mp4Ba","Start Handle:" + url + i);
                    GetListPage(url + i);
                    Logger.Info("Mp4Ba","End Handle:" + url + i);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private static void GetListPage(string url)
        {

            string html = GetGeneralContent(url);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//tbody[@id='data_list']");
            foreach (HtmlNode node in htmlNode.Elements("tr"))
            {
                try
                {
                    var elements = node.Elements("td").ToArray();
                    if (elements.Length < 3)
                        continue;
                    var category = elements[1].InnerText.Trim();
                    Console.WriteLine(category);
                    var detailTd = elements[2];
                    var detailLink = detailTd.Element("a");
                    var detailUrl = detailLink.GetAttributeValue("href", "").Trim();
                    Console.WriteLine(detailUrl);
                    var title = detailLink.InnerText.Trim().Replace(".Mp4Ba", "");
                    Console.WriteLine(title);
                    string resourceId = GetResourceId(detailUrl);
                    if (CrawlerUtility.ExistRecord(resourceId,ResourceType.Mp4Ba))
                    {
                        continue;
                    }
                    var blog = new Blog { Title = title.Length > 200 ? title.Substring(0, 200) : title };
                    string coverUrl = "";
                    var link = GetIntroDetail("http://www.mp4ba.com/" + detailUrl, blog, out coverUrl);
                    bool syncFlag = false;
                    syncFlag=SaveBlog(blog, link);
                    if (!syncFlag)
                    {
                        Console.WriteLine("Blog Sync Fail,detailUrl:{0},blogId:{1}", detailUrl, blog.Id);
                        continue;
                    }
                    syncFlag = SaveRecomment(blog, coverUrl, category);
                    if (!syncFlag)
                    {
                        Console.WriteLine("Recomment Sync Fail,blogId:{0},imageUrl:{1}", blog.Id, coverUrl);
                    }
                    CrawlerUtility.AddResourceRecord(blog.Id, resourceId, ResourceType.Mp4Ba);
                    Console.WriteLine("Blog Added:" + blog.Id);
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
            blog.Id = SyncUtility.SyncBlog(blog);
            if (blog.Id == 0)
                return false;
            SyncUtility.SyncLinks(blog.Id, new List<Link> {link});
            return true;
        }

        private static bool SaveRecomment(Blog blog, string imageUrl,string category)
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
            if(category.Contains("电视剧"))
            {
                recomment.Category = RecommentCategory.剧集;
            }
            if (category == "欧美电视剧")
            {
                recomment.Category=RecommentCategory.美剧;
            }
            if (category == "日韩电视剧")
            {
                recomment.Category = RecommentCategory.韩剧;
            }
            if (category == "综艺娱乐")
            {
                recomment.Category=RecommentCategory.综艺;
            }
            string localFilePath = Const.CoverFileDirectory + fileName;
            var image = Domain.Helper.ImgHandler.ZoomPicture(Image.FromFile(localFilePath), 200, 110);
            image.Save(localFilePath);
            string yunUrl = UploadUtility.UploadLocalFile(localFilePath);
            Console.WriteLine("localFile:{0} upload to yunUrl:{1}", localFilePath, yunUrl);
            if (!string.IsNullOrEmpty(yunUrl))
            {
                recomment.YunUrl = yunUrl;
                recomment.ImageStatus = ImageStatus.Yun;
            }
            return SyncUtility.SyncRecomment(recomment);
        }

        public static Link GetIntroDetail(string url, Blog blog,out string coverUrl)
        {
            string html = GetGeneralContent(url);
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var introDiv =document.DocumentNode.SelectSingleNode("//div[@id='btm']//div[@class='intro']");
            coverUrl = GetFirstImageUrl(document);
            var strongFirst = introDiv.Element("strong");
            if (strongFirst != null)
            {
                var mp4BaLink = strongFirst.NextSibling;
                if (mp4BaLink != null)
                {
                    var strongLast = mp4BaLink.NextSibling;
                    if (strongLast != null)
                    {
                        introDiv.RemoveChild(strongLast);
                    }
                    introDiv.RemoveChild(mp4BaLink);
                }
                introDiv.RemoveChild(strongFirst);
                
            }
            
            string introTobeReplace1 =
                "<span style=\"color: rgb(255,0,0)\"><span style=\"background-color: rgb(0,0,255)\"><strong>主下载</strong></span></span>";
            string introTobeReplace3 =
                "主下载：BT种子下载在页面的左上角，种子请用IE浏览器点击或者另存为下载到本地，再用BT软件或者迅雷等工具加载。";
            string introTobeReplace4 = "请大家尽量用种子下载，如果喜欢在线观看的，可以使用种子或者磁力链接把资源离线到各种云盘或者使用迅雷影音等支持种子加载的播放器。";
            string introTobeReplace5 = "种子、磁力链下载在页面左上角";
            string introTobeReplace6 = "<span style=\"color: rgb(51, 153, 102);\"><strong>最近高清MP4吧，被很多网站山寨（以高清MP4吧或者电玩人等名义的站点，或者域名相似之类）<br>  我们始终没有也不会要求捐助、办理会员等等这些利益行为，也没有QQ群号、博客、微博等等，<br>  所以请喜欢MP4吧的朋友，认准本站唯一域名：</strong></span><a href=\"http://www.mp4ba.com\" target=\"_blank\"><span style=\"color: rgb(51, 153, 102);\"><strong>www.mp4ba.com</strong></span></a><span style=\"color: rgb(51, 153, 102);\"><strong>（其他域名一概与本站点无关，请勿被骗）</strong></span><br>";
            string introTobeReplace7 =
                "最近高清MP4吧，被很多网站山寨（以高清MP4吧或者电玩人等名义的站点，或者域名相似之类）我们始终没有也不会要求捐助、办理会员等等这些利益行为，也没有QQ群号、博客、微博等等，所以请喜欢MP4吧的朋友，认准本站唯一域名：www.mp4ba.com（其他域名一概与本站点无关，请勿被骗）";
            blog.HtmlContent =
                introDiv.InnerHtml.Replace(introTobeReplace1, "")
                    .Replace(introTobeReplace3, "")
                    .Replace(introTobeReplace4, "")
                    .Replace(introTobeReplace5, "")
                    .Replace(introTobeReplace6, "");
            blog.Content =
                introDiv.InnerText.Replace(introTobeReplace3, "")
                    .Replace(introTobeReplace4, "")
                    .Replace(introTobeReplace5, "")
                    .Replace(introTobeReplace7,"")
                    .Replace("\r\n", "").Trim();
            Console.WriteLine(blog.Content);
            var linkDiv = document.DocumentNode.SelectSingleNode("//div[@id='btm']//div[@class='slayout']//div[@class='box']//p[@class='original magnet']//a");
            var managet = linkDiv.GetAttributeValue("href", "");
            Console.WriteLine("managet:" + managet);
            if (managet.Length > 500)
            {
                Console.WriteLine("managet too long");
                managet = managet.Substring(0, 500);
            }
            var link = new Link()
            {
                Description = managet,
                Url = managet
            };
            return link;
        }

        private static string GetFirstImageUrl(HtmlDocument document)
        {
            var imgNode = document.DocumentNode.SelectSingleNode("//div[@id='btm']//div[@class='intro']//img");
            if (imgNode == null)
                return "";
            string imgSrc = imgNode.GetAttributeValue("src", "");
            Console.WriteLine("imgUrl:" + imgSrc);
            return imgSrc;
        }

        public static string GetResourceId(string detailUrl)
        {
            return detailUrl.Replace("show.php?hash=", "");
        }

        private static string GetGeneralContent(string strUrl)
        {
            string referer = "http://www.mp4ba.com/";
            var cookies = new List<Cookie>()
            {
                new Cookie("CNZZDATA5925857", "cnzz_eid%3D921376316-1462025230-%26ntime%3D1467044423", "/",
                    "www.mp4ba.com")
            };

            string html = CrawlerUtility.GetGeneralContent(strUrl, referer, cookies);
            if (html.Contains("服务器维护中，请稍后访问"))
            {
                Console.WriteLine("服务器维护中，请稍后访问,60秒后继续操作");
                html = "";
            }
            if (string.IsNullOrEmpty(html))
            {
                Thread.Sleep(10 * 1000);
            }
            else
            {
                Thread.Sleep(5*1000);
            }
            return html;

        }       

    }
}
