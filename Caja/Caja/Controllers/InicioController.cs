using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Caja.Models;
using Caja.Datos;

namespace Caja.Controllers
{
    public class InicioController : Controller
    {
        private string Autor;

        public string MensajeMotivador { get; set; }

        // GET: Inicio
        public ActionResult Index()
        {
            LoginController lo = new LoginController();
            lo.SmartLogin("~/Inicio/Index", "VALIDA");
            if (System.Web.HttpContext.Current.Session["usuario"] != null)
            {

                Datos.UsuarioTableAdapters.MENSAJES_MOTIVADORESTableAdapter mMot = new Datos.UsuarioTableAdapters.MENSAJES_MOTIVADORESTableAdapter();
                Datos.Usuario.MENSAJES_MOTIVADORESDataTable mensajes = new Datos.Usuario.MENSAJES_MOTIVADORESDataTable();
                mMot.Fill(mensajes);
                string mensaje = mensajes[0].VALOR;
                string autor = mensajes[0].DESCRIPCION;
                var model = new Inicio(MensajeMotivador = mensaje, Autor = autor);
                TempData["mensajeMot"] = model.MensajeMotivador;
                TempData["autor"] = model.Autor;
                return View();
            }
            else
            {
                return lo.VerificaConexion("~/Inicio/Index");
            }
        }
    }
}