using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Caja.Datos;
using Caja.Models;
using Oracle.ManagedDataAccess.Client;
using Caja.Class;
using CrystalDecisions;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.IO;
using BarcodeLib;
using System.Drawing.Imaging;
using System.Data;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Caja.Controllers
{
    public class FacturacionController : Controller
    {
        decimal Suc, CodPtoVta, CodigoTipoComprobante, CotizacionMoneda,
            ImporteOtrosTributos, ImporteSubtotal,
            ItemsItemCantidad, NUMEROComprobante, NUMEROPuntoVenta, SubTotalesItemCantidad, ImporteTotal;
        int CajaNum, Turno, CajaEstado, CodigoConcepto, OtrosTributosItemCantidad, CompAsociadosItemCantidad, CodigoTipoDocumento,
            TIPO_COMPROBANTE, Emision, P_Numero, TIPO_PEDIDO, bandera, PEDIDO_NUMERO, P_ID, SumaPar, SumaImpar, ProductoSum,
            DivEntera, DigVerificador, TipoCompAsos, EmisionCompAsos, NumCompAsos;
        long valor, P_ID_VENTA;
        Int64 chbSelect;
        string FechaApertura, CodigoMoneda, FEchaEmision, FEchaservicioDesde, FEchaServicioHasta, FEchaVencimiento, FEchaVencimientoPago,
            NUMERODocumento, Obervaciones, MENSAJE, P_FEchaEmision, P_NUMERODocumento, Decimales, Referencia, totLetras,
            RetAfip, DirVacia;
        string v_empresa_inicio, v_empresa_abrev, v_empresa_nomb, v_empresa_cuit, v_empresacat_iva, v_empresa_ing_bru, v_fe_ubicacion_cert, v_fe_clave;
        int v_codigo_empresa;
        double v_fe_cuit;

        double ImporteGravado, ImporteNoGravado, ImporteExento, P_TOTAL;
        String sConnectionString = System.Web.HttpContext.Current.Session["Connection"] as string;
        byte[] CodigoBarras;

        DataSet dsSuc = new DataSet();
        DataSet dsPto = new DataSet();

        public Class.Conv converter = new Class.Conv();

        Datos.dsTelecomTableAdapters.COMPROBANTE_PDFTableAdapter taCompr = new Datos.dsTelecomTableAdapters.COMPROBANTE_PDFTableAdapter();
        Datos.dsTelecomTableAdapters.OPERACIONES_CAJATableAdapter taOpCaja = new Datos.dsTelecomTableAdapters.OPERACIONES_CAJATableAdapter();
        Datos.dsTelecomTableAdapters.SUCURSALESTableAdapter taSucursal = new Datos.dsTelecomTableAdapters.SUCURSALESTableAdapter();
        Datos.dsTelecomTableAdapters.PTOS_VTATableAdapter taPtoVta = new Datos.dsTelecomTableAdapters.PTOS_VTATableAdapter();
        Datos.dsTelecomTableAdapters.CajaNumeroTableAdapter taCajaNum = new Datos.dsTelecomTableAdapters.CajaNumeroTableAdapter();
        Datos.dsTelecomTableAdapters.CajaEncabezadoTableAdapter taEncabezado = new Datos.dsTelecomTableAdapters.CajaEncabezadoTableAdapter();
        Datos.dsTelecomTableAdapters.USUARIOSTableAdapter taUser = new Datos.dsTelecomTableAdapters.USUARIOSTableAdapter();
        Datos.dsTelecomTableAdapters.FE_VENTA_ITEMSTableAdapter taItemsVta = new Datos.dsTelecomTableAdapters.FE_VENTA_ITEMSTableAdapter();
        Datos.dsTelecomTableAdapters.EMPRESASTableAdapter taEmpresas = new Datos.dsTelecomTableAdapters.EMPRESASTableAdapter();
        Datos.dsTelecomTableAdapters.CAJA_JERARQUIATableAdapter taJerarquia = new Datos.dsTelecomTableAdapters.CAJA_JERARQUIATableAdapter();

        Datos.dsTelecom dsTelecom = new Datos.dsTelecom();

        List<FacturacionViewModel.OpCaja> GridOperaciones = new List<FacturacionViewModel.OpCaja>();
        List<FacturacionViewModel.Sucursal> Sucursal = new List<FacturacionViewModel.Sucursal>();
        List<FacturacionViewModel.PtosVta> PtoVta = new List<FacturacionViewModel.PtosVta>();

        public int V_codigo_empresa
        {
            get
            {
                return v_codigo_empresa;
            }

            set
            {
                v_codigo_empresa = value;
            }
        }

        void CargaDatosCaja(int CajaNum)
        {
            try
            {
                double TotEfectivo = 0, TotTarjeta = 0, TotTotal;
                //Consulta

                taOpCaja.FillByNumCaja(dsTelecom.OPERACIONES_CAJA, CajaNum);
                for (int i = 0; i < dsTelecom.OPERACIONES_CAJA.Count(); i++)
                {
                    GridOperaciones.Add(new
                    FacturacionViewModel.OpCaja
                    {
                        Id = dsTelecom.OPERACIONES_CAJA[i].ID,
                        Fecha = dsTelecom.OPERACIONES_CAJA[i].FECHA.ToString("dd/MM/yyyy"),
                        Numero = int.Parse(dsTelecom.OPERACIONES_CAJA[i].NUMERO),
                        Abreviatura = dsTelecom.OPERACIONES_CAJA[i].ABREVIATURA.ToString(),
                        Descripcion = dsTelecom.OPERACIONES_CAJA[i].DESCRIPCION.ToString(),
                        Efectivo = double.Parse(dsTelecom.OPERACIONES_CAJA[i].EFECTIVO.ToString()),
                        Tarjeta = double.Parse(dsTelecom.OPERACIONES_CAJA[i].TARJETA.ToString()),
                        Total = double.Parse(dsTelecom.OPERACIONES_CAJA[i].TOTAL.ToString()),
                    });
                    TotEfectivo = TotEfectivo + double.Parse(dsTelecom.OPERACIONES_CAJA[i].EFECTIVO.ToString());
                    TotTarjeta = TotTarjeta + double.Parse(dsTelecom.OPERACIONES_CAJA[i].TARJETA.ToString());
                }
                TotTotal = TotTarjeta + TotEfectivo;
                ViewData["Efectivo"] = TotEfectivo;
                ViewData["Tarjeta"] = TotTarjeta;
                ViewData["TotTotal"] = TotTotal;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void PermisoNotaC()
        {
            try
            {
                //Permisos Usuario
                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();

                OracleCommand Scmd = new OracleCommand();
                Scmd.Connection = conn;
                Scmd.CommandText = "TELECOM.SECURITY_PACK.tiene_permiso_n";
                Scmd.CommandType = System.Data.CommandType.StoredProcedure;
                Scmd.Parameters.Add("returnVal", OracleDbType.Int64, 999999);
                Scmd.Parameters["returnVal"].Direction = System.Data.ParameterDirection.ReturnValue;
                Scmd.Parameters.Add("pid_funcion", 28);
                Scmd.Parameters.Add("pusuario", System.Web.HttpContext.Current.Session["usuario"].ToString().ToUpper());

                Scmd.ExecuteNonQuery();
                int NCPermiso = int.Parse(Scmd.Parameters["returnVal"].Value.ToString());
                ViewData["NCPermiso"] = NCPermiso;

                conn.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        void CargaSuc()
        {
            try
            {
                PermisoNotaC();

                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();
                OracleCommand fscmd = new OracleCommand();
                fscmd.Connection = conn;
                fscmd.CommandText = "Select CODIGO, NOMBRE from Telecom.Sucursales SU where Inhabilitado is null AND TELECOM.SECURITY_PACK.VALIDAR_FILTRO(:Usuario, 'CODIGO_SUP_GES', SU.CODIGO) = 1";
                fscmd.CommandType = System.Data.CommandType.Text;
                fscmd.Parameters.Add("?Usuario", OracleDbType.NVarchar2, 255);
                fscmd.Parameters["?Usuario"].Value = System.Web.HttpContext.Current.Session["usuario"].ToString().ToUpper();
                fscmd.ExecuteNonQuery();

                conn.Close();

                OracleDataAdapter da = new OracleDataAdapter(fscmd);
                da.Fill(dsSuc, "Sucursal");

                Array DatoSuc = dsSuc.Tables["Sucursal"].Rows[0].ItemArray;
                Suc = decimal.Parse(DatoSuc.GetValue(0).ToString());
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void CargaddlSuc()
        {
            try
            {
                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();
                OracleCommand fscmd = new OracleCommand();
                fscmd.Connection = conn;
                fscmd.CommandText = "Select CODIGO, NOMBRE from Telecom.Sucursales SU where Inhabilitado is null AND TELECOM.SECURITY_PACK.VALIDAR_FILTRO(:Usuario, 'CODIGO_SUP_GES', SU.CODIGO) = 1";
                fscmd.CommandType = System.Data.CommandType.Text;
                fscmd.Parameters.Add("?Usuario", OracleDbType.NVarchar2, 255);
                fscmd.Parameters["?Usuario"].Value = System.Web.HttpContext.Current.Session["usuario"].ToString().ToUpper();
                fscmd.ExecuteNonQuery();

                conn.Close();

                OracleDataAdapter da = new OracleDataAdapter(fscmd);
                da.Fill(dsSuc, "Sucursal");

                //Suc y Pto Vta Usuario
                //Suc = dsTelecom.USUARIOS[0].CODIGO_SUP_GES;
                //Session["Sucursal"] = Suc;
                //CodPtoVta = dsTelecom.USUARIOS[0].PTO_VTA;
                //Session["PtoVta"] = CodPtoVta;
                //DropDown Sucursal                
                Array DatoSuc = dsSuc.Tables["Sucursal"].Rows[0].ItemArray;
                Suc = decimal.Parse(DatoSuc.GetValue(0).ToString());

                taSucursal.Fill(dsTelecom.SUCURSALES);
                var modelo = new FacturacionViewModel.Sucursal();
                modelo.LstSucursal = new List<SelectListItem>();
                List<SelectListItem> Sucursales = new List<SelectListItem>();
                foreach (DataRow drow in dsSuc.Tables["Sucursal"].Rows)
                {
                    modelo.LstSucursal.Add(new SelectListItem() { Text = drow[1].ToString(), Value = drow[0].ToString() });
                }
                //ViewData["TiposDocumentos"] = TipoDocumentos;
                ViewData["Sucursales"] = modelo.LstSucursal;
                ViewBag.Sucursal = modelo.LstSucursal;
            }
            catch (Exception)
            {

                throw;
            }
        }
        void CargaddlPtoVta(int Suc)
        {
            try
            {
                //PtoVta
                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();
                OracleCommand fpcmd = new OracleCommand();
                fpcmd.Connection = conn;
                fpcmd.CommandText = "select CODIGO, NOMBRE from telecom.ptos_vta pto inner join TELECOM.USUARIOS us on us.USUARIO = :Usuario where pto.Sucursal = :Sucursal and pto.CATEGORIA_CODIGO = 1 and pto.INHABILITADO is null and(TELECOM.SECURITY_PACK.VALIDAR_FILTRO(:Usuario, 'PTO_VTA', pto.CODIGO) = 1 or us.PTO_VTA is null)";
                fpcmd.CommandType = System.Data.CommandType.Text;
                fpcmd.Parameters.Add("?Usuario", OracleDbType.NVarchar2, 255);
                fpcmd.Parameters["?Usuario"].Value = System.Web.HttpContext.Current.Session["usuario"].ToString().ToUpper();
                fpcmd.Parameters.Add("?Sucursal", OracleDbType.Int64, 999999);
                fpcmd.Parameters["?Sucursal"].Value = Suc;

                fpcmd.ExecuteNonQuery();

                OracleDataAdapter daPto = new OracleDataAdapter(fpcmd);
                daPto.Fill(dsPto, "Ptoventa");

                conn.Close();

                //taPtoVta.FillBySucursal(dsTelecom.PTOS_VTA, Suc); Busca ptos de ventas filtrado por sucursal
                var modPtoVta = new FacturacionViewModel.PtosVta();
                modPtoVta.lstPtoVta = new List<SelectListItem>();

                //Recorre dstelecom y carga ptos de vta en ddl
                foreach (DataRow drow in dsPto.Tables["Ptoventa"].Rows)
                {
                    modPtoVta.lstPtoVta.Add(new SelectListItem() { Text = drow[1].ToString(), Value = drow[0].ToString() });
                }

                ViewBag.PtosVta = modPtoVta.lstPtoVta;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }
        void CEncabezadoCaja(int NumeroCaja)
        {
            CajaNum = NumeroCaja;
            taEncabezado.FillByCaja(dsTelecom.CajaEncabezado, CajaNum);
            Turno = int.Parse(dsTelecom.CajaEncabezado[0].TURNO.ToString());
            FechaApertura = dsTelecom.CajaEncabezado[0].FECHA_APERTURA.ToString("dd/MM/yyyy");
            CajaEstado = dsTelecom.CajaEncabezado[0].ESTADO;
            //Cargar Controles cn info
            ViewData["FechaApertura"] = FechaApertura;
            ViewData["NroCaja"] = CajaNum;
            ViewData["EstadoCaja"] = CajaEstado;
            ViewData["TurnoCaja"] = Turno;
        }
        void CargarEncabezado(int PtosVta)
        {
            try
            {
                taCajaNum.FillByPtoVta(dsTelecom.CajaNumero, PtosVta);

                if (!dsTelecom.CajaNumero[0].IsNull(0))
                {
                    CajaNum = int.Parse(dsTelecom.CajaNumero[0]._MAX_C_NUMERO_.ToString());
                    taEncabezado.FillByCaja(dsTelecom.CajaEncabezado, CajaNum);
                    Turno = int.Parse(dsTelecom.CajaEncabezado[0].TURNO.ToString());
                    FechaApertura = dsTelecom.CajaEncabezado[0].FECHA_APERTURA.ToString("dd/MM/yyyy");
                    CajaEstado = dsTelecom.CajaEncabezado[0].ESTADO;
                    //Cargar Controles cn info
                    ViewData["FechaApertura"] = FechaApertura;
                    ViewData["NroCaja"] = CajaNum;
                    ViewData["EstadoCaja"] = CajaEstado;
                    ViewData["TurnoCaja"] = Turno;
                }
                else
                {
                    CajaNum = 0;
                    Turno = 0;
                    FechaApertura = null;
                    CajaEstado = 0;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        void ddlSeleccion(int? PtosVta, int? Sucursal, int? txtNum)
        {
            //Permisos Usuario
            PermisoNotaC();

            //DropDown Sucursal
            //Permiso usuario Filtro
            //Sucursal

            CargaddlSuc();

            if (Sucursal != null)
            {
                ViewBag.SelectedItem = Sucursal;
            }
            else
            {
                ViewBag.SelectedItem = int.Parse(Session["Sucursal"].ToString());
            }

            //DropDownPtoVta
            CargaddlPtoVta(ViewBag.SelectedItem);
            if (PtosVta != null)
            {
                ViewBag.SelectItemPto = PtosVta;
            }
            else
            {
                PtosVta = int.Parse(Session["PtoVta"].ToString());
                ViewBag.SelectItemPto = PtosVta;
            }
            //taCajaNum.FillByPtoVta(dsTelecom.CajaNumero, PtosVta);
            ////Carjar Encabezado
            //CargarEncabezado(ViewBag.SelectItemPto);
            CajaNum = int.Parse(txtNum.ToString());
            CargaDatosCaja(CajaNum);
            CEncabezadoCaja(CajaNum);
        }

        void VerificaBloqueo(int TIPO_PEDIDO, int PEDIDO_NUMERO)
        {
            try
            {
                if (TIPO_PEDIDO == 1)
                {
                    OracleConnection cnx = new OracleConnection(sConnectionString);
                    cnx.Open();

                    OracleCommand Upd = new OracleCommand("update GESTION.VENTAS@ACCESORIOS.SERVERORAC.CBB.NET ife set ife.INTENTO_FE = 1 where ife.ID = :PedidoNum", cnx);
                    Upd.CommandType = System.Data.CommandType.Text;
                    Upd.Parameters.Add("?PedidoNum", OracleDbType.Int64, 999999999);
                    Upd.Parameters["?PedidoNum"].Value = PEDIDO_NUMERO;
                    Upd.ExecuteNonQuery();
                    cnx.Close();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Substring(4, 5) == "02049")
                {

                    TempData["MsjError"] = "El pedido se encuentra bloqueado. Verifique que no se encuentre tomado por otro usuario o aplicacion";
                    //ViewBag.ShowMessage = true;
                }
                else
                {
                    TempData["MsjError"] = ex.Message;
                    //ViewBag.ShowMessage = true;
                }

                throw;
            }

        }        

        [HttpGet]
        // GET: Facturacion
        public ActionResult Index(FacturacionViewModel.OpCaja list, FacturacionViewModel.Sucursal lstSuc)
        {
            LoginController lo = new LoginController();
            lo.SmartLogin("~/Facturacion/Index", "VALIDA");

            ViewBag.ShowMessage = false;
            ViewBag.Correo = false;

            if (System.Web.HttpContext.Current.Session["usuario"] != null)
            {
                try
                {
                    taUser.FillByUs(dsTelecom.USUARIOS, System.Web.HttpContext.Current.Session["usuario"].ToString().ToUpper());
                    //Permisos Usuario Nota Credito                    
                    PermisoNotaC();

                    //Permiso usuario Filtro
                    //Sucursal
                    CargaddlSuc();

                    //DropDownPtoVta
                    CargaddlPtoVta(int.Parse(Suc.ToString()));

                    //Seleccionar 1° item ptovta
                    Array DatoPto = dsPto.Tables["Ptoventa"].Rows[0].ItemArray;
                    CodPtoVta = decimal.Parse(DatoPto.GetValue(0).ToString());
                    TempData["CodPtoVta"] = CodPtoVta;

                    taCajaNum.FillByPtoVta(dsTelecom.CajaNumero, CodPtoVta);

                    CajaNum = int.Parse(dsTelecom.CajaNumero[0]._MAX_C_NUMERO_.ToString());
                    CEncabezadoCaja(CajaNum);
                    CargaDatosCaja(CajaNum);

                    if (TempData["MsjError"] != null)
                    {
                        ViewBag.Mensaje = TempData["MsjError"].ToString();
                    }
                    return View(GridOperaciones);
                }
                catch (Exception ex)
                {
                    return View(ex.Message);
                }
            }
            else
            {
                return lo.VerificaConexion("~/Inicio/Index");
            }
        }

        [HttpPost]
        public ActionResult SigCaja(int? Sucursal, int txtNum)
        {
            try
            {
                CodPtoVta = int.Parse(TempData["CodPtoVta"].ToString());
                TempData["CodPtoVta"] = CodPtoVta;
                CargaSuc();
                CargaddlSuc();
                CargaddlPtoVta(int.Parse(Sucursal.ToString()));
                taCajaNum.FillByPtoVta(dsTelecom.CajaNumero, CodPtoVta);
                if (txtNum != dsTelecom.CajaNumero[0]._MAX_C_NUMERO_)
                {
                    taCajaNum.FillSiguiente(dsTelecom.CajaNumero, CodPtoVta, txtNum);
                    CargaDatosCaja(int.Parse(dsTelecom.CajaNumero[0]._MAX_C_NUMERO_.ToString()));
                }
                else
                {
                    CargaDatosCaja(int.Parse(dsTelecom.CajaNumero[0]._MAX_C_NUMERO_.ToString()));
                    TempData["MsjError"] = "No existe siguiente caja para el punto de venta seleccionado!";
                }
                CEncabezadoCaja(int.Parse(dsTelecom.CajaNumero[0]._MAX_C_NUMERO_.ToString()));
                return View("Index", GridOperaciones);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ActionResult AntCaja(int? Sucursal, int txtNum)
        {
            try
            {
                CodPtoVta = int.Parse(TempData["CodPtoVta"].ToString());
                TempData["CodPtoVta"] = CodPtoVta;
                //CargaSuc();
                PermisoNotaC();
                CargaddlSuc();
                CargaddlPtoVta(int.Parse(Sucursal.ToString()));
                taCajaNum.FillAnterior(dsTelecom.CajaNumero, CodPtoVta, txtNum);
                CEncabezadoCaja(int.Parse(dsTelecom.CajaNumero[0]._MAX_C_NUMERO_.ToString()));
                CargaDatosCaja(int.Parse(dsTelecom.CajaNumero[0]._MAX_C_NUMERO_.ToString()));
                return View("Index", GridOperaciones);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult btnBusqueda(int? PtosVta, int? Sucursal, int txtNum)
        {
            try
            {
                //taJerarquia.FillPtoSuc(dsTelecom.CAJA_JERARQUIA, txtNum);
                //Sucursal = int.Parse(dsTelecom.CAJA_JERARQUIA[0].SUCURSAL_CODIGO.ToString());
                //PtosVta = int.Parse(dsTelecom.CAJA_JERARQUIA[0].PTO_VTA_CODIGO.ToString());

                ddlSeleccion(PtosVta, Sucursal, txtNum);
                return View("Index", GridOperaciones);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public ActionResult btnBuscar(string submitButton, int? PtosVta, int? Sucursal, int txtNum, FormCollection form)
        {
            try
            {
                switch (submitButton)
                {
                    case "SigCaja":
                        // delegate sending to another controller action
                        return (SigCaja(Sucursal, txtNum));
                    case "AntCaja":
                        //    // call another action to perform the cancellation
                        return (AntCaja(Sucursal, txtNum));
                    case "Buscar":
                        return (btnBusqueda(PtosVta, Sucursal, txtNum));
                    case "Cobrar":
                        // delegate sending to another controller action
                        return (Cobrar(form, PtosVta, Sucursal, txtNum));
                    case "Nota de Credito":
                        // call another action to perform the cancellation
                        return (NotaCredito(form, PtosVta, Sucursal, txtNum));
                    case "Imprimir":
                        return (Imprimir(form, PtosVta, Sucursal));
                    case "EnvMail":
                        return (EnviarCorreo(form, PtosVta, Sucursal, txtNum));
                    default:
                        //    // If they've submitted the form without a submitButton, 
                        //    // just return the view again.
                        return RedirectToAction("Index", "Facturacion");
                }
            }
            catch (Exception ex)
            {

                if (ex.Message.ToString().Equals("OracleConnection.ConnectionString no es válido"))
                {
                    return RedirectToAction("SmartLogin", "Login");
                }
                else
                {
                    return RedirectToAction("Index", "Facturacion");
                }
            }
        }

        public JsonResult getstate(int id)
        {
            try
            {
                Suc = (decimal)id;

                taPtoVta.FillBySucursal(dsTelecom.PTOS_VTA, Suc);
                var modPtovta = new FacturacionViewModel.PtosVta();
                modPtovta.lstPtoVta = new List<SelectListItem>();
                //List<SelectListItem> PuntosVta = new List<SelectListItem>();
                for (int i = 0; i < dsTelecom.PTOS_VTA.Count; i++)
                {
                    modPtovta.lstPtoVta.Add(new SelectListItem() { Text = dsTelecom.PTOS_VTA[i].NOMBRE, Value = dsTelecom.PTOS_VTA[i].CODIGO.ToString() });
                }
                ViewBag.PtoVta = modPtovta.lstPtoVta;
                CodPtoVta = dsTelecom.PTOS_VTA[0].CODIGO;
                //CargarEncabezado(int.Parse(CodPtoVta.ToString()));               

                return Json(new SelectList(modPtovta.lstPtoVta, "Value", "Text", JsonRequestBehavior.AllowGet));
            }
            catch (Exception)
            {

                throw;
            }
        }

        public JsonResult getPtosVta(int Sucursal)
        {
            Suc = (decimal)Sucursal;
            Session["Sucursal"] = Suc;
            taPtoVta.FillBySucursal(dsTelecom.PTOS_VTA, Suc);
            var modDatosCaja = new FacturacionViewModel.DatosCaja();
            var lstDatosCaja = new List<Caja.Models.FacturacionViewModel.DatosCaja>();
            for (int j = 0; j < dsTelecom.PTOS_VTA.Count; j++)
            {
                CargarEncabezado(dsTelecom.PTOS_VTA[j].CODIGO);
                lstDatosCaja.Add(new FacturacionViewModel.DatosCaja() { PtoVta = dsTelecom.PTOS_VTA[j].CODIGO, CajaNum = CajaNum, FechaApertura = FechaApertura, EstadoCaja = CajaEstado, Turno = Turno });
            }
            Session["PtoVenta"] = dsTelecom.PTOS_VTA[0].CODIGO;
            return Json(lstDatosCaja, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getcaja(int id)
        {
            CodPtoVta = (decimal)id;
            Session["PtoVenta"] = CodPtoVta;
            TempData["CodPtoVta"] = CodPtoVta;
            CargarEncabezado(int.Parse(CodPtoVta.ToString()));
            var modDatosCaja = new FacturacionViewModel.DatosCaja() { CajaNum = CajaNum, EstadoCaja = CajaEstado, FechaApertura = FechaApertura, Turno = Turno };
            return Json(modDatosCaja, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Cobrar(FormCollection form, int? PtosVta, int? Sucursal, int txtNum)
        {
            /*String v_user = System.Web.HttpContext.Current.Session["usuario"] as String;
            String v_pass = System.Web.HttpContext.Current.Session["password"] as String;
            String sConnectionString = "DATA SOURCE=TELECOM;PASSWORD=" + v_pass + ";USER ID=" + v_user;*/
            //String sConnectionString = "DATA SOURCE=TELECOM;PASSWORD=;USER ID=TELECOM";
            ViewBag.ShowMessage = true;
            ddlSeleccion(PtosVta, Sucursal, txtNum);

            var fac = new Caja.Models.FacturacionViewModel.FacElectronica();
            try
            {
                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();

                var ch = form.GetValues("assignChkBx");
                if (ch != null)
                {
                    for (int i = 0; i < ch.Count(); i++)
                    {
                        //Prueba
                        //OracleCommand cmd = new OracleCommand("TELECOM.prueba_aldo2.PRUEBA_ALDO2", conn);
                        //cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        //cmd.Parameters.Add("PARAMETRO", Int64.Parse(ch[i]));
                        //cmd.Parameters.Add("SALIDA", OracleDbType.Char, 1, 2, System.Data.ParameterDirection.Output);

                        ////Permite FE                      
                        OracleCommand cmd = new OracleCommand("TELECOM.Facturacion_Electronica.permite_FE", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("P_comprobante_caja_id", Int64.Parse(ch[i]));
                        cmd.Parameters.Add("P_CodigoConcepto", OracleDbType.Double, 1, 2, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CodigoMoneda", OracleDbType.Varchar2, 255, 3, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CodigoTipoComprobante", OracleDbType.Double, 1, 4, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CodigoTipoDocumento", OracleDbType.Double, 1, 5, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CompAsociadosItemCantidad", OracleDbType.Double, 1, 6, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CotizacionMoneda", OracleDbType.Double, 1, 7, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaEmision", OracleDbType.Varchar2, 255, 8, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaservicioDesde", OracleDbType.Varchar2, 255, 9, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaServicioHasta", OracleDbType.Varchar2, 255, 10, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaVencimiento", OracleDbType.Varchar2, 255, 11, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaVencimientoPago", OracleDbType.Varchar2, 255, 12, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteExento", OracleDbType.Double, 1, 13, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteGravado", OracleDbType.Double, 1, 14, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteNoGravado", OracleDbType.Double, 1, 15, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteOtrosTributos", OracleDbType.Double, 1, 16, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteSubtotal", OracleDbType.Double, 1, 17, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteTotal", OracleDbType.Double, 1, 18, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ItemsItemCantidad", OracleDbType.Double, 1, 19, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_NUMEROComprobante", OracleDbType.Double, 1, 20, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_NUMERODocumento", OracleDbType.Varchar2, 255, 21, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_NUMEROPuntoVenta", OracleDbType.Double, 1, 22, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_Obervaciones", OracleDbType.Varchar2, 255, 23, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_OtrosTributosItemCantidad", OracleDbType.Double, 1, 24, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_SubTotalesItemCantidad", OracleDbType.Double, 1, 25, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 26, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_PEDIDO_NUMERO", OracleDbType.Double, 1, 27, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_TIPO_PEDIDO", OracleDbType.Double, 1, 28, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_BANDERA", OracleDbType.Double, 1, 29, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CODIGO_EMPRESA", OracleDbType.Double, 1, 30, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_ABREV", OracleDbType.Varchar2, 255, 31, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_NOMB", OracleDbType.Varchar2, 255, 32, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_CUIT", OracleDbType.Varchar2, 255, 33, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_CAT_IVA", OracleDbType.Varchar2, 255, 34, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_ING_BRU", OracleDbType.Varchar2, 255, 35, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_INCIO", OracleDbType.Varchar2, 255, 36, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FE_CUIT", OracleDbType.Double, 1, 37, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FE_CLAVE", OracleDbType.Varchar2, 255, 38, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FE_UBICACION_CERT", OracleDbType.Varchar2, 255, 39, System.Data.ParameterDirection.Output);

                        cmd.ExecuteNonQuery();

                        bandera = int.Parse(cmd.Parameters["P_BANDERA"].Value.ToString());
                        if (bandera == 1)
                        {

                            PEDIDO_NUMERO = int.Parse(cmd.Parameters["P_PEDIDO_NUMERO"].Value.ToString());
                            TIPO_PEDIDO = int.Parse(cmd.Parameters["P_TIPO_PEDIDO"].Value.ToString());
                            Emision = int.Parse(cmd.Parameters["P_NUMEROPuntoVenta"].Value.ToString());
                            P_Numero = int.Parse(cmd.Parameters["P_NUMEROComprobante"].Value.ToString());
                            TIPO_COMPROBANTE = int.Parse(cmd.Parameters["P_CodigoTipoComprobante"].Value.ToString());
                            Decimales = cmd.Parameters["P_ImporteTotal"].Value.ToString();
                            P_TOTAL = Convert.ToDouble(Decimales.Replace(".", ","));
                            CodigoConcepto = int.Parse(cmd.Parameters["P_CodigoConcepto"].Value.ToString());
                            P_FEchaEmision = cmd.Parameters["P_FEchaEmision"].Value.ToString();
                            CodigoTipoDocumento = int.Parse(cmd.Parameters["P_CodigoTipoDocumento"].Value.ToString());
                            P_NUMERODocumento = cmd.Parameters["P_NUMERODocumento"].Value.ToString();
                            OtrosTributosItemCantidad = int.Parse(cmd.Parameters["P_OtrosTributosItemCantidad"].Value.ToString());
                            CompAsociadosItemCantidad = int.Parse(cmd.Parameters["P_CompAsociadosItemCantidad"].Value.ToString());
                            Decimales = cmd.Parameters["P_ImporteGravado"].Value.ToString();
                            ImporteGravado = Convert.ToDouble(Decimales.Replace(".", ","));
                            Decimales = cmd.Parameters["P_ImporteNoGravado"].Value.ToString();
                            ImporteNoGravado = Convert.ToDouble(Decimales.Replace(".", ","));
                            Decimales = cmd.Parameters["P_ImporteExento"].Value.ToString();
                            ImporteExento = Convert.ToDouble(Decimales.Replace(".", ","));
                            FEchaservicioDesde = cmd.Parameters["P_FEchaservicioDesde"].Value.ToString();
                            FEchaServicioHasta = cmd.Parameters["P_FEchaServicioHasta"].Value.ToString();
                            FEchaVencimiento = cmd.Parameters["P_FEchaVencimiento"].Value.ToString();
                            FEchaVencimientoPago = cmd.Parameters["P_FEchaVencimientoPago"].Value.ToString();
                            V_codigo_empresa = int.Parse(cmd.Parameters["P_CODIGO_EMPRESA"].Value.ToString());
                            v_empresa_abrev = cmd.Parameters["P_EMPRESA_ABREV"].Value.ToString();
                            v_empresa_nomb = cmd.Parameters["P_EMPRESA_NOMB"].Value.ToString();
                            v_empresa_cuit = cmd.Parameters["P_EMPRESA_CUIT"].Value.ToString();
                            v_empresacat_iva = cmd.Parameters["P_EMPRESA_CAT_IVA"].Value.ToString();
                            v_empresa_ing_bru = cmd.Parameters["P_EMPRESA_ING_BRU"].Value.ToString();
                            v_empresa_inicio = cmd.Parameters["P_EMPRESA_INCIO"].Value.ToString();
                            Decimales = cmd.Parameters["P_FE_CUIT"].Value.ToString();
                            v_fe_cuit = Convert.ToDouble(Decimales.Replace(".", ","));
                            v_fe_clave = cmd.Parameters["P_FE_CLAVE"].Value.ToString();
                            v_fe_ubicacion_cert = cmd.Parameters["P_FE_UBICACION_CERT"].Value.ToString();

                            //Proc Verifica Bloqueo
                            VerificaBloqueo(TIPO_PEDIDO, PEDIDO_NUMERO);

                            //Guardar Solicitud
                            OracleCommand gs = new OracleCommand("TELECOM.Facturacion_electronica.GUARDAR_SOLICITUD", conn);
                            gs.CommandType = System.Data.CommandType.StoredProcedure;
                            gs.Parameters.Add("P_COMPROBANTE_CAJA_ID", Int64.Parse(ch[i]));
                            gs.Parameters.Add("P_TIPO_PEDIDO", TIPO_PEDIDO);
                            gs.Parameters.Add("P_PEDIDO_NUMERO", PEDIDO_NUMERO);
                            gs.Parameters.Add("P_EMISION", Emision);
                            gs.Parameters.Add("P_NUMERO", P_Numero);
                            gs.Parameters.Add("P_TIPO_COMPROBANTE", TIPO_COMPROBANTE);
                            gs.Parameters.Add("P_TOTAL", P_TOTAL);
                            gs.Parameters.Add("P_ID", OracleDbType.Long, 9999999, 8, System.Data.ParameterDirection.Output);
                            gs.Parameters.Add("P_TIPO_DOC", CodigoTipoDocumento);
                            if (P_NUMERODocumento == "null")
                            {
                                gs.Parameters.Add("P_DOC_NRO", null);
                            }
                            else
                            {
                                gs.Parameters.Add("P_DOC_NRO", P_NUMERODocumento);
                            }
                            gs.ExecuteNonQuery();
                            P_ID = int.Parse(gs.Parameters["P_ID"].Value.ToString());

                            //CLASE AFIP
                            var FE = new Class.Afip();
                            RetAfip = FE.CAE(TIPO_COMPROBANTE, Emision, P_Numero, CodigoConcepto, P_FEchaEmision, CodigoTipoDocumento,
                                P_NUMERODocumento, P_TOTAL, PEDIDO_NUMERO, TIPO_PEDIDO, OtrosTributosItemCantidad, CompAsociadosItemCantidad, ImporteGravado, ImporteNoGravado, ImporteExento, FEchaservicioDesde, FEchaServicioHasta, FEchaVencimiento, FEchaVencimientoPago, int.Parse(ch[i]), P_ID,
                                V_codigo_empresa, v_empresa_inicio, v_empresa_abrev, v_empresa_nomb, v_empresa_cuit, v_empresacat_iva, v_empresa_ing_bru, v_fe_ubicacion_cert, v_fe_clave, v_fe_cuit);

                            if (RetAfip == "Resultado Verdadero")
                            {
                                //Datos PDF
                                OracleCommand of = new OracleCommand("TELECOM.Facturacion_electronica.OBTENER_FACTURA", conn);
                                of.CommandType = System.Data.CommandType.StoredProcedure;
                                of.Parameters.Add("P_COMPROBANTE_CAJA_ID", Int64.Parse(ch[i]));
                                of.Parameters.Add("P_ID_VENTA", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
                                of.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 3, System.Data.ParameterDirection.Output);
                                of.Parameters.Add("p_descripcion", OracleDbType.Varchar2, 255, 4, System.Data.ParameterDirection.Output);
                                of.ExecuteNonQuery();
                                P_ID_VENTA = long.Parse(of.Parameters["P_ID_VENTA"].Value.ToString());
                                MENSAJE = of.Parameters["P_MENSAJE"].Value.ToString();
                                Referencia = of.Parameters["p_descripcion"].Value.ToString();
                                conn.Close();

                                //crear procedimiento
                                //return GenerarPDF(P_ID_VENTA, PEDIDO_NUMERO, TIPO_PEDIDO, Referencia);
                                return RedirectToAction("GenerarPDF", "Facturacion", new { p_id_venta = P_ID_VENTA, P_PEDIDO_NUMERO = PEDIDO_NUMERO, P_TIPO_PEDIDO = TIPO_PEDIDO, P_Referencia = Referencia });
                            }
                            else
                            {
                                MENSAJE = RetAfip;
                                TempData["MsjError"] = MENSAJE;
                                //ViewBag.ShowMessage = true;
                            }
                        }
                        else
                        {
                            MENSAJE = cmd.Parameters["P_MENSAJE"].Value.ToString();
                            TempData["MsjError"] = MENSAJE;
                            //ViewBag.ShowMessage = true;
                        }
                    }
                }
                else
                {
                    MENSAJE = "Debe seleccionar una Operacion";
                    TempData["MsjError"] = MENSAJE;
                    //ViewBag.ShowMessage = true;
                }

                TempData["MsjError"] = MENSAJE;
                return View("Index", GridOperaciones);
            }
            catch (Exception ex)
            {

                return View("Index", GridOperaciones);
            }
        }

        public ActionResult NotaCredito(FormCollection form, int? PtosVta, int? Sucursal, int txtNum)
        {
            try
            {
                ViewBag.ShowMessage = true;
                ddlSeleccion(PtosVta, Sucursal, txtNum);

                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();

                var ch = form.GetValues("assignChkBx");
                if (ch != null)
                {
                    for (int i = 0; i < ch.Count(); i++)
                    {
                        //Prueba
                        //OracleCommand cmd = new OracleCommand("TELECOM.prueba_aldo2.PRUEBA_ALDO2", conn);
                        //cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        //cmd.Parameters.Add("PARAMETRO", Int64.Parse(ch[i]));
                        //cmd.Parameters.Add("SALIDA", OracleDbType.Char, 1, 2, System.Data.ParameterDirection.Output);

                        ////Permite FE                      
                        OracleCommand cmd = new OracleCommand("TELECOM.Facturacion_Electronica.permite_nc", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.Add("P_comprobante_caja_id", Int64.Parse(ch[i]));
                        cmd.Parameters.Add("P_CodigoConcepto", OracleDbType.Double, 1, 2, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CodigoMoneda", OracleDbType.Varchar2, 255, 3, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CodigoTipoComprobante", OracleDbType.Double, 1, 4, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CodigoTipoDocumento", OracleDbType.Double, 1, 5, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CompAsociadosItemCantidad", OracleDbType.Double, 1, 6, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CotizacionMoneda", OracleDbType.Double, 1, 7, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaEmision", OracleDbType.Varchar2, 255, 8, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaservicioDesde", OracleDbType.Varchar2, 255, 9, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaServicioHasta", OracleDbType.Varchar2, 255, 10, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaVencimiento", OracleDbType.Varchar2, 255, 11, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FEchaVencimientoPago", OracleDbType.Varchar2, 255, 12, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteExento", OracleDbType.Double, 1, 13, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteGravado", OracleDbType.Double, 1, 14, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteNoGravado", OracleDbType.Double, 1, 15, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteOtrosTributos", OracleDbType.Double, 1, 16, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteSubtotal", OracleDbType.Double, 1, 17, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ImporteTotal", OracleDbType.Double, 1, 18, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_ItemsItemCantidad", OracleDbType.Double, 1, 19, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_NUMEROComprobante", OracleDbType.Double, 1, 20, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_NUMERODocumento", OracleDbType.Varchar2, 255, 21, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_NUMEROPuntoVenta", OracleDbType.Double, 1, 22, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_Obervaciones", OracleDbType.Varchar2, 255, 23, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_OtrosTributosItemCantidad", OracleDbType.Double, 1, 24, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_SubTotalesItemCantidad", OracleDbType.Double, 1, 25, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 26, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_PEDIDO_NUMERO", OracleDbType.Double, 1, 27, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_TIPO_PEDIDO", OracleDbType.Double, 1, 28, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_BANDERA", OracleDbType.Double, 1, 29, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_TIPO_COMP_ASOCIADO", OracleDbType.Double, 1, 30, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMISION_ASOCIADO", OracleDbType.Double, 1, 31, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_NUMERO_ASOCIADO", OracleDbType.Double, 1, 32, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_CODIGO_EMPRESA", OracleDbType.Double, 1, 33, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_ABREV", OracleDbType.Varchar2, 255, 34, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_NOMB", OracleDbType.Varchar2, 255, 35, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_CUIT", OracleDbType.Varchar2, 255, 36, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_CAT_IVA", OracleDbType.Varchar2, 255, 37, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_ING_BRU", OracleDbType.Varchar2, 255, 38, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_EMPRESA_INCIO", OracleDbType.Varchar2, 255, 39, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FE_CUIT", OracleDbType.Double, 1, 40, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FE_CLAVE", OracleDbType.Varchar2, 255, 41, System.Data.ParameterDirection.Output);
                        cmd.Parameters.Add("P_FE_UBICACION_CERT", OracleDbType.Varchar2, 255, 41, System.Data.ParameterDirection.Output);

                        cmd.ExecuteNonQuery();



                        bandera = int.Parse(cmd.Parameters["P_BANDERA"].Value.ToString());
                        if (bandera == 1)
                        {
                            PEDIDO_NUMERO = int.Parse(cmd.Parameters["P_PEDIDO_NUMERO"].Value.ToString());
                            TIPO_PEDIDO = int.Parse(cmd.Parameters["P_TIPO_PEDIDO"].Value.ToString());
                            Emision = int.Parse(cmd.Parameters["P_NUMEROPuntoVenta"].Value.ToString());
                            P_Numero = int.Parse(cmd.Parameters["P_NUMEROComprobante"].Value.ToString());
                            TIPO_COMPROBANTE = int.Parse(cmd.Parameters["P_CodigoTipoComprobante"].Value.ToString());
                            Decimales = cmd.Parameters["P_ImporteTotal"].Value.ToString();
                            P_TOTAL = Convert.ToDouble(Decimales.Replace(".", ","));
                            bandera = Int16.Parse(cmd.Parameters["P_BANDERA"].Value.ToString());
                            CodigoConcepto = int.Parse(cmd.Parameters["P_CodigoConcepto"].Value.ToString());
                            P_FEchaEmision = cmd.Parameters["P_FEchaEmision"].Value.ToString();
                            CodigoTipoDocumento = int.Parse(cmd.Parameters["P_CodigoTipoDocumento"].Value.ToString());
                            P_NUMERODocumento = cmd.Parameters["P_NUMERODocumento"].Value.ToString();
                            OtrosTributosItemCantidad = int.Parse(cmd.Parameters["P_OtrosTributosItemCantidad"].Value.ToString());
                            CompAsociadosItemCantidad = int.Parse(cmd.Parameters["P_CompAsociadosItemCantidad"].Value.ToString());
                            Decimales = cmd.Parameters["P_ImporteGravado"].Value.ToString();
                            ImporteGravado = Convert.ToDouble(Decimales.Replace(".", ","));
                            Decimales = cmd.Parameters["P_ImporteNoGravado"].Value.ToString();
                            ImporteNoGravado = Convert.ToDouble(Decimales.Replace(".", ","));
                            Decimales = cmd.Parameters["P_ImporteExento"].Value.ToString();
                            ImporteExento = Convert.ToDouble(Decimales.Replace(".", ","));
                            FEchaservicioDesde = cmd.Parameters["P_FEchaservicioDesde"].Value.ToString();
                            FEchaServicioHasta = cmd.Parameters["P_FEchaServicioHasta"].Value.ToString();
                            FEchaVencimiento = cmd.Parameters["P_FEchaVencimiento"].Value.ToString();
                            FEchaVencimientoPago = cmd.Parameters["P_FEchaVencimientoPago"].Value.ToString();
                            V_codigo_empresa = int.Parse(cmd.Parameters["P_CODIGO_EMPRESA"].Value.ToString());
                            v_empresa_abrev = cmd.Parameters["P_EMPRESA_ABREV"].Value.ToString();
                            v_empresa_nomb = cmd.Parameters["P_EMPRESA_NOMB"].Value.ToString();
                            v_empresa_cuit = cmd.Parameters["P_EMPRESA_CUIT"].Value.ToString();
                            v_empresacat_iva = cmd.Parameters["P_EMPRESA_CAT_IVA"].Value.ToString();
                            v_empresa_ing_bru = cmd.Parameters["P_EMPRESA_ING_BRU"].Value.ToString();
                            v_empresa_inicio = cmd.Parameters["P_EMPRESA_INCIO"].Value.ToString();
                            Decimales = cmd.Parameters["P_FE_CUIT"].Value.ToString();
                            v_fe_cuit = Convert.ToDouble(Decimales.Replace(".", ","));
                            v_fe_clave = cmd.Parameters["P_FE_CLAVE"].Value.ToString();
                            v_fe_ubicacion_cert = cmd.Parameters["P_FE_UBICACION_CERT"].Value.ToString();

                            TipoCompAsos = int.Parse(cmd.Parameters["P_TIPO_COMP_ASOCIADO"].Value.ToString());
                            EmisionCompAsos = int.Parse(cmd.Parameters["P_EMISION_ASOCIADO"].Value.ToString());
                            NumCompAsos = int.Parse(cmd.Parameters["P_NUMERO_ASOCIADO"].Value.ToString());

                            MENSAJE = "Nota de Credito realizada con exito!";
                            //TempData["MsjError"] = MENSAJE;
                            TempData["MsjError"] = System.Web.HttpContext.Current.Session["ErrorPrueba"];

                            VerificaBloqueo(TIPO_PEDIDO, PEDIDO_NUMERO);
                            //Guardar Solicitud
                            OracleCommand gs = new OracleCommand("TELECOM.Facturacion_electronica.GUARDAR_SOLICITUD", conn);
                            gs.CommandType = System.Data.CommandType.StoredProcedure;
                            gs.Parameters.Add("P_COMPROBANTE_CAJA_ID", Int64.Parse(ch[i]));
                            gs.Parameters.Add("P_TIPO_PEDIDO", TIPO_PEDIDO);
                            gs.Parameters.Add("P_PEDIDO_NUMERO", PEDIDO_NUMERO);
                            gs.Parameters.Add("P_EMISION", Emision);
                            gs.Parameters.Add("P_NUMERO", P_Numero);
                            gs.Parameters.Add("P_TIPO_COMPROBANTE", TIPO_COMPROBANTE);
                            gs.Parameters.Add("P_TOTAL", P_TOTAL);
                            gs.Parameters.Add("P_ID", OracleDbType.Long, 9999999, 8, System.Data.ParameterDirection.Output);
                            gs.Parameters.Add("P_TIPO_DOC", CodigoTipoDocumento);
                            if (P_NUMERODocumento == "null")
                            {
                                gs.Parameters.Add("P_DOC_NRO", null);
                            }
                            else
                            {
                                gs.Parameters.Add("P_DOC_NRO", P_NUMERODocumento);
                            }
                            gs.ExecuteNonQuery();
                            P_ID = int.Parse(gs.Parameters["P_ID"].Value.ToString());

                            //CLASE AFIP
                            var FE = new Class.Afip();
                            RetAfip = FE.CAE(TIPO_COMPROBANTE, Emision, P_Numero, CodigoConcepto, P_FEchaEmision, CodigoTipoDocumento,
                                P_NUMERODocumento, P_TOTAL, PEDIDO_NUMERO, TIPO_PEDIDO, OtrosTributosItemCantidad, CompAsociadosItemCantidad,
                                ImporteGravado, ImporteNoGravado, ImporteExento, FEchaservicioDesde, FEchaServicioHasta, FEchaVencimiento,
                                FEchaVencimientoPago, int.Parse(ch[i]), P_ID,
                                V_codigo_empresa, v_empresa_inicio, v_empresa_abrev, v_empresa_nomb, v_empresa_cuit, v_empresacat_iva, v_empresa_ing_bru, v_fe_ubicacion_cert, v_fe_clave, v_fe_cuit,
                                TipoCompAsos, EmisionCompAsos, NumCompAsos);

                            //Datos PDF
                            OracleCommand of = new OracleCommand("TELECOM.Facturacion_electronica.OBTENER_FACTURA", conn);
                            of.CommandType = System.Data.CommandType.StoredProcedure;
                            of.Parameters.Add("P_COMPROBANTE_CAJA_ID", Int64.Parse(ch[i]));
                            of.Parameters.Add("P_ID_VENTA", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
                            of.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 3, System.Data.ParameterDirection.Output);
                            of.Parameters.Add("p_descripcion", OracleDbType.Varchar2, 255, 4, System.Data.ParameterDirection.Output);
                            of.ExecuteNonQuery();
                            P_ID_VENTA = long.Parse(of.Parameters["P_ID_VENTA"].Value.ToString());
                            MENSAJE = of.Parameters["P_MENSAJE"].Value.ToString();
                            Referencia = of.Parameters["p_descripcion"].Value.ToString();
                            conn.Close();

                            //crear procedimiento
                            //return GenerarPDF(P_ID_VENTA, PEDIDO_NUMERO, TIPO_PEDIDO, Referencia);
                            return RedirectToAction("GenerarPDF", "Facturacion", new { p_id_venta = P_ID_VENTA, P_PEDIDO_NUMERO = PEDIDO_NUMERO, P_TIPO_PEDIDO = TIPO_PEDIDO, P_Referencia = Referencia });
                        }
                        else
                        {
                            MENSAJE = cmd.Parameters["P_MENSAJE"].Value.ToString();
                            TempData["MsjError"] = MENSAJE;
                        }
                    }
                }
                else
                {
                    MENSAJE = "Debe seleccionar una Operacion";
                    TempData["MsjError"] = MENSAJE;
                }
                return View("Index", GridOperaciones);
            }
            catch (Exception ex)
            {

                return View("Index", GridOperaciones);
            }
        }

        public ActionResult Imprimir(FormCollection form, int? PtosVta, int? Sucursal)
        {
            try
            {
                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();

                var ch = form.GetValues("assignChkBx");
                if (ch != null)
                {
                    for (int i = 0; i < ch.Count(); i++)
                    {
                        //Datos PDF
                        OracleCommand of = new OracleCommand("TELECOM.Facturacion_electronica.OBTENER_FACTURA", conn);
                        of.CommandType = System.Data.CommandType.StoredProcedure;
                        of.Parameters.Add("P_COMPROBANTE_CAJA_ID", Int64.Parse(ch[i]));
                        of.Parameters.Add("P_ID_VENTA", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
                        of.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 3, System.Data.ParameterDirection.Output);
                        of.Parameters.Add("p_descripcion", OracleDbType.Varchar2, 255, 4, System.Data.ParameterDirection.Output);
                        of.ExecuteNonQuery();
                        P_ID_VENTA = long.Parse(of.Parameters["P_ID_VENTA"].Value.ToString());
                        MENSAJE = of.Parameters["P_MENSAJE"].Value.ToString();
                        Referencia = of.Parameters["p_descripcion"].Value.ToString();

                        OracleCommand DATOS = new OracleCommand("TELECOM.Facturacion_electronica.OBTENER_PEDIDO_FACTURA", conn);
                        DATOS.CommandType = System.Data.CommandType.StoredProcedure;
                        DATOS.Parameters.Add("P_COMPROBANTE_CAJA_ID", Int64.Parse(ch[i]));
                        DATOS.Parameters.Add("P_PEDIDO_NUMERO", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
                        DATOS.Parameters.Add("P_TIPO_PEDIDO", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
                        DATOS.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 4, System.Data.ParameterDirection.Output);
                        DATOS.ExecuteNonQuery();
                        PEDIDO_NUMERO = int.Parse(DATOS.Parameters["P_PEDIDO_NUMERO"].Value.ToString());
                        TIPO_PEDIDO = int.Parse(DATOS.Parameters["P_TIPO_PEDIDO"].Value.ToString());
                        MENSAJE = DATOS.Parameters["P_MENSAJE"].Value.ToString();
                        conn.Close();
                        //return GenerarPDF(P_ID_VENTA, PEDIDO_NUMERO, TIPO_PEDIDO, Referencia);
                        return RedirectToAction("GenerarPDF", "Facturacion", new { p_id_venta = P_ID_VENTA, P_PEDIDO_NUMERO = PEDIDO_NUMERO, P_TIPO_PEDIDO = TIPO_PEDIDO, P_Referencia = Referencia });
                    }
                }
                else
                {
                    MENSAJE = "Debe seleccionar una Operacion";
                    TempData["MsjError"] = MENSAJE;
                }
            }
            catch (Exception ex)
            {
                MENSAJE = "Debe seleccionar una Operacion";
                TempData["MsjError"] = MENSAJE;
                throw;
            }
            return RedirectToAction("Index", "Facturacion");
        }
        public ActionResult GenerarPDF(long p_id_venta, long P_PEDIDO_NUMERO, long P_TIPO_PEDIDO, string P_Referencia, string Direccion)
        {
            try
            {
                var fac = new Caja.Models.FacturacionViewModel.FacElectronica();
                fac.ID_VENTA = p_id_venta;
                fac.Pedido_numero = P_PEDIDO_NUMERO;
                fac.Tipo_pedido = P_TIPO_PEDIDO;
                if (fac.ID_VENTA != 0 && fac.Pedido_numero != 0)
                {
                    taCompr.FillByID(dsTelecom.COMPROBANTE_PDF, fac.ID_VENTA);
                    taItemsVta.FillByNumyTipo(dsTelecom.FE_VENTA_ITEMS, fac.Pedido_numero, fac.Tipo_pedido);
                    taEmpresas.FillCUIT(dsTelecom.EMPRESAS);

                    if (dsTelecom.COMPROBANTE_PDF.Count() != 0)
                    {
                        totLetras = converter.enletras(dsTelecom.COMPROBANTE_PDF[0].TOTAL.ToString());

                        BarcodeLib.Barcode Codigo = new BarcodeLib.Barcode();
                        Codigo.IncludeLabel = true;
                        Codigo.Dispose();
                        Codigo.Encode(BarcodeLib.TYPE.CODE128, dsTelecom.COMPROBANTE_PDF[0].CODIGOBARRA, 400, 100);
                        //Codigo.SaveImage(Server.MapPath("~/Content/CodigoBarra/" + dsTelecom.COMPROBANTE_PDF[0].CODIGOBARRA + ".jpg"), SaveTypes.JPG);
                        Codigo.SaveImage("c:\\AfipXML\\" + dsTelecom.COMPROBANTE_PDF[0].CODIGOBARRA + ".jpg", SaveTypes.JPG);
                        CodigoBarras = Codigo.GetImageData(SaveTypes.JPG);

                        ReportDocument RepOrden = new ReportDocument();

                        if (dsTelecom.COMPROBANTE_PDF[0].TIPO_FACTURA == "A")
                        {
                            RepOrden.Load(Server.MapPath("~/Reports/ComprobanteAFIP.rpt"));
                        }
                        else
                        {
                            RepOrden.Load(Server.MapPath("~/Reports/ComprobanteB.rpt"));
                        }
                        //RepOrden.SetParameterValue(0, "C:/Repositor/Caja/Caja/Caja/Content/imagenes/CodigoBarra.png");
                        //RepOrden.Load(Server.MapPath("~/Reports/ComprobanteAFIP.rpt"));
                        RepOrden.SetDatabaseLogon(System.Web.HttpContext.Current.Session["usuario"].ToString(), System.Web.HttpContext.Current.Session["password"].ToString());

                        RepOrden.SetDataSource(dsTelecom);

                        RepOrden.SetParameterValue("PesosLetra", totLetras);
                        //RepOrden.SetParameterValue("CodBarras", Server.MapPath("~/Content/CodigoBarra/" + dsTelecom.COMPROBANTE_PDF[0].CODIGOBARRA + ".jpg")); 
                        RepOrden.SetParameterValue("CodBarras", "c:\\AfipXML\\" + dsTelecom.COMPROBANTE_PDF[0].CODIGOBARRA + ".jpg");


                        Stream stream = RepOrden.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);

                        RepOrden.Dispose();

                        //Guardar PDF en servidor
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            System.IO.File.WriteAllBytes(Server.MapPath("~/Comprobantes/" + P_Referencia + ".pdf"), ms.ToArray());
                        }

                        //ENNVIO MAIL 
                        if (Direccion != "")
                        {                             
                            Mail(Direccion, P_Referencia);
                        }
                        else
                        {
                            return File(stream, "application/pdf", P_Referencia + ".pdf");
                        }
                                                                                                        
                    }
                    else
                    {
                        MENSAJE = "No se puede generar factura sin CAE";
                        TempData["MsjError"] = MENSAJE;
                    }
                }
                else
                {
                    MENSAJE = RetAfip;
                    //TempData["MsjError"] = MENSAJE;
                    TempData["MsjError"] = System.Web.HttpContext.Current.Session["ErrorPrueba"];

                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return RedirectToAction("Index", "Facturacion");
        }        

        public ActionResult EnviarMail(FormCollection form, string txtCorreo)
        {
            Referencia = System.Web.HttpContext.Current.Session["Referencia"].ToString();
            chbSelect = int.Parse(System.Web.HttpContext.Current.Session["chxSelect"].ToString());
            OracleConnection conn = new OracleConnection(sConnectionString);
            conn.Open();

            OracleCommand DATOS = new OracleCommand("TELECOM.Facturacion_electronica.OBTENER_PEDIDO_FACTURA", conn);
            DATOS.CommandType = System.Data.CommandType.StoredProcedure;
            DATOS.Parameters.Add("P_COMPROBANTE_CAJA_ID", chbSelect);
            DATOS.Parameters.Add("P_PEDIDO_NUMERO", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
            DATOS.Parameters.Add("P_TIPO_PEDIDO", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
            DATOS.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 4, System.Data.ParameterDirection.Output);
            DATOS.ExecuteNonQuery();
            PEDIDO_NUMERO = int.Parse(DATOS.Parameters["P_PEDIDO_NUMERO"].Value.ToString());
            TIPO_PEDIDO = int.Parse(DATOS.Parameters["P_TIPO_PEDIDO"].Value.ToString());
            MENSAJE = DATOS.Parameters["P_MENSAJE"].Value.ToString();

            P_ID_VENTA = int.Parse(System.Web.HttpContext.Current.Session["IDVenta"].ToString());

            GenerarPDF(P_ID_VENTA, PEDIDO_NUMERO, TIPO_PEDIDO, Referencia, txtCorreo); 
            
            return RedirectToAction("Index", "Facturacion");
        }
        public ActionResult EnviarCorreo(FormCollection form, int? PtosVta, int? Sucursal, int txtNum)
        {
            try
            {
                ddlSeleccion(PtosVta, Sucursal, txtNum);
                ViewBag.Correo = true;
                var ch = form.GetValues("assignChkBx");
                if (ch != null)
                {
                    for (int i = 0; i < ch.Count(); i++)
                    {
                        chbSelect = Int64.Parse(ch[i]);
                        OracleConnection conn = new OracleConnection(sConnectionString);
                        conn.Open();
                        //Datos PDF
                        OracleCommand of = new OracleCommand("TELECOM.Facturacion_electronica.OBTENER_FACTURA", conn);
                        of.CommandType = System.Data.CommandType.StoredProcedure;
                        of.Parameters.Add("P_COMPROBANTE_CAJA_ID", Int64.Parse(ch[i]));
                        of.Parameters.Add("P_ID_VENTA", OracleDbType.Long, 999999999, 2, System.Data.ParameterDirection.Output);
                        of.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2, 255, 3, System.Data.ParameterDirection.Output);
                        of.Parameters.Add("p_descripcion", OracleDbType.Varchar2, 255, 4, System.Data.ParameterDirection.Output);
                        of.ExecuteNonQuery();
                        P_ID_VENTA = long.Parse(of.Parameters["P_ID_VENTA"].Value.ToString());
                        MENSAJE = of.Parameters["P_MENSAJE"].Value.ToString();
                        Referencia = of.Parameters["p_descripcion"].Value.ToString();                        
                        
                        conn.Close();

                        System.Web.HttpContext.Current.Session["Referencia"] = Referencia;
                        System.Web.HttpContext.Current.Session["IDVenta"] = P_ID_VENTA;
                        System.Web.HttpContext.Current.Session["chxSelect"] = chbSelect;

                    }
                }
                

                return View("Index", GridOperaciones);
            }
            catch (Exception)
            {

                throw;
            }            
        }

        void Mail(string Direccion, string P_Referencia)
        {
            try
            {
                MailMessage Mail = new MailMessage();
                Mail.To.Add(Direccion);
                Mail.From = new MailAddress("no-responder@borigenbetzel.com.ar");
                Mail.Subject = "Prueba";
                Mail.Body = "HOLA PRUEBA!";

                string Adjunto = Server.MapPath("~/Comprobantes/" + P_Referencia + ".pdf");
                Mail.Attachments.Add(new System.Net.Mail.Attachment(Adjunto));
                SmtpClient smpt = new SmtpClient();
                //smpt.Credentials = new NetworkCredential("mesa_ayuda@borigenbetzel.com.ar", "Dyc.839");
                smpt.Credentials = new NetworkCredential("correointerno@borigenbetzel.com.ar", "tam4281049");
                smpt.Port = 25;
                smpt.Host = "smtp.borigenbetzel.com.ar";
                string mensaje = string.Empty;
                smpt.UseDefaultCredentials = false;
                smpt.DeliveryMethod = SmtpDeliveryMethod.Network;

                smpt.EnableSsl = true;
                //smpt.ClientCertificates
                ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };

                smpt.Send(Mail);
                TempData["MsjError"] = "Comprobante enviado con exito!";
            }
            catch (Exception ex)
            {

                throw;
            }            
        }
    }

}