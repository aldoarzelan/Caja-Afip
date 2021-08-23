using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caja.Class
{
    public class CAEDetalleRespuesta
    {
        public string Cae { get; internal set; }
        public string CaeFechaVencimiento { get; internal set; }
        public long ComprobanteDesde { get; internal set; }
        public string ComprobanteFecha { get; internal set; }
        public long ComprobanteHasta { get; internal set; }
        public short Concepto { get; internal set; }
        public long DocumentoNumero { get; internal set; }
        public double ImporteExento { get; internal set; }
        public double ImporteIVA { get; internal set; }
        public double ImporteNeto { get; internal set; }
        public double ImporteTotal { get; internal set; }
        public double ImporteTributos { get; internal set; }
        public List<Observacion> Observaciones { get; internal set; }
        public string Resultado { get; internal set; }
        public short TipoDocumento { get; internal set; }
    }
}