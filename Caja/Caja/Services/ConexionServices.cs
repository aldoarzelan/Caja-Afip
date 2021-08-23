using Caja.Class;
using Caja.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Caja.Services
{
    public class ConexionServices
    {
        String mensaje, mensaje2;
        Boolean exito = false;
        Boolean v_RecuperarClave = false;
        String Url;
        private string usuario;
        int intentos = 0;

        public Conexion ObtenerConexion(string p_usuario, string p_clave)
        {
            String sConnectionString = "DATA SOURCE=(DESCRIPTION=(ADDRESS =(PROTOCOL = TCP)(HOST = 10.10.0.8)(PORT = 1521))(CONNECT_DATA =(SERVER = dedicated)(SID = ORAC)));PASSWORD=" + p_clave + ";USER ID=" + p_usuario;
            intentos++;
            OracleConnection conn;
            try
            {
                conn = new OracleConnection(sConnectionString);
                conn.Open();
               /* Datos.Usuario dsPrueba = new Datos.Usuario();
                Datos.UsuarioTableAdapters.QueriesTableAdapter dad = new Datos.UsuarioTableAdapters.QueriesTableAdapter();
               // Datos.UsuarioTableAdapters.MENSAJES_MOTIVADORESTableAdapter d = new Datos.UsuarioTableAdapters.MENSAJES_MOTIVADORESTableAdapter();

                //dad.Connection = sConnectionString;
                //d.Fill(dsPrueba.MENSAJES_MOTIVADORES);
               // mensaje = dsPrueba.MENSAJES_MOTIVADORES[0].VALOR; // MODIFICAR PARA QUE SEA RANDOMICO
                usuario = dad.obtener_usuario().ToString();
                 dad.Dispose();
                 dad.Connection = null;*/
                mensaje = "ok";
                System.Web.HttpContext.Current.Session["usuario"] = p_usuario;
                System.Web.HttpContext.Current.Session["password"] = p_clave;
                System.Web.HttpContext.Current.Session["estado_conexion"] = true;
                System.Web.HttpContext.Current.Session["ultimoLog"] = DateTime.Now;
                System.Web.HttpContext.Current.Session["Connection"] = sConnectionString;
                System.Web.HttpContext.Current.Session["intentos"] = 1;
                exito = true;
                //conn.Dispose();
                conn.Close();
            }
            catch (Exception e)
            {
                System.Web.HttpContext.Current.Session["usuario"] = null;
                System.Web.HttpContext.Current.Session["password"] = null;
                System.Web.HttpContext.Current.Session["estado_conexion"] = false;
                System.Web.HttpContext.Current.Session["ultimoLog"] = null;
                System.Web.HttpContext.Current.Session["Connection"] = null;
                System.Web.HttpContext.Current.Session["intentos"] = 1;
                usuario = null;
                
                if (e.ToString().Contains("the password has expired"))
                {
                    mensaje = "La clave a expirado, por favor cambiar la clave.";
                    v_RecuperarClave = true;
                }
                else
                {
                    if (e.ToString().Contains("the account is locked"))
                    {
                        mensaje = "El usuario se encuentra bloqueado.";
                    }
                    else
                    {
                        mensaje = "La contraseña y/o el usuario son incorrectos.";
                    }
                }
                exito = false;
            }
            return new Conexion() { Mensaje = mensaje, Estado = exito, Usuario = p_usuario, Clave = p_clave, Intentos =intentos, RecuperarClave = v_RecuperarClave};
        }
        
        public bool cambiarClave(String p_usuario, String p_clave)
        {
            try
            {
                String sConnectionString = "DATA SOURCE=(DESCRIPTION=(ADDRESS =(PROTOCOL = TCP)(HOST = 10.10.0.8)(PORT = 1521))(CONNECT_DATA =(SERVER = dedicated)(SID = ORAC))); PASSWORD = ELECTRO50 ;USER ID= FACTUELECT";
                OracleConnection conn = new OracleConnection(sConnectionString);
                conn.Open();
                const string comillas = "\"";
                string v_usuario = p_usuario.ToUpper();
                string v_clave =p_clave.ToUpper();
                //string sql = string.Concat(string.Concat(string.Concat(string.Concat("TELECOM.CAMBIAR_CLAVE;");
                OracleCommand gs = new OracleCommand("TELECOM.CAMBIAR_CLAVE", conn);
                gs.CommandType = System.Data.CommandType.StoredProcedure;
                gs.Parameters.Add("P_USUARIO", v_usuario);
                gs.Parameters.Add("P_CLAVE", v_clave);
                gs.Parameters.Add("P_MENSAJE", OracleDbType.Varchar2,255,3, System.Data.ParameterDirection.Output);
                gs.ExecuteNonQuery();
                conn.Close();
                mensaje = gs.Parameters["P_MENSAJE"].Value.ToString();
                System.Web.HttpContext.Current.Session["usuario"] = p_usuario;
                System.Web.HttpContext.Current.Session["password"] = p_clave;
                System.Web.HttpContext.Current.Session["estado_conexion"] = true;
                System.Web.HttpContext.Current.Session["ultimoLog"] = DateTime.Now;
                return true;
            } catch (Exception e)
            {
                mensaje = "No fue posible modificar la clave";
                return false;
            }
        }
        
    }
}