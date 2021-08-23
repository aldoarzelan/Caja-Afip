using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WSAFIPFE;

namespace Caja.Models.Class
{
    public class PruebaServicioViewModels
    {
        public string cIdentificador { get; set; }
        public Factura fe = new Factura();        
        public string Error { get; set; }
                                              
    }
}