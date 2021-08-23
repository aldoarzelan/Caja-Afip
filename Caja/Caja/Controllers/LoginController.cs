using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Caja.Models;

namespace Caja.Controllers
{
    public class LoginController : Controller
    {
        Datos.UsuarioTableAdapters.MENU_WEBTableAdapter taMenu = new Datos.UsuarioTableAdapters.MENU_WEBTableAdapter();
        Datos.UsuarioTableAdapters.MENU_HIJOSTableAdapter TaHijos = new Datos.UsuarioTableAdapters.MENU_HIJOSTableAdapter();
        Datos.Usuario dsUsuario = new Datos.Usuario();

        // GET: Login
        public ActionResult Login(String usuario, String pass, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.usuario = usuario;
            ViewBag.password = pass;
            return View();
        }

        // GET: Login
        public ActionResult SmartLogin(string returnUrl, string accion)
        {
            if (TempData["mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["mensaje"].ToString();
                if (TempData["recuperarClave"] != null)
                {
                    ViewBag.Recupera = TempData["recuperarClave"].ToString(); //INDICO QUE SE DEBE ABRIR EL MODAL PARA RECUPERAR CLAVE
                }
            }

            if (accion == "CERRAR")
            {
                return CerrarSession();
            }
            else
            {
                if (accion == "VALIDA") //VALIDO LA CONEXION
                {
                    ViewBag.ReturnUrl = returnUrl;
                    string v_user = System.Web.HttpContext.Current.Session["usuario"] as String;
                    string v_pass = System.Web.HttpContext.Current.Session["password"] as String;
                    return ValidaConexion(v_user, v_pass, returnUrl);
                }
                else
                {
                    return View();
                }
            }
        }


        //[HttpPost]
        public ActionResult ValidaConexion(string Usuario, string Password, string p_url)
        {
            string v_user;
            string v_pass;
            if (Usuario == null || Password == null)
            {
                v_user = System.Web.HttpContext.Current.Session["usuario"] as String;
                v_pass = System.Web.HttpContext.Current.Session["password"] as String;
            }
            else
            {
                v_user = Usuario;
                v_pass = Password;
            }
            var conex = new Services.ConexionServices();
            var model = conex.ObtenerConexion(v_user, v_pass);
            ViewBag.usuario = model.Usuario;
            ViewBag.password = model.Clave;
            ViewData["usuario"] = System.Web.HttpContext.Current.Session["usuario"] as String;
            ViewData["password"] = System.Web.HttpContext.Current.Session["password"] as String;
            ViewData["Url"] = System.Web.HttpContext.Current.Session["Url"] as String;
            try
            {
                model.Url = ViewData["Url"].ToString();
            }
            catch
            {
                model.Url = p_url;// "~/Inicio/Index";
            }
            if (model.Usuario == null || ViewData["usuario"] == null)
            {
                TempData["mensaje"] = model.Mensaje;
                TempData["login"] = 1;
                TempData["recuperarClave"] = model.RecuperarClave;
                //                ViewData["Mensaje"] = model.Mensaje;
                model.Intentos = 1;
                return RedirectToAction("SmartLogin", "Login");
            }
            else
            {
                /* model.Usuario = ViewData["usuario"].ToString();
                 model.Password = ViewData["usuario"].ToString();*/
                TempData["login"] = 0;
                if (p_url == null)
                {
                    p_url = "~/Inicio/Index";
                }
                return Redirect(p_url);
            }

        }
        //[HttpPost]
        public ActionResult CerrarSession()
        {
            System.Web.HttpContext.Current.Session["usuario"] = null;
            System.Web.HttpContext.Current.Session["password"] = null;
            System.Web.HttpContext.Current.Session["ultimoLog"] = null;
            return View();
        }

        //[HttpPost]
        public ActionResult VerificaConexion(string p_url)
        {
            String v_user = System.Web.HttpContext.Current.Session["usuario"] as String;
            String v_pass = System.Web.HttpContext.Current.Session["password"] as String;
            DateTime v_log;
            if (System.Web.HttpContext.Current.Session["ultimoLog"] != null)
            {
                v_log = DateTime.Parse(System.Web.HttpContext.Current.Session["ultimoLog"].ToString());
            }
            else
            {
                v_log = DateTime.Parse("01/01/1990");
            }
            DateTime v_actual = DateTime.Now;
            TimeSpan difference = v_actual.Subtract(v_log);
            DateTime totalDate = DateTime.MinValue + difference;
            int minutos = totalDate.Minute;
            if (v_user != null && v_log != null && minutos <= 10)
            {
                return View();
            }
            else
            {
                v_user = null;
                v_pass = null;
                return ValidaConexion(v_user, v_pass, p_url);
            }
        }
        public ActionResult Index()
        {
            if (TempData["mensaje"] != null)
            {
                ViewBag.Mensaje = TempData["mensaje"].ToString();
            }
            return View();
        }

        //[HttpPost]
        public ActionResult RecuperarClave(string Usuario, string Password, string Passwordnew, string Passwordnew2)
        {
            string v_user;
            string v_pass;
            if (Usuario == null || Password == null)
            {
                v_user = System.Web.HttpContext.Current.Session["usuario"] as String;
                v_pass = System.Web.HttpContext.Current.Session["password"] as String;
            }
            else
            {
                v_user = Usuario;
                v_pass = Password;
            }
            var conex = new Services.ConexionServices();
            var model = conex.ObtenerConexion(v_user, v_pass);
            ViewBag.usuario = model.Usuario;
            ViewBag.password = model.Clave;
            ViewData["usuario"] = System.Web.HttpContext.Current.Session["usuario"] as String;
            ViewData["password"] = System.Web.HttpContext.Current.Session["password"] as String;
            ViewData["Url"] = System.Web.HttpContext.Current.Session["Url"] as String;
            try
            {
                model.Url = ViewData["Url"].ToString();
            }
            catch
            {
                model.Url = "~/Inicio/Index";
            }
            if (model.Usuario == null || ViewData["usuario"] ==null)
            {
                //TempData["recuperarClave"] = model.RecuperarClave;
                if (model.RecuperarClave)
                {
                    if (Passwordnew == Passwordnew2)
                    {
                        conex.cambiarClave(Usuario, Passwordnew);
                        model = conex.ObtenerConexion(v_user, v_pass);
                        TempData["mensaje"] = "La clave se cambio con éxito";
                    }
                    else
                    {
                        TempData["mensaje"] = "Las claves ingresadas no coinciden";
                    }
                }

                ViewBag.Mensaje = TempData["mensaje"];
                return RedirectToAction("SmartLogin", "Login");
            }
            else
            {
                TempData["mensaje"] = "El usuario esta Vacio";
                ViewBag.Mensaje = TempData["mensaje"];
                return Redirect(model.Url);
            }
        }

        public ActionResult SidebarMenu()
        {
            /*Datos.dsTelecomTableAdapters.COMPROBANTE_PDFTableAdapter taCompr = new Datos.dsTelecomTableAdapters.COMPROBANTE_PDFTableAdapter();
            Datos.UsuarioTableadapters.*/
            taMenu.FillByUser(dsUsuario.MENU_WEB, System.Web.HttpContext.Current.Session["usuario"].ToString().ToUpper());

            List<MenuItem> list = new List<MenuItem>();
            List<MenuItem> submenu = new List<MenuItem>();

            for (int i = 0; i < dsUsuario.MENU_WEB.Count(); i++)
            {
                TaHijos.FillByPadre(dsUsuario.MENU_HIJOS, System.Web.HttpContext.Current.Session["usuario"].ToString().ToUpper(), dsUsuario.MENU_WEB[i].ID);
                string PWeb, PNombre, PClass;
                int PSubniveles;
                PWeb = dsUsuario.MENU_WEB[i].WEB;
                PNombre = dsUsuario.MENU_WEB[i].NOMBRE;
                PClass = dsUsuario.MENU_WEB[i].CLASS;
                PSubniveles = dsUsuario.MENU_WEB[i].SUBNIVELES;
                for (int j = 0; j < dsUsuario.MENU_HIJOS.Count(); j++)
                {
                    submenu.Clear();
                    string Web, Nombre, Class;
                    int Subniveles;
                    Web = dsUsuario.MENU_HIJOS[j].WEB;
                    Nombre = dsUsuario.MENU_HIJOS[j].NOMBRE;
                    Class = dsUsuario.MENU_HIJOS[j].CLASS;
                    Subniveles = dsUsuario.MENU_HIJOS[j].SUBNIVELES;
                    submenu.Add(new MenuItem { Link =  Web, LinkName = Nombre, Class = Class, SubNiveles = Subniveles});                    
                }
                if (submenu.Count() != 0)
                {
                    list.Add(new MenuItem { Link = PWeb, LinkName = PNombre, Class = PClass, SubNiveles = PSubniveles, SubMenu = submenu });                   
                }
                else
                {
                    list.Add(new MenuItem { Link = PWeb, LinkName = PNombre, Class = PClass, SubNiveles = PSubniveles });
                }
            }

            //list.Add(new MenuItem { Link = "/Inicio/index", LinkName = "Inicio", Class = "fa fa-home", SubNiveles = 0 });
            //submenu.Add(new MenuItem { Link = "/Facturacion", LinkName = "Operaciones de Caja", Class = "fa fa-circle-o", SubNiveles = 0 });
            //submenu.Add(new MenuItem { Link = "/PruebaServicio", LinkName = "Prueba Servicio", Class = "fa fa-circle-o", SubNiveles = 0 });
            //list.Add(new MenuItem { Link = "/Facturacion", LinkName = "Caja", Class = "fa fa-dashboard", SubNiveles = 2, SubMenu = submenu });

            return PartialView("SidebarMenu",list);
        }
    }
}