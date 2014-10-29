using System;
using System.Collections.Generic;
using System.Data;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// グリッド表示用返却データ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultData
    {
        public int total;
        public int page;
        public int records;
        public List<ResultRecord> rows = new List<ResultRecord>();

        //************************************************************************
        /// <summary>
        /// DataTableからResultDataを作成する。
        /// </summary>
        /// <param name="argTable">DataTable</param>
        /// <returns>ResultData</returns>
        //************************************************************************
        public static ResultData CreateResultData(DataTable argTable)
        {
            var result = new ResultData();
            result.records = argTable.Rows.Count;
            foreach (DataRow row in argTable.Rows)
                result.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });

            return result;
        }
    }

    //************************************************************************
    /// <summary>
    /// グリッド表示用返却データ レコード
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultRecord
    {
        public int id;
        public object[] cell;
    }

#if ResultTable
    //************************************************************************
    /// <summary>
    /// 返却データテーブル
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultTable
    {
        public int total;
        public int page;
        public int records;
        public List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
    }
#endif
    //************************************************************************
    /// <summary>
    /// 返却データセット
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultDataSet
    {
        public Dictionary<string, object> firstRow = new Dictionary<string, object>();
        public Dictionary<string, ResultData> tables = new Dictionary<string, ResultData>();
        //public Dictionary<string, ResultTable> tables = new Dictionary<string, ResultTable>();

        //************************************************************************
        /// <summary>
        /// DataSetからResultDataSetを作成する。
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns>ResultDataSet</returns>
        //************************************************************************
        public static ResultDataSet CreateResultDataSet(DataSet ds)
        {
            ResultDataSet resultDs = new ResultDataSet();

            DataTable table = ds.Tables[0];

            // 最初の行のデータを設定
            if (table.Rows.Count > 0)
            {
                DataRow row = table.Rows[0];

                foreach (DataColumn dcol in table.Columns)
                    resultDs.firstRow.Add(dcol.ColumnName, row[dcol.ColumnName]);
            }

            // DataTableを設定
            foreach (DataTable dt in ds.Tables)
            {
                var rd = ResultData.CreateResultData(dt);
                resultDs.tables.Add(dt.TableName, rd);

#if ResultTable
                                ResultTable rt = new ResultTable();
                                rt.records = dt.Rows.Count;
                                resultDs.tables.Add(dt.TableName, rt);

                                foreach (DataRow row in dt.Rows)
                                {
                                    Dictionary<string, object> record = new Dictionary<string, object>();
                                    rt.rows.Add(record);

                                    foreach (DataColumn dcol in dt.Columns)
                                    {
                                        string name = dcol.ColumnName;
                                        if (name == "ROWNUMBER") record.Add("id", Convert.ToInt32(row[name]));
                                        else if (name == "削除") record.Add("状態", row[name]);
                                        else record.Add(name, row[name]);
                                    }
                                }
#endif
            }

            /*
            // 新規の場合
            string mode = Request.QueryString["_mode"];
            if (mode == "new")
            {
                // 全て新規行にする
                foreach (DataTable dt in ds.Tables)
                    foreach (DataRow row in dt.Rows) row.SetAdded();
            }

            // 親はクリア
            if (mode == "new") table.Rows.Clear();
             */

            return resultDs;
        }
    }

    //************************************************************************
    /// <summary>
    /// 処理結果返却データ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultStatus
    {
        public bool error;
        public int id;
        public List<ResultMessage> messages = new List<ResultMessage>();
    }

    //************************************************************************
    /// <summary>
    /// データテーブル名、行番号、フィールドデータ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class RowField
    {
        public string DataTableName;
        public int RowNumber;
        public string FieldName;

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argRowField">CMRowField</param>
        //************************************************************************
        public RowField(CMRowField argRowField)
        {
            DataTableName = argRowField.DataTableName;
            RowNumber = argRowField.RowNumber;
            FieldName = argRowField.FieldName;
        }
    }

    //************************************************************************
    /// <summary>
    /// メッセージ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultMessage
    {
        public string messageCd;
        public string message;
        public RowField rowField;
    }
}
