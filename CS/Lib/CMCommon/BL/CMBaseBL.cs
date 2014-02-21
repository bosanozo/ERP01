/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;

using log4net;

using Seasar.Quill;
using Seasar.Quill.Database.DataSource.Impl;
using Seasar.Extension.Tx.Impl;
using Seasar.Extension.ADO;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// �t�@�T�[�h�w�̊��N���X�ł��B
    /// </summary>
    //************************************************************************
    public class CMBaseBL
    {
        #region ���K�[�t�B�[���h
        private ILog m_logger;
        #endregion

        #region �C���W�F�N�V�����p�t�B�[���h
        protected CMCommonDA m_commonDA;
        #endregion

        private IDbConnection m_connection;

        #region �v���p�e�B
        /// <summary>
        /// ���K�[
        /// </summary>
        protected ILog Log
        {
            get { return m_logger; }
        }

        /// <summary>
        /// ���ʏ����f�[�^�A�N�Z�X�w
        /// </summary>
        protected CMCommonDA CommonDA
        {
            get { return m_commonDA; }
        }

        /// <summary>�f�[�^�\�[�X</summary>
        public SelectableDataSourceProxyWithDictionary DataSource
        {
            get
            {
                QuillInjector inj = QuillInjector.GetInstance();
                var cmp = inj.Container.GetComponent(typeof(SelectableDataSourceProxyWithDictionary));
                return cmp.GetComponentObject(typeof(SelectableDataSourceProxyWithDictionary))
                    as SelectableDataSourceProxyWithDictionary;
            }
        }

        /// <summary>�R�l�N�V����</summary>
        public IDbConnection Connection {
            get
            {
                if (m_connection == null)
                {
                    // �R�l�N�V������factory����쐬����
                    var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
                    m_connection = factory.CreateConnection();
                    m_connection.ConnectionString = ((TxDataSource)DataSource.GetDataSource()).ConnectionString;
                }

                return m_connection;
            }
            set
            {
                m_connection = value;
            }
        }
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMBaseBL()
        {
            // ���K�[���擾
            m_logger = LogManager.GetLogger(this.GetType());
        }
        #endregion

        #region protected���\�b�h
        #endregion
    }
}
