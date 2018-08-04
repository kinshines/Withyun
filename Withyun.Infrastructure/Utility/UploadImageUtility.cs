using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UploadImage;

namespace Withyun.Infrastructure.Utility
{
    public class UploadImageUtility
    {
        readonly UploadProfile profile;

        public UploadImageUtility(IOptions<UploadProfile> option)
        {
            profile = option.Value;
        }

        public string UploadLocalFile(string filepath)
        {
            string response = "";
            try
            {
                var wrapper = new TietukuWrapper(profile);
                response = wrapper.UpFile(profile.AlbumId, filepath);

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

        public string UploadUrl(string url)
        {
            string response = "";
            try
            {
                var wrapper = new TietukuWrapper(profile);
                response = wrapper.UpUrl(profile.AlbumId, url);
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

        private void Info(string message, params object[] args)
        {
            Logger.Info("UploadUtility", message, args);
        }
    }
}
