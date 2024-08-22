using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.Models;
using Wine_e_commerce.App_Start;
using System.Collections;
using System.IO;

namespace Wine_e_commerce.Areas.Admin.Controllers
{
    public class KhachHangsController : Controller
    {
        private RUOUEntities1 db = new RUOUEntities1();
        private DesEncryptionHelper desHelper = new DesEncryptionHelper("0123456789abcdef");
        // GET: Admin/KhachHangs
        [RoleUser(functionCode ="KhachHangList")]
        public ActionResult Index(string searchString, int? page)
        {
                IQueryable<KhachHang> khachHangs = db.KhachHangs;
                khachHangs = khachHangs.OrderByDescending(p => p.MaKhachHang);
                //
                if (!string.IsNullOrEmpty(searchString))
                {
                    khachHangs = FilterProducts(khachHangs, searchString);
                    ViewBag.SearchString = searchString;
                }
                else
                {
                    ViewBag.SearchString = "";
                }
                //
                if (khachHangs.Count() <= 0)
                {
                    ViewBag.notify = "Không có tài khoản nào phù hợp";
                }
                else
                {
                    ViewBag.notify = "";
                }
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                ViewBag.navActive = 3;
                return View(khachHangs.ToPagedList(pageNumber, pageSize));
        }
        private IQueryable<KhachHang> FilterProducts(IQueryable<KhachHang> khachHangs, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                khachHangs = khachHangs.Where(s => s.TenKhachHang.Contains(searchString));
            }
            ViewBag.searchString = searchString ?? "";
            return khachHangs;
        }

        [RoleUser(functionCode = "ViewDetailKhachHang")]
        public ActionResult Details(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                KhachHang khachHang = db.KhachHangs.Find(id);
                if (khachHang == null)
                {
                    return HttpNotFound();
                }
                return View(khachHang);
        }

        
        [RoleUser(functionCode = "AddKhachHang")]
        public ActionResult Create()
        {
                return View();
        }

        
        [HttpPost]
        public ActionResult Create([Bind(Include = "TenKhachHang,MaKhachHang,DiaChi,TenDangNhap,MatKhau,SoDienThoai,Image")] KhachHang khachHang, HttpPostedFileBase imgFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool Exist = db.KhachHangs.Any(k => k.TenDangNhap.Contains(khachHang.TenDangNhap));
                    if (!Exist)
                    {
                        if(imgFile != null && imgFile.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(imgFile.FileName);
                            string path = Path.Combine(Server.MapPath("~/images/Personal_Images/"), fileName);
                            imgFile.SaveAs(path);
                            khachHang.Image = fileName;
                        }
                        else
                        {
                            khachHang.Image = "default_avatar.png";
                        }
                        khachHang.MatKhau = desHelper.Encrypt(khachHang.MatKhau);
                        db.KhachHangs.Add(khachHang);
                        db.SaveChanges();
                        return Json(new {status = true, JsonRequestBehavior.AllowGet});
                    }
                    else
                    {
                        return Json(new {status = false, error = "Đã tồn tại tên đăng nhập này"});
                    }
                }

                return Json(new {status = false, error = "Có lỗi trong quá trình tạo!"});
            }
            catch
            {
                return Json(new { status = false, error = "Có lỗi trong quá trình tạo!" });
            }
        }

        [RoleUser(functionCode = "UpdateKhachHang")]
        public ActionResult Edit(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                KhachHang khachHang = db.KhachHangs.Find(id);
                if (khachHang == null)
                {
                    return HttpNotFound();
                }
                if (khachHang != null)
                {
                    ViewBag.password = desHelper.Decrypt(khachHang.MatKhau);
                }
                return View(khachHang);
        }
        
        [HttpPost]
        public ActionResult Edit([Bind(Include = "TenKhachHang,MaKhachHang,DiaChi,TenDangNhap,MatKhau,SoDienThoai,Image")] KhachHang khachHang, HttpPostedFileBase imgFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if(imgFile != null && imgFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imgFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/images/Personal_Images/"), fileName);
                        imgFile.SaveAs(path);
                        khachHang.Image = fileName;
                    }
                    khachHang.MatKhau = desHelper.Encrypt(khachHang.MatKhau);
                    db.Entry(khachHang).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new {status = true, JsonRequestBehavior.AllowGet});
                }
                return Json(new { status = false, error = "Có lỗi trong quá trình sửa!" });
            }
            catch
            {
                return Json(new { status = false, error = "Có lỗi trong quá trình sửa!" });
            }
        }

        
        [RoleUser(functionCode = "DeleteKhachHang")]
        /*public ActionResult Delete(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                KhachHang khachHang = db.KhachHangs.Find(id);
                if (khachHang == null)
                {
                    return HttpNotFound();
                }
            if (khachHang.TenDangNhap.Contains("Admin")) return RedirectToAction("Index", "Check");
                return View(khachHang);
        }*/

        // POST: Admin/KhachHangs/Delete/5
        [HttpPost]
        [RoleUser(functionCode = "DeleteKhachHang")]
        public ActionResult handleDelete(int id)
        {
            try
            {
                KhachHang khachHang = db.KhachHangs.Find(id);
                GioHang gioHang = db.GioHangs.FirstOrDefault(p => p.idKhachHang == id);
                List<DonHang> donHang = db.DonHangs.Where(p => p.idKhachHang == id).ToList();
                if (donHang.Count != 0)
                {
                    db.DonHangs.RemoveRange(donHang);
                }
                if (gioHang != null)
                {
                    db.GioHangs.Remove(gioHang);
                }
                db.KhachHangs.Remove(khachHang);
                db.SaveChanges();
                return Json(new {status = true, JsonRequestBehavior.AllowGet});
            }
            catch
            {
                return Json(new { status = false, error = "Có lỗi trong quá trình xóa!" });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        
    }
}
