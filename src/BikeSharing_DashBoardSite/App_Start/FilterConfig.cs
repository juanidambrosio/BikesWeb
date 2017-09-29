using System.Web;
using System.Web.Mvc;

namespace BikeSharing_DashBoardSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
