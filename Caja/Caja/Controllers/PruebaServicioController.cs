using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Caja.Models.Class;
using Caja.Class;

namespace Caja.Controllers
{
    public class PruebaServicioController : Controller
    {
        // GET: PruebaServicio
        public ActionResult Index()
        {          
           
            return View();        
        }
        [HttpPost]
        public ActionResult btnPrueba()
        {
            var dll = new PruebaServicioViewModels();
            var Afip = new Afip();
            //ViewBag.Mensaje = Afip.CAE();
            //Afip.UltimoComprobante();
            
            return View("Index");
        }
        public ActionResult btnConsulta()
        {
            var dll = new PruebaServicioViewModels();
            var afip = new Afip();
            //afip.ConsultaDatos();

            return View("Index");
        }
    }
}