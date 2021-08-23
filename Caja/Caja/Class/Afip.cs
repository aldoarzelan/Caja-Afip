using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Caja.Models.Class;
using Oracle.ManagedDataAccess.Client;
using Caja.Models;
using System.Xml;
using Caja.wsfev1;
using System.Web.Services.Protocols;

namespace Caja.Class
{
 
    public class Afip
    {
        //Caja.Mtxca.MTXCAService ws= new MTXCAService();
        wsfev1.Service wsfe = new wsfev1.Service();
        
        Datos.dsTelecomTableAdapters.FE_VENTA_ITEMSTableAdapter taVtaItem = new Datos.dsTelecomTableAdapters.FE_VENTA_ITEMSTableAdapter();
        Datos.dsTelecomTableAdapters.FE_VENTA_ITEMS_IVA_TableAdapter taVtasIva = new Datos.dsTelecomTableAdapters.FE_VENTA_ITEMS_IVA_TableAdapter();
        Datos.dsTelecom dsTelecom = new Datos.dsTelecom();

        //valores de auth
        DateTime ticketVenc;
        string sing, token;
        long CuitRepresentada;

        //valores para encabezado
        double ImporteSubTotal;

        //bool lREsultado, Acceso, EConexion;
        double ImpOtrosTributos;
        int UltFactura, IndErrores, ErrorCod, RespTipoComp, NumComp, PEmision, BandAct;
        string ErrorMsg, MsgConcat, CaeVencimiento, MsjSalida;
        string AfipError;
       
        string Cae;
        bool fact_anteriormente;
        bool comp_utilizado;
        FEAuthRequest feAutReq;
        FECAERequest solicitudCAE;
        FECAEResponse respuesta;
        FECompConsultaResponse respuestaConsulta;
        CAEDetalleRespuesta respuestaConvertida; //solo para consultarCAE
        Err[] errores;
        Obs[] Observaciones;
        /* CodigoDescripcionType[] observaciones;
       CodigoDescripcionType evento;
       ComprobanteCAEResponseType CAEResponse;*/

        public string Resultado { get; private set; }
        bool resultado = true;
        WsaaConect Conexion = new Caja.Class.WsaaConect(); //creo una instancia de la clase wsaa conect
        XmlDocument xmlTicket = new XmlDocument();

