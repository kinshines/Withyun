using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using NLogUtility;

namespace Domain.Helper
{
    public class ImgHandler
    {
        /// <summary>
        /// 剪裁头像图片
        /// </summary>
        /// <param name="pointX">X坐标</param>
        /// <param name="pointY">Y坐标</param>
        /// <param name="imgPath">被截图图片路径</param>
        /// <param name="rlSize">截图矩形的大小</param>
        public static string CutAvatar(string imgPath, int pointX = 0, int pointY = 0, int width = 0, int height = 0)
        {
            System.Drawing.Bitmap bitmap = null;   //按截图区域生成Bitmap
            System.Drawing.Image thumbImg = null;  //被截图 
            System.Drawing.Graphics gps = null;    //存绘图对象   
            System.Drawing.Image finalImg = null;  //最终图片
            try
            {
                int finalWidth = 100;
                int finalHeight = 100;
                if (!string.IsNullOrEmpty(imgPath))
                {
                    bitmap = new System.Drawing.Bitmap(width, height);
                    //thumbImg = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath(imgUrl));
                    //thumbImg = System.Drawing.Image.FromFile(imgUrl.Trim());

                    thumbImg = StreamHelper.Url2Img(imgPath);

                    gps = System.Drawing.Graphics.FromImage(bitmap);      //读到绘图对象
                    gps.DrawImage(thumbImg, new Rectangle(0, 0, width, height), new Rectangle(pointX, pointY, width, height), GraphicsUnit.Pixel);

                    finalImg = GetThumbNailImage(bitmap, finalWidth, finalHeight);

                    //以下代码为保存图片时，设置压缩质量  
                    EncoderParameters ep = new EncoderParameters();
                    long[] qy = new long[1];
                    qy[0] = 80;//设置压缩的比例1-100  
                    EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                    ep.Param[0] = eParam;

                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICIinfo = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPEG"))
                        {
                            jpegICIinfo = arrayICI[x];
                            break;
                        }
                    }

                    string finalUrl = imgPath.Replace("original", "avatar");
                    string finalPath = finalUrl;
                    string finalPathDir = finalPath.Substring(0, finalPath.LastIndexOf("\\"));

                    if (!Directory.Exists(finalPathDir))
                    {
                        Directory.CreateDirectory(finalPathDir);
                    }

                    if (jpegICIinfo != null)
                    {
                        finalImg.Save(finalPath, jpegICIinfo, ep);
                    }
                    else
                    {
                        finalImg.Save(finalPath);
                    }

                    return finalUrl;

                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "剪裁头像图片 出现异常");
                return null;
            }
            finally
            {
                bitmap.Dispose();
                thumbImg.Dispose();
                gps.Dispose();
                finalImg.Dispose();
                GC.Collect();
            }
        }

