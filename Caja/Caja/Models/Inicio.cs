using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Caja.Models
{
    public class Inicio
    {

        public string MensajeMotivador { get; set; }
        public string Autor { get; set; }

        public Inicio(string msj, string autor)
        {
            MensajeMotivador= msj;
            Autor = autor;
        }
    }

}