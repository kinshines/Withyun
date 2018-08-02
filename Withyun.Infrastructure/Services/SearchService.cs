using System;
using System.Collections.Generic;
using System.Linq;
using SolrNet;
using SolrNet.Commands.Parameters;
using Withyun.Core.Entities;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Utility;

namespace Withyun.Infrastructure.Services
{
    public class SearchService
    {
        readonly ISolrOperations<Blog> solr;
        public SearchService(ISolrOperations<Blog> solrOperations)
        {
            solr = solrOperations;
        }
        public void AddBlog(Blog blog)
        {
            try
            {
                if(blog.Status!=BlogStatus.Publish)
                    return;
                solr.Add(blog);
                solr.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AddBlog,blogId:{0}", blog.Id);
            }
        }
        public void DeleteBlog(Blog blog)
        {
            try
            {
                solr.Delete(blog);
                solr.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DeleteBlog,blogId:{0}", blog.Id);
            }
        }

        public void DeleteById(int blogId)
        {
            try
            {
                solr.Delete(new string[] { blogId.ToString() });
                solr.Commit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "DeleteById,blogId:{0}", blogId);
            }
        }

        public SolrQueryResults<Blog> Query(string keyword, int? page,out int pageCount)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 20;
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
    }
}