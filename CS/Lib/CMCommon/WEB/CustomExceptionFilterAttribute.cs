using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// WebAPI用ExceptionFilter
    /// </summary>
    //************************************************************************
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var result = new ResultStatus { error = true };
            var status = HttpStatusCode.InternalServerError;

            if (actionExecutedContext.Exception is CMException)
            {
                var ex = actionExecutedContext.Exception as CMException;

                status = HttpStatusCode.OK;

                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = ex.CMMessage.MessageCd,
                    message = ex.CMMessage.ToString(),
                    rowField = new RowField(ex.CMMessage.RowField)
                });
            }
            else if (actionExecutedContext.Exception is SqlException)
            {
                var ex = actionExecutedContext.Exception as SqlException;

                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = "EV002",
                    message = CMMessageManager.GetMessage("EV002", ex.Message)
                });
            }
            else
            {
                var ex = actionExecutedContext.Exception;

                // エラーメッセージを設定
                result.messages.Add(new ResultMessage
                {
                    messageCd = "EV001",
                    message = CMMessageManager.GetMessage("EV001", ex.Message)
                });
            }

            var response = new HttpResponseMessage(status)
            {
                Content = new ObjectContent<ResultStatus>(result, new JsonMediaTypeFormatter())
            };

            actionExecutedContext.Response = response;
        }
    }
}