        /*elementos de la clase de origen parametro*/
        int CodigoComprobante, PtoVenta, NumComprobante, CodConcepto, TipoDoc, PedidoNumero, PedidoTipo, OtrosTributosCant, CompAsociadosCant, Comprobante_caja_id, ID, AsosCodComp, AsosNumCom, AsosPtoVta;
        double ImpTotal, ImporteGravado, ImporteNoGravado, ImporteExento;
        string FechaEmision, NumDoc, FEchaservicioDesde, FEchaservicioHasta, FEchaVencimiento, FEchaVencimientoPago;
        string ubicacion_Ticket;
        String sConnectionString = System.Web.HttpContext.Current.Session["Connection"] as string;
        public string CAE(int p_CodigoComprobante, int p_PtoVenta, int p_NumComprobante, int p_CodConcepto, string p_FechaEmision, int p_TipoDoc, string p_NumDoc, double p_ImpTotal, int p_PedidoNumero, int p_PedidoTipo, int p_OtrosTributosCant, int p_CompAsociadosCant, double p_ImporteGravado, double p_ImporteNoGravado, double p_ImporteExento, string p_FEchaservicioDesde, string p_FEchaservicioHasta, string p_FEchaVencimiento, string p_FEchaVencimientoPago, int p_Comprobante_caja_id, int P_ID,
            int p_codigo_empresa,string p_empresa_inicio, string p_empresa_abrev, string p_empresa_nomb, string p_empresa_cuit, string p_empresacat_iva, string p_empresa_ing_bru, string p_fe_ubicacion_cert, string p_fe_clave, double p_fe_cuit, int? p_AsosCodComp = null, int? p_AsosNumCom = null, int? p_AsosPtoVta = null)
        {
            CuitRepresentada = (long)p_fe_cuit; //20351941635 prueba; //30708696079 gdb
            //muevo parametros a elementos de la clase
            CodigoComprobante = p_CodigoComprobante;
            PtoVenta = p_PtoVenta;
            NumComprobante = p_NumComprobante;
            CodConcepto = p_CodConcepto;
            TipoDoc = p_TipoDoc;
            PedidoNumero = p_PedidoNumero;
            PedidoTipo = p_PedidoTipo;
            OtrosTributosCant = p_OtrosTributosCant;
            CompAsociadosCant = p_CompAsociadosCant;
            Comprobante_caja_id = p_Comprobante_caja_id;
            ID = P_ID;
            if (p_AsosCodComp != null)
            {
                AsosCodComp = (int)p_AsosCodComp;
                AsosNumCom = (int)p_AsosNumCom;
                AsosPtoVta = (int)p_AsosPtoVta;
            }
            ImpTotal = p_ImpTotal;
            ImporteGravado = p_ImporteGravado;
            ImporteNoGravado = p_ImporteNoGravado;
            ImporteExento = p_ImporteExento;
            FechaEmision = p_FechaEmision;
            NumDoc = p_NumDoc;
            FEchaservicioDesde = p_FEchaservicioDesde;
            FEchaservicioHasta = p_FEchaservicioHasta;
            FEchaVencimiento = p_FEchaVencimiento;
            FEchaVencimientoPago = p_FEchaVencimientoPago;
            ubicacion_Ticket = "~/Certificado/xmlTicket" + p_empresa_abrev + ".xml";
            /*incio el proceso de crear el archivo xml de autenticación*/
            try //verifico si existe un archivo xml y lo restauro
            {                
                xmlTicket.Load(HttpContext.Current.Server.MapPath(ubicacion_Ticket));
                Conexion.setXML(xmlTicket);
            }
            catch (Exception e) //si no existe indico que lo debo obtener
            {
                resultado = false;
            }

            try
            {
                if (!Conexion.TicketEsValido || !resultado) // o no es valido el ticket o no existe por lo que lo debo obtener
                {
                    int obtengoTicket = Conexion.obtenerXml(p_fe_ubicacion_cert,p_fe_clave, ubicacion_Ticket);

                    if (Conexion.TicketEsValido && obtengoTicket == 0) //obtuve el ticket ok y ademas es valido
                    {
                        string a = Conexion.getXML();
                        xmlTicket.Load(HttpContext.Current.Server.MapPath(ubicacion_Ticket));//cargo el xml en la variable y luego lo guardo
                        resultado = true;
                    }
                    else
                    {
                        resultado = false;
                    }
                }
            }
            catch (Exception e)
            {
                AfipError = "Fallo Acceso Ticket: "+e.Message+Conexion.Error;
            }
            /*fin del proceso de crear el archivo xml de autenticación*/

            if (resultado)
            {
                /*completo autorizacion*/
                ticketVenc = Conexion.ExpirationTime;
                feAutReq = new FEAuthRequest();
                feAutReq.Sign = Conexion.Sign;
                feAutReq.Token = Conexion.Token;
                feAutReq.Cuit = CuitRepresentada;
                /*fin completo autorizacion*/

                /*verificamos si ya fue solicitado anteriormente*/                
                FECompConsultaReq consultaCAE = new FECompConsultaReq();
                consultaCAE.CbteNro = NumComprobante;
                consultaCAE.PtoVta =  PtoVenta;
                consultaCAE.CbteTipo =  (short)CodigoComprobante;

                try
                {
                    respuestaConsulta = wsfe.FECompConsultar(feAutReq, consultaCAE);// feAutReq, consultaCAE, out observaciones, out errores, out evento);
                    errores = respuestaConsulta.Errors;
                    if (respuestaConsulta.ResultGet != null)
                    {
                        //si el hueco es igual en el cliente, total y neto lo podemos asociar al mismo para que no facture dos veces
                        if (respuestaConsulta.ResultGet.ImpTotal == ImpTotal &&
                            (respuestaConsulta.ResultGet.DocNro.ToString() == NumDoc.ToString() || (respuestaConsulta.ResultGet.DocNro == 0 && NumDoc.ToString().Equals("null")) )
                            && respuestaConsulta.ResultGet.ImpNeto == ImporteGravado ) 
                        {
                            fact_anteriormente = true;
                            Cae = respuestaConsulta.ResultGet.CodAutorizacion.ToString();
                            NumComp = NumComprobante;
                            FechaEmision = respuestaConsulta.ResultGet.CbteFch;
                            CaeVencimiento =respuestaConsulta.ResultGet.FchVto;
                            RespTipoComp = CodigoComprobante;
                            PEmision = respuestaConsulta.ResultGet.PtoVta;
                            AfipError = AfipError+"Se recupero el CAE del comprobante seleccionado! Ahora puede imprimir el Comprobante; ";
                        }
                        else //si no es el mismo entonces verificamos si ya tiene un cae
                        {
                            if (respuestaConsulta.ResultGet.CodAutorizacion != null) //si se obtuvo cae entonces hay un hueco por lo que lo registraremos en la base de datos y un incidente automatico
                            {
                                comp_utilizado = true;
                                grabarhueco();
                                if (BandAct ==1)
                                {
                                    obtenerUltimoNumero();
                                }
                                else
                                {
                                    Err err = new Err();
                                    err.Code = 999;
                                    err.Msg = "existe un hueco con el comprobante " + NumComp + " no es posible continuar";
                                    errores[0] = err;
                                }
                            }
                            else
                            {
                                comp_utilizado = false;
                            }
                        }
                    }else //no existe factura con ese numero de comprobante
                    {
                        fact_anteriormente = false;
                    }
                }
                catch (Exception e) { fact_anteriormente = false; }

                /*fin verificamos si ya fue solicitado anteriormente*/
                if (fact_anteriormente == true)
                {
                     Resultado = "A";
                    /* ErrorGeneral = dll.fe.fxErrorMsg1;
                     UltimoError = dll.fe.UltimoMensajeError;
                     ErrorDetalle = dll.fe.FxRespuestaObservacionDescripcion1;*/
                }
                else
                {
                    /*inicio solicitud cae*/
                    ObtenerCae();
                }


                //ERRORES
                if (errores == null && Resultado == "A" )
                {
                    IndErrores = 0;
                    MsgConcat = "OK";
                    //Actualizar Datos
                    //String sConnectionString = "DATA SOURCE=TELECOM;PASSWORD=;USER ID=TELECOM";
                    OracleConnection conn = new OracleConnection(sConnectionString);
                    conn.Open();
                    OracleCommand cmd = new OracleCommand("TELECOM.Facturacion_Electronica.ACTUALIZAR_DATOS", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add("P_comprobante_caja_id", Comprobante_caja_id);
                    cmd.Parameters.Add("P_ID", P_ID);
                    cmd.Parameters.Add("P_ESTADO", Resultado);
                    cmd.Parameters.Add("p_cae", Cae);
                    cmd.Parameters.Add("P_MENSAJE", MsgConcat);
                    cmd.Parameters.Add("P_CUIT", CuitRepresentada);
                    cmd.Parameters.Add("P_FECHA_EMISION", FechaEmision.ToString());
                    cmd.Parameters.Add("P_FECHA_VENCIMIENTO_CAE", CaeVencimiento.ToString());
                    cmd.Parameters.Add("P_RESPUESTA_TIPO_COMPROBANTE", RespTipoComp);
                    cmd.Parameters.Add("P_RESPUESTA_NUMERO_COMPROBANTE", NumComp);
                    cmd.Parameters.Add("P_RESPUESTA_EMISION", PEmision);
                    cmd.Parameters.Add("P_MENSAJE_SALIDA", OracleDbType.Varchar2, 255, 12, System.Data.ParameterDirection.Output);
                    cmd.Parameters.Add("P_BANDERA", OracleDbType.Int64, 1, 13, System.Data.ParameterDirection.Output);
                    cmd.ExecuteNonQuery();

                    MsjSalida = cmd.Parameters["P_MENSAJE_SALIDA"].Value.ToString();
                    BandAct = int.Parse(cmd.Parameters["P_BANDERA"].Value.ToString());
                    conn.Dispose();
                    conn.Close();
                }
                else
                {
                    if (errores !=null)
                    {
                        IndErrores = errores.Length;
                    }
                    else
                    {
                        IndErrores = 0;
                    }
                }
                if (IndErrores > 0)
                {
                    for (int i = 0; i < IndErrores; i++)
                    {
                        ErrorCod = errores[i].Code;
                        ErrorMsg = errores[i].Msg;
                        if (i == 0)
                        {
                            MsgConcat = ErrorCod + ": " + ErrorMsg;
                        }
                        else
                        {
                            MsgConcat = MsgConcat + "; " + ErrorCod + ": " + ErrorMsg;
                        }
                    }
                    //AfipError = AfipError + ErrorGeneral;
                }
            }
            else
            {
                AfipError = AfipError + "Fallo Acceso: " + Conexion.Error+"; ";
            }
            return AfipError;
        }

        private void grabarhueco()
        {
            // int comprobante_caja_hueco =obtenerComprobanteCaja();
            OracleConnection conn = new OracleConnection(sConnectionString);
            conn.Open();

            OracleCommand cmd = new OracleCommand("TELECOM.Facturacion_Electronica.HUECO_COMPROBANTE_CAJA", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.Add("P_RESPUESTA_TIPO_COMPROBANTE", (int)respuestaConsulta.ResultGet.CbteTipo);
            cmd.Parameters.Add("P_RESPUESTA_NUMERO_COMPROBANTE", (int)respuestaConsulta.ResultGet.CbteDesde);
            cmd.Parameters.Add("P_RESPUESTA_EMISION", (int)respuestaConsulta.ResultGet.PtoVta);            
            cmd.Parameters.Add("P_TOTAL", (double)respuestaConsulta.ResultGet.ImpTotal);
            cmd.Parameters.Add("P_FECHA_EMISION", respuestaConsulta.ResultGet.CbteFch.ToString());
            cmd.Parameters.Add("P_TIPO_DOCUMENTO", (int) respuestaConsulta.ResultGet.DocTipo);
            cmd.Parameters.Add("P_DOCUMENTO_NRO", (int)respuestaConsulta.ResultGet.DocNro);
            cmd.Parameters.Add("P_COMPROBANTE_CAJA", OracleDbType.Int64, 999999999, 8, System.Data.ParameterDirection.Output);
            cmd.Parameters.Add("P_MENSAJE_SALIDA", OracleDbType.Varchar2, 255, 9, System.Data.ParameterDirection.Output);
            cmd.Parameters.Add("P_BANDERA", OracleDbType.Int64, 1, 10, System.Data.ParameterDirection.Output);
            cmd.ExecuteNonQuery();

            MsjSalida = cmd.Parameters["P_MENSAJE_SALIDA"].Value.ToString();
            BandAct = int.Parse(cmd.Parameters["P_BANDERA"].Value.ToString());
            int comprobante_caja_hueco = int.Parse(cmd.Parameters["P_COMPROBANTE_CAJA"].Value.ToString());

            if (BandAct == 1 && comprobante_caja_hueco !=0)
            {
                OracleCommand cmd2 = new OracleCommand("TELECOM.Facturacion_Electronica.ACTUALIZAR_DATOS", conn);
                cmd2.CommandType = System.Data.CommandType.StoredProcedure;
                cmd2.Parameters.Add("P_comprobante_caja_id", comprobante_caja_hueco); 
                cmd2.Parameters.Add("P_ID", null);
                cmd2.Parameters.Add("P_ESTADO", "A");
                cmd2.Parameters.Add("p_cae", respuestaConsulta.ResultGet.CodAutorizacion);
                cmd2.Parameters.Add("P_MENSAJE", "RECUPERO HUECO");
                cmd2.Parameters.Add("P_CUIT", CuitRepresentada);
                cmd2.Parameters.Add("P_FECHA_EMISION", respuestaConsulta.ResultGet.CbteFch);
                cmd2.Parameters.Add("P_FECHA_VENCIMIENTO_CAE", respuestaConsulta.ResultGet.FchVto);
                cmd2.Parameters.Add("P_RESPUESTA_TIPO_COMPROBANTE", (int)respuestaConsulta.ResultGet.CbteTipo);
                cmd2.Parameters.Add("P_RESPUESTA_NUMERO_COMPROBANTE", (int)NumComprobante);
                cmd2.Parameters.Add("P_RESPUESTA_EMISION", (int)respuestaConsulta.ResultGet.PtoVta);
                cmd2.Parameters.Add("P_MENSAJE_SALIDA", OracleDbType.Varchar2, 255, 12, System.Data.ParameterDirection.Output);
                cmd2.Parameters.Add("P_BANDERA", OracleDbType.Int64, 1, 13, System.Data.ParameterDirection.Output);
                cmd2.ExecuteNonQuery();

                MsjSalida = cmd2.Parameters["P_MENSAJE_SALIDA"].Value.ToString();
                BandAct = int.Parse(cmd2.Parameters["P_BANDERA"].Value.ToString());
            }
            conn.Close();
        }

       /* private int obtenerComprobanteCaja()
        {
            OracleConnection conn_comp = new OracleConnection(sConnectionString);
            conn_comp.Open();
            OracleCommand fscmd = new OracleCommand();
            fscmd.Connection = conn_comp;
            fscmd.CommandText = "select fe.COMPROBANTE_CAJA_ID from TELECOM.FE_SOLICITUD fe where fe.EMISION = :PtoVenta and fe.NUMERO = :NumComprobante and fe.TIPO_COMPROBANTE = :CodigoComprobante and fe.cae is null and fe.TOTAL = :ImpTotal and fe.ID = (select max(sol.id) from telecom.fe_solicitud sol where fe.COMPROBANTE_CAJA_ID = sol.COMPROBANTE_CAJA_ID)";
            fscmd.CommandType = System.Data.CommandType.Text;
            fscmd.Parameters.Add("?ImpTotal", OracleDbType.NVarchar2, 255);
            fscmd.Parameters["?ImpTotal"].Value = ImpTotal;
            fscmd.Parameters.Add("?CodigoComprobante", OracleDbType.Double, 255);
            fscmd.Parameters["?CodigoComprobante"].Value = CodigoComprobante;
            fscmd.Parameters.Add("?NumComprobante", OracleDbType.Int64, 255);
            fscmd.Parameters["?NumComprobante"].Value = NumComprobante;
            fscmd.Parameters.Add("?PtoVenta", OracleDbType.Int64, 255);
            fscmd.Parameters["?PtoVenta"].Value = PtoVenta;
            fscmd.ExecuteNonQuery();

            conn_comp.Close();
        }*/

        private void obtenerUltimoNumero()
        {
            FERecuperaLastCbteResponse consReqType = new FERecuperaLastCbteResponse();
            //errores = null; //reinicio errores en vacio

            consReqType = this.wsfe.FECompUltimoAutorizado(feAutReq, PtoVenta, CodigoComprobante); 
            NumComprobante = (int) consReqType.CbteNro+ 1;
        }

        private void ObtenerCae()
        {
            try
            {
                //Encabezado 
                solicitudCAE = new FECAERequest();
                FECAECabRequest encabezadoCAE = new FECAECabRequest();
                FECAEDetRequest[] ListdetalleCAE = new FECAEDetRequest[1];
                FECAEDetRequest detalleCAE = new FECAEDetRequest();
                encabezadoCAE.CantReg = 1;
                encabezadoCAE.CbteTipo = CodigoComprobante;
                encabezadoCAE.PtoVta = PtoVenta;
                                
                detalleCAE.Concepto = CodConcepto;
                detalleCAE.CbteDesde = NumComprobante;
                detalleCAE.CbteHasta = NumComprobante;
                if (FechaEmision != null)
                {
                    detalleCAE.CbteFch = FechaEmision;
                }                
                detalleCAE.MonId = "PES";
                detalleCAE.MonCotiz = 1;
                if (NumDoc != null && NumDoc != "null")
                {
                    detalleCAE.DocNro = long.Parse(NumDoc);
                    detalleCAE.DocTipo = (short)TipoDoc;
                }
                else
                {
                    detalleCAE.DocTipo = 99;
                }
                detalleCAE.ImpTotal = ImpTotal;
                if (ImporteGravado > 0)
                {
                    detalleCAE.ImpNeto = ImporteGravado;
                }
                if (ImporteNoGravado > 0)
                {
                    detalleCAE.ImpTotConc = ImporteNoGravado;
                }
                if (ImporteExento > 0)
                {
                    detalleCAE.ImpOpEx = ImporteExento;
                }

                ImporteSubTotal = ImporteGravado + ImporteNoGravado + ImporteExento;

                //solicitudCAE.importeSubtotal = (decimal)ImporteSubTotal;
                if (FEchaservicioDesde.ToString() != null && FEchaservicioDesde.ToString() != "\"\"" && FEchaservicioDesde.ToString() != "null")
                {
                    detalleCAE.FchServDesde = FEchaservicioDesde;
                    detalleCAE.FchServHasta = FEchaservicioHasta;
                }
                /* if (FEchaVencimiento.ToString() != null && FEchaVencimiento.ToString() != "\"\"" && FEchaVencimiento.ToString() != "null")
                 {
                     solicitudCAE.fechaVencimiento = DateTime.ParseExact(FEchaVencimiento, "yyyyMMdd", null);
                     solicitudCAE.fechaVencimientoSpecified = true;
                 }*/
                if (FEchaVencimientoPago.ToString() != null && FEchaVencimientoPago.ToString() != "\"\"" && FEchaVencimientoPago.ToString() != "null")
                {
                    detalleCAE.FchVtoPago = FEchaVencimientoPago;
                }
                /*detalleCAE.Opcionales[0].Id = Comprobante_caja_id.ToString();
                detalleCAE.Opcionales[0].Valor =Comprobante_caja_id.ToString();*/

                // int PedidoNumero, int PedidoTipo, int Comprobante_caja_id, int P_ID)
                if (OtrosTributosCant != 0)
                /*{
                    solicitudCAE.importeOtrosTributosSpecified = false;
                }
                else*/
                {
                    detalleCAE.ImpTrib = ImpOtrosTributos;
                    detalleCAE.Tributos = OtrosTributos(OtrosTributosCant); ;
                }

                if (CompAsociadosCant > 0)
                {
                    //trabajamos solo con un comprobante asociado
                    CbteAsoc[] listNC = new CbteAsoc[1];
                    CbteAsoc NC = new CbteAsoc();
                    NC.Nro = (long)AsosNumCom;
                    NC.PtoVta = (int)AsosPtoVta;
                    NC.Tipo = (short)AsosCodComp;
                    listNC[0] = NC;
                    detalleCAE.CbtesAsoc = listNC;
                }
                //subtotales iva
                taVtasIva.FillGrupByIva(dsTelecom._FE_VENTA_ITEMS_IVA_, PedidoNumero, PedidoTipo);
                var CantSubtotales = dsTelecom._FE_VENTA_ITEMS_IVA_.Count();
                AlicIva[] ListSubtIva = new AlicIva[CantSubtotales];
                double v_total_iva = 0;
                if (CantSubtotales > 0)
                {
                    IvaTipoResponse TiposIva = new IvaTipoResponse();
                    TiposIva = this.wsfe.FEParamGetTiposIva(feAutReq);
                    for (int i = 0; i < CantSubtotales; i++)
                    {
                        v_total_iva = v_total_iva + (double)dsTelecom._FE_VENTA_ITEMS_IVA_[i]._SUM_FE_ITEMIMPORTEIVA_;
                        AlicIva subtIva = new AlicIva();
                        subtIva.Id = (int)dsTelecom._FE_VENTA_ITEMS_IVA_[i].CODIGOCONDICIONIVA;
                        subtIva.BaseImp = (double)dsTelecom._FE_VENTA_ITEMS_IVA_[i].BASE_IMPONIBLE;
                        subtIva.Importe = (double)dsTelecom._FE_VENTA_ITEMS_IVA_[i]._SUM_FE_ITEMIMPORTEIVA_;
                        ListSubtIva[i] = subtIva;
                    }
                    detalleCAE.Iva = ListSubtIva;
                    detalleCAE.ImpIVA = v_total_iva;
                }
                // fin subtotales iva

                //detalles
                taVtaItem.FillByNumyTipo(dsTelecom.FE_VENTA_ITEMS, PedidoNumero, PedidoTipo);
                // solicitudCAE.arrayItems = ItemsCantidad(dsTelecom.FE_VENTA_ITEMS.Count, CodigoComprobante);
                //fin detalles
                bool respuesta_ws = true;
                try
                {
                    /*obtenemos cae*/
                    ListdetalleCAE[0] = detalleCAE;
                    solicitudCAE.FeDetReq = ListdetalleCAE;
                    solicitudCAE.FeCabReq = encabezadoCAE;
                    respuesta = this.wsfe.FECAESolicitar(feAutReq, solicitudCAE);
                    Resultado = respuesta.FeCabResp.Resultado;
                    errores = respuesta.Errors;
                    if (respuesta.FeDetResp[0].Observaciones != null)
                    {
                        if (respuesta.FeDetResp[0].Observaciones.Length > 0)
                        {
                            Observaciones = new Obs[respuesta.FeDetResp[0].Observaciones.Length];
                            for (int i = 0; i < respuesta.FeDetResp[0].Observaciones.Length; i++)
                            {
                                Obs obs = new Obs();
                                obs.Code = respuesta.FeDetResp[0].Observaciones[i].Code;
                                obs.Msg = respuesta.FeDetResp[0].Observaciones[i].Msg + ";";
                                Observaciones[i] = obs;
                                AfipError = AfipError + obs.Msg;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    feAutReq = new FEAuthRequest();
                    feAutReq.Sign = Conexion.Sign;
                    feAutReq.Token = Conexion.Token;
                    feAutReq.Cuit = CuitRepresentada;
                    wsfev1.FEPtoVentaResponse respuesta = this.wsfe.FEParamGetPtosVenta(feAutReq);
                    feAutReq.Cuit = CuitRepresentada;
                    AfipError = "Resultado Falso, Ocurrio un problema al solicitar CAE";
                    respuesta_ws = false;
                    AfipError = e.Message;
                }
                //manejar errores
                if (respuesta.FeCabResp.Resultado != null && respuesta_ws)
                {
                    Resultado = respuesta.FeCabResp.Resultado;
                    FECAEDetResponse CAEResponse = new FECAEDetResponse();
                    if (Resultado == "A")
                    {
                        CAEResponse = respuesta.FeDetResp[0];
                        if (CAEResponse.CAE != null)
                        {
                            Cae = CAEResponse.CAE;
                        }
                        NumComp = (int)CAEResponse.CbteDesde;
                        FechaEmision = CAEResponse.CbteFch;
                        CaeVencimiento = CAEResponse.CAEFchVto;
                        RespTipoComp = respuesta.FeCabResp.CbteTipo;
                        PEmision = respuesta.FeCabResp.PtoVta;
                        AfipError = "Resultado Verdadero";
                    }
                    else
                    {
                        AfipError = AfipError+ "Resultado Falso";

                    }
                }
                else
                {
                    Resultado = "R";
                    //ErrorGeneral = CAEResponse.
                    //UltimoError = dll.fe.UltimoMensajeError;
                    //ErrorDetalle = dll.fe.FxRespuestaObservacionDescripcion1;
                }
            }catch (Exception e)
            {
                AfipError = AfipError+ e.Message;
            }
        }
        /*public ItemType[] ItemsCantidad(int Cantidad, int CodComprobante)
        {
            ItemType[] ListItems = new ItemType[Cantidad];
            for (int i = 0; i < Cantidad; i++)
            {
                ItemType Item = new ItemType();
                Item.descripcion = dsTelecom.FE_VENTA_ITEMS[i].DESCRIPCION;
                Item.codigo = dsTelecom.FE_VENTA_ITEMS[i].ARTICULO_CODIGO;

                if (dsTelecom.FE_VENTA_ITEMS[i].FE_UNIDAD_MEDIDA_COD>0) {
                    Item.unidadesMtx = (short)dsTelecom.FE_VENTA_ITEMS[i].FE_UNIDAD_MEDIDA_COD;
                    Item.codigoMtx = dsTelecom.FE_VENTA_ITEMS[i].CODIGOMTX.ToString();
                    Item.unidadesMtxSpecified = true;
                }
                if (dsTelecom.FE_VENTA_ITEMS[i].CANTIDAD > 0)
                {
                    Item.cantidad = dsTelecom.FE_VENTA_ITEMS[i].CANTIDAD;
                    Item.cantidadSpecified = true;                    
                }
                Item.codigoUnidadMedida = (short) dsTelecom.FE_VENTA_ITEMS[i].FE_UNIDAD_MEDIDA_COD;
                if (dsTelecom.FE_VENTA_ITEMS[i].ITEMPRECIOUNITARIO > 0)
                {
                    Item.precioUnitario = dsTelecom.FE_VENTA_ITEMS[i].ITEMPRECIOUNITARIO;
                    Item.precioUnitarioSpecified = true;                    
                }
                if (dsTelecom.FE_VENTA_ITEMS[i].BONIFICACION>0)
                {
                    Item.importeBonificacion = dsTelecom.FE_VENTA_ITEMS[i].BONIFICACION;
                    Item.importeBonificacionSpecified = true;
                }
                Item.importeItem = dsTelecom.FE_VENTA_ITEMS[i].IMPORTEITEM;
                Item.codigoCondicionIVA =  (short)  dsTelecom.FE_VENTA_ITEMS[i].CODIGOCONDICIONIVA;

                if (CodComprobante == 1 || CodComprobante == 2 || CodComprobante == 3)
                {
                    Item.importeIVA = dsTelecom.FE_VENTA_ITEMS[i].ITEMIMPORTEIVA;
                    Item.importeIVASpecified = true;                    
                }
                ListItems[i] = Item;
            }
            return ListItems;
        }*/
        public Tributo[] OtrosTributos(int CantAtr)
        {//se debe mejorar en caso que se declaren otros tributos
            Tributo[] listOtroTrib = new Tributo[CantAtr];
            Tributo Trib = new Tributo();
            for (int i = 0; i < CantAtr; i++)
            {
                //listOtroTrib[i].codigo = (short) i; indice
                Trib.Id = 1;
                Trib.Desc = "otro tributos";
                Trib.BaseImp = 1000;
                listOtroTrib[i].Importe = 16;
                listOtroTrib[i] = Trib;
            }
            return listOtroTrib;
        }
       
    }

}
