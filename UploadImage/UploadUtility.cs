using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using NLogUtility;

namespace UploadImage
{
    public class UploadUtility
    {
        private static readonly string AccessKey = ConfigurationManager.AppSettings["AccessKey"];
        private static readonly string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
        private static readonly string OpenKey = ConfigurationManager.AppSettings["OpenKey"];
        private static readonly string AlbumId = ConfigurationManager.AppSettings["AlbumId"];

        public static string UploadLocalFile(string filepath)
        {
            string response = "";
            try
            {
                var wrapper = new TietukuWrapper(AccessKey, SecretKey, OpenKey);
                response = wrapper.UpFile(AlbumId, filepath);
                
                var result = JsonConvert.DeserializeObject<UploadResult>(response);
                if (string.IsNullOrEmpty(result.LinkUrl))
                {
                    var error = JsonConvert.DeserializeObject<ErrorResult>(response);
                    if (error.Code > 0)
                    {
                        Info("upload null,filepath:{0},error code:{1},error info:{2}", filepath, error.Code, error.Info);
                    }
                    else
                    {
                        Info("upload null,filepath:{0},response:{1}", filepath, response);
                    }
                }
                return result.LinkUrl;
            }
            catch (Exception ex)
            {
                Info("upload error,filepath:{0},response:{1},message:{2}", filepath, response, ex.Message);
                return "";
            }
        }

        public static string UploadUrl(string url)
        {
            string response = "";
            try
            {
                var wrapper = new TietukuWrapper(AccessKey, SecretKey, OpenKey);
                response = wrapper.UpUrl(AlbumId, url);
                var result = JsonConvert.DeserializeObject<UploadResult>(response);
                if (string.IsNullOrEmpty(result.LinkUrl))
                {
                    var error = JsonConvert.DeserializeObject<ErrorResult>(response);
                    if (error.Code > 0)
                    {
                        Info("upload null,url:{0},error code:{1},error info:{2}", url, error.Code, error.Info);
                    }
                    else
                    {
                        Info("upload null,url:{0},response:{1}", url, response);
                    }
                }
                return result.LinkUrl;
            }
            catch (Exception ex)
            {
                Info("upload error,url:{0},response:{1},message:{2}", url, response, ex.Message);
                return "";
            }
        }

        private static void Info(string message, params object[] args)
        {
            Logger.Info("UploadUtility", message, args);
        }
    }
}
