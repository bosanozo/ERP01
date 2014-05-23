using System;
using System.Collections.Generic;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// �O���b�h�\���p�ԋp�f�[�^
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
    /// �O���b�h�\���p�ԋp�f�[�^ ���R�[�h
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
    /// �ԋp�f�[�^�e�[�u��
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
    /// �ԋp�f�[�^�Z�b�g
    /// </summary>
    //************************************************************************
    [Serializable]
    public class ResultDataSet
    {
        public Dictionary<string, object> firstRow = new Dictionary<string, object>();
        public Dictionary<string, ResultData> tables = new Dictionary<string, ResultData>();
        //public Dictionary<string, ResultTable> tables = new Dictionary<string, ResultTable>();
    }

    //************************************************************************
    /// <summary>
    /// �������ʕԋp�f�[�^
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
    /// ���b�Z�[�W
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
