using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using HaimaDAL;
using HaimaDAL.SQL;
using NLogUtility;

namespace CommonCrawler
{
    public class CrawlerUtility
    {
        public static bool ExistRecord(string resourceId, ResourceType type)
        {
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.ResourceRecord);
                query.DataBaseAlias = Const.LogConnection;
                query.AddWhere("ResourceId", resourceId.Trim());
                query.AddWhere("ResourceType", (int) type);
                return query.ExecuteCount() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return true;
            }
        }

        public static int ExistContinutedRecord(string resourceId, ResourceType type)
        {
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.ResourceRecord);
                query.DataBaseAlias = Const.LogConnection;
                query.SelectColumn("BlogId");
                query.AddWhere("ResourceId", resourceId.Trim());
                query.AddWhere("ResourceType", (int)type);
                query.AddWhere("Continued", true);
                return SafeValueHelper.ToInt32(query.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return 0;
            }
        }

        public static bool AddResourceRecord(int blogId, string resourceId, ResourceType type,bool continuted=false)
        {
            try
            {
                IInsertDataSourceFace query = new InsertSQL(TableName.ResourceRecord);
                query.DataBaseAlias = Const.LogConnection;
                query.AddFieldValue("BlogId", blogId);
                query.AddFieldValue("ResourceId", resourceId.Trim());
                query.AddFieldValue("ResourceType", (int) type);
                query.AddFieldValue("Continued", continuted);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }
        public static bool UpdateRecordOver(int blogId)
        {
            try
            {
                IUpdateDataSourceFace query = new UpdateSQL(TableName.ResourceRecord);
                query.DataBaseAlias = Const.LogConnection; 
                query.AddWhere("BlogId", blogId);
                query.AddFieldValue("Continued", false);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static bool InitialFetchRecord(ResourceType type)
        {
            try
            {
                IInsertDataSourceFace query=new InsertSQL(TableName.FetchRecord);
                query.DataBaseAlias = Const.LogConnection;
                query.AddFieldValue("ResourceType", (int) type);
                query.AddFieldValue("FetchCount",0);
                query.AddFieldValue("RequestCount", 0);
                query.AddFieldValue("TimeStamp", DateTime.Now);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static bool DailyFetched(ResourceType type)
        {
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.FetchRecord);
                query.DataBaseAlias = Const.LogConnection;
                query.AddWhere("ResourceType", (int)type);
                query.AddWhere("TimeStamp", Comparison.GreaterThan, DateTime.Now.Date);
                return query.ExecuteCount() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return true;
            }
        }

        public static int GetFetchId(ResourceType type)
        {
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.FetchRecord);
                query.DataBaseAlias = Const.LogConnection;
                query.SelectColumn("Id");
                query.AddWhere("ResourceType", (int)type);
                query.AddOrderBy("Id",Sort.Descending);
                query.Top = 1;
                return SafeValueHelper.ToInt32(query.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return 0;
            }
        }

        public static bool UpdateFetchCount(int fetchCount, int requestCount, int id)
        {
            try
            {
                IUpdateDataSourceFace query=new UpdateSQL(TableName.FetchRecord);
                query.DataBaseAlias = Const.LogConnection;
                query.AddWhere("Id", id);
                query.AddFieldValue("FetchCount", fetchCount);
                query.AddFieldValue("RequestCount", requestCount);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static string GetFileContent(string strUrl, string fileDirectory,string referer)
        {
            string date = DateTime.Now.ToString("yy/MM/dd/").Replace('/', '\\');
            string fileName = date + Guid.NewGuid().ToString("N") + Path.GetExtension(strUrl);
            try
            {
                var request = WebRequest.Create(strUrl) as HttpWebRequest;
                request.Referer = referer;
                //request.Timeout = 5000;
                //request.ReadWriteTimeout = 5000;
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.18 Safari/537.36";
                var response = request.GetResponse() as HttpWebResponse;
                Stream reader = response.GetResponseStream();
                if (!Directory.Exists(fileDirectory + date))
                {
                    Directory.CreateDirectory(fileDirectory + date);
                }
                FileStream writer = new FileStream(fileDirectory + fileName, FileMode.OpenOrCreate, FileAccess.Write);

                byte[] buff = new byte[512];

                int c = 0; //实际读取的字节数 

                while ((c = reader.Read(buff, 0, buff.Length)) > 0)
                {

                    writer.Write(buff, 0, c);

                }

                writer.Close();

                writer.Dispose();



                reader.Close();

                reader.Dispose();

                response.Close();

            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetFileContent,url:{0}", strUrl);
                return "";
            }

            return fileName;

        }

        public static string GetGeneralContent(string strUrl,string referer,List<Cookie> cookies)
        {
            string html = string.Empty;

            try
            {
                var request = WebRequest.Create(strUrl) as HttpWebRequest;
                request.Referer = referer;
                //request.Timeout = 5000;
                //request.ReadWriteTimeout = 5000;
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.18 Safari/537.36";
                request.CookieContainer = new CookieContainer();
                foreach (var cookie in cookies)
                {
                    request.CookieContainer.Add(cookie);
                }

                var response = request.GetResponse() as HttpWebResponse;
                var encode = string.Empty;
                if (response.CharacterSet == "ISO-8859-1")
                    encode = "gb2312";
                else
                    encode = response.CharacterSet;
                Stream stream;

                if (response.ContentEncoding.ToLower() == "gzip")
                {
                    stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                }
                else
                {
                    stream = response.GetResponseStream();
                }
                var sr = new StreamReader(stream, Encoding.GetEncoding(encode));
                html = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GetGeneralContent,url:{0}", strUrl);
            }

            return html;

        }
    }
}
