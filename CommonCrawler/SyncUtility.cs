using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using NLogUtility;
using RestSharp;

namespace CommonCrawler
{
    public class SyncUtility
    {
        const string BaseUrl = "http://www.withyun.com/api/";
        const string Blog = "Blog";
        const string ImageUrl = "ImageUrl";
        const string Link = "Link";
        const string Recomment = "Recomment";
        public static int SyncBlog(Blog blog)
        {
            int blogId = 0;
            try
            {
                var client = new RestClient(BaseUrl);
                var requst = new RestRequest(Blog, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                requst.AddBody(blog);
                var response = client.Execute<int>(requst);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    blogId = response.Data;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return blogId;
        }

        public static bool SyncLinks(int blogId, List<Link> linkList)
        {
            try
            {
                var client = new RestClient(BaseUrl);
                var requst = new RestRequest(Link, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                requst.AddBody(linkList);
                requst.AddQueryParameter("blogId", blogId.ToString());
                var response = client.Execute(requst);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static bool SyncImageUrl(ImageUrl image)
        {
            if (string.IsNullOrEmpty(image.YunUrl))
            {
                return ImageUrlDao.ExistLocalImage(image) || ImageUrlDao.Add(image);
            }
            try
            {
                var client = new RestClient(BaseUrl);
                var requst = new RestRequest(ImageUrl, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                requst.AddBody(image);
                var response = client.Execute(requst);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }


        public static bool SyncRecomment(Recomment recomment)
        {
            if (string.IsNullOrEmpty(recomment.YunUrl))
            {
                return RecommentDao.ExistLocalImage(recomment) || RecommentDao.Add(recomment);
            }
            try
            {
                var client = new RestClient(BaseUrl);
                var requst = new RestRequest(Recomment, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                requst.AddBody(recomment);
                var response = client.Execute(requst);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }
    }
}
