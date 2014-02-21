using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

using Seasar.Framework.Aop;
using Seasar.Framework.Log;

using Seasar.Extension.Tx;
using Seasar.Extension.Tx.Impl;
using Seasar.Quill.Database.Tx.Impl;
using Seasar.Quill.Attrs;

namespace NEXS.ERP.CM.BL
{
    public class CMTransactionAttribute : TransactionAttribute
    {
        private TransactionScopeOption? m_scopeOption;
        private IsolationLevel? m_isolationLevel;
        private int? m_timeout;

        #region プロパティ
        /// <summary>TransactionScopeOption</summary>
        public TransactionScopeOption ScopeOption
        {
            get
            {
                return m_scopeOption.Value;
            }
            set
            {
                m_scopeOption = value;
            }
        }

        /// <summary>TransactionScopeOption有無</summary>
        public bool HasScopeOption
        {
            get
            {
                return m_scopeOption.HasValue;
            }
        }

        /// <summary>トランザクション分離レベル</summary>
        public IsolationLevel IsolationLevel
        {
            get
            {
                return m_isolationLevel.Value;
            }
            set
            {
                m_isolationLevel = value;
            }
        }

        /// <summary>トランザクション分離レベル有無</summary>
        public bool HasIsolationLevel
        {
            get
            {
                return m_isolationLevel.HasValue;
            }
        }

        /// <summary>トランザクションタイムアウト秒数</summary>
        public int Timeout
        {
            get
            {
                return m_timeout.Value;
            }
            set
            {
                m_timeout = value;
            }
        }

        /// <summary>トランザクションタイムアウト秒数有無</summary>
        public bool HasTimeout
        {
            get
            {
                return m_timeout.HasValue;
            }
        }
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMTransactionAttribute()
        {
        }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="timeout">トランザクションタイムアウト秒数</param>
        //************************************************************************
        public CMTransactionAttribute(int timeout)
        {
            Timeout = timeout;
        }
        #endregion
    }
}
