using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.App_Start;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.Controllers
{
    public class AccountController : Controller
    {
        private DesEncryptionHelper desHelper = new DesEncryptionHelper("0123456789abcdef");
        private RUOUEntities1 db = new RUOUEntities1();
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Banned()
        {
            return View();
        }
        public ActionResult Register() {
            KhachHang kh = SessionConfig.getUser();
            if (kh != null) return RedirectToAction("Index", "Home");
            else return View();
        }
        [HttpPost]
        public ActionResult Register(string name ,string username, string password, string sdt, string diachi)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var encryptedPassword = desHelper.Encrypt(password);
                    KhachHang newUser = new KhachHang();
                    newUser.TenKhachHang = name;
                    newUser.TenDangNhap = username;
                    newUser.MatKhau = encryptedPassword;
                    newUser.SoDienThoai = sdt;
                    newUser.DiaChi = diachi;
                    newUser.Image = "default_avatar.png";
                    //
                    var user = db.KhachHangs.FirstOrDefault(u => u.TenDangNhap == username);
                    if (user == null)
                    {
                        db.KhachHangs.Add(newUser);
                        ViewBag.checkUser = "";
                        db.SaveChanges();
                        return Json(new { status = true, JsonRequestBehavior.AllowGet });
                    }
                    else
                    {
                        return Json(new { status = false, error = "Đã tồn tại tên đăng nhập này" });
                    }
                }
                return View();
            }catch
            {
                return Json(new { status = false, error = "Lỗi không thể đăng ký" });
            }
        }
        public ActionResult Login()
        {
            KhachHang kh = SessionConfig.getUser();
            if (kh == null)
            {
                return View();
            }
            else
            {
                return  RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = db.KhachHangs.FirstOrDefault(u => u.TenDangNhap == username);
            if(user != null)
            {
                var decryptedPassword = desHelper.Decrypt(user.MatKhau);
                if(decryptedPassword == password)
                {
                    //set session
                    SessionConfig.setUser(user);
                    ViewBag.checkLogin = "";
                    if (user.TenDangNhap.Equals("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.checkLogin = "Sai tên đăng nhập hoặc mật khẩu";
            return View();
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        //clientProfile
        [RoleUser]
        public ActionResult ClientProfile()
        {
            KhachHang client = SessionConfig.getUser();
            return View(client);
        }
        [HttpPost]
        [RoleUser]
        public ActionResult EditProfile([Bind(Include = "TenKhachHang, MaKhachHang, DiaChi, TenDangNhap, MatKhau, SoDienThoai, Image")] KhachHang kh)
        {
            //will change into ajax
            try
            {
                if (ModelState.IsValid)
                {
                    var f = Request.Files["ImgEdit"];
                    if (f != null && f.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(f.FileName);
                        string path = Server.MapPath("~/images/Personal_Images/" + fileName);
                        f.SaveAs(path);
                        kh.Image = fileName;
                    }
                    db.Entry(kh).State = EntityState.Modified;
                    db.SaveChanges();
                    SessionConfig.setUser(kh);
                    return Json(new { status = true, JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return Json(new { status = false, error = "Có lỗi trong quá trình sửa thông tin!" });
                }
            }
            catch
            {
                return Json(new { status = false, error = "Có lỗi trong quá trình sửa thông tin!" });
            }
        }


        [HttpPost]
        [RoleUser]
        public ActionResult EditPassword(string currentPassword, string newpassword, string renewpassword)
        {
            try
            {
                // Truy xuất đối tượng KhachHang từ cùng một DbContext
                KhachHang admin = (KhachHang)SessionConfig.getUser();
                var dbAdmin = db.KhachHangs.Find(admin.MaKhachHang);

                if (dbAdmin == null)
                {
                    return Json(new { status = false, error = "Người dùng không tồn tại!" });
                }

                if (newpassword != renewpassword)
                {
                    return Json(new { status = false, error = "Mật khẩu nhập lại không trùng khớp!" });
                }

                // Giải mã mật khẩu từ cơ sở dữ liệu
                string password = desHelper.Decrypt(dbAdmin.MatKhau);
                if (password == currentPassword)
                {
                    // Mã hóa mật khẩu mới và cập nhật
                    dbAdmin.MatKhau = desHelper.Encrypt(newpassword);
                    db.Entry(dbAdmin).State = EntityState.Modified;
                    db.SaveChanges();

                    // Cập nhật session với thông tin mới
                    SessionConfig.setUser(dbAdmin);

                    return Json(new { status = true, JsonRequestBehavior.AllowGet });
                }
                else
                {
                    return Json(new { status = false, error = "Mật khẩu không đúng!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, error = ex.Message });
            }
        }
    }
}