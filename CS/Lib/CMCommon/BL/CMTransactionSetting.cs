using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Seasar.Extension.Tx;
using Seasar.Extension.Tx.Impl;
using Seasar.Quill.Database.Tx.Impl;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// Transaction設定。
    /// アプリケーション構成ファイルで指定することにより独自のTransaction管理を行う。
    /// </summary>
    //************************************************************************
    public class CMTransactionSetting : TypicalTransactionSetting
    {
        //************************************************************************
        /// <summary>
        /// Transactionハンドラ生成
        /// </summary>
        /// <returns>Transactionハンドラ</returns>
        //************************************************************************
        protected override ITransactionHandler CreateTransactionHandler()
        {
            return new CMTransactionHandler();
        }
    }
}
