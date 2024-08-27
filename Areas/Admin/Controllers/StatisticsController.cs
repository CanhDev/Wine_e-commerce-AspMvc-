using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.App_Start;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.Areas.Admin.Controllers
{
    public class StatisticsController : Controller
    {
        private RUOUEntities1 db = new RUOUEntities1();
        // GET: Admin/Statistics
        [RoleUser(functionCode = "Admin_Statistics")]
        public ActionResult Index()
        {
            ViewBag.navActive = 0;
            return View();
        }
        [RoleUser(functionCode = "Admin_Statistics")]
        public ActionResult getStatistic(string startDate, string endDate)
        {
            var query = from o in db.DonHangs
                        join i in db.ItemCarts
                        on o.MaDonHang equals i.idDonHang
                        join p in db.SanPhams
                        on i.idSanPham equals p.ProductID
                        select new
                        {
                            createdDate = o.ngayDat,
                            quantity = i.SoLuong,
                            price = p.Price,
                            originalPrice = p.PurchasePrice
                        };
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime start = DateTime.ParseExact(startDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.createdDate >= start);
            }
            if (!string.IsNullOrEmpty(endDate))
            {
                DateTime end = DateTime.ParseExact(endDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.createdDate < end);
            }
            //
            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.createdDate)).Select(x => new
            {
                Date = x.Key.Value,
                TotalBuy = x.Sum(y => y.quantity * y.originalPrice),
                TotalSell = x.Sum(y => y.quantity * y.price)
            }).Select(x => new
            {
                Date = x.Date,
                DoanhThu = x.TotalSell,
                LoiNhuan = x.TotalSell- x.TotalBuy
            });
            return Json(new {Data = result}, JsonRequestBehavior.AllowGet);
        }
    }
}