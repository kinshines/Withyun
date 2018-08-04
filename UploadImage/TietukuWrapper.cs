using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using RestSharp;

namespace UploadImage
{
    public class TietukuWrapper
    {
        const string ApiUrlUp = "http://up.imgapi.com/";
        const string ApiUrlGet = "http://api.imgapi.com/v2/api/";

        private string GetToken(Dictionary<string, object> param)
        {
            return TokenUtility.CreateToken(AccessKey, SecretKey, param);
        }
        private void SignParam(Dictionary<string, object> param)
        {
            var token = GetToken(param);
            param.Clear();
            param.Add("Token", token);
        }
        private Dictionary<string, object> GetParamDict(string action)
        {
            var dict = new Dictionary<string, object>();
            dict.Add("deadline", DateTime.Now.ToUnixTimestamp() + 60);
            if (!string.IsNullOrWhiteSpace(action))
                dict.Add("action", action);
            return dict;
        }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string OpenKey { get; set; }
        public TietukuWrapper(UploadProfile profile)
        {
            this.AccessKey = profile.AccessKey;
            this.SecretKey = profile.SecretKey;
            this.OpenKey = profile.OpenKey;
        }
        public string UpFile(string aid, string filepath)
        {
            var param = GetParamDict(null);
            param.Add("aid", aid);
            param.Add("from", "file");
            SignParam(param);
            param.Add("file", new FileInfo(filepath));
            return AjaxUtility.MultipartFormDataPost(ApiUrlUp, param);
        }
        public string UpUrl(string aid, string fileurl)
        {
            var param = GetParamDict(null);
            param.Add("aid", aid);
            param.Add("from", "web");
            SignParam(param);
            param.Add("fileurl", fileurl);
            return AjaxUtility.Post(ApiUrlUp, param);
        }

        /// <summary>
        /// 分页码。每页28条
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public AlbumList GetAlbums(int p = 1)
        {
            var param = new Dictionary<string, object> {{"p", p}};
            var response = RestGetWithOpenKey("getalbum", param);
            return JsonConvert.DeserializeObject<AlbumList>(response);
        }

        public List<Picture> GetRandomPictures()
        {
            var response = RestGetWithOpenKey("getrandrec");
            return JsonConvert.DeserializeObject<List<Picture>>(response);
        }
        /// <summary>
        /// 全部图片列表
        /// </summary>
        /// <param name="cid">图片类型ID</param>
        /// <param name="p">页数(每页固定30张图片)</param>
        /// <returns></returns>
        public AllPictureList GetAllPictures(int p = 1, int cid = 1)
        {
            var param = new Dictionary<string, object> {{"p", p}, {"cid", cid}};
            var response = RestGetWithOpenKey("getnewpic", param);
            return JsonConvert.DeserializeObject<AllPictureList>(response);
        }
        /// <summary>
        /// 获取相册内图片
        /// </summary>
        /// <param name="p">页数(每页固定30张图片)</param>
        /// <param name="aid">相册ID</param>
        /// <returns></returns>
        public AlbumPictureList GetPicturesByAlbum(int p = 1, int? aid = null)
        {
            var param = new Dictionary<string, object> {{"p", p}};
            if (aid != null)
            {
                param.Add("aid", aid);
            }
            var response = RestGetWithOpenKey("getpiclist", param);
            return JsonConvert.DeserializeObject<AlbumPictureList>(response);
        }

        public PictureDetail GetOnePicture(string id = "", string findurl = "")
        {
            var param = new Dictionary<string, object> ();
            if (!string.IsNullOrWhiteSpace(findurl))
            {
                param.Add("findurl", findurl);
            }
            if (!string.IsNullOrWhiteSpace(id))
            {
                param.Add("id", id);
            }
            var response = RestGetWithOpenKey("getonepic", param);
            return JsonConvert.DeserializeObject<PictureDetail>(response);
        }

        public LovePictureList GetLovePictures(int p=1)
        {
            var param = new Dictionary<string, object> { { "p", p } };
            var response= RestGetWithOpenKey("getlovepic", param);
            return JsonConvert.DeserializeObject<LovePictureList>(response);
        }

        public List<Catalog> GetCatalogs()
        {
            var response = RestGetWithOpenKey("getcatalog");
            return JsonConvert.DeserializeObject<List<Catalog>>(response);
        }

        private string RestGetWithOpenKey(string action, Dictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            if (string.IsNullOrEmpty(OpenKey))
                throw new NullReferenceException("OpenKey is required!");
            parameters.Add("key", OpenKey);
            return AjaxUtility.Get(ApiUrlGet, action, parameters);
        }
    }
    public static class Extensions
    {
        public static int ToUnixTimestamp(this DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }
    }
    public static class AjaxUtility
    {
        public static Encoding DefaultEncoding = Encoding.UTF8;
        public static string Post(string url, IEnumerable<KeyValuePair<string, object>> parameters = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            var ajaxEncoding = DefaultEncoding;
            HttpWebRequest request = null;
            //如果是发送HTTPS请求
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            if (parameters != null && parameters.Count() > 0)
            {
                string buffer = string.Join("&", parameters.Select(i => string.Format("{0}={1}", i.Key, i.Value)));
                byte[] data = ajaxEncoding.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, DefaultEncoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        public static string Get(string baseUrl, string resource, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var client = new RestClient(baseUrl);
            var requst = new RestRequest(resource, Method.GET);
            foreach (KeyValuePair<string, object> pair in parameters)
            {
                requst.AddParameter(pair.Key, pair.Value);
            }
            IRestResponse response=client.Execute(requst);
            return response.Content;
        }
        public static string MultipartFormDataPost(string postUrl, Dictionary<string, object> postParameters, string userAgent = null)
        {
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            string contentType = "multipart/form-data; boundary=" + formDataBoundary;
            byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);
            HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;
            if (request == null)
            {
                throw new NullReferenceException("request is not a http request");
            }

            // Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.UserAgent = userAgent;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;

            // You could add authentication here as well if needed:
            // request.PreAuthenticate = true;
            // request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            // request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes("username" + ":" + "password")));

            // Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, DefaultEncoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受
        }
        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;
            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(DefaultEncoding.GetBytes("\r\n"), 0, DefaultEncoding.GetByteCount("\r\n"));
                needsCLRF = true;
                if (param.Value is FileInfo)
                {
                    var fileinfo = (FileInfo)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                                                  boundary,
                                                  param.Key,
                                                  fileinfo.Name ?? param.Key,
                                                  "application/octet-stream");
                    formDataStream.Write(DefaultEncoding.GetBytes(header), 0, DefaultEncoding.GetByteCount(header));
                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    using (var fs = new FileStream(fileinfo.FullName, FileMode.Open))
                    {
                        var bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, bytes.Length);
                        formDataStream.Write(bytes, 0, bytes.Length);
                    }
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                                                    boundary,
                                                    param.Key,
                                                    param.Value);
                    formDataStream.Write(DefaultEncoding.GetBytes(postData), 0, DefaultEncoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(DefaultEncoding.GetBytes(footer), 0, DefaultEncoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }
    }
    public class TokenUtility
    {
        public static string CreateToken(string accesskey, string secretKey, Dictionary<string, object> param)
        {
            var json = string.Format("{{{0}}}", string.Join(",", param.Select(i => string.Format("\"{0}\":{1}", i.Key, i.Value is string ? string.Format("\"{0}\"", i.Value) : i.Value))));
            var base64param = Base64Util.Base64(json);
            var sign = HmacUtil.HmacSha1(base64param, secretKey);
            var token = accesskey + ":" + sign + ":" + base64param;
            return token;
        }
        public class HmacUtil
        {
            public static String HmacSha1(String value, String key)
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                var mac = new System.Security.Cryptography.HMACSHA1(keyBytes);
                byte[] rawHmac = mac.ComputeHash(Encoding.UTF8.GetBytes(value));
                return Base64Util.Base64(rawHmac);
            }
        }
        public class Base64Util
        {
            public static String Base64(string target)
            {
                var byteArray = Encoding.UTF8.GetBytes(target);
                return Base64(byteArray);
            }
            public static string Base64(byte[] target)
            {
                return Convert.ToBase64String(target).Replace('+', '-').Replace('/', '_');
            }
        }
    }
}
