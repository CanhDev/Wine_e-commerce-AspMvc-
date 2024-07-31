using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.App_Start
{
    public static class SessionConfig
    {
        public static void setUser(KhachHang user)
        {
            HttpContext.Current.Session["userId"] = user.MaKhachHang;
        }

        public static int? getUserId()
        {
            return HttpContext.Current.Session["userId"] as int?;
        }

        public static KhachHang getUser()
        {
            int? userId = getUserId();
            if (userId.HasValue)
            {
                using (var db = new RUOUEntities1())
                {
                    return db.KhachHangs.Find(userId.Value);
                }
            }
            return null;
        }
    }

}