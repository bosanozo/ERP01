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
