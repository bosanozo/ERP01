using System.Web.Mvc;

namespace WebApp.Areas.CM
{
    public class CMAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "CM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "CM_default",
                "CM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}