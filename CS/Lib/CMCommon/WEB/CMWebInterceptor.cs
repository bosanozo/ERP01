using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using Seasar.Framework.Aop;

namespace NEXS.ERP.CM.WEB
{
    public class CMWebInterceptor : IMethodInterceptor
    {
        public object Invoke(IMethodInvocation invocation)
        {
            object ret = null;

            try
            {
                ret = invocation.Proceed();
            }
            catch (Exception ex)
            {
                //var aaa = invocation.Target;
                throw;
            }

            return ret;
        }
    }
}
