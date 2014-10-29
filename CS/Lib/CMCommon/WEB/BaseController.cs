using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Text;
using System.Threading.Tasks;

using log4net;
using Seasar.Quill;

using DocumentFormat.OpenXml;
using SpreadsheetLight;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// コントローラの基底クラス
    /// </summary>
    //************************************************************************
    public class BaseController : Controller
    {
        #region ロガーフィールド
        private ILog m_logger;
        #endregion

        #region インジェクション用フィールド
        protected ICMCommonBL m_commonBL;
        #endregion

        #region プロパティ
        /// <summary>
        /// ロガー
        /// </summary>
        protected ILog Log
        {
            get { return m_logger; }
        }

        /// <summary>
        /// 共通処理ファサード
        /// </summary>
        protected ICMCommonBL CommonBL
        {
            get { return m_commonBL; }
        }        
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public BaseController()
        {
            // ロガーを取得
            m_logger = LogManager.GetLogger(this.GetType());

            // インジェクション実行
            QuillInjector injector = QuillInjector.GetInstance();
            injector.Inject(this);
        }
        #endregion
    }
}
