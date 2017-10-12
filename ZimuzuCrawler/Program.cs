using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using CommonCrawler;
using Domain.Models;
using HtmlAgilityPack;
using NLogUtility;
using UploadImage;

namespace ZimuzuCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://www.zimuzu.tv/eresourcelist?channel=&area=&category=&format=&year=&sort=update&page=";
            for (int i = 556; i >=1; i--)
            {
                try
                {
                    Logger.Info("Zimuzu", "Start Handle:" + url + i);
                    GetListPage(url + i);
                    Logger.Info("Zimuzu", "End Handle:" + url + i);
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
            if (html == "")
                return;
            HtmlDocument document=new HtmlDocument();
            document.LoadHtml(html);
            HtmlNode htmlNode = document.DocumentNode.SelectSingleNode("//div[@class='resource-showlist']//ul");
            if(htmlNode==null)
                return;
            foreach (HtmlNode node in htmlNode.Elements("li"))
            {
                try
                {
                    bool continuted = false;
                    string continuteType = "";
                    var nodeArray = node.Elements("div").ToArray();
                    var infoNode = nodeArray[1];
                    if(infoNode==null)
                        continue;
                    var detailNode = infoNode.Element("dl").Element("dt").Element("strong").Element("a");
                    var detailContinuedNode = infoNode.Element("dl").Element("dt").Element("font");
                    if(detailNode==null)
                        continue;
                    string title = detailNode.InnerText;
                    string detailUrl = detailNode.GetAttributeValue("href", "");
                    if(detailUrl=="")
                        continue;
                    Console.WriteLine(title);
                    if (detailContinuedNode != null)
                    {
                        continuteType = detailContinuedNode.InnerText;
                        Console.WriteLine(continuteType);
                    }
                    if (continuteType.Contains("[尚未开播]"))
                    {
                        continue;
                    }
                    if (continuteType.Contains("连载中]") || continuteType.Contains("季完结]"))
                    {
                        continuted = true;
                    }
                    
                    Console.WriteLine(detailUrl);
                    string resourceId = GetResourceId(detailUrl);
                    if (CrawlerUtility.ExistRecord(resourceId, ResourceType.Zimuzu))
                    {
                        int blogId = CrawlerUtility.ExistContinutedRecord(resourceId, ResourceType.Zimuzu);
                        if (blogId>0)
                        {
                            Console.WriteLine("resourceId:{0} exist,to be updated blogId:{1}", resourceId, blogId);
                            if (AppendBlogLinks(resourceId, blogId))
                            {
                                Console.WriteLine("Blog Updated:" + blogId);
                            }
                            else
                            {
                                Console.WriteLine("Blog Sync Fail,resourceId:{0},blogId:{1}", resourceId, blogId);
                            }
                            if (!continuted)
                            {
                                CrawlerUtility.UpdateRecordOver(blogId);
                            }
                        }
                        else
                        {
                            Console.WriteLine("resourceId:{0} exist", resourceId);
                        }
                        continue;
                    }
                        
                    var blog = new Blog {Title = title.Length > 200 ? title.Substring(0, 200) : title};
                    string blogImgUrl = "";
                    string coverImgUrl = "";
                    GetBlogContent(detailUrl, blog, out blogImgUrl,out coverImgUrl);
                    string linkUrl = detailUrl.Replace("resource", "resource/list");
                    List<Link> linkList = GetBlogLink(linkUrl);
                    if (string.IsNullOrEmpty(blog.Content) || linkList.Count == 0)
                        continue;
                    ImageUrl imageUrl = DownloadBlogImgToLocal(blogImgUrl);
                    bool syncFlag = false;
                    syncFlag=SaveBlog(blog, imageUrl, linkList);
                    if (!syncFlag)
                    {
                        Console.WriteLine("Blog Sync Fail,detailUrl:{0},blogId:{1}", detailUrl, blog.Id);
                        continue;
                    }
                    syncFlag=SaveRecomment(blog, coverImgUrl);
                    if (!syncFlag)
                    {
                        Console.WriteLine("Recomment Sync Fail,blogId:{0},imageUrl:{1}", blog.Id, imageUrl);
                    }
                    CrawlerUtility.AddResourceRecord(blog.Id, resourceId, ResourceType.Zimuzu, continuted);
                    Console.WriteLine("Blog Added:" + blog.Id);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                } 
            }
        }

        private static bool AppendBlogLinks(string resourceId,int blogId)
        {
            string linkUrl = "http://www.zimuzu.tv/resource/list/" + resourceId;
            List<Link> linkList = GetBlogLink(linkUrl);
            if (linkList.Count == 0)
                return true;
            return SyncUtility.SyncLinks(blogId, linkList);
        }

        private static string GetResourceId(string detailUrl)
        {
            string id = Regex.Replace(detailUrl, @"[^\d]", "");
            return id.Trim();
        }

        private static bool SaveRecomment(Blog blog, string coverImgUrl)
        {
            if (coverImgUrl == "")
                return true;
            string fileName = CrawlerUtility.GetFileContent(coverImgUrl, Const.CoverFileDirectory, "http://www.zimuzu.tv/");
            if(string.IsNullOrEmpty(fileName))
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
                Category = SetCategoryByTitle(blog.Title)
            };
            if (!string.IsNullOrEmpty(yunUrl))
            {
                recomment.YunUrl = yunUrl;
                recomment.ImageStatus = ImageStatus.Yun;
            }
            return SyncUtility.SyncRecomment(recomment);
        }

        private static RecommentCategory SetCategoryByTitle(string title)
        {
            if (title.Contains("【电影】"))
                return RecommentCategory.电影;
            if (title.Contains("【美剧】"))
                return RecommentCategory.美剧;
            if (title.Contains("【日剧】"))
                return RecommentCategory.日剧;
            if (title.Contains("【韩剧】"))
                return RecommentCategory.韩剧;          
            if (title.Contains("【动画】"))
                return RecommentCategory.动漫;
            if (title.Contains("【真人秀】") || title.Contains("【脱口秀】"))
                return RecommentCategory.综艺;
            if (title.Contains("【纪录片】"))
                return RecommentCategory.资料;
            return RecommentCategory.剧集;
        }

        private static bool SaveBlog(Blog blog, ImageUrl imageUrl, List<Link> linkList)
        {
            blog.HtmlContent = "<html><body><p>" + blog.Content + "</p></body></html>";
            blog.Id = SyncUtility.SyncBlog(blog);
            if (blog.Id == 0)
                return false;
            SyncUtility.SyncLinks(blog.Id, linkList);
            if (imageUrl != null)
            {
                imageUrl.BlogId = blog.Id;
                SyncUtility.SyncImageUrl(imageUrl);
            }
            return true;
        }

        private static void GetBlogContent(string url,Blog blog,out string blogImgUrl,out string coverImgUrl)
        {
            blogImgUrl = "";
            coverImgUrl = "";
            try
            {
                string html = GetGeneralContent(url);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                HtmlNode resourceNode = document.DocumentNode.SelectSingleNode("//div[@class='box resource-con']");
                if (resourceNode == null)
                    return;
                var nodeArray = resourceNode.Elements("div").ToArray();
                HtmlNode imgNode = nodeArray[1];
                string orignalImgUrl = imgNode.Element("p").Element("a").GetAttributeValue("href", "");
                Console.WriteLine(orignalImgUrl);
                string bigImgUrl = imgNode.Element("p").Element("a").Element("img").GetAttributeValue("src", "");
                Console.WriteLine(bigImgUrl);
                HtmlNode infoNode = nodeArray[2];
                HtmlNode descriptionNodel = infoNode.Element("ul").Elements("li").ToList().Last();
                string description = descriptionNodel.Element("div").InnerText;
                Console.WriteLine(description);
                blog.Content = description;
                blogImgUrl = orignalImgUrl;
                coverImgUrl = bigImgUrl;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetBlogContent error,url:{0}", url);
            }
        }

        private static List<Link> GetBlogLink(string url)
        {
            List<Link> linkList=new List<Link>();
            try
            {
                string html = GetGeneralContent(url);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(html);
                HtmlNode mediaBox = document.DocumentNode.SelectSingleNode("//div[@class='media-box']");
                if (mediaBox == null)
                    return linkList;
                var nodeArray = mediaBox.Elements("div").ToArray();
                if (nodeArray.Length < 2)
                    return linkList;
                for (int index = 1; index < nodeArray.Length - 1; index++)
                {
                    try
                    {
                        HtmlNode listNode = nodeArray[index];
                        if (listNode == null)
                            continue;
                        foreach (HtmlNode linkNode in listNode.Element("ul").Elements("li"))
                        {
                            try
                            {
                                var linkNodeArray = linkNode.Elements("div").ToArray();
                                string linkTitle = linkNodeArray[0].Element("a").InnerText;
                                string linkUrl = linkNodeArray[1].Element("a").GetAttributeValue("href", "");
                                if (linkUrl.Contains("baidupcs.com"))
                                {
                                    linkUrl = linkNodeArray[2].Element("a").GetAttributeValue("href", "");
                                }
                                Console.WriteLine("linkTitle:" + linkTitle);
                                Console.WriteLine("linkUrl:" + linkUrl);
                                if (linkUrl.Contains("baidupcs.com"))
                                {
                                    continue;
                                }
                                if (linkUrl.Length > 500)
                                {
                                    linkUrl = linkUrl.Substring(0, 500);
                                    Logger.Info("Zimuzu","linkUrl too long,linkUrl:{0}", linkUrl);
                                }
                                Link link = new Link {Description = linkTitle, Url = linkUrl};
                                linkList.Add(link);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "GetBlogLink foreach loop,url:{0}", url);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "GetBlogLink for loop,url:{0}", url);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetBlogLink,url:{0}", url);
            }
            return linkList;
        }

        private static string GetGeneralContent(string strUrl)
        {
            string referer = "http://www.zimuzu.tv/";
            var cookies = new List<Cookie>
            {
                new Cookie("CNZZDATA1254180690", "176448891-1468518959-%7C1468679132", "/", "www.zimuzu.tv"),
                new Cookie("PHPSESSID", "ht8gtggpqjlth57ef41b7g03k0", "/", "www.zimuzu.tv"),
                new Cookie("GINFO",
                    "uid%3D3667676%26nickname%3Dkinshine%26group_id%3D1%26avatar_t%3D%26main_group_id%3D0%26common_group_id%3D54",
                    "/", ".zimuzu.tv"),
                new Cookie("GKEY", "e4b20dc7275fac6a7f984afcb5938ff3", "/", ".zimuzu.tv"),
                new Cookie("mykeywords",
                    "%3A1%3A%7Bi%3A0%3Bs%3A21%3A%22%E8%82%A5%E7%91%9E%E7%9A%84%E7%96%AF%E7%8B%82%E6%97%A5%E8%AE%B0%22%3B%7D",
                    "/", ".zimuzu.tv")
            };
            string html = CrawlerUtility.GetGeneralContent(strUrl, referer, cookies);
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

        private static ImageUrl DownloadBlogImgToLocal(string blogImgUrl)
        {
            ImageUrl imageUrl=new ImageUrl();
            string fileName = CrawlerUtility.GetFileContent(blogImgUrl, Const.BlogFileDirectory, "http://www.zimuzu.tv/");
            if (fileName == "")
                return null;
            string localFilePath = Const.BlogFileDirectory + fileName;
            using (Image downloadImage = Image.FromFile(localFilePath))
            {
                if (downloadImage.Size.Width > 500)
                {
                    var image = Domain.Helper.ImgHandler.ZoomPictureProportionately(downloadImage, 500, 500 * downloadImage.Size.Height / downloadImage.Size.Width);
                    image.Save(localFilePath);
                }
            }
            
            string yunUrl = UploadUtility.UploadLocalFile(localFilePath);

            Console.WriteLine("localFile:{0} upload to yunUrl:{1}", localFilePath, yunUrl);
            imageUrl.Url = fileName;
            imageUrl.ImageStatus=ImageStatus.Local;
            imageUrl.TimeStamp=DateTime.Now;
            if (!string.IsNullOrEmpty(yunUrl))
            {
                imageUrl.YunUrl = yunUrl;
                imageUrl.ImageStatus=ImageStatus.Yun;
            }
            return imageUrl;
        }

    }
}
