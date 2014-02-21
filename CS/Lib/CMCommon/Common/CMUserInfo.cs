using System;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ���[�U�����i�[���邽�߂̃N���X
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMUserInfo
    {
        /// <summary>���[�U�h�c</summary>
        public string Id { get; set; }

        /// <summary>���[�U��</summary>
        public string Name { get; set; }

        /// <summary>���[��</summary>
        public string[] Roles { get; set; }

        /// <summary>�g�D�R�[�h</summary>
        public string SoshikiCd { get; set; }

        /// <summary>�g�D��</summary>
        public string SoshikiName { get; set; }

        /// <summary>�g�D�K�w�敪</summary>
        public string SoshikiKaisoKbn { get; set; }
    }
}
