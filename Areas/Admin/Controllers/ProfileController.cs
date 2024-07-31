﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.App_Start;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.Areas.Admin.Controllers
{
    public class ProfileController : Controller
    {

        private RUOUEntities1 db = new RUOUEntities1();
        private DesEncryptionHelper desHelper = new DesEncryptionHelper("0123456789abcdef");

        //

        [RoleUser(functionCode = "AdminProfile")]
        public ActionResult Index()
        {
            KhachHang admin = (KhachHang)SessionConfig.getUser();
            ViewBag.navActive = 4;
            return View(admin);
        }

        [HttpPost]
        [RoleUser(functionCode = "AdminProfile")]
        public ActionResult Edit([Bind(Include = "TenKhachHang, MaKhachHang, DiaChi, TenDangNhap, MatKhau, SoDienThoai, Image")] KhachHang kh)
        {
            //will change into ajax
            try
            {
                if(ModelState.IsValid)
                {
                    var f = Request.Files["ImgEdit"];
                    if( f != null && f.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(f.FileName);
                        string path = Server.MapPath("~/images/Personal_Images/" + fileName);
                        f.SaveAs(path);
                        kh.Image = fileName;
                    }
                    db.Entry(kh).State = EntityState.Modified;
                    db.SaveChanges();
                    SessionConfig.setUser(kh);
                    return Json(new {status = true, JsonRequestBehavior.AllowGet});
                }
                else
                {
                    return Json(new {status = false, error = "Có lỗi trong quá trình sửa thông tin!"});
                }
            }
            catch {
                return Json(new { status = false, error = "Có lỗi trong quá trình sửa thông tin!" });
            }
        }


        [HttpPost]
        [RoleUser(functionCode = "AdminProfile")]
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