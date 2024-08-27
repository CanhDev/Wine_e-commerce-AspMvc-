using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.Models;
using PagedList;
using Wine_e_commerce.App_Start;

namespace Wine_e_commerce.Areas.Admin.Controllers
{
    public class SanPhamsController : Controller
    {
        private RUOUEntities1 db = new RUOUEntities1();
        // GET: Admin/SanPhams
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult Index(string searchString, int? page)
        {
                IQueryable<SanPham> sanPhams = db.SanPhams.Include(s => s.LoaiSanPham).Include(s => s.NhaSanXuat);
                sanPhams = sanPhams.OrderByDescending(p => p.ProductID);
                //
                if (!string.IsNullOrEmpty(searchString))
                {
                    sanPhams = FilterProducts(sanPhams, searchString);
                    ViewBag.SearchString = searchString;
                }
                else
                {
                    ViewBag.SearchString = "";
                }
                //
                if (sanPhams.Count() <= 0)
                {
                    ViewBag.notify = "Không có sản phẩm nào phù hợp";
                }
                else
                {
                    ViewBag.notify = "";
                }
                int pageSize = 10;
                int pageNumber = (page ?? 1);
                ViewBag.navActive = 1;
                return View(sanPhams.ToPagedList(pageNumber, pageSize));
        }
        private IQueryable<SanPham> FilterProducts(IQueryable<SanPham> products, string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.ProductName.Contains(searchString));
            }
            ViewBag.searchString = searchString ?? "";
            return products;
        }
        // GET: Admin/SanPhams/Details/5
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult Details(int? id)
        {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                SanPham sanPham = db.SanPhams.Find(id);
                if (sanPham == null)
                {
                    return HttpNotFound();
                }
                return View(sanPham);
        }

        // GET: Admin/SanPhams/Create
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult Create()
        {
                ViewBag.CatalogyID = new SelectList(db.LoaiSanPhams, "CatalogyID", "CatalogyName");
                ViewBag.MaNhaSanXuat = new SelectList(db.NhaSanXuats, "MaNhaSanXuat", "TenNhaSanXuat");
                return View();
        }

        // POST: Admin/SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult Create([Bind(Include = "ProductID,ProductName,Description,PurchasePrice,Price,Quantity,Vintage,CatalogyID,Image,Region,MaNhaSanXuat")] SanPham sanPham,
            HttpPostedFileBase ImageFile)
        {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (ImageFile != null && ImageFile.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(ImageFile.FileName);
                            string path = Path.Combine(Server.MapPath("~/Images/Ruou/"), fileName);
                            ImageFile.SaveAs(path);
                            sanPham.Image = fileName;
                        }
                        else
                        {
                            return Json(new { status = false, error = "Lỗi trong quá trình tạo" });
                        }
                        db.SanPhams.Add(sanPham);
                        db.SaveChanges();
                        return Json(new { status = true, JsonRequestBehavior.AllowGet});
                    }
                     ViewBag.CatalogyID = new SelectList(db.LoaiSanPhams, "CatalogyID", "CatalogyName", sanPham.CatalogyID);
                     ViewBag.MaNhaSanXuat = new SelectList(db.NhaSanXuats, "MaNhaSanXuat", "TenNhaSanXuat", sanPham.MaNhaSanXuat);
                     return Json(new { status = false, error = "Lỗi trong quá trình tạo" });
                }
                catch(Exception ex)
                {
                    return Json(new { status = false, error = ex.Message });
                }
        }

        // GET: Admin/SanPhams/Edit/5
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult Edit(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                SanPham sanPham = db.SanPhams.Find(id);
                if (sanPham == null)
                {
                    return HttpNotFound();
                }
                ViewBag.CatalogyID = new SelectList(db.LoaiSanPhams, "CatalogyID", "CatalogyName", sanPham.CatalogyID);
                ViewBag.MaNhaSanXuat = new SelectList(db.NhaSanXuats, "MaNhaSanXuat", "TenNhaSanXuat", sanPham.MaNhaSanXuat);
                return View(sanPham);
        }

        // POST: Admin/SanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult Edit([Bind(Include = "ProductID,ProductName,Description,PurchasePrice,Price,Quantity,Vintage,CatalogyID,Image,Region,MaNhaSanXuat")] SanPham sanPham)
        {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var f = Request.Files["imgEdit"];
                        if (f != null && f.ContentLength > 0)
                        {
                            string fileName = System.IO.Path.GetFileName(f.FileName);
                            string path = Server.MapPath("~/images/Ruou/" + fileName);
                            f.SaveAs(path);
                            sanPham.Image = fileName;
                        }
                        db.Entry(sanPham).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new {status = true, JsonRequestBehavior.AllowGet});
                    }
                    else
                    {
                        ViewBag.CatalogyID = new SelectList(db.LoaiSanPhams, "CatalogyID", "CatalogyName", sanPham.CatalogyID);
                        ViewBag.MaNhaSanXuat = new SelectList(db.NhaSanXuats, "MaNhaSanXuat", "TenNhaSanXuat", sanPham.MaNhaSanXuat);
                        return Json(new {status = false, error = "Đã xảy ra lỗi khi sửa sản phẩm. Vui lòng thử lại sau." });
                    }
                }
                catch
                {
                    return Json(new { status = false, error = "Đã xảy ra lỗi khi sửa sản phẩm. Vui lòng thử lại sau." });
                }
        }

        // GET: Admin/SanPhams/Delete/5
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult Delete(int? id)
        {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                SanPham sanPham = db.SanPhams.Find(id);
                if (sanPham == null)
                {
                    return HttpNotFound();
                }
                return View(sanPham);
        }

        // POST: Admin/SanPhams/Delete/5
        [HttpPost]
        [RoleUser(functionCode = "Admin_ProductManagement")]
        public ActionResult HandleDelete(int id)
        {
            try
            {
                SanPham sanPham = db.SanPhams.Find(id);
                ItemCart item = db.ItemCarts.FirstOrDefault(i => i.idSanPham == id);
                if(item != null)
                {
                    db.ItemCarts.Remove(item);
                }
                db.SanPhams.Remove(sanPham);
                db.SaveChanges();
                return Json(new { status = true, JsonRequestBehavior.AllowGet });
            }
            catch(Exception ex)
            {
                return Json(new {status = false, error = ex.Message});
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
