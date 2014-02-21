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
using System.Linq;
using System.Text;
using System.Web;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// �F�؊Ǘ��t�@�T�[�h
    /// </summary>
    //************************************************************************
    public class CMAuthenticationBL : CMBaseBL, ICMAuthenticationBL
    {
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string PASSWD = "PASSWD";
        private const string ROLE = "ROLE";

        #region �C���W�F�N�V�����p�t�B�[���h
        protected CMUserInfoDA m_dataAccess;
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMAuthenticationBL() 
        {
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// ���[�UID�ƃp�X���[�h�ɂ��F�؂����s���܂��B
        /// ���̃��\�b�h�́A���[�J���Ăяo���Ń��[�UID�ƃp�X���[�h�̌��؂������s���A
        /// Web�A�v���P�[�V�������p�̃��\�b�h�ł��B
        /// </summary>
        /// <param name="userId">���[�UID�B</param>
        /// <param name="password">�p�X���[�h�B</param>
        /// <returns>���[�U���F�؂ł����ꍇ�� true �B</returns>
        //************************************************************************
        public bool Authenticate(string userId, string password)
        {
            DataRow row = GetUserData(userId, password);

            // ���[�U�h�c�ƃp�X���[�h����Ј������擾�i�擾�ł���� true�j
            if (row != null)
            {
                CMInformationManager.UserInfo = CreateUserInfo(row);
            }

            return row != null;
        }

        //************************************************************************
        /// <summary>
        /// ���[�UID�Ŏw�肳�ꂽ���[�U�̃��[�U�����擾���܂��B
        /// ���̃��\�b�h�́A��ɃT�[�o���ŔF�؍ς݂̃��[�U�����擾���邽�߂�
        /// �g�p����܂��B
        /// </summary>
        /// <param name="userId">���[�UID�B</param>
        /// <returns>���[�U���B���[�U��񂪎擾�ł��Ȃ������ꍇ�� null�Q�ƁB</returns>
        //************************************************************************
        public CMUserInfo GetUserInfo(string userId)
        {
            // ���[�U�h�c����Ј������擾
            return CreateUserInfo(GetUserData(userId));
        }

        //************************************************************************
        /// <summary>
        /// DataRow����<see cref="CMUserInfo"/> ���쐬���܂��B
        /// null ��n���ƁA��O�ɂ͂Ȃ炸�� null�Q�Ƃ�Ԃ��܂��B
        /// ���̃��\�b�h�́A���̃t�@�T�[�h�̊e���\�b�h����g�p�����������\�b�h�ł��B
        /// </summary>
        /// <param name="userData">���[�U�̏����i�[���Ă���DataRow�B</param>
        /// <returns>���[�U����ێ����� <see cref="CMUserInfo"/>�B
        /// <paramref name="userData"/> �� null �̏ꍇ�� null�Q�ƁB</returns>
        //************************************************************************
        private CMUserInfo CreateUserInfo(DataRow userData)
        {
            if (userData == null) return null;

            CMUserInfo userInfo = new CMUserInfo();
            // ���[�U���̐ݒ�
            userInfo.Id = userData[ID].ToString();
            userInfo.Name = userData[NAME].ToString();
            // ���[���̐ݒ�
            userInfo.Roles = userData[ROLE].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            userInfo.SoshikiCd = userData["�g�DCD"].ToString();
            userInfo.SoshikiName = userData["�g�D��"].ToString();
            userInfo.SoshikiKaisoKbn = userData["�g�D�K�w�敪"].ToString();

            return userInfo;
        }

        //************************************************************************
        /// <summary>
        /// ���[�UID�ƃp�X���[�h�Ŏw�肳�ꂽ���[�U�̏����i�[���Ă���DataRow���擾���܂��B
        /// ���̃��\�b�h�́A���̃t�@�T�[�h�̊e���\�b�h����g�p�����������\�b�h�ł��B
        /// </summary>
        /// <param name="userId">���[�UID�B</param>
        /// <param name="password">�p�X���[�h�B</param>
        /// <returns>�w�肳�ꂽ���[�U�̏����i�[���Ă���DataRow�B
        /// �Ώۂ̃��[�U��񂪎擾�ł��Ȃ������ꍇ�� null�Q�ƁB</returns>
        //************************************************************************
        private DataRow GetUserData(string userId, string password)
        {
            // ���[�U�̃p�X���[�h�`�F�b�N���v���W�F�N�g�ŃJ�X�^�}�C�Y����ꍇ�́A
            // ��ɂ��̃��\�b�h���J�X�^�}�C�Y���܂��i�p�X���[�h�𒼐ڃ`�F�b�N����ꍇ�̂݁j�B

            // ���[�U�h�c����Ј������擾
            DataRow userData = GetUserData(userId);
            if (userData == null) return null;

            // �p�X���[�h�̃`�F�b�N�i�p�X���[�h�͏�ɑ啶������������ʂ��ă`�F�b�N�j
            if (password != userData[PASSWD].ToString()) return null;

            return userData;
        }

        //************************************************************************
        /// <summary>
        /// ���[�UID�Ŏw�肳�ꂽ���[�U�̏����i�[���Ă���DataRow���擾���܂��B
        /// ���̃��\�b�h�́A���̃t�@�T�[�h�̊e���\�b�h����g�p�����������\�b�h�ł��B
        /// </summary>
        /// <param name="userId">���[�UID�B</param>
        /// <returns>�w�肳�ꂽ���[�U�̏����i�[���Ă���DataRow�B
        /// �Ώۂ̃��[�U��񂪎擾�ł��Ȃ������ꍇ�� null�Q�ƁB</returns>
        //************************************************************************
        private DataRow GetUserData(string userId)
        {
            // �f�[�^�A�N�Z�X�w�쐬
            m_dataAccess.Connection = Connection;
            // ���[�U�[���擾
            return m_dataAccess.FindById(userId);
        }
    }
}
