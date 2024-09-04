using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wine_e_commerce.Models;

namespace Wine_e_commerce.Controllers
{
    public class HomeController : Controller
    {
        private RUOUEntities1 db = new RUOUEntities1();
        // GET: Home
        public ActionResult Index()
        {
            List<SanPham> query_8sp = db.SanPhams.Take(8).ToList();
            
            var query_8spv2 = (from sp in db.SanPhams
                         orderby sp.Price descending
                         select sp).Take(8).ToList();
            ViewBag.query_8spv2 = query_8spv2;

            return View(query_8sp);
        }
    }
}