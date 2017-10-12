using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HaimaDAL;
using HaimaDAL.SQL;
using NLogUtility;

namespace UploadImage
{
    public class UploadAccountQueue
    {
        private static volatile Queue<UploadAccount> _instance = null;
        private static readonly object LockHelper = new object();
        public static Queue<UploadAccount> GetQueue()
        {
            if (_instance == null)
            {
                lock (LockHelper)
                {
                    if (_instance == null)
                    {
                        _instance = new Queue<UploadAccount>();
                        var list = GetAccountList();
                        foreach (UploadAccount account in list)
                        {
                            _instance.Enqueue(account);
                        }
                    }
                }
            }
            return _instance;
        }

        private static List<UploadAccount> GetAccountList()
        {
            List<UploadAccount> list=new List<UploadAccount>();
            DataSet ds = GetAccountDataSet();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    UploadAccount model = new UploadAccount
                    {
                        AccessKey = dr["AccessKey"].ToString(),
                        AlbumId = dr["AlbumId"].ToString(),
                        SecretKey = dr["SecretKey"].ToString(),
                        Email = dr["Email"].ToString(),
                        Id = SafeValueHelper.ToInt32(dr["Id"]),
                        Mobile = dr["Mobile"].ToString(),
                        NickName = dr["NickName"].ToString(),
                        OpenKey = SafeValueHelper.ToString(dr["OpenKey"]),
                        Password = dr["Password"].ToString(),
                        RegisterTime = SafeValueHelper.ToDateTime(dr["RegisterTime"])
                    };
                    list.Add(model);
                }
            }
            return list;
        } 
        private static DataSet GetAccountDataSet()
        {
            try
            {
                ISelectDataSourceFace query = new SelectSQL("UploadAccount");
                query.DataBaseAlias = "LogConnection";
                query.AddWhere("Enabled", true);
                return query.ExecuteDataSet();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

    }
}
