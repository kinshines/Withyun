using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Domain.Helper;
using Domain.Models;
using Microsoft.Practices.ServiceLocation;
using NLogUtility;
using SolrNet;
using SolrNet.Commands.Parameters;

namespace Domain.Services
{
    public class SearchService
    {
        public static void AddBlog(Blog blog)
        {
            try
            {
                if(blog.Status!=BlogStatus.Publish)
                    return;
                var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Blog>>();
                solr.Add(blog);
                solr.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AddBlog,blogId:{0}", blog.Id);
            }
        }
        public static void DeleteBlog(Blog blog)
        {
            try
            {
                var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Blog>>();
                solr.Delete(blog);
                solr.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DeleteBlog,blogId:{0}", blog.Id);
            }
        }

        public static void DeleteById(int blogId)
        {
            try
            {
                var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Blog>>();
                solr.Delete(new string[] { blogId.ToString() });
                solr.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DeleteById,blogId:{0}", blogId);
            }
        }

        public static SolrQueryResults<Blog> Query(string keyword, int? page,out int pageCount)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 20;
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Blog>>();
            AbstractSolrQuery query = new SolrQueryByField("titlecontent", keyword);
            QueryOptions options = new QueryOptions
            {
                Start = (pageNumber - 1)*pageSize,
                Rows = pageSize,
                Highlight = new HighlightingParameters
                {
                    Fields = new[] {"title", "content"},
                    BeforeTerm = "<em>",
                    AfterTerm = "</em>"
                },
                OrderBy = new []{new SortOrder("score",Order.DESC) }
            };
            var result = solr.Query(query, options);
            foreach (var b in result)
            {
                foreach (var h in result.Highlights[b.Id.ToString()])
                {
                    if (h.Key == "title")
                    {
                        b.Title = string.Join(",", h.Value.ToArray());
                    }
                    if (h.Key == "content")
                    {
                        b.Content = string.Join(", ", h.Value.ToArray());
                    }
                }
            }
            pageCount = (int)Math.Ceiling((float)result.NumFound / pageSize);
            return result;
        }

        public static void InitContainer()
        {
            string solrUrl = ConfigurationManager.AppSettings["SolrUrl"];
            SolrNet.Startup.Init<Blog>(solrUrl);
        }
    }
}