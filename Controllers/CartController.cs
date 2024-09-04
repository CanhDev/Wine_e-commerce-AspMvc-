using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.App_Start;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.Controllers
{
    public class CartController : Controller
    {
        private RUOUEntities1 db = new RUOUEntities1();
        [RoleUser]
        public ActionResult Index()
        {
            KhachHang kh = SessionConfig.getUser();
            GioHang UserCart = db.GioHangs.Where(g => g.idKhachHang == kh.MaKhachHang).FirstOrDefault();
            if (UserCart == null)
            {
                UserCart = new GioHang { idKhachHang = kh.MaKhachHang, NgayThem = DateTime.Now };
                db.GioHangs.Add(UserCart);
                db.SaveChanges();
            }
            
            //
            return View(UserCart);
        }
        public ActionResult addItem(int productId, int soluong = 1)
        {
            KhachHang kh = SessionConfig.getUser();
            if(kh != null)
            {
                GioHang UserCart = db.GioHangs.Where(g => g.idKhachHang == kh.MaKhachHang).FirstOrDefault();
                if (UserCart == null)
                {
                    UserCart = new GioHang { idKhachHang = kh.MaKhachHang, NgayThem = DateTime.Now };
                    db.GioHangs.Add(UserCart);
                    db.SaveChanges();
                }
                //
                var product = db.SanPhams.Find(productId);
                if (product == null || product.Quantity < soluong)
                {
                    return Json(new { status = false, error = "Lỗi sản phẩm" });
                }
                var cartItem = db.ItemCarts.FirstOrDefault(ci => ci.idGioHang == UserCart.MaGioHang && ci.idSanPham == productId);
                if (cartItem == null)
                {
                    cartItem = new ItemCart
                    {
                        idGioHang = UserCart.MaGioHang,
                        idSanPham = productId,
                        SoLuong = soluong,
                        Gia = product.Price
                    };
                    db.ItemCarts.Add(cartItem);
                }
                else
                {
                    cartItem.SoLuong += soluong;
                }
                product.Quantity -= soluong;
                db.SaveChanges();
                return Json(new { status = true, JsonRequestBehavior.AllowGet });
            }
            else
            {
                return Json(new { status = false, error = "Bạn chưa đăng nhập" });
            }
        }
        

        [RoleUser]
        [HttpPost]
        public ActionResult removeItem(int? id)
        {
            try
            {
                var item = db.ItemCarts.Find(id);
                if (item != null)
                {
                    var product = db.SanPhams.Find(item.idSanPham);
                    if (product != null)
                    {
                        product.Quantity += item.SoLuong.GetValueOrDefault();
                    }
                    db.ItemCarts.Remove(item);
                    db.SaveChanges();
                    return Json(new {status = true, JsonRequestBehavior.AllowGet});
                }
                else
                {
                    return Json(new {status = false, error = "Lỗi"});
                }
            }
            catch(Exception ex)
            {
                return Json(new {status = false, error = ex.Message});
            }
        }
        [RoleUser]
        [HttpPost]
        public ActionResult RemoveAllItem(int? idCart)
        {
            GioHang cart = db.GioHangs.Find(idCart);
            if (cart.ItemCarts.Any())
            {
                db.ItemCarts.RemoveRange(cart.ItemCarts);
                db.SaveChanges();
                return Json(new {status = true});
            }
            return Json(new {status = false});
        }
        [RoleUser]
        [HttpPost]
        public ActionResult updateQuantity(int? id, string option, double price)
        {
            try
            {
                ItemCart item = db.ItemCarts.Find(id);
                if (option.Equals("+"))
                {
                    ++item.SoLuong;
                }
                else
                {
                    --item.SoLuong;
                }
                db.SaveChanges();
                return Json(new {status = true, value = item.SoLuong.Value, money = item.SoLuong.Value * price , JsonRequestBehavior.AllowGet});
            }
            catch(Exception ex)
            {
                return Json(new { status = false, error = ex.Message });
            }
        }
        public ActionResult test()
        {
            return View();
        }
    }
}