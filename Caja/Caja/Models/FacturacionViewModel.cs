using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;



namespace Caja.Models
{
    public class FacturacionViewModel
    {
        public class DatosCaja
        {
            public int PtoVta { get; set; }
            public int CajaNum { get; set; }
            public int Turno { get; set; }
            public string FechaApertura { get; set; }
            public int EstadoCaja { get; set; }
            //public List<DatosCaja> lstDatosCaja { get; set; }
            //public DatosCaja()
            //{
            //    lstDatosCaja = new List<DatosCaja>();
            //}        

        }
        public class PtosVta
        {
            public IList<SelectListItem> lstPtoVta { get; set; }
            public PtosVta()
            {
                lstPtoVta = new List<SelectListItem>();
            }
        }
        public class Sucursal
        {
            public int Codigo { get; set; }
            public string NomSucursal { get; set; }
            public int SupVentas { get; set; }
            public int SupGestores { get; set; }
            public int Coordinador { get; set; }
            public int Agente { get; set; }
            public int Caja { get; set; }

            public IList<SelectListItem> LstSucursal { get; set; }
            public Sucursal()
            {
                LstSucursal = new List<SelectListItem>();
            }
        }

        public class OpCaja
        {
            public int Codigo { get; set; }
            public long Id { get; set; }
            public string Fecha { get; set; }
            public int Numero { get; set; }
            public string Abreviatura { get; set; }
            public string Descripcion { get; set; }
            public double Efectivo { get; set; }
            public double Tarjeta { get; set; }
            public double Total { get; set; }
            public int Estado { get; set; }
            public double TotalEfectivo { get; set; }
            public double TotalTarjeta { get; set; }
            public double TotalOp { get; set; }
            public string Mensaje { get; set; }
        }

        public class TotalCaja
        {
            public string Total { get; set; }
            public double TotalEfectivo { get; set; }
            public double TotalTarjeta { get; set; }
            public double ToTotal { get; set; }
        }

        public class FacElectronica
        {
            public decimal P_CodigoConcepto { get; set; }
            public string P_CodigoMoneda { get; set; }
            public decimal P_CodigoTipoComprobante { get; set; }
            public decimal P_CodigoTipoDocumento { get; set; }
            public decimal P_CompAsociadosItemCantidad { get; set; }
            public decimal P_CotizacionMoneda { get; set; }
            public string P_FEchaEmision { get; set; }
            public string P_FEchaservicioDesde { get; set; }
            public string P_FEchaServicioHasta { get; set; }
            public string P_FEchaVencimiento { get; set; }
            public string P_FEchaVencimientoPago { get; set; }
            public decimal P_ImporteExento { get; set; }
            public decimal P_ImporteGravado { get; set; }
            public decimal P_ImporteNoGravado { get; set; }
            public decimal P_ImporteOtrosTributos { get; set; }
            public decimal P_ImporteSubtotal { get; set; }
            public decimal P_ImporteTotal { get; set; }
            public decimal P_ItemsItemCantidad { get; set; }
            public decimal P_NUMEROComprobante { get; set; }
            public string P_NUMERODocumento { get; set; }
            public decimal P_NUMEROPuntoVenta { get; set; }
            public string P_Obervaciones { get; set; }
            public decimal P_OtrosTributosItemCantidad { get; set; }
            public decimal P_SubTotalesItemCantidad { get; set; }
            public string P_MENSAJE { get; set; }
            public decimal P_PEDIDO_NUMERO { get; set; }
            public decimal P_TIPO_PEDIDO { get; set; }
            public string CAE { get; set; }
            public long ID_VENTA { get; set; }
            public string CodigoBarra { get; set; }
            public long CodBarraNum { get; set; }
            public string TicketAcceso { get; set; }
            public long Pedido_numero { get; set; }
            public long Tipo_pedido { get; set; }
        }
    }
}
