using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;
using HaimaDAL;
using HaimaDAL.SQL;
using NLogUtility;

namespace CommonCrawler
{
    public class RecommentDao
    {
        public static bool Add(Recomment recomment)
        {
            try
            {
                IInsertDataSourceFace query = new InsertSQL(TableName.Recomment);
                query.DataBaseAlias = Const.LogConnection;
                query.AddFieldValue("Title", recomment.Title);
                query.AddFieldValue("BlogId", recomment.BlogId);
                query.AddFieldValue("Category", recomment.Category);
                query.AddFieldValue("CoverName", recomment.CoverName);
                query.AddFieldValue("TimeStamp", DateTime.Now);
                query.AddFieldValue("YunUrl", recomment.YunUrl);
                query.AddFieldValue("ImageStatus", recomment.ImageStatus);
                return query.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public static bool ExistLocalImage(Recomment image)
        {
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.Recomment);
                query.DataBaseAlias = Const.LogConnection;
                query.AddWhere("CoverName", image.CoverName);
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
                IDeleteDataSourceFace query = new DeleteSQL(TableName.Recomment);
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

        public static List<Recomment> GetToUploadList()
        {
            List<Recomment> list=new List<Recomment>();
            try
            {
                ISelectDataSourceFace query = new SelectSQL(TableName.Recomment);
                query.DataBaseAlias = Const.LogConnection;
                query.AddWhere("ImageStatus", ImageStatus.Local);
                DataSet ds = query.ExecuteDataSet();
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    list.AddRange(from DataRow dr in ds.Tables[0].Rows
                                  select new Recomment()
                                  {
                                      BlogId = SafeValueHelper.ToInt32(dr["BlogId"]),
                                      Category = (RecommentCategory)SafeValueHelper.ToInt32(dr["Category"]),
                                      CoverName = SafeValueHelper.ToString(dr["CoverName"]),
                                      Id = SafeValueHelper.ToInt32(dr["Id"]),
                                      Title = SafeValueHelper.ToString(dr["Title"]),
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
