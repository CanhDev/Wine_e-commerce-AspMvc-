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

namespace Wine_e_commerce.Areas.Admin.Controllers
{
    public class DonHangsController : Controller
    {
        private RUOUEntities1 db = new RUOUEntities1();

        // GET: Admin/DonHangs
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult Index(string searchString, int? page)
        {
                IQueryable<DonHang> donHangs = db.DonHangs.Include(d => d.KhachHang);
                donHangs = donHangs.OrderByDescending(p => p.MaDonHang);
                //
                if (!string.IsNullOrEmpty(searchString))
                {
                    donHangs = FilterProducts(donHangs, searchString);
                    ViewBag.SearchString = searchString;
                }
                else
                {
                    ViewBag.SearchString = "";
                }
                //
                if (donHangs.Count() <= 0)
                {
                    ViewBag.notify = "Không có đơn hàng nào phù hợp";
                }
                else
                {
                    ViewBag.notify = "";
                }
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                ViewBag.navActive = 2;
                return View(donHangs.ToPagedList(pageNumber, pageSize));
        }
        private IQueryable<DonHang> FilterProducts(IQueryable<DonHang> donHangs, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                donHangs = donHangs.Where(s => s.KhachHang.TenKhachHang.Contains(searchString));
            }
            ViewBag.searchString = searchString ?? "";
            return donHangs;
        }
        // GET: Admin/DonHangs/Details/5
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult Details(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DonHang donHang = db.DonHangs.Find(id);
                if (donHang == null)
                {
                    return HttpNotFound();
                }
                return View(donHang);
        }

        // GET: Admin/DonHangs/Create
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult Create()
        {
                ViewBag.idKhachHang = new SelectList(db.KhachHangs, "MaKhachHang", "TenKhachHang");
                return View();
        }

        // POST: Admin/DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult Create([Bind(Include = "MaDonHang,PhuongThucThanhToan,TongSanPham,DiaChiNhanHang,TongTien,idKhachHang,ngayDat")] DonHang donHang)
        {
                if (ModelState.IsValid)
                {
                    db.DonHangs.Add(donHang);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                ViewBag.idKhachHang = new SelectList(db.KhachHangs, "MaKhachHang", "TenKhachHang", donHang.idKhachHang);
                return View(donHang);
        }

        // GET: Admin/DonHangs/Edit/5
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult Edit(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DonHang donHang = db.DonHangs.Find(id);
                if (donHang == null)
                {
                    return HttpNotFound();
                }
                ViewBag.idKhachHang = new SelectList(db.KhachHangs, "MaKhachHang", "TenKhachHang", donHang.idKhachHang);
                return View(donHang);
        }

        // POST: Admin/DonHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult Edit([Bind(Include = "MaDonHang,PhuongThucThanhToan,TongSanPham,DiaChiNhanHang,TongTien,idKhachHang,ngayDat")] DonHang donHang)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(donHang).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new {status = true, JsonRequestBehavior.AllowGet});
                }
                ViewBag.idKhachHang = new SelectList(db.KhachHangs, "MaKhachHang", "TenKhachHang", donHang.idKhachHang);
                return Json(new {status = false, error = "Lỗi trong quá trình sửa!"});
            }
            catch
            {
                return Json(new { status = false, error = "Lỗi trong quá trình sửa!" });
            }
        }
/*
        // GET: Admin/DonHangs/Delete/5
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult Delete(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                DonHang donHang = db.DonHangs.Find(id);
                if (donHang == null)
                {
                    return HttpNotFound();
                }
                return View(donHang);
        }*/

        // POST: Admin/DonHangs/Delete/5
        [HttpPost]
        [RoleUser(functionCode = "Admin_OrderManagement")]
        public ActionResult HandleDelete(int id)
        {
            try
            {
                DonHang donHang = db.DonHangs.Find(id);
                db.DonHangs.Remove(donHang);
                db.SaveChanges();
                return Json(new {status = true, JsonRequestBehavior.AllowGet});
            }
            catch
            {
                return Json(new {status = false, error = "Có lỗi trong quá trình xóa!"});
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
