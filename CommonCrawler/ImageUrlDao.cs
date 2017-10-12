using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Domain.Models;
using HaimaDAL;
using HaimaDAL.SQL;
using NLogUtility;

namespace CommonCrawler
{
    public class ImageUrlDao
    {
        public static bool Add(ImageUrl image)
        {
            try
            {
                IInsertDataSourceFace query = new InsertSQL(TableName.ImageUrl);
                query.DataBaseAlias = Const.LogConnection;
                query.AddFieldValue("BlogId", image.BlogId);
                query.AddFieldValue("Url", image.Url);
                query.AddFieldValue("TimeStamp", DateTime.Now);
                query.AddFieldValue("YunUrl", image.YunUrl);
                query.AddFieldValue("ImageStatus", image.ImageStatus);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static bool ExistLocalImage(ImageUrl image)
        {
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.ImageUrl);
                query.DataBaseAlias = Const.LogConnection; 
                query.AddWhere("Url", image.Url);
                return query.ExecuteCount() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static bool DeleteRecord(int id)
        {
            try
            {
                IDeleteDataSourceFace query = new DeleteSQL(TableName.ImageUrl);
                query.DataBaseAlias = Const.LogConnection; 
                query.AddWhere("Id", id);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static List<ImageUrl> GetToUploadList()
        {
            List<ImageUrl> list = new List<ImageUrl>();
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.ImageUrl);
                query.DataBaseAlias = Const.LogConnection; 
                query.AddWhere("ImageStatus", ImageStatus.Local);
                DataSet ds = query.ExecuteDataSet();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    list.AddRange(from DataRow dr in ds.Tables[0].Rows
                        select new ImageUrl()
                        {
                            BlogId = SafeValueHelper.ToInt32(dr["BlogId"]),
                            Url = SafeValueHelper.ToString(dr["Url"]),
                            Id = SafeValueHelper.ToInt32(dr["Id"])
                        });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return list;
        }

    }
}