        ///<summary>
        /// 对给定的一个图片（Image对象）生成一个指定大小的缩略图。
        ///</summary>
        ///<param name="originalImage">原始图片</param>
        ///<param name="thumMaxWidth">缩略图的宽度</param>
        ///<param name="thumMaxHeight">缩略图的高度</param>
        ///<returns>返回缩略图的Image对象</returns>
        public static Image GetThumbNailImage(Image originalImage, int thumMaxWidth, int thumMaxHeight)
        {
            Size thumRealSize = System.Drawing.Size.Empty;
            Image newImage = originalImage;
            Graphics graphics = null;
            try
            {
                thumRealSize = GetNewSize(thumMaxWidth, thumMaxHeight, originalImage.Width, originalImage.Height);
                newImage = new System.Drawing.Bitmap(thumRealSize.Width, thumRealSize.Height);
                graphics = Graphics.FromImage(newImage);
                graphics.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, thumRealSize.Width, thumRealSize.Height), new Rectangle(0, 0, originalImage.Width, originalImage.Height), GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ImgHandler.GetThumbNailImage 缩略图 出现异常");
            }
            finally
            {
                if (graphics != null)
                {
                    graphics.Dispose();
                    graphics = null;
                }
            }
            return newImage;
        }

        ///<summary>
        /// 获取一个图片按等比例缩小后的大小。
        ///</summary>
        ///<param name="maxWidth">需要缩小到的宽度</param>
        ///<param name="maxHeight">需要缩小到的高度</param>
        ///<param name="imageOriginalWidth">图片的原始宽度</param>
        ///<param name="imageOriginalHeight">图片的原始高度</param>
        ///<returns>返回图片按等比例缩小后的实际大小</returns>
        public static Size GetNewSize(int maxWidth, int maxHeight, int imageOriginalWidth, int imageOriginalHeight)
        {
            double w = 0.0;
            double h = 0.0;
            double sw = Convert.ToDouble(imageOriginalWidth);
            double sh = Convert.ToDouble(imageOriginalHeight);
            double mw = Convert.ToDouble(maxWidth);
            double mh = Convert.ToDouble(maxHeight);
            if (sw < mw && sh < mh)
            {
                w = sw;
                h = sh;
            }
            else if ((sw / sh) > (mw / mh))
            {
                w = maxWidth;
                h = (w * sh) / sw;
            }
            else
            {
                h = maxHeight;
                w = (h * sw) / sh;
            }
            return new Size(Convert.ToInt32(w), Convert.ToInt32(h));
        }


        /// <summary>
        /// 按固定大小缩放图片
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static Image ZoomPicture(Image sourceImage, int targetWidth, int targetHeight)
        {
            try
            {
                Bitmap saveImage = new Bitmap(targetWidth, targetHeight);
                Graphics g = Graphics.FromImage(saveImage);
                g.Clear(Color.White);
                g.DrawImage(sourceImage, 0, 0, targetWidth, targetHeight);
                sourceImage.Dispose();

                return saveImage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ImgHandler.ZoomPicture 出现异常");
                return null;
            }
        }

        /// <summary>
        /// 按比例缩放图片
        /// 参考自：http://www.cnblogs.com/roucheng/p/3509606.html
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="targetWidth"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static Image ZoomPictureProportionately(Image sourceImage, int targetWidth, int targetHeight)
        {
            try
            {
                int intWidth; //新的图片宽
                int intHeight; //新的图片高
                Bitmap saveImage = new Bitmap(targetWidth, targetHeight);
                Graphics g = Graphics.FromImage(saveImage);
                g.Clear(Color.White);

                if (sourceImage.Width > targetWidth && sourceImage.Height <= targetHeight)//宽度比目的图片宽度大，长度比目的图片长度小
                {
                    intWidth = targetWidth;
                    intHeight = (intWidth * sourceImage.Height) / sourceImage.Width;
                }
                else if (sourceImage.Width <= targetWidth && sourceImage.Height > targetHeight)//宽度比目的图片宽度小，长度比目的图片长度大
                {
                    intHeight = targetHeight;
                    intWidth = (intHeight * sourceImage.Width) / sourceImage.Height;
                }
                else if (sourceImage.Width <= targetWidth && sourceImage.Height <= targetHeight) //长宽比目的图片长宽都小
                {
                    intHeight = sourceImage.Width;
                    intWidth = sourceImage.Height;
                }
                else//长宽比目的图片的长宽都大
                {
                    intWidth = targetWidth;
                    intHeight = (intWidth * sourceImage.Height) / sourceImage.Width;
                    if (intHeight > targetHeight)//重新计算
                    {
                        intHeight = targetHeight;
                        intWidth = (intHeight * sourceImage.Width) / sourceImage.Height;
                    }
                }

                g.DrawImage(sourceImage, (targetWidth - intWidth) / 2, (targetHeight - intHeight) / 2, intWidth, intHeight);
                sourceImage.Dispose();

                return saveImage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "ImgHandler.ZoomPictureProportionately 出现异常");
            }

            return null;
        }


    }
}