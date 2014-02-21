/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// �Ɩ��A�v���P�[�V������O�N���X
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMException : Exception
    {
        #region �v���p�e�B
        /// <summary>
        /// ���b�Z�[�W
        /// </summary>
        public CMMessage CMMessage
        {
            get { return (CMMessage)Data["CMMessage"]; }
            set { Data.Add("CMMessage", value); }
        }
        #endregion

        #region �R���X�g���N�^
        // �e�N���X��������p�����R���X�g���N�^
        public CMException() { }
        public CMException(string argMessage) : base(argMessage) { }
        public CMException(string argMessage, Exception argInnerException)
            : base(argMessage, argInnerException) { }
        public CMException(SerializationInfo argSerializationInfo, StreamingContext argStreamingContext)
            : base(argSerializationInfo, argStreamingContext) { }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argCMMessage">���b�Z�[�W�f�[�^</param>
        //************************************************************************
        public CMException(CMMessage argCMMessage)
        {
            CMMessage = argCMMessage;
        }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argCMMessage">���b�Z�[�W�f�[�^</param>
        /// <param name="argInnerException">��O�����̌��ƂȂ�����O</param>
        //************************************************************************
        public CMException(CMMessage argCMMessage, Exception argInnerException)
            : base(argInnerException.Message, argInnerException)
        {
            CMMessage = argCMMessage;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// ���b�Z�[�W�������Ԃ��B
        /// </summary>
        //************************************************************************
        public override string ToString()
        {
            if (CMMessage != null)
            {
                // �S���b�Z�[�W�������A��
                StringBuilder builder = new StringBuilder(CMMessage.ToString());

                // ���b�Z�[�W���G���[�ȊO�̏ꍇ�͊ȗ�������
                if (CMMessage.MessageCd.Length >= 1 && CMMessage.MessageCd[0] != 'E')
                {
                    if (InnerException != null)
                        builder.AppendLine().Append(InnerException.GetType().FullName)
                            .Append(": ").Append(InnerException.Message);
                }
                else builder.AppendLine().Append(base.ToString());

                return builder.ToString();
            }
            else return base.ToString();
        }
    }
}
