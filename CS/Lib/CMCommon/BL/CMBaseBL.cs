/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
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
    /// ファサード層の基底クラスです。
    /// </summary>
    //************************************************************************
    public class CMBaseBL
    {
        #region ロガーフィールド
        private ILog m_logger;
        #endregion

        #region インジェクション用フィールド
        protected CMCommonDA m_commonDA;
        #endregion

        private IDbConnection m_connection;

        #region プロパティ
        /// <summary>
        /// ロガー
        /// </summary>
        protected ILog Log
        {
            get { return m_logger; }
        }

        /// <summary>
        /// 共通処理データアクセス層
        /// </summary>
        protected CMCommonDA CommonDA
        {
            get { return m_commonDA; }
        }

        /// <summary>データソース</summary>
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

        /// <summary>コネクション</summary>
        public IDbConnection Connection {
            get
            {
                if (m_connection == null)
                {
                    // コネクションはfactoryから作成する
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

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMBaseBL()
        {
            // ロガーを取得
            m_logger = LogManager.GetLogger(this.GetType());
        }
        #endregion

        #region protectedメソッド
        #endregion
    }
}
