using System;
using System.Collections.Generic;

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
    /// メッセージ
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultMessage
    {
        public string messageCd;
        public string message;
        public CMRowField rowField;
    }
}
