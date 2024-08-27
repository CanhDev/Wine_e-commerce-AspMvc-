using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.App_Start
{
    public class RoleUser : AuthorizeAttribute
    {
        public string functionCode { get; set; }
        private static RUOUEntities1 db = new RUOUEntities1();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var user = SessionConfig.getUser();
            if (user == null)
            {
                //var returnUrl = filterContext.RequestContext.HttpContext.Request.RawUrl.ToString();
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login",
                        area = "",
                       // returnUrl = returnUrl
                    })
                );
                return;
            }

            // Kiểm tra quyền
            else if(functionCode != null)
            {
                    var function = db.my_Funtion.FirstOrDefault(m => m.FunctionCode == functionCode);
                    if (function == null)
                    {
                        throw new Exception("Không tìm thấy function với mã chức năng này.");
                    }
                    int id = function.ID;
                    var hasPermission = db.Authorizes.Any(
                        m => m.idKhachHang == user.MaKhachHang && m.idChucNang == id
                    );

                    if (hasPermission)
                    {
                        return;
                    }
                    else
                    {
                        //var returnUrl = filterContext.RequestContext.HttpContext.Request.RawUrl.ToString();
                        filterContext.Result = new RedirectToRouteResult(
                            new RouteValueDictionary(new
                            {
                                controller = "Check",
                                action = "Index",
                                area = "Admin",
                                //returnUrl = returnUrl
                            })
                        );
                    }
            }
            else
            {
                return;
            }
        }
    }
}