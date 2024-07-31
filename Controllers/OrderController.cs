using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.App_Start;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.Controllers
{
    public class OrderController : Controller
    {
        private RUOUEntities1 db = new RUOUEntities1();
        [RoleUser]
        public ActionResult Index()
        {
            KhachHang kh = SessionConfig.getUser();
            var Orders = db.DonHangs.Where(d => d.idKhachHang == kh.MaKhachHang).ToList();
            return View(Orders);
        }

        [RoleUser]
        public ActionResult DisplayOrder(int idCart)
        {
            GioHang UserCart = db.GioHangs.Find(idCart);
            if (UserCart != null && UserCart.ItemCarts.Any())
            {
                int count = 0; decimal total = 0;
                count = UserCart.ItemCarts.Count();
                total += UserCart.ItemCarts.Sum(p => p.SanPham.Price * p.SoLuong.GetValueOrDefault());
                //
                ViewBag.count = count; ViewBag.total = total; ViewBag.idUserCart = idCart;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [RoleUser]
        public ActionResult HandleOrder(int count, decimal total, int idCart, string address) {
            KhachHang kh = SessionConfig.getUser();
                DonHang order = new DonHang
                {
                    idKhachHang = kh.MaKhachHang,
                    PhuongThucThanhToan = "Thanh toán khi nhận hàng",
                    TongSanPham = count,
                    TongTien = total,
                    ngayDat = DateTime.Now,
                    DiaChiNhanHang = address
                };
                db.DonHangs.Add(order);
                db.SaveChanges();
                List<ItemCart> ds = db.ItemCarts.Where(i => i.idGioHang == idCart).ToList();
                foreach(var item in ds)
                {
                    if(item.SanPham != null)
                    {
                        ItemCart newItem = new ItemCart()
                        {
                            idSanPham = item.idSanPham,
                            SoLuong = item.SoLuong,
                            Gia = item.Gia,
                            idDonHang = order.MaDonHang
                        };
                        db.ItemCarts.Add(newItem);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
        }
        [HttpPost]
        [RoleUser]
        public ActionResult RemoveOrder(int id)
        {
            try
            {
                DonHang order = db.DonHangs.Find(id);
                if (order != null)
                {
                    db.DonHangs.Remove(order);
                    db.SaveChanges();
                    return Json(new {status = true, JsonRequestBehavior.AllowGet});
                }
                return Json(new {status = false, error = "Đơn hàng không tồn tại"});
            }
            catch
            {
                return Json(new { status = false, error = "Lỗi trong quá trình xóa đơn hàng!" });
            }
        }
        [RoleUser]
        public ActionResult ViewItem(int id)
        {
            KhachHang kh = SessionConfig.getUser();
            if(kh != null)
            {
                List<ItemCart> items = db.ItemCarts.Where(i => i.idDonHang == id).ToList();
                return View(items);
            }
            return HttpNotFound();
        }
    }
}