using System;
using System.Collections.Generic;
using System.Data;
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
    public class CMTransactionHandler : ITransactionHandler
    {
        private static readonly Logger _logger = Logger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region ITransactionHandler メンバ

        object ITransactionHandler.Handle(IMethodInvocation invocation, bool alreadyInTransaction)
        {
            bool began = !alreadyInTransaction;

            if (began)
            {
                _logger.Log("DSSR0003", null);
            }

            // トランザクション属性を取得
            object[] attrs = invocation.Method.GetCustomAttributes(typeof(TransactionAttribute), true);

            // デフォルト値設定
            TransactionScopeOption scopeOption = TransactionScopeOption.Required;
            TransactionOptions opt = new TransactionOptions();
            opt.IsolationLevel =  System.Transactions.IsolationLevel.ReadCommitted;
            opt.Timeout = TimeSpan.FromSeconds(300);

            // CMTransactionの場合
            if (attrs.Length > 0 && attrs[0] is CMTransactionAttribute)
            {
                CMTransactionAttribute attr = attrs[0] as CMTransactionAttribute;
                if (attr.HasScopeOption) scopeOption = attr.ScopeOption;
                if (attr.HasIsolationLevel) opt.IsolationLevel = attr.IsolationLevel;
                if (attr.HasTimeout) opt.Timeout = TimeSpan.FromSeconds(attr.Timeout);
            }

            // TransactionScope実行
            using (TransactionScope scope = new TransactionScope(scopeOption, opt))
            {
                try
                {
                    // コネクション自動オープン
                    CMBaseBL bl = invocation.Target as CMBaseBL;
                    if (bl != null &&
                        bl.Connection.State == ConnectionState.Closed)
                        bl.Connection.Open();

                    object obj = invocation.Proceed();
                    if (began)
                    {
                        _logger.Log("DSSR0004", null);
                    }
                    scope.Complete();
                    return obj;
                }
                catch
                {
                    if (began)
                    {
                        _logger.Log("DSSR0005", null);
                    }
                    throw;
                }
                finally
                {
                    // コネクション自動クローズ
                    CMBaseBL bl = invocation.Target as CMBaseBL;
                    if (bl != null &&
                        bl.Connection.State != ConnectionState.Closed)
                        bl.Connection.Close();
                }
            }
        }

        #endregion
    }
}
