using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Model.Internal;
using MovilBusiness.Printer.Model;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MovilBusiness.DataAccess
{
    public class DS_RepresentantesParametros
    {
        private static DS_RepresentantesParametros Instance;

        private DS_RepresentantesParametros() { }

        public static DS_RepresentantesParametros GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DS_RepresentantesParametros();
            }

            return Instance;
        }

        private bool ReadBoolParam(string parReferencia, bool byDefault = false)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(ParValor, '0') as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { parReferencia.ToUpper() });

                if (list != null && list.Count > 0)
                {
                    return list[0].ParValor.Trim().Equals("1");
                } else if (byDefault)
                {
                    return true;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;

        }

        private string ReadStringParam(string parReferencia)
        {
            try
            {
                List<RepresentantesParametros> list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { parReferencia.ToUpper() });
                if (list.Count > 0)
                {
                    return list[0].ParValor;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return "";
        }

        private int ReadIntParam(string parReferencia)
        {
            try
            {
                List<RepresentantesParametros> list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select Cast(ParValor as int) as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { parReferencia.ToUpper() });

                if (list.Count > 0)
                {
                    return Convert.ToInt32(list[0].ParValor);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return -1;
        }

        private int ReadIntParamDifReturn(string parReferencia)
        {
            try
            {
                List<RepresentantesParametros> list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select Cast(ParValor as int) as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { parReferencia.ToUpper() });

                if (list.Count > 0)
                {
                    return Convert.ToInt32(list[0].ParValor);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return -9999;
        }

        private double ReadDoubleParam(string parReferencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select Cast(ParValor as int) as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { parReferencia.ToUpper() });

                if (list.Count > 0)
                {
                    if (double.TryParse(list[0].ParValor, out double result))
                    {
                        return result;
                    }
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return -1;
        }

        private decimal ReadDecimalParam(string parReferencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select Cast(ParValor as int) as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { parReferencia.ToUpper() });

                if (list.Count > 0)
                {
                    if (decimal.TryParse(list[0].ParValor, out decimal result))
                    {
                        return result;
                    }
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return -1;
        }

        public void SaveImpresora(PrinterMetaData data)
        {
            try
            {
                var repCodigo = Arguments.CurrentUser?.RepCodigo;

                if (string.IsNullOrWhiteSpace(repCodigo))
                {
                    repCodigo = "mdsoft";
                }

                Hash map = new Hash("RepresentantesParametros");
                map.SaveScriptForServer = !Functions.IsSincronizacionTest() && Arguments.CurrentUser != null;

                map.Add("RepCodigo", repCodigo);
                map.Add("ParDescripcion", "Mac de la impresora");
                map.Add("ParValor", data.PrinterMac);
                map.Add("ParReferencia", "IMPRESORA");
                map.Add("UsuInicioSesion", repCodigo);

                var oldPrinter = ReadStringParam("IMPRESORA");

                if (string.IsNullOrWhiteSpace(oldPrinter))
                {
                    map.Add("rowguid", Guid.NewGuid().ToString());
                    map.ExecuteInsert();
                }
                else
                {
                    map.ExecuteUpdate("ltrim(rtrim(upper(ParReferencia))) = 'IMPRESORA' and RepCodigo = '" + repCodigo + "'");
                }

                map.Clear();
                map.Add("RepCodigo", repCodigo);
                map.Add("ParDescripcion", "Lenguaje de la impresora");
                map.Add("ParValor", data.PrinterLanguage);
                map.Add("ParReferencia", "PRINTERLANGUAGE");
                map.Add("UsuInicioSesion", repCodigo);

                var oldLanguage = ReadStringParam("PRINTERLANGUAGE");

                if (string.IsNullOrWhiteSpace(oldLanguage))
                {
                    map.Add("rowguid", Guid.NewGuid().ToString());
                    map.ExecuteInsert();
                }
                else
                {
                    map.ExecuteUpdate("ltrim(rtrim(upper(ParReferencia))) = 'PRINTERLANGUAGE' and RepCodigo = '" + repCodigo + "'");
                }

                map.Clear();
                map.Add("RepCodigo", repCodigo);
                map.Add("ParDescripcion", "Tamano de la impresora");
                map.Add("ParValor", data.PrinterSize);
                map.Add("ParReferencia", "PRINTERSIZE");
                map.Add("UsuInicioSesion", repCodigo);

                var oldSize = ReadStringParam("PRINTERSIZE");

                if (string.IsNullOrWhiteSpace(oldSize))
                {
                    map.Add("rowguid", Guid.NewGuid().ToString());
                    map.ExecuteInsert();
                }
                else
                {
                    map.ExecuteUpdate("ltrim(rtrim(upper(ParReferencia))) = 'PRINTERSIZE' and RepCodigo = '" + repCodigo + "'");
                }


            }
            catch (Exception e)
            {
                Console.Write(e.Message + " Debe realizar una carga inicial antes de configurar la impresora");
            }

        }

        public void SaveVersion(string repCodigo, string version)
        {
            Hash map = new Hash("RepresentantesParametros");
            map.Add("RepCodigo", repCodigo);
            map.Add("ParDescripcion", "Version de la aplicacion");
            map.Add("ParValor", version);
            map.Add("ParReferencia", "VERSION");
            map.Add("UsuInicioSesion", repCodigo);

            var oldVersion = GetVersion();

            if (string.IsNullOrWhiteSpace(oldVersion))
            {
                map.Add("rowguid", Guid.NewGuid().ToString());
                map.ExecuteInsert();
            }
            else
            {
                map.ExecuteUpdate("ltrim(rtrim(upper(ParReferencia))) = 'VERSION' and RepCodigo = '" + repCodigo.Trim() + "'");
            }

        }

        public void SaveSupervisorIndicador(string repcodigo)
        {
            SqliteManager.GetInstance().Execute("delete from RepresentantesParametros where trim(ParReferencia) = '[SUPERVISOR]' ", new string[] { });

            if (Arguments.CurrentUser.RepIndicadorSupervisor)
            {
                var map = new Hash("RepresentantesParametros") { SaveScriptForServer = false };
                map.Add("RepCodigo", repcodigo);
                map.Add("ParDescripcion", "indica que el representante es un supervisor");
                map.Add("ParValor", "1");
                map.Add("ParReferencia", "[SUPERVISOR]");
                map.Add("rowguid", Guid.NewGuid().ToString());
                map.Add("UsuInicioSesion", repcodigo);
                map.ExecuteInsert();
            }
        }

        public string GetSupervisorAvailableForLogin()
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select RepCodigo from RepresentantesParametros where trim(ParReferencia) = '[SUPERVISOR]' and trim(ParValor) = '1' ", new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].RepCodigo;
                }
            }
            catch (Exception) { }

            return null;
        }

        public PrinterMetaData GetImpresora()
        {
            PrinterMetaData data = new PrinterMetaData
            {
                PrinterMac = ReadStringParam("IMPRESORA"),
                PrinterSize = ReadIntParam("PRINTERSIZE"),
                PrinterLanguage = ReadStringParam("PRINTERLANGUAGE")
            };

            return data;

        }


        public string GetRepSuscriptor() { return ReadStringParam("SUSCRIPTOR"); }

        //parametros de los diferentes modulos
        public bool GetParPedidos() { return ReadBoolParam("PEDIDOS"); }
        public bool GetPedidosAceptarOfertasDecimales() { return ReadBoolParam("PEDOFTDEC") && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS); }
        public bool GetParVentas() { return ReadBoolParam("VENTAS"); }
        public bool GetParCotizaciones() { return ReadBoolParam("COTIZACION"); }
        public bool GetParCompras() { return ReadBoolParam("PUSHMONEY"); }
        public int GetDiasTransaccionesVisibles() { return ReadIntParam("DIASTRANVI"); }

        /// <summary>
        /// 0- Inactivo
        /// 1- Activa Inventario Fisico
        /// 2- Activa Inventario Consignacion
        /// </summary>
        /// <returns></returns>
        public int GetParInventarioFisico() { return ReadIntParam("INVENTARIOF"); }
        public bool GetParDevoluciones() { return ReadBoolParam("DEVOLUCION"); }
        public bool GetParProductosEnVisitas() { return ReadBoolParam("PRODUC"); }
        public bool GetParCobros() { return ReadBoolParam("COBROS"); }
        public bool GetParDepositos() { return ReadBoolParam("DEPOSITOS"); }
        public bool GetParProspectos() { return ReadBoolParam("SOLCREDITO"); }
        public bool GetParGastos() { return ReadBoolParam("GASTOS"); }
        public bool GetParEntregaFacturaCamion() { return ReadBoolParam("EFATCARCAM"); }
        public bool GetParDepositosCompras() { return ReadBoolParam("DepCompras"); }
        public bool GetParEntregaDocumentos() { return ReadBoolParam("ENTREGADOC"); }
        //public bool GetParProyectosIngenieria() { return ReadBoolParam("PROYECTOSING"); }
        //public bool GetParRequisiconInventario() { return ReadBoolParam("REQINV"); }

        /* ParValor = 1: Cuadre vendedores
         ParValor = 2: Cuadre preventa 
         ParValor = 3: Cuadre vendedores con ayudantes*/
        public int GetParCuadres() { return ReadIntParam("CUADRES"); }
        public bool GetParCuadresDiarios() { return ReadBoolParam("CUADIARIO"); }
        public bool GetParCuadresVehiculosRepresentante() { return ReadBoolParam("CUAVEHREP"); }
        public bool GetParNoCrearVisitas() { return ReadBoolParam("NOVISITA"); }
        public bool GetParTipoConsultaVisitas() { return ReadBoolParam("CONSVISITA"); }
        public bool GetParCuadresOcultarContador() { return ReadBoolParam("NOCONTADOR"); }
        public bool GetParComentarioEnVisita() { return ReadBoolParam("VISCOMEN") || ReadBoolParam("PEDCOMEN"); }
        public bool GetParCargasInventario() { return ReadBoolParam("CARGAS") && !GetParVentasNoRebajaInventario(); }
        public int GetParDevolucionesCantidadMaximaProductos() { return ReadIntParam("DEVCANTDETMAX"); } //en devoluciones no permite agregar mas productos que el parvalor de este parametro
        public bool GetParDevolucionesCantidadOUnidades() { return ReadBoolParam("DevCanOUnd"); }//en devoluciones no permite agregar un producto con cantidad y unidades debe ser una de las dos
        public bool GetParPedidosFiltrarProductosPorCategoria1() { return ReadBoolParam("PEDCAT1"); }
        public bool GetParPedidosFiltrarProductosPorCategoria2() { return ReadBoolParam("PedFilCat2"); }
        //public bool GetParPedidosFiltrarLineaPorGrupoProductosCliente() { return ReadBoolParam("PEDCATFIL"); }
        public bool GetParLineasPorSector() { return ReadBoolParam("PROLINXSEC"); }
        public int GetParPedidosVisualizarFotoEnDetalleProductos()
        {
            int result = ReadIntParam("PEDSELIMG");

            return result == 1 &&
                Arguments.Values.CurrentModule == Modules.PEDIDOS ? 1 :
                result == 2 &&
                Arguments.Values.CurrentModule == Modules.PEDIDOS ? 2 : -1;
        }

        public bool GetParDatosClienteNegociaciones() { return ReadBoolParam("DADDIF"); } //en la pantalla de datos clientes activa el tab de negociaciones

        public bool GetParDevolucionesValidarCantidadDetalleConProUnidades() { return ReadBoolParam("DevUndCant"); }

        /*se usa al momento de buscar el detalle de */
        public int GetParOfertasMancomunadas()
        {
            var result = 1;

            var raw = ReadIntParam("OFEMANC");

            if (raw == 2)
            {
                result = raw;
            }

            return result;
        }

        /** En devoluciones al agregar un producto valida que la fecha de vencimiento de este no exceda o no sea menor al parvalor de este parametro, para saber si se validara 
         que no exceda o que no sea anterior en la tabla PoliticasDevolucionDetalle con el PodTipo 1 y 2 respectivamente */
        public int GetParDevolucionesDiasVencimiento() { return ReadIntParam("DVDiasAnts"); }
        public bool GetParDevolucionesFacturaObligatoria() { return ReadBoolParam("DevFacVal"); } //en devoluciones al agregar un producto la factura es obligatoria.

        public bool GetParDevolucionesFechaObligatoria() { return ReadBoolParam("DEVFECHOB"); } // en devoluciones al agregar un producto la fecha de vencimiento es obligatoria
        public bool GetParDevolucionesLoteObligatorio() { return ReadBoolParam("DevLoteOB"); } //en devolucion al agregar un producto el lote es obligatorio.

        public bool GetParCuadresUsarKilometraje() { return ReadBoolParam("CUAKILOMETRAJE"); }
        public bool GetParCuadresUsarContador() { return ReadBoolParam("CUACONTADOR"); }
        public int GetParCiclosSemanas() { return ReadIntParam("CICLSEMA"); }
        public int GetParCiclosSemanasxCodigoVendedor(string repcodigo) { return ReadIntParam($"CICLSEMA{repcodigo}"); }
        public string GetParRecFacturasVencidasColor() { return ReadStringParam("RECVENCCOLOR"); }
        public bool GetParDepositoSectores() { return ReadBoolParam("DEPSEC") && GetParSectores() > 0; } //sectores en depositos

        public bool GetParRecibosPorSector() { return ReadBoolParam("RECBUSSEC") && GetParSectores() > 0; } //al crear un nuevo recibo carga las facturas por sector
        public bool GetParAreaCrtlCreditoClienteSubString() { return ReadBoolParam("ARECRTSUB"); } //en la busqueda de los recibos se hace un substring al crtareacredit de clientesdetalle
        public bool GetParRecibosImportadoraLaPlaza() { return ReadBoolParam("RecClsILP"); } //lo usa la plaza, preguntar klk k hace
        public bool GetParRecPedVen() { return ReadBoolParam("RecPedVen"); }
        public bool GetParReconciliacion() { return ReadBoolParam("RECONCILIA"); }
        //public bool GetParRecibosMultiMoneda() { return ReadBoolParam("RecMultiMonRecMultiMon"); }
        public bool GetParCrearRecibos() { return ReadBoolParam("RECIBOS"); }

        // public bool GetParRecibosAutAllDesc() { return ReadBoolParam("AUTALLDESC"); }

        public bool GetParVisEntrarAutomaticamenteCobros() { return ReadBoolParam("VISCOBAUT"); }//al entrar a una visita entra automaticamente a cobros;

        /*el parvalor se usa para saber que row usar si el 1 o el 2*/
        public int GetParAuditoriaMercado() { return ReadIntParam("AudMercado"); }

        public bool GetParRecibosTabGeneral() { return ReadBoolParam("RECTABGEN") && Arguments.Values.CurrentModule == Modules.COBROS; }
        public int GetParShowDescuentoOfertasTipoRow() { return ReadIntParam("DESOFSHOW"); }
        public bool GetParDevolucionesMotivoUnico() { return ReadBoolParam("DEVMOTUNICO"); }//en devoluciones en el tab de configurar pide el motivo de la devolucion y solo deja agregar productos con un solo motivo
        public bool GetParDevolucionesCintillo() { return ReadBoolParam("DEVCINT"); } //en devoluciones pide el cintillo para dejar guardar, en el tab de configurar.
        public bool GetParDevolucionesNumeroDocumento() { return ReadBoolParam("DEVNUMDOC"); } //en devoluciones pide el numero de documento para dejar guardar, en el tab de configurar

        /** * activa la accion en devoluciones,
        * ParValor = 'D' le agrega la accion al detalle en las devoluciones, mientras que con
        * ParValor = 'C' o '1' solo agrega la accion a la cabecera en las devoluciones C o 1 porque antes era boolean
        * ParValor = 'C|0' o 'C|1' ... , esto es con el fin de seleccionar l a posicion del combo por defecto segun el valor despues del pay ( '|' )*/
        public string GetParDevolucionesAccion() { return ReadStringParam("DEVACCION"); }

        /**En devoluciones al seleccionar un producto si el parValor = 1 activa un spinner de lotes si es igual a 2 o null o cero deja el edit */
        public int GetParDevolucionesLotes() { return ReadIntParam("PROLOTE"); }

        public bool GetParFiltrarClienteFormasPago() { return ReadBoolParam("CLIFORPAGO",true); }

        public bool GetParRecibosFotoChequeObligatorio() { return Arguments.Values.CurrentModule == Modules.COBROS && ReadBoolParam("RECFOTOCHK"); } // En cobros si se inserta forma de pago cheque debe de añadir una foto para agregar

        //el parvalor del parametro es igual al limite de dias para los cheques diferidos
        public ChkDiferidosAutorizar GetParChequesDiferidosLimiteDias()
        {
            if (Arguments.Values.CurrentModule == Modules.COBROS)
            {
                var param = ReadStringParam("RECFUTLMT");

                if (string.IsNullOrWhiteSpace(param))
                {
                    return null;
                }

                var raw = param.Split('|');

                if (int.TryParse(raw[0], out int dias) && dias > 0)
                {
                    var autorizar = 0;

                    if (raw.Length > 1)
                    {
                        int.TryParse(raw[1], out autorizar);
                    }

                    var result = new ChkDiferidosAutorizar();
                    result.DiasLimite = dias;
                    result.Autorizar = autorizar == 1;
                    return result;
                }

            }

            return null;
        }


        public bool GetParRecibosSoloUnaFormaDePago() { return Arguments.Values.CurrentModule == Modules.COBROS && ReadBoolParam("RECFPUNO"); }

        public int GetParRecibosSecuenciaPorSector() { return ReadIntParam("RECSECSEC"); }//en recibos al guardar un recibo la secuencia la saca de RepCodigo-SectorID y en el rectipo pone el SectorID
        public bool GetParRecibosRecTipoChkDiferidos() { return ReadBoolParam("RECTIPOCHKDIF"); }//en recibos al guardar si se tiene un chk diferido pone el rectipo = 2
        public bool GetParRecibosRecTipoBySector() { return ReadBoolParam("RecTipoSec") && GetParSectores() > 0; } //al guardar un recibo el rectipo es igual al SectorID que se tenga

        public bool GetParOrdenPago() { return ReadBoolParam("ORDENPAGO"); }
        public bool GetParRecibosNCaFacturasConAbono() { return ReadBoolParam("RECNCTOAB"); } //permite aplicar notas de credito a facturas abonadas.
        public bool GetParRecibosAplicarNCaTodasFacturas() { return ReadBoolParam("RECNCALL"); } //en recibos al aplicar una NC aparecen todas las facturas en la listas a aplicar.
        public int GetParDescuentoManual() { return ReadIntParam("DESCMANUAL"); }//Habilita Spinner de descuentos manuales en los Recibos (Factura --> Detalle) y el ParValor es e limite Maximo.

        public bool GetParRecibosDescuentoFacturasSemiAutomatico() { return ReadBoolParam("DESSEMIAUT"); }
        public int GetParSAC() { return ReadIntParam("SAC"); }
        public bool GetParRecibosNotasCreditoAplicacionAutomatica() { return ReadBoolParam("RECAPLAUT"); }
        public bool GetParRecibosNoDescuentoAlSaldar() { return ReadBoolParam("RecNoDescSal"); }

        public bool GetParRecibosAutorizarDescuento() { return ReadBoolParam("RecDescPin") && Arguments.Values.CurrentModule == Modules.COBROS; }
        public bool GetParRecibosOcultarDescuentoSiNoTiene() { return ReadBoolParam("DesManOcul"); } //en recibos si una factura no tiene descuento oculta el spinner de descuento en el dialogo de detale

        /*en recibos a los documentos que * Concede descuentos a los documentos que figuren en el Campo Autorizaciones.AutReferencia.
        * este campo consta de la siguientes sintaxis.cxcreferencia-%desc(eje: AC123458-5.00).
        * y como es nromal solo de los habilita si el pin es correcto.*/
        public bool GetParRecibosDescuentoFromAutorizaciones() { return ReadBoolParam("AUTGENCXC"); }

        public bool GetImpresionSoloFacturaCreditos() { return ReadBoolParam("CUASOLOFAC"); }
        /**\
     * En el modulo de Recibos al autorizar descuentos con Pin de autorizacion, habilita el Spinner
     * desde 0 hasta el  % del Parametro
     *
     * (Esta parametro trabaja en conjunto con RECDESPIN)
     *
     */
        public int GetParRecibosDescuentoEscalonado()
        {
            return ReadIntParam("RECDESPINESCAL");
        }

        public bool GetParRecibosRecNumeroObligatorio() { return ReadBoolParam("RECNUMREC"); }//al guardar el recibo verifica que el rec numero en el tab general no este vacio.

        // public bool GetParRecibosEditarDescuentoManual() { return ReadBoolParam("DESMANREC"); } //permite editar el descuento manual en recibos
        //public bool GetParRecibosSiglas() { return ReadBoolParam("RECSIGL"); }

        public bool GetParCobrosVerLimiteCredito() { return ReadBoolParam("COBLIMCRE"); }//ver layouts de limite de credito en la pantalla de cobros

        /// <summary>
        ///ya no se utiliza, ahora se utiliza Arguments.Values.CurrentClient.CliIndicadorDepositaFactura para la verificacion
        /// </summary>
        /// <returns></returns>
        // public bool GetParCobrosEntregaDocumentos() { return ReadBoolParam("RECENTREG"); }//en el menu de cobros se activa el modulo de entrega de documentos.

        /*En cobros al seleccionar una factura, carga el Combo de descuentos en base al descuento que tenga la factura empezando desde 0 hasta el % de la factura*/
        /* Valor 1: Carga los descuentos disponibles desde 0 hasta el porciento dado en DescuentosFactura
           Valor 2: Carga los descuentos disponibles desde el porciento dado en DescuentosFactura hasta 0
           Valor 3: Carga los descuentos disponibles desde el porciento dado en DescuentosFactura hasta 0 e incluye el valor del parametro RECDESADI y RECDESADI2*/
        public int GetParRecibosPorcientoDescuentoDisponibleParaFacturas() { return ReadIntParam("RECDESCOMB"); }

        //Descuento agregado adicionalmente al rango dado en el valor 3 del parametro RECDESCOMB
        public double GetDescuentoManualAdicional()
        {
            double value = ReadDoubleParam("RECDESADI");

            if (value == -1 || value == 0)
            {
                value = 8.4746;
            }

            return value;
        }

        public double Get2doDescuentoManualAdicional()
        {
            double value = ReadDoubleParam("RECDESADI2");

            if (value == -1 || value == 0)
            {
                value = 12;
            }

            return value;
        }
        public bool GetParRecibosNoChequesDiferidos() { return ReadBoolParam("RECNOCKF"); } ///en recibos no permite chk diferidos.

        public bool GetParRecibosTiposCheques() { return ReadBoolParam("SpnTipoChq"); } //en cobros en el tab de forma de pago activa un spinner para seleccionar el tipo de chk, el spinner se carga de usosmultiples

        
        public bool GetParRecibosAutorizarAbono() { return ReadBoolParam("RecAutAB"); } //al aplicar un abono si los dias de la factura son mayor a los del parametro RCAutABDia se pide una autorizacion.


        // Valor: 1 = al aplicar un abono si los dias de la factura son mayor a los del parametro RCAutABDia se pide una autorizacion.
        // Valor: 2 = no permite aplicar abono a factura, pide una autorizacion.
        public int GetParRecibosSoloAbonoConAutorizacion() { return ReadIntParam("RECAUTABONO"); } 

        public int GetParRecibosAbonoDiasPermitidos()
        {
            var limite = ReadIntParam("RCAutABDia");

            if (limite < 0)
            {
                limite = 0;
            }

            return limite;
        }



        public bool GetParNoListaPrecios() { return ReadBoolParam("SINLP"); }

        /* /**
     * Retorna el tipo de relacion de los clientes.
     * ParValores: 1--> Relacion 1 a 1. es decir que a un cliente le corresppode un vendedor. y toda la informacion del cliente la buscara de la tabla Clientes.
     *             2 --> Relacion 1 a muchos. es decir que a un cliente le puede corresponder varios vendedores. la informacion del cliente se buscaria desde ClientesDtealle*/
        public int GetParTipoRelacionClientes() { return ReadIntParam("CLIRELACION"); }

        public int GetParVisitaVerClienteCredito() { return ReadIntParam("VERCREDCLI"); } //al entrar en la visita abajo del nombre del cliente aparece un layout con la info del credito del cliente tal como: balance, lim credito, credito disponible.

        public bool GetParRecibosNoPermitirSobranteSiHayAbono() { return ReadBoolParam("RECNOSOBSIAB"); }

        public bool GetParVisLabelSectorRenombrarCompania() { return ReadBoolParam("LBLSECTOR") && GetParSectores() > 0; } //en operaciones al label de sector le pone compañia

        public bool GetParVisitasCamara() { return ReadBoolParam("CAMVIST") && Device.RuntimePlatform == Device.Android; }

        /*Habilita los sectores
         * ParValor 1= habilita los sectores y los lipCodigo se toman de la tabla de clientes.
         ParValor 2= habilita los sectores y los lipCodigo se toman de la tabla de clientesDetalle basado en el sector que se seleccione.
         ParValor 3= habilita el modal de seleccion de sector antes de crear la visita, y la visita se cierra en dicho modal.
         ParValor 4= lo mismo que en el 3, pero los sectores le van apareciendo de manera secuencial, es decir cuando visite el primero le aparece el segundo*/
        public int GetParSectores()
        {
            var value = ReadIntParam("PEDSECTOR");

            if (value == -1)
            {
                value = ReadIntParam("VISSECTOR");
            }

            return value;

        }

        public int GetParImpresionAutomatica() { return ReadIntParam("IMPRAUTO"); }

        public bool GetParCliEstatusBySector() { return ReadBoolParam("CLIESTBYSECTOR"); } //al seleccionar un sector el estatus del cliente cambia dependiendo este, obviamente cambia en la variable estatica CurrentClient en OnRuntime.

        /**
     * determina si el recibo que se va a realizar tiene sobrante, y basado en el ParValor de este Parametro realiza lo sigte:
     * 0 --> desactivado
     * 1 --> Despliega un mensaje
     * 2 --> Determina si existen facturas con saldo pendiente y si encuentra una, despliega un mensaje que dice "Existen facturas pendientes por saldar, no puede haber recibos con sobrante."
     */
        public string[] GetParRecibosNoSobrante()
        {
            return ReadStringParam("RECNOSOBRA").Split(',');
        }

        public bool GetParRecibosOtrosDocumentosBalance() { return ReadBoolParam("OTCOBBAL"); }

        public bool GetParRecibosAplicacionExcedente() { return ReadBoolParam("RECAPLEXC"); }

        public bool GetParRecibosValidarMontoCabeceraVSdetalle() { return ReadBoolParam("RecCandado"); } //al guardar el recibo valida el total de la cabecera con la suma de los totales del detalle del recibo y si no coinciden no deja guardar el recibo

        public int GetParTipoAdValorem() { return ReadIntParam("TIPADVALOREM"); }//para saber el tipo de advalorem usado, parvalor 1 es para Porcentaje, parvalor 2 es para monto, parvalor 3 advalorem por tipo de negocio de la tabla de ProductosTiposNegocioAdvalorem

        public bool GetParGPS()
        {
            var result = true;
            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(ParValor, '') as ParValor from RepresentantesParametros " +
                    "where trim(upper(ParReferencia)) = 'GPS' and trim(RepCodigo) = ?",
                    new string[] { Arguments.CurrentUser.RepCodigo.Trim() });

                if (list != null && list.Count > 0)
                {
                    result = list[0].ParValor.Trim() == "1";
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return result;
        }

        public int GetFormatoImpresionCargasInventario()
        {
            int value = ReadIntParam("FORMCAR");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionConteoFisico()
        {
            int value = ReadIntParam("FORMCON");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionCuadres()
        {
            int value = ReadIntParam("FORMCUA");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionConduces()
        {
            int value = ReadIntParam("FORMCON");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionCompras()
        {
            int value = ReadIntParam("FORMCOM");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionCotizaciones()
        {
            int value = ReadIntParam("FORMCOT");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionInventario()
        {
            int value = ReadIntParam("FORMINV");

            if (value == -1)
            {
                value = 1;
            }

            return value;

        }

        public int GetFormatoImpresionInventarioFisicos()
        {
            int value = ReadIntParam("FORMINVFIS");

            if (value == -1)
            {
                value = 1;
            }

            return value;

        }

        public int GetFormatoImpresionRecibos()
        {
            int value = ReadIntParam("FORMREC");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionRecibosPushMoney()
        {
            var value = ReadStringParam("FORMPUSHM");

            return string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
        }

        public int GetFormatoImpresionNCDPP()
        {
            int value = ReadIntParam("FORMNCDPP");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionProspectos()
        {
            int value = ReadIntParam("FORMPRO");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionSAC()
        {
            int value = ReadIntParam("FORMSAC");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int[] GetFormatoImpresionPedidos()
        {
            var value = ReadStringParam("FORMPED");

            /*if (value == -1)
            {
                value = 1;
            }*/

            var split = value.Split('|');

            var results = new List<int>();

            foreach (var par in split)
            {
                if (int.TryParse(par, out int param))
                {
                    results.Add(param);
                }
            }

            if (results.Count == 0)
            {
                results.Add(1);
            }

            return results.ToArray();
        }
        public int GetFormatoImpresionPedidosPDF()
        {
            var value = ReadStringParam("FORMPEDPDF");

            return string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
        }

        public int GetFormatoImpresionCotizacionesPDF()
        {
            var value = ReadStringParam("FORMCOTPDF");

            return string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
        }

        public int GetFormatoImpresionVentas()
        {
            int value = ReadIntParam("FORMVEN");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoInventarioFisico()
        {
            int value = ReadIntParam("FORMINVF");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionColocacionProductos()
        {
            int value = ReadIntParam("FORMCOLP");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }


        public int GetFormatoImpresionDevoluciones()
        {
            int value = ReadIntParam("FORMDEV");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionDepositos()
        {
            int value = ReadIntParam("FORMDEP");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionDepositosCompras()
        {
            int value = ReadIntParam("FORMDEPCOM");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionDepositoGastos()
        {
            return ReadIntParam("FORMDEPGAS");
        }

        public int GetFormatoImpresionEntregasRepartidor()
        {
            return ReadIntParam("FORMENTREP");
        }

        public int GetFormatoImpresionEntregasDocumentos()
        {
            int value = ReadIntParam("FORMENT");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionEstadosCuentas()
        {
            int value = ReadIntParam("FORMEST");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }

        public int GetFormatoImpresionNotaCredito() { return ReadIntParam("FORMNC"); }

        public string GetParAlmacenDefault()
        {
            string param = ReadStringParam("ALMIDEFAUL");

            if (param == null)
            {
                param = "0";
            }

            return param;
        }



        public int GetFormatoImpresionGastos()
        {
            int value = ReadIntParam("FORMGAS");

            if (value == -1)
            {
                value = 1;
            }

            return value;
        }


        public bool GetParDevLoteMayusculas() //en devoluciones el lote lo pone en mayusculas
        {
            return ReadBoolParam("DEVLOTEMAYUS");
        }

        public bool GetParPedidosCargarProductosAutomatico()
        {
            return ReadBoolParam("PEDCARGARTODO");
        }

        public bool GetParBuscarClientesOnTextChanged() { return ReadBoolParam("CLITEXTCHANGED"); }

        public bool GetParPedidosNoUsarInventariosAlmacenes() { return ReadBoolParam("PEDPRONOAL") && Arguments.Values.CurrentModule == Modules.PEDIDOS; }

        public int GetFormatoVisualizacionProductos()
        {
            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                case Modules.PRODUCTOS:
                case Modules.COBROS:
                    return ReadIntParam("PEDFORMVIS");
                case Modules.DEVOLUCIONES:
                    return ReadIntParam("DEVFORMVIS");
                case Modules.VENTAS:
                    return ReadIntParam("VENFORMVIS");
                case Modules.INVFISICO:
                    return ReadIntParam("INVFORMVIS");
                case Modules.COMPRAS:
                    return ReadIntParam("COMFORMVIS");
                case Modules.COTIZACIONES:
                    return ReadIntParam("COTFORMVIS");
                case Modules.CONTEOSFISICOS:
                    return ReadIntParam("VENFORMVIS");
                default:
                    return 18;
            }
        }

        public int GetFormatoVisualizacionProductosLocal()
        {
            return ReadIntParam("PRODFORMVIS");
        }

        public bool GetParPedidosCambiarDiseñoRow()
        {
            return ReadBoolParam("PEDCAMBIARROW") && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS);
        }

        public void SaveFormatoVisualizacionProductos(int parValor)
        {
            /* RepresentantesParametros rp = new RepresentantesParametros();
        rp.setRepCodigo(SystemState.RepCodigo);
        rp.setParReferencia("PEDFORMVIS");
        rp.setParDescripcion("La forma de visualizacion del row de los productos en pedidos");
        rp.setParValor(String.valueOf(formato));
        rp.setrowguid(UUID.randomUUID().toString());
        rp.setUsuInicioSesion(SystemState.RepCodigo);
        rp.setRepFechaActualizacion(Funciones.getFechaToString());
        try {

            SystemState._dbImpl.execSQL("delete from RepresentantesParametros where LTRIM(RTRIM(UPPER(ParReferencia))) = 'PEDFORMVIS'");

            SystemState._dbImpl.save(rp);
        } catch (Exception e) {
            Log.e(Configuration.TAG, e.getMessage());
        }
*/
            Hash map = new Hash("RepresentantesParametros") { SaveScriptForServer = true };

            RepresentantesParametros result = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor, rowguid from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { "PRODFORMVIS" }).FirstOrDefault();
            if (result != null)
            {
                map.Add("ParDescripcion", "La visualizacion del row");
                map.Add("ParValor", parValor);
                map.Add("RepFechaActualizacion", Functions.CurrentDate());
                map.ExecuteUpdate("rowguid = '" + result.rowguid + "'");
            }
            else
            {
                map.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                map.Add("ParReferencia", "PRODFORMVIS");
                map.Add("ParDescripcion", "La visualizacion del row");
                map.Add("ParValor", parValor);
                map.Add("rowguid", Guid.NewGuid().ToString());
                map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                map.Add("RepFechaActualizacion", Functions.CurrentDate());

                try
                {
                    SqliteManager.GetInstance().Execute("delete from RepresentantesParametros where LTRIM(RTRIM(UPPER(ParReferencia))) = 'PRODFORMVIS'");
                    map.ExecuteInsert();
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }
            }
        }

        public bool GetParCobrosChequesDevueltos() { return ReadBoolParam("ConChqDvt"); }
        public bool GetParCobrosConsultarNotasCreditos() { return ReadBoolParam("COBCONSNC"); }

        /// <summary>
        /// se utiliza para saber capturar el area en el inventario fisico en la cabecera o en el detalle
        /// </summary>
        /// <returns>C para indicar que se usara en la cabecera y D para saber que se usara en el detalle</returns>
        public string GetParInventarioFisicoArea()
        {
            return ReadStringParam("INVFAREA");// && Arguments.Values.CurrentModule == Modules.INVFISICO;
        }

        /// <summary>
        /// se utiliza para saber capturar el area en el modulo de colocacion de mercancias en la cabecera o en el detalle
        /// </summary>
        /// <returns>C para indicar que se usara en la cabecera y D para saber que se usara en el detalle</returns>
        public string GetParColocacionProductosCapturarArea()
        {
            return ReadStringParam("COLPRODAREA");
        }

        public string GetParSACGeneralCamposDisponibles()
        {
            return ReadStringParam("SACGENERAL");
        }

        public string GetParAuditoriasMercadosCamposAUsar() { return ReadStringParam("AMDIALOGCH"); }

        public bool GetParInventariosFisicosPrecios()
        {
            return ReadBoolParam("INVFPRECIO") && Arguments.Values.CurrentModule == Modules.INVFISICO;
        }

        /* usa la columna proGrupoProductos de productos en vez de grupoproductosdetalle */
        public bool GetParGrupoProductosJson()
        {
            return ReadBoolParam("PROGRPPRO");
        }

        public bool GetParPedidosConsultarOfertas()
        {
            var par = ReadBoolParam("CONOFER") || ReadBoolParam("PEDCONOFER");
            return par && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        public bool GetParVentasConsultarOfertas()
        {
            return ReadBoolParam("VENCONOFER") && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PROMOCIONES || Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA);
        }

        public bool GetParPedidosHistoricoFacturas()
        {
            var par = ReadBoolParam("HISFACT") || ReadBoolParam("PEDHISFACT");
            return par && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.COTIZACIONES);
        }

        public bool GetParHistoricosPedidos()
        {
            return ReadBoolParam("HISPED") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        public bool GetParHistoricosPromedio()
        {
            return ReadBoolParam("PEDHISTPROM") && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS);
        }

        public bool GetParPermitirDecimales()
        {
            return ReadBoolParam("PRODEC");
        }

        public bool GetParRevisionOfertas() { return ReadBoolParam("PedOfeRevi"); }


        public bool GetParRevenimiento() { return ReadBoolParam("PedAddReve"); }

        public bool GetParPedidosVisualizarOfertasYDescuentosEnProductosDetalle() { return (ReadBoolParam("PEDSELOFE") && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS)) || GetParCotizacionesOfertasyDescuentos(); }

        public string RutaImagenesProductos()
        {
            var result = ReadStringParam("PRORUTAIMG");

            if (string.IsNullOrWhiteSpace(result))
            {
                var cross = DependencyService.Get<IAppInfo>();

                result = cross.ProductsImagePath();
            }

            return result;
        }

        public bool GetParCambiarVisualizacionProductos() { return ReadBoolParam("PEDLISPRODGRID"); }

        public bool GetParPedidosPreliminar(string tablemodule)
        {
            try
            {

                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select 1 as ParValor from Estados where ltrim(rtrim(Upper(EstTabla))) = ? and EstEstado = ?", new string[] { tablemodule.ToUpper(), "3" });


                return list != null && list.Count > 0;

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return false;

        }

        /// <summary>
        /// al entrar a devoluciones primero se debe de elegir la factura a devolver
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesProductosFacturas() { return Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && ReadBoolParam("DEVPRODFACT"); }

        /// <summary>
        /// en devoluciones al elegir un producto muestra un combobox para elegir la factura
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesFacturaProductoCombo()
        {
            return ReadBoolParam("DEVDETCOMBFACT") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && !GetParDevolucionesProductosFacturas();
        }

        public bool GetParCalcularDescuentoEnBaseItbis() { return ReadBoolParam("RECDESCITBIS"); }

        public string GetParPrefijoFotos()
        {
            var result = ReadStringParam("FOTOPREFIJO");

            if (string.IsNullOrWhiteSpace(result))
            {
                return "";
            }

            return result;
        }

        public bool GetParBusquedaAvanzadaProductos() { return ReadBoolParam("PRODBUSAVANZADA"); }

        public bool GetParVisualizacionGridDetallada() { return ReadBoolParam("PRODGRIDDETALLE"); }

        public bool GetParPresupuestosOnlineDefault() { return ReadBoolParam("PRESUONLINE"); }

        public List<ConnectionInfo> GetParConexiones()
        {
            try
            {
                return SqliteManager.GetInstance().Query<ConnectionInfo>("select ParReferencia as Key, ParDescripcion as Descripcion, ParValor as Url from RepresentantesParametros " +
                    "where UPPER(ltrim(rtrim(ParReferencia))) like ? order by ParDescripcion", new string[] { "CONEXION%" });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new List<ConnectionInfo>();
            }
        }

        public bool GetParEncuestas() { return ReadBoolParam("ENCUESTAS"); }
        public bool GetParEncuestasSoloUnaVezPorCliente() { return ReadBoolParam("CUESTUNO"); }
        public bool GetParInventarioFisicoAceptarProductosCantidadCero() { return Arguments.Values.CurrentModule == Modules.INVFISICO && ReadBoolParam("INVFPRODCERO"); }

        public int GetParVisitaComentarioObligatorio() { return ReadIntParam("COMEOBLI"); }
        public bool GetParPresupuestosClientes() { return ReadBoolParam("PRESU"); }
        public bool GetParDatosClienteEnVisita() { return ReadBoolParam("DATOS"); }
        public string GetVersion() { return ReadStringParam("VERSION"); }
        public bool GetParCambiarClave() { return ReadBoolParam("REPPASS"); }

        public bool GetParAsignarRutas() { return ReadBoolParam("ASIGNARUTA"); }
        public bool GetParReclamaciones() { return ReadBoolParam("RECLAMAC"); }
        public bool GetParPedidosBackOrder() { return ReadBoolParam("PEDBACKORDER"); }


        public bool GetParRecibosSplitNotasDeCredito(string cxcReferencia, out bool allowEditAmount)
        {
            //return ReadBoolParam("RECSPLITNC"); 
            allowEditAmount = false;

            var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select upper(ifnull(t.ttcCaracteristicas, '')) as Referencia from TiposTransaccionesCxc t " +
                "inner join RecibosDocumentosTemp r on r.Sigla = t.ttcSigla " +
                "where r.Referencia = '" + cxcReferencia + "' and (upper(ifnull(t.ttcCaracteristicas, '')) like '%P%' or upper(ifnull(t.ttcCaracteristicas, '')) like '%S%') limit 1", new string[] { });

            foreach(var item in list){
                if(item.Referencia != null && item.Referencia.Contains("S"))
                {
                    allowEditAmount = true;
                    break;
                }
            }

            return list != null && list.Count > 0;
        }

        /// <summary>
        /// Establece el filtro de busqueda combinada por defecto
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaCombinadaPorDefault()
        {
            return ReadBoolParam("PEDBUSC");
        }

        /// <summary>
        /// Parametro para que tipo visita no tenga una opcion elegida por defecto
        /// </summary>
        /// <returns></returns>
        public bool GetParSeleccioneVisita()
        {
            return ReadBoolParam("SELECTVIS");
        }

        /// <summary>
        /// Al crear una visita abre cobros automaticamente:
        /// Con Parvalor = 1 => abre cobros automaticamente
        /// Con Parvalor = 2 => aparece el boton visita cobros en el dialogo de opciones del cliente que crea una visita a cobros automaticamente
        /// </summary>
        /// <returns></returns>
        public int GetParVisitaCobrosAutomatica()
        {
            return ReadIntParam("VISCOBAUT");
        }

        /// <summary>
        /// Activa la visualizacion del rebate en operaciones
        /// y el presupuesto de rebate en Presupuestos
        /// </summary>
        /// <returns></returns>
        public bool GetParVisRebate()
        {
            return ReadBoolParam("VISREBATE");
        }

        /// <summary>
        /// Muestra un switch en la configuracion de Pedidos con el texto "En espera" 
        /// el cual guarda su valor en el campo PedIndicadorRevision
        /// </summary>
        /// <returns></returns>

        public bool GetParPedIndicadorRevision()
        {
            return ReadBoolParam("PEDINDICADORREV");
        }

        /// <summary>
        /// Mostrar referencia de los productos en Pedidos
        /// </summary>
        /// <returns></returns>
        public bool GetReferenciaProductoPed()
        {
            return ReadBoolParam("PEDPROREF");
        }

        public bool GetParProductosProximosAVencer()
        {
            return ReadBoolParam("PROPROXVEN");
        }

        public string GetParProductosProximosAVencerOrderByColumnas()
        {
            return ReadStringParam("ORDERBYCOLUMN");
        }

        public string GetParProductosProximosAVencerGroupByColumnas()
        {
            return ReadStringParam("GROUPBYCOLUMN");
        }

        /*permite capturar el centro de costo en gastos*/
        public bool GetParGastosCapturarCentroDeCosto() { return ReadBoolParam("GASCCOSTO"); }
        public bool GetParPedidosDescuentoManual() { return GetParPedidosDescuentoManualGeneral() <= 0 && (ReadBoolParam("PEDDESCUN") || GetParPedidosOfertasyDescuentosManuales()) && Arguments.Values.CurrentModule == Modules.PEDIDOS; } //permite capturar el % de descuento en pedidos al agregar un producto
        public bool GetParCotizacionesDescuentoManual() { return ReadBoolParam("COTDESCUN") && Arguments.Values.CurrentModule == Modules.COTIZACIONES; } //permite capturar el % de descuento en cotizaciones al agregar un producto
        public bool GetParDevolucionesDescuentoManual() { return ReadBoolParam("DEVDESCUN") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES; } //permite obtener el % de descuento en devoluciones que viene de historicoFacturas
        public bool GetParLipCodigoClientePorSector() { return ReadBoolParam("PROLIPXSEC"); }
        public bool GetParCliEstatusPorSector() { return ReadBoolParam("CLIESTBYSECTOR"); }
        public bool GetParFiltrarProductosPorSector() { return ReadBoolParam("FILPROXSECTOR"); }

        public int GetParConIdFormaPagoContado()
        {
            var valorDefault = ReadIntParam("FORPAGOCONT");
            var valorDefault2 = ReadIntParam("VENFORPAGO");

            if (valorDefault == -1)
            {
                return valorDefault2;
            }

            return valorDefault;
        }
        /// <summary>
        ///  ///Cuando existe este parametro carga la condicion de pago defininida en el parvalor del parametro 
        /// </summary>
        /// <returns></returns>
        public int GetParConidDefault() { return ReadIntParam("PEDCONIDDEF"); }

        public bool GetParConidContadoEn0() { return ReadBoolParam("CONIDCONT"); }

        public bool GetParPedidosRevisionDeDescuentos()
        {
            return ReadBoolParam("PEDDESCREVI") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        public bool GetParComprasTipoPago() { return ReadBoolParam("COMTIPOPAGO") && Arguments.Values.CurrentModule == Modules.COMPRAS; }
        public bool GetParComprasNoAnularSiFueDepositado() { return ReadBoolParam("ComDepNoAn"); }


        /////////////////////////////////////////////////
        //////////Parametros para edicion de precios/////
        /////////////////////////////////////////////////
        //Con parvalor:
        //1 => Permite editar el precio pero no pasarse del ProPrecioMin
        //2 => Permite editar el precio pero que no sea menor al precio del producto
        //3 => permite editar el precio pero que no sea mayor al precio del producto
        //4 => permite editar el precio al que le de la gana
        //5 => permite editar el precio pero no colocar uno menor al ProPrecioMin
        public int GetParEditarPrecio()
        {
            return ReadIntParam("EDITARPRECIOS");
        }

        public bool GetParCotizacionesEditarPrecio() { return ReadBoolParam("CotEditPre") && Arguments.Values.CurrentModule == Modules.COTIZACIONES; }
        public bool GetParPedidosEditarPrecio() { return ReadBoolParam("PedEditPre") && Arguments.Values.CurrentModule == Modules.PEDIDOS; }

        public bool GetParVentasEditarPrecio() { return ReadBoolParam("VenEditPre") && Arguments.Values.CurrentModule == Modules.VENTAS; }

        //1 => permite editar el precio para utilizar un precio negociado tomado como un descuento y calculado tomando en cuenta que el producto incluye el impuesto.
        public bool GetParPedidosEditarPrecioNegconItebis() { return ReadBoolParam("PreNegIteb"); }
        public bool GetParCotizacionesTipos() { return ReadBoolParam("CotTipo") && Arguments.Values.CurrentModule == Modules.COTIZACIONES; }

        public bool GetParDepositosDePushMoney() { return ReadBoolParam("DEPCOMPRAS"); }

        public string GetParDepComprasMontoCajaChica() { return ReadStringParam("CAJACHICA"); }

        public bool GetParPedidosCamposAdicionales()
        {
            return ReadBoolParam("CAMPOSADI") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        public bool GetParDevolucionesCamposAdicionales()
        {
            return ReadBoolParam("DEVCAMPOSADI") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES;
        }

        /// <summary>
        /// Si ruta RUTAVISITATIPO = 2 muestra lo clientes uno a uno y remueve el tab todo y el control de busqueda   
        /// </summary>
        /// <returns></returns>
        public int GetParRutaVisitaTipo()
        {
            return ReadIntParam("RUTAVISITATIPO");
        }

        public int GetParRutaVisitaRepartidor()
        {
            return ReadIntParam("RUTAVISITAREP");
        }

        //Activa Tipo de Trasporte en Pedidos
        public bool GetParTipoTrasporte() { return ReadBoolParam("PEDTIPTRAN"); }


        //Activa CliIDMAster
        public bool GetParCliIdMaster() { return ReadBoolParam("PEDCLIMAST"); }

        /// <summary>
        /// Si ruta RUTAVISITATIPO = 2 y este parametro esta activo se muestra el tab todos y el cuadro de busqueda 
        /// </summary>
        /// <returns></returns>
        public bool GetParRutaVisitaTipo2Mixto()
        {
            return ReadBoolParam("RUTVISTIPO2MIX");
        }

        /// <summary>
        /// DESHABILITA EL CONTROL DE RECHAZAR CARGAS
        /// </summary>
        /// <returns></returns>
        public bool GetParDesactivarRechazoCarga()
        {
            return ReadBoolParam("CARNORECHAZAR");
        }

        /// <summary>
        /// Este parametro deshabilita la impresion en la apertura de cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParNoImpresionAperturaCuadre()
        {
            return ReadBoolParam("CUAAPENOIMPR");
        }

        /// <summary>
        /// Este parametro deshabilita la impresion en la apertura de cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParCargasAutomaticasAperturaCuadre()
        {
            return ReadBoolParam("CUAAPECARAUTO");
        }

        /// <summary>
        /// no permite cerrar el cuadre si hay entregas pendientes
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadresValidarEntregasPendienteParaCerrar()
        {
            return ReadBoolParam("CUACIEVALENT");
        }

        public bool GetParRutaVisitasFechaEstado()
        {
            return ReadBoolParam("RUTVISFECESTADO");
        }

        /* se utiliza para saber la cantidad de dias permitidos al asignar un cliente por asignacion de rutas*/
        public int GetParLimiteDiasParaAsignarRuta()
        {
            return ReadIntParam("RUTDIASLIMITE");
        }

        public bool GetParPedidosOfertasyDescuentosManuales()
        {
            return ReadBoolParam("PEDOFERTAS") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// descuento manual en pedidos
        /// 1- habilita text para digitar el descuento manual a todos los productos
        /// 2- habilita text para digitar el descuento manual a solos los productos marcados en DescuentoRecargos
        /// </summary>
        /// <returns></returns>
        public int GetParPedidosDescuentosManuales()
        {
            return ReadIntParam("PEDDESMAN");
        }

        public bool GetParPedidosOfertasManualesValidarMontoMinimo()
        {
            return ReadBoolParam("PEDOFEPREMIN");
        }

        public bool GetParProspectosCedulaObligatorio()
        {
            return ReadBoolParam("PROSRNC");
        }

        public bool GetParProspectosOcultarCrediticios()
        {
            return ReadBoolParam("SCCREDITICIOS");
        }

        public bool GetParProspectosOcultarReferencias()
        {
            return ReadBoolParam("SCREFERENCIAS");
        }

        public bool GetParProspectosOcultarPropietario()
        {
            return ReadBoolParam("SCPROPIETARIO");
        }

        public bool GetParProspectosOcultarFamiliar()
        {
            return ReadBoolParam("SCFAMILIAR");
        }

        public string GetParProspectosCampoObligatorios()
        {
            return ReadStringParam("PROSCAMPOBLIG");
        }

        public int GetParProspectosCantidadFotoObligatoria()
        {
            return ReadIntParam("PROSIMGOBLIG");
        }

        //devuelve la cantidad de referencias obligatorias dependiendo del tipo de referencia seleccionada
        //el valor es por ejemplo [3|01],[2|02]
        //indicando que se debe de agregar minimo 3 referencias para el tipo de referencia 01 y 2 para el tipo de referencia 02
        //este 01 se obtiene en la tabla usosmultiples con el codigogrupo = SOLREFTIPO
        public List<ProspectosCantidadReferenciasObligatorias> GetParProspectosCantidadReferenciasObligatoria()
        {
            try
            {
                var param = ReadStringParam("PROSREFOBLIG");

                if (param == null)
                {
                    return null;
                }

                var refs = param.Split(',');

                var result = new List<ProspectosCantidadReferenciasObligatorias>();

                foreach (var reference in refs)
                {
                    var raw = reference.Replace("[", "").Replace("]", "");

                    var content = raw.Split('|');

                    if (content.Length != 2)
                    {
                        continue;
                    }

                    var item = new ProspectosCantidadReferenciasObligatorias();
                    item.CodigoReferencia = content[1];

                    int.TryParse(content[0], out int cantidad);

                    item.CantidadObligatoria = cantidad;

                    result.Add(item);
                }

                return result;

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        /// <summary>
        /// Este parametro hace que la validacion por el parametro PROSREFOBLIG solo se haga si el prospecto es a credito
        /// </summary>
        /// <returns></returns>
        public bool GetParProspectosValidarCantidadReferenciasSiEsCredito()
        {
            return ReadBoolParam("PROSREFCRED");
        }

        public bool GetParCerrarVisitaDespuesTransaccion()
        {
            return (ReadBoolParam("VISPEDCER") && Arguments.Values.CurrentModule == Modules.PEDIDOS) || (ReadBoolParam("VENCERVIS") && Arguments.Values.CurrentModule == Modules.VENTAS || (ReadBoolParam("VISCERVIS") && Arguments.Values.CurrentModule == Modules.VISITAS));
        }
        //Sincronizacion automatica
        public bool GetSyncAuto() { return ReadBoolParam("SYNCAUTO"); }
        //Consulta productos no vendidos
        public int GetParProductoNoVendido() { return ReadIntParam("PRONOVEN"); }

        public bool GetParProductosNoVendidosOnline() { return ReadBoolParam("ONPRONOVEN"); }

        public bool GetParDepositosPorSociedad() { return ReadBoolParam("DEPSOC") && GetParSectores() > 0; }
        public bool GetParMenuCobros() { return ReadBoolParam("MENUCOBRO"); }//Habilita los botones de recibo nuevo, estado de cuenta y entrega de documentos en el page de cobros

        /** se usa para saber si el usuario puede cobrar en varias monedas, es decir, si puede agregar formas de pago en varias monedas y hacer la conversion */
        public bool GetParRecibosPuedeCobrarEnVariasMonedas() { return Arguments.Values.CurrentModule == Modules.COBROS && ReadBoolParam("RECMULTIMON"); }
        /// <summary>
        /// Se usa para limitar la cantidad de caracteres en comentarios
        /// </summary>
        /// <returns>Cantidad limite de caracteress</returns>
        public int GetParLenghtComent() { return ReadIntParam("VISCOMLENGHT"); }
        /// <summary>
        /// Busca los productos desde clientesproductosvendidos en inventario fisico
        /// </summary>        
        public bool GetParProductosFromClientesProductosVendidos() { return ReadBoolParam("INVCLIVEN"); }

        public bool GetParColocacionProductosFromClientesProductosVendidos() { return ReadBoolParam("COLPRODCLIVEN"); }
        /// <summary>
        //  verifica cada vez que el representante sincroniza y guarda un registro en la tabla ReplicacionesSuscriptoresSincronizaciones.
        /// </summary>
        /// <returns></returns>
        public bool GetParRepresentanteSaveSincronizaciones() { return ReadBoolParam("REPSAVESYNC"); }
        /// <summary>
        /// PEDDIR con ParValor = 1 la Direccion no es obligatoria,
        ///        Con ParValor = 2 la Direccion es obligatoria
        /// </summary>
        /// <returns></returns>
        public int GetParPedidosDirrecion() { return ReadIntParam("PEDDIR"); }
        public int GetParCotizacionDirrecion() { return ReadIntParam("COTDIR"); }
        /// <summary>
        /// Guardar PedidosDescuentos//
        /// </summary>
        /// <returns></returns>
        public bool GetParSavePedidosDescuentos() { return ReadBoolParam("PEDDESCSAVE"); }

        /*activa el modulo de entrega repartidor en operaciones = 1*/
        /*no activa el modulo de entrega repartidor en operaciones pero permite imprimir todas las transacciones hechas*/
        //public bool GetParEntregasRepartidor(bool IsFOrValidStatud = false)
        //{
        //    return IsFOrValidStatud ? ReadIntParam("ENTREPTRAN") == 2 : ReadIntParam("ENTREPTRAN") == 1;
        //}

        /*1= activa el modulo de entrega repartidor en operaciones para entregar facturas*/
        /*2= no activa el modulo de entrega repartidor en operaciones pero permite imprimir todas las transacciones hechas*/
        /*3= activa el modulo de entrega repartidor en operaciones para entregar pedidos */
        public int GetParEntregasRepartidor()
        {
            return ReadIntParam("ENTREPTRAN");
        }

        //al entrar al modulo de entregas repartidor los producto vienen con las cantidades agregadas
        public bool GetParEntregasRepartidorProductoAutoCantidad()
        {
            return ReadBoolParam("ENTREPPRODAUTO");
        }

        public bool GetParConducesProductoAutoCantidad()
        {
            return ReadBoolParam("CONPRODAUTO");
        }
        /// <summary>
        /// Ocultar el campo unidades en Modal de agregar productos en devoluciones
        /// </summary>
        /// <returns></returns>
        public bool GetParOcultarUnidadesEnDevolucion() { return ReadBoolParam("DEVNOUNID") || ReadBoolParam("DEVOCULU"); }

        /// <summary>
        /// en los formato de impresion carga la empresa por el SecCodigo de la transaccion
        /// </summary>
        /// <returns></returns>
        public bool GetParEmpresasBySector() { return ReadBoolParam("EMPSECCOD"); }

        /// <summary>
        ///  Recibos con orden de pago
        /// </summary>
        /// <returns></returns>
        public bool GetParRecFormOrdenPago() { return ReadBoolParam("ORDENPAGO") && Arguments.Values.CurrentModule == Modules.COBROS; }

        /// <summary>
        /// Permite aplicar descuento por pronto pago a abonos
        /// </summary>
        /// <returns></returns>
        public bool GetParDescuentoAbonos() { return ReadBoolParam("RECDESAB"); }

        public int GetParRecibosNCPorDescuentoProntoPago() { return ReadIntParam("RECNCDPP"); }

        /// <summary>
        /// En pedidos si se da un descuento mayor al campo proMaximoDescuento en productos el descuento se actualiza a este descuento
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosDescuentoMaximo() { return Arguments.Values.CurrentModule == Modules.PEDIDOS && ReadBoolParam("PEDMAXDES"); }

        /// <summary>
        /// Si existe este parametro el estado de cuenta solo va cargar los documentos pendientes sin RCB, si no existe los estados se van a generar tal cual esta la consulta de cobros
        /// </summary>
        /// <returns></returns>        
        public int GetSourceEstadoCuenta()
        {
            return ReadIntParam("EstSinRCB");
        }

        /// <summary>
        /// Cuando existe este parametro se habilita un boton en los rows de productos que muestra el inventario almacen y cantidad
        /// </summary>
        /// <returns></returns>     
        public bool GetShowInventarioAlmacenesEnPedidos()
        {
            return ReadBoolParam("PEDROWINV");
        }

        public bool GetParPedProAlmBySector()
        {
            return ReadBoolParam("PEDALMSEC");
        }

        /// <summary>
        /// Cuando existe este parametro se habilita un boton en los rows de productos que muestra el inventario almacen y cantidaddo existe este parametro se habilita un boton en los rows de productos que muestra el inventario almacen y cantidad
        /// </summary>
        /// <returns></returns>     
        public bool GetShowInventarioAlmacenesEnCotizaciones()
        {
            return ReadBoolParam("COTROWINV");

        }

        public string GetParUnidadesMedidasVendedorUtiliza()
        {
            var value = ReadStringParam("UNMCODIGO");

            if (!string.IsNullOrWhiteSpace(value))
            {

                string[] list = value.Split('|');

                string result = "";

                foreach (string q in list)
                {
                    result += (!string.IsNullOrWhiteSpace(result) ? ", " : "") + "'" + q + "'";
                }

                return result.ToLower();
            }

            return null;
        }

        /// <summary>
        /// Habilita el telefono del cliente en ClientesPage y habilita un boton que llama Marcar telefono
        /// </summary>
        /// <returns></returns>
        public bool GetCallCliTelefono()
        {
            return ReadBoolParam("DIALClITELEFONO");
        }

        /// <summary>
        /// En pedidos, ventas, cotizaciones, devoluciones, pushmoney las cantidades de los productos se convierten en la unidad mas pequena
        /// y luego se hace el calculo de cuantas cajas y unidades son al momento de guardar la transaccion, y en el row de los productos
        /// sale el precio caja y el precio unitario
        /// </summary>
        /// <returns></returns>
        public bool GetParCajasUnidadesProductos()
        {
            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros " +
                "where upper(trim(ParReferencia)) = upper('PProDetDIS') and ParValor in (1, 2, 3) ", new string[] { });


            return list != null && list.Count > 0 && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.COTIZACIONES);
        }

        public bool GetParConvertirCajasAUnidadesSinDetalleProductos()
        {
            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros " +
               "where upper(trim(ParReferencia)) = upper('PProDetDIS') and ParValor in (3) ", new string[] { });

            return list != null && list.Count > 0 && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES || Arguments.Values.CurrentModule == Modules.COTIZACIONES);
        }

        /// <summary>
        /// al label cantidad del dialog de agregarProductos en pedidos le cambia la descripcion al parvalor del parametro
        /// </summary>
        /// <returns></returns>
        public string GetParLabelCantidadDescripcion()
        {
            return ReadStringParam("LBLCANTIDAD");
        }

        /// <summary>
        /// al label cantidad del dialog de agregarProductos en conteosfisicos le cambia la descripcion al parvalor del parametro
        /// </summary>
        /// <returns></returns>
        public string GetParLabelCantidadDescripcionConteoFisico()
        {
            return ReadStringParam("LBLCANTIDACONTF");
        }

        /// <summary>
        /// al label cantidad del dialog de agregarProductos en auditorias de precios le cambia la descripcion al parvalor del parametro
        /// </summary>
        /// <returns></returns>
        public string GetParLabelCantidadDescriptionAudPrecio()
        {
            var result = "";

            if (Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS)
            {
                result = ReadStringParam("AUDPLBLCANTIDAD");
            }
            else
            {
                result = "";
            }

            return result;
        }

        /// <summary>
        /// Si existe este parametro el conteo de semanas de la ruta visitas se va a tomar desde la tabla SemanasAnios
        /// </summary>
        /// <returns></returns>
        public bool GetParSemanasAnios()
        {
            return ReadBoolParam("SEMANASANIOS");
        }
        /// <summary>
        /// Habilita Modulo de tareas en Operaciones
        /// </summary>
        /// <returns></returns>
        public bool GetParTareas()
        {
            return ReadBoolParam("TAREAS");
        }

        /// <summary>
        /// Con este parametro se agrega un picker en pedidos configurar donde se cargan las monedas disponibles.
        /// Por default tiene la moneda del cliente seleccionada y cuando se cambia busca los productos con el moncodigo seleccionado
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidoMultiMoneda(bool onlyForClient = false)
        {
            /* if(Arguments.Values.CurrentModule == Modules.COTIZACIONES)
             {
                 return ReadBoolParam("CotMultiMon");
             }
             else
             {
                 return ReadBoolParam("PedMultiMon");
             }*/


            var parReferencia = Arguments.Values.CurrentModule == Modules.COTIZACIONES ? "CotMultiMon" : "PedMultiMon";


            var values = "'1', '2'";

            if (onlyForClient)
            {
                values = "'2'";
            }

            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros " +
               "where upper(trim(ParReferencia)) = upper('" + parReferencia.Trim() + "') and ParValor in (" + values + ") ", new string[] { });

            return list != null && list.Count > 0;
        }

        /// <summary>
        /// solo permite seleccionar la moneda que el cliente tenga asignada en pedidos y cotizaciones
        /// </summary>
        /// <returns></returns>
        public bool GetParMultiMonedaSoloMonedaCliente()
        {
            if (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                return GetParPedidoMultiMoneda(true);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Si tiene este parametro realiza la busqueda de los productos sin importar el moncodigo y realiza la conversion a la moneda del cliente
        /// </summary>
        /// <returns></returns>
        public bool GetParConvertiPedidoMultiMoneda()
        {
            return ReadBoolParam("PEDCONVMONEDA");
        }

        /// <summary>
        /// activa el modulo de conteo fisico
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoFisicoVendedor()
        {
            return ReadBoolParam("INVFVENDEDOR") && GetParCuadres() > 0;
        }

        /// <summary>
        /// muestra en el home en btn de cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParHomeBtnCuadres()
        {
            return ReadBoolParam("HOMECUADRE");
        }

        public bool GetParHomeBtnCargas()
        {
            return ReadBoolParam("HOMECARGAS");
        }

        public bool GetParHomeBtnReportes()
        {
            return ReadBoolParam("HOMEREPORTE");
        }

        public bool GetParHomeBtnConsultaInventarios()
        {
            return ReadBoolParam("HOMECONINV");
        }

        public bool GetParHomeBtnRequisicionInventario()
        {
            return ReadBoolParam("HOMEREQINV");
        }

        public bool GetParHomeBtnDepositos()
        {
            return ReadBoolParam("HOMEDEPOS");
        }
        public bool GetParHomeBtnProductos()
        {
            return ReadBoolParam("HOMEPROD");
        }
        public bool GetParHomeBtnPresupuestos()
        {
            return ReadBoolParam("HOMEPRES");
        }

        public bool GetParHomeBtnConteoFisico()
        {
            return ReadBoolParam("HOMEINVF") && GetParConteoFisicoVendedor();
        }

        public bool GetParPedidosIniciarEnConfiguracion()
        {
            return (ReadBoolParam("PEDCONFIGINI") && Arguments.Values.CurrentModule == Modules.PEDIDOS)
                || (ReadBoolParam("VENCONFIGINI") && Arguments.Values.CurrentModule == Modules.VENTAS)
                || (ReadBoolParam("COTCONFIGINI") && Arguments.Values.CurrentModule == Modules.COTIZACIONES);
        }

        /// <summary>
        /// solo permite abrir un cuadre al dia
        /// </summary>
        /// <returns></returns>
        public bool GetParUnSoloCuadrePorDia()
        {
            return ReadBoolParam("CUAUNIDIA");
        }

        public string GetParRecibosDialogDetalleBtnDetalleDescripcion()
        {
            return ReadStringParam("RECBDETALLETIT");
        }

        public bool GetParSincronizarAlAbrirCuadre()
        {
            return ReadBoolParam("SYNABRIRCUA");
        }

        public bool GetParSincronizarAlCerrarCuadre()
        {
            return ReadBoolParam("SYNCERRCUA");
        }

        /// <summary>
        /// al intentar hacer un conteo fisico pide la clave de un auditor
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoFisicoPorAuditor()
        {
            return ReadBoolParam("INVFAUDITOR");
        }

        /// <summary>
        ///Obliga al vendedor a digitar una cantidad que sea multiplo del valor que este en el campo: ProCantidadMultiploVenta
        ///Ej: ProCantidadMultiploVenta = 3. solo podra digitar(3, 6, 9,12....) cantidades del producto
        /// </summary>
        /// <returns></returns>

        public bool GetParProductoMultiplos()
        {
            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                    return ReadBoolParam("PEDPRODMUL", true);
                case Modules.VENTAS:
                    return ReadBoolParam("VENPRODMUL", true);
                case Modules.COTIZACIONES:
                    return ReadBoolParam("COTPRODMUL", true);
                case Modules.COMPRAS:
                    return ReadBoolParam("COMPRODMUL");
                case Modules.DEVOLUCIONES:
                    return ReadBoolParam("DEVPRODMUL");
                case Modules.INVFISICO:
                    return ReadBoolParam("INVPRODMUL");
                default:
                    return false;

            }

        }

        /// <summary>
        /// Permite deshabilitar el modificar la condicion de pago si no se tiene crédito
        /// </summary>
        /// <returns></returns>

        public bool GetDeshabilitarConfig()
        {
            return ReadBoolParam("PEDCONFDESH");
        }

        /// <summary>
        /// permitir hacer recibos sin facturas
        /// </summary>
        /// <returns></returns>
        public bool GetParReciboSinAplicacion()
        {
            return ReadBoolParam("RECACuenta");
        }

        /// <summary>
        /// Habilita un EDITEXT en el modal de agregar productos para agregar ofertas de forma manual en cotizaciones
        /// </summary>
        /// <returns></returns>
        public bool GetParCotOfertasManuales() //Nombre anterior de parametro: OFEMANUAL
        {
            return ReadBoolParam("COTOFEMANUAL") && (Arguments.Values.CurrentModule == Modules.COTIZACIONES);
        }

        /// <summary>
        /// Habilita un EDITEXT en el modal de agregar productos para agregar ofertas de forma manual en ventas
        /// </summary>
        /// <returns></returns>
        public bool GetParVenOfertasManuales()
        {
            return ReadBoolParam("VENOFEMANUAL") && (Arguments.Values.CurrentModule == Modules.VENTAS);
        }

        /// <summary>
        /// Habilita un EDITEXT en el modal de agregar productos para agregar ofertas de forma manual en pedidos
        /// </summary>
        /// <returns></returns>
        public bool GetParPedOfertasManuales()
        {
            return ReadBoolParam("PEDOFEMANUAL") && (Arguments.Values.CurrentModule == Modules.PEDIDOS);
        }

        /// <summary>
        /// activa la funcionalidad de usar un almacen para despacho y un almacen para devolucionu
        /// </summary>
        /// <returns></returns>
        public bool GetParUsarMultiAlmacenes()
        {
            return ReadBoolParam("MULTIALMACEN") && Arguments.Values.CurrentModule != Modules.AUDITORIAPRECIOS && Arguments.Values.CurrentModule != Modules.TRASPASOS && Arguments.Values.CurrentModule != Modules.PROMOCIONES;
        }

        /// <summary>
        /// Id del almacen de despacho (que es el usado en entrega facturas o entregas de pedidos)
        /// </summary>
        /// <returns></returns>
        public int GetParAlmacenIdParaDespacho()
        {
            return ReadIntParam("DESALMID");
        }

        public int GetParAlmacenIdParaMalEstado()
        {
            return ReadIntParam("MALESTADOAlMID");
        }

        /// <summary>
        /// Id del almacen de devolucion (que es el usado en ventas)
        /// </summary>
        /// <returns></returns>
        public int GetParAlmacenIdParaDevolucion()
        {
            return ReadIntParam("DEVALMID");
        }
        /// <summary>
        /// Tipo 1: Porcentual
        /// tipo 2: por monto
        /// </summary>
        /// <returns></returns>
        public int GetParADVALOREMTIPO()
        {
            return ReadIntParam("ADVALOREMTIPO");
        }
        /// <summary>
        ///si tiene una factura pendiente lo bloquea para ventas
        /// </summary>
        /// <returns></returns>
        public bool GetParBloqueVentasLimiteDecreditoFacturasVencidas()
        {
            return ReadBoolParam("BLOCKVENLCFV");
        }

        /// <summary>
        /// limite de credito en ventas
        /// </summary>
        /// <returns></returns>
        public bool LimiteCreditoInVentas()
        {
            return ReadBoolParam("VENLIMCRE") && (Arguments.Values.CurrentModule == Modules.VENTAS);
        }

        /// <summary>
        /// 1- Bloquear la condicion de pago si ya se selecciono uno o mas productos. con el fin de que no se pueda modificar mientras tenga productos seleccionados.
        /// 2- Bloquea la condicion de pago al entrar a ventas.
        /// 3- Bloquea la condicion de pago al entrar a pedidos.
        /// </summary>
        /// <returns></returns>
        public int BloquearCondicionPago()
        {
            return ReadIntParam("CONPLOCK");
        }

        /// <summary>
        /// Oculta el tab de configurar
        /// </summary>
        /// <returns></returns>
        public bool OcultarTabConfigurar()
        {
            return ReadBoolParam("COMHIDETABCONF") && (Arguments.Values.CurrentModule == Modules.COMPRAS || Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Modules.INVFISICO);
        }

        /// <summary>
        /// mostrar productos solo en inventario
        /// </summary>
        /// <returns></returns>
        public bool GetProductosEnInventario()
        {
            return ReadBoolParam("VENPROINV");
        }
        /// <summary>
        /// bloquea el crear una visita si tiene cargas pendientes.
        /// </summary>
        /// <returns></returns>
        public bool GetParCargaObligatoria()
        {
            return ReadBoolParam("CARGAOBLI");
        }

        /// <summary>
        /// bloquea el crear una visita si tiene cargas negativas pendientes.
        /// </summary>
        /// <returns></returns>
        public bool GetParCargaNegativaObligatoria()
        {
            return ReadBoolParam("CARGANEGOBLI");
        }

        public bool GetCantidadDetalleInventario()
        {
            return ReadBoolParam("CANTDETINV");
        }

        /// <summary>
        /// Para imprimir la fecha de impresion al lado de el nombre de la empresa en los reportes.
        /// </summary>
        /// <returns></returns>
        public bool Getdateprintheader()
        {
            return ReadBoolParam("DATEPRINTH");
        }

        /// <summary>
        /// Fija el limite de facturas Pendientes permitidas para hacer una nueva venta. ParValor = Limite.ej: si es 1, solo podra realizar una venta mientras el cliente tenga 1 factura pendiente.si tiene mas lo bloquea.
        /// </summary>
        /// <returns></returns>
        public int GetNoVentaFacturapendiente()
        {
            return ReadIntParam("NOVENFACTPEN");
        }

        /// <summary>
        /// Dependiente no obligatorio
        /// </summary>
        /// <returns></returns>
        public bool GetDependienteNoObligatorio()
        {
            return ReadBoolParam("DEPNOOBL") && (Arguments.Values.CurrentModule == Modules.COMPRAS);
        }

        /// <summary>
        /// ocultar modulo visita virtual
        /// </summary>
        /// <returns></returns>
        public bool GetNoVisitaVirtual()
        {
            return ReadBoolParam("NOVISVIRT");
        }

        /// <summary>
        /// buscar cliente al escribir
        /// </summary>
        /// <returns></returns>
        public bool GetBuscarClienteAlEscribir()
        {
            return ReadBoolParam("SEARCHCLIWR");
        }

        /// <summary>
        /// descuento por listaprecios
        /// </summary>
        /// <returns></returns>
        public bool GetParPedDescLip()
        {
            return ReadBoolParam("PedDescLip") && (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.PEDIDOS);
        }

        public bool GetDescuentoxPrecioNegociado()
        {
            return ReadBoolParam("DESPRECNEG") && (Arguments.Values.CurrentModule == Modules.PEDIDOS);
        }
        /// <summary>
        /// ACTIVAR SEGUNDO TIPO DE FILTRO DE PRODUCTOS
        /// </summary>
        /// <returns></returns>
        public bool GetSecondFilterProduct()
        {
            return ReadBoolParam("SECFILTPRO");
        }

        //en recibos pide una autorizacion para agregar formas de pago transferencias
        public bool GetParRecibosAutorizarPagoTransferencia()
        {
            return Arguments.Values.CurrentModule == Modules.COBROS && ReadBoolParam("RECTRANSAUT");
        }

        /// <summary>
        /// NUMERO DE CHEQUES PERMITIDOS PARA VALIDAR CUANDO HASTA QUE NUMERO TIENE PERMITIDO PARA HACER VENTAS A CREDITO
        /// </summary>
        /// <returns></returns>
        public int GetNumberChkDPermit()
        {
            return ReadIntParam("NUMCHKDPER");
        }
        /// <summary>
        /// no permite cerrar el cuadre si hay clientes sin visitar
        /// </summary>
        /// <returns></returns>
        public string GetParCuadresValidarClientesVisitadosParaCerrar()
        {
            return ReadStringParam("CUAVALCIERRE");
        }

        /// <summary>
        /// LOTE DINAMICO EN DEVOLUCINES, SELECCION O MANUAL
        /// </summary>
        /// <returns></returns>
        public bool GetParDevDynamicLote()
        {
            return ReadBoolParam("DEVLOTEDYNAMIC");
        }


        public bool GetParRecepcionDevolucion()
        {
            return ReadBoolParam("RECEPDEV");
        }

        /// <summary>
        /// PARA DEFINIR LA CANTIDAD DE CAMBIOS QUE SE ENVIARAN AL SERVIDOR AL SINCRONIZAR
        /// </summary>
        /// <returns></returns>
        public int GetCountCambios()
        {
            return ReadIntParam("COUNTCAMB");
        }

        /// <summary>
        /// Anular la ultima venta
        /// </summary>
        /// <returns></returns>
        public int GetAnularUltimaVenta()
        {
            return ReadIntParam("ANULVENCA");
        }

        /// <summary>
        /// parametro para indicar cual es el tipo de comprobante que es fiscal usado en el modulo de entregas repartidor
        /// para si es fiscal los productos devueltos se muevan al almacen de productos de no venta
        /// </summary>
        /// <returns></returns>
        public string GetParTipoComprobanteFiscal()
        {
            return ReadStringParam("COMPFISCAL");
        }

        public int GetAlmacenIdProductosNoDevolucion()
        {
            return ReadIntParam("NODEVALMID");
        }
        ////Metodo usado especificar motivo rechazo via MotivoDevolucion general entrega de facturas y pedidos
        public bool GetParEntregasRepartidorMotivoDevolucion()
        {
            return ReadBoolParam("ENTREPMOTDEV");
        }

        ////Metodo usado especificar motivo rechazo via MotivoDevolucion solo en entrega de pedidos
        public bool GetParEntregasRepartidorRechazadasMotivoDevolucion()
        {
            return ReadBoolParam("ENTREPRECH");
        }

        ////Metodo usado especificar motivo rechazo via tiposMensaje solo en entrega de pedidos
        public bool GetParEntregasRepartidorMensajeRechazado()
        {
            return ReadBoolParam("ENTREPMENDEV");
        }

        public bool GetParEntregasRepartidorPorSector()
        {
            return ReadBoolParam("ENTREPSECTOR") && Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR;
        }

        /// <summary>
        /// Crear factura y cuentaxCobrar al vendedor por la diferencia de productos al hacer el conteo fisico
        /// </summary>
        /// <returns></returns>
        public bool GetParCrearFacturaByConteoFisico()
        {
            return ReadBoolParam("FACTCONTF");
        }

        /// <summary>
        /// Parametro para que cuando  falten ofertas por dar en ofertas mancomunadas 
        /// no permita continuar con la venta o pedido sin aceptar todas las ofertas
        /// </summary>
        /// <returns></returns>
        public bool GetParOfertasFaltantesObligatorias()
        {
            return ReadBoolParam("OFEMANOBL");
        }

        /// <summary>
        /// en entregas repartidor si un producto no se da completo se tumba la oferta dada por este, se enlaza el OfeID del producto oferta que es el ProID del producto
        /// por la cual se dio la oferta
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasRepartidorValidarOfertas()
        {
            return ReadBoolParam("ENTREPOFEVAL") && Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR;
        }

        public bool GetParConduces()
        {
            return ReadBoolParam("CONDUCES");
        }

        /// <summary>
        /// Al momento se imprimir, si se tiene este parametro, no imprime el logo de la empresa
        /// </summary>
        /// <returns></returns>
        public bool GetParNoLogo()
        {
            return ReadBoolParam("NOLOGO");
        }

        /// <summary>
        /// Parametro para definir el cliente que se usara para el vendedor a la hora de hacerle una factura al vendedor por la diferencia de un conteo fisico
        /// (esto sera con parametro hasta que se incluya la columna cliid en la tabla de representantes).
        /// </summary>
        /// <returns></returns>
        public int GetParClienteForRepresentantes()
        {
            return ReadIntParam("CLIFORREP");
        }

        /// <summary>
        /// Cargas obligatorias, no permitir rechazar las cargas.
        /// </summary>
        /// <returns></returns>
        public bool GetParCargasObligatorias()
        {
            return ReadBoolParam("CAROBL");
        }


        /// <summary>
        /// habilita modulo de cambios de mercancia
        /// </summary>
        /// <returns></returns>
        public bool GetParCambiosMercancia()
        {
            return ReadBoolParam("CAMBIOS");
        }

        /// <summary>
        /// Al guardar una entrega y intentar imprimirla imprime todas las entregas que se hayan hecho en la visita de manera automatica
        /// </summary>
        /// <returns></returns>
        public bool GetParImprimirEntregasDeLaVisita()
        {
            return ReadBoolParam("ENTIMPVIS");
        }


        /// <summary>
        /// si existe el nombre del modulo de conduces se iguala a este parametro
        /// </summary>
        /// <returns></returns>
        public string GetParConducesNombreModulo()
        {
            return ReadStringParam("CONNOMBRE");
        }


        /// <summary>
        /// Para saldar un recibo automaticamente creado por la venta de contado
        /// </summary>
        /// <returns></returns>
        public bool GetParVentaSaldarReciboAutomatico()
        {
            return ReadBoolParam("VENRECSALAUTO");
        }


        public bool GetParVentasConReciboObligatorio()
        {
            return ReadBoolParam("VENRECOBLI");
        }

        /// <summary>
        /// al entrar a operaciones si se tienen entregas pendientes entra a la pantalla de entregas automaticamente
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasEntrarAutomatico()
        {
            return ReadBoolParam("ENTAUTOINI");
        }

        /// <summary>
        /// al cerrar un cuadre resta del inventario los productos del cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadrarInventarioAlCerrarCuadre()
        {
            return ReadBoolParam("CUAINV");
        }

        // Si en cantidades se utiliza monto por libras detalladas y no usa unidades
        public bool GetCantidadxLibras()
        {
            return ReadBoolParam("CANTLIBRA");
        }


        /// <summary>
        /// al guardar una entrega guarda un recibo por los productos entregados
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasRepartidorGuardarRecibo()
        {
            return ReadBoolParam("ENTREPREC");
        }

        /// <summary>
        /// al guardar un conduce guarda un recibo por los productos entregados
        /// </summary>
        /// <returns></returns>
        public bool GetParConducesGuardarRecibo()
        {
            return ReadBoolParam("CONREC");
        }

        /// <summary>
        /// al guardar la entrega si se tienen entregas de otro sector se entra automaticamente al otro sector
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasRepartidorEntrarEntregasAutomaticamenteOtrosSector()
        {
            return ReadBoolParam("ENTREPAUTOSEC") && GetParSectores() > 0;
        }


        /// <summary>
        /// para mostrar el id del producto que dio esa oferta
        /// </summary>
        /// <returns></returns>
        public bool GetParProIDOferta()
        {
            //return ReadBoolParam("ProIDOferta");
            return ReadBoolParam("PEDOFESHOWPROID");
        }


        /// <summary>
        /// para cargar todos los centros de distribuccion decada cliente que lo tenga 
        /// </summary>
        /// <returns></returns>

        public bool GetParPedidosCentroDistribucion()
        {
            return ReadBoolParam("PEDCENDIST") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// filtrar los centros de distribucion por sector, valor de default = 1
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosCentrosDistribucionFiltrarPorSector()
        {
            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(ParValor, '1') as ParValor from RepresentantesParametros " +
               "where UPPER(ltrim(rtrim(ParReferencia))) = ? ", new string[] { "PEDCENDISTSEC" });

            if (list != null && list.Count > 0)
            {
                return list[0].ParValor.Equals("1");
            }


            return true;
        }
        /// <summary>
        /// se utiliza para mostrar o ocurtar el limite de credito de cliente dependiendo si cumple o no con el campo
        /// si este parametro VERCREDCLI, esta activo entoncces mostrar este
        /// </summary>
        /// <returns></returns>
        /// public int GetParVisitaVerClienteCredito() { return ReadIntParam("VERCREDCLI"); }
        public bool GetParOcultarLimiteCredito()
        {
            return ReadBoolParam("CLIOCULIMCRE");

        }


        /// <summary>
        /// APLICAR NOTA CREDITO AUTOMATICA A FACTURA
        /// </summary>
        /// <returns></returns>
        public bool GetParNotaCreditoAutoFactura()
        {
            return ReadBoolParam("NOTCREAUTFAT");
        }

        /// <summary>
        /// aplicar todos los productos ofertados en oferta mancomunada
        /// </summary>
        /// <returns></returns>
        public bool GetParItemsOfertadosObligatorios()
        {
            return ReadBoolParam("OFEITEMOBL");
        }

        /// <summary>
        /// No aceptar el recibo si hay facturas pentientes mas viejas sin saldar
        /// parvalor = 1 no permite
        /// parvalor = 2 pide autorizacion
        /// </summary>
        /// <returns></returns>
        public int GetParNoAceptarReciboSiHayFacturasViejasSinSaldar()
        {
            return ReadIntParam("RECOLDSINSAL");
        }

        public bool GetParLimCre()
        {
            return ReadBoolParam("CLIOCULIMCRE");
        }

        /// <summary>
        /// Se ultiliza para ocultar la opcion de asignar todos los clientes en la asignacion de rutas visitas
        /// </summary>
        /// <returns></returns>
        public bool GetParOcultarAsignarTodosAsignacionRuta()
        {
            return ReadBoolParam("RUTOCULASITODO");
        }

        /// <summary>
        /// en pedidos indica el % para descuento por pronto pago
        /// </summary>
        /// <returns></returns>
        public int GetParPedidosDpp()
        {
            return ReadIntParam("PEDDPP");
        }

        /// <summary>
        /// Aparece switch para identificar si incluir las ordenes de pago o no en los depositos
        /// </summary>
        /// <returns></returns>
        public bool GetParDepositosOrdenPago()
        {
            return ReadBoolParam("DEPORDPOPC");
        }
        /// <summary>
        ///  Muestra el combo para seleccionar el cliente despues del cual se va a visitar 
        ///  tomando en cuenta la columna CliRutPosicion del maestro de clientes
        /// </summary>

        public bool GetParProspectoCliRutPosicion()
        {
            //return ReadBoolParam("VISITAANTES");
            return ReadBoolParam("PROCLIRUTPOSIC");

        }

        /// <summary>
        /// validar campo RNC o Cedula en Prospectos
        /// </summary>

        public bool GetValidarRncProspectos()
        {
            return ReadBoolParam("PROSVALRNC");
        }

        /// <summary>
        /// 1- Advertencia si el pedido excede el limite de credito Disponible (Limite de Credito - Balance Pendiente)
        /// 2- Valida que el Monto del Pedido no supere el Crédito Disponible (Limite de Credito - Balance Pendiente) del cliente. No permite hacer el pedido pero habilita la autorización.
        /// 3- Valida que el cliente tenga crédito > 0 si la condición de pago del Cliente es a Crédito para entrar al Módulo de Pedidos pero habilita la autorización.
        /// 4- Valida que el Monto del Pedido no supere el Crédito Total (Limite de Credito) del cliente. No permite hacer el pedido pero habilita la autorización.
        /// </summary>
        /// <returns></returns>
        public int LimiteCreditoEnPedidos()
        {
            return ReadIntParam("PEDLIMCRE");
        }


        /// <summary>
        /// No permite hacer pedido si supera limite de credito 
        /// </summary>
        /// <returns></returns>
        /// Se dejo de usar se cambia por PEDLIMCRE
        //public bool LimiteCreditoNoPedidos()
        //{
        //    return ReadBoolParam("PEDNOLIMCRE") && (Arguments.Values.CurrentModule == Modules.PEDIDOS);
        //}

        /// <summary>
        /// valida rnc segun comprobante fiscal 01 -02
        /// </summary>
        /// <returns></returns>
        public bool GetValidaRNCWithComprobanteFiscal()
        {
            return ReadBoolParam("PEDNORNC") && (Arguments.Values.CurrentModule == Modules.PEDIDOS);
        }

        /// <summary>
        /// valor maximo para pedido con consumidor final
        /// </summary>
        /// <returns></returns>
        public double GetValorMaximoPedidoWithConsumidorFinal()
        {
            return ReadDoubleParam("PEDMONMAXCF");
        }



        /// <summary>
        /// permitir cantidades decimales en el productos si en prodatos3 tiene una A
        /// </summary>

        public bool GetParProdustosDecimalValidadosDeProDatos3()
        {
            return ReadBoolParam("PRODECDATOS3");
        }


        /// <summary>
        /// Este tipo de visita no permite ningun tipo de transaccion, solo comentario funciona con el parametro COMEOBLI de 
        /// comentario obligatorio
        /// </summary>
        /// <returns></returns>
        public bool GetParSoloComentarioEnTipoVisitaNoVisitado()
        {
            return ReadBoolParam("VISTIP4NOTRANS");
        }


        /// <summary>
        /// Si ruta ConTranBusCli = 2 muestra una forma diferente de busqueda
        /// </summary>
        /// <returns></returns>
        public int GetParConsultaTrancacionBusquedaDiferente()
        {
            return ReadIntParam("CONTRABUSCLI");

        }


        /// <summary>
        /// EN pedidos habilita las promociones
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosPromociones()
        {
            return ReadBoolParam("PEDPROMOCION") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// EN pedidos la agregar un producto si se especifica un descuento o ofertas manuales valida que el valor total de descuento + oferta este dentro del rango
        /// especificado en la columna LipRangoPrecioMinimo de la tabla ListaPrecios
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosRangoPrecioMinimoOfertasManuales()
        {
            return ReadBoolParam("PEDRANMINPRE") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        //Posee el mismo comportamiento de la descripcion del PEDRANMINPRE pero fuera de las Ofertas Manuales
        public bool GetParPedidosRangoPrecioMinimo()
        {
            return ReadBoolParam("PEDMINPRE") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// VALIDAR SI UN RECIBO TIENE DIFERENCIA O NO PARA SALDARLO AL CAMBIA LA TAZA CON AUTORIZACION
        /// </summary>
        /// <returns></returns>
        public double GetParRecibosAutorizacionTazaFactura()
        {
            return ReadIntParam("RECAUTTAZA");
        }

        /// <summary>
        /// Encuesta obligatoria
        /// </summary>
        /// <returns></returns>
        public bool GetParEncuestaObligatoria()
        {
            return ReadBoolParam("ENCUOBL");
        }

        /// <summary>
        /// Autorizacion y comentario obligatorio al anular un recibo
        /// </summary>
        /// <returns></returns>
        public bool GetParAutorizacionAnularRecibo()
        {
            return ReadBoolParam("RECAUTANU");
        }

        /// <summary>
        /// En ruta visitas los clientes aparecen uno por uno de acuerdo al orden de las visitas.
        /// </summary>
        /// <returns></returns>
        public bool GetParRutaVisitasOnebyOne()
        {
            return ReadBoolParam("RUTAVISUNO");
        }

        /// <summary>
        /// Alerta en pedidos cuando el cliente tiene una factura con mas de 30 dias
        /// </summary>
        /// <returns></returns>
        public bool GetParAlertaFacturas30dias()
        {
            return ReadBoolParam("PEDALERT30");
        }

        /// <summary>
        /// Alerta en pedidos cuando el cliente tiene una factura con mas de 30 dias
        /// </summary>
        /// <returns></returns>
        public bool GetParAlertaFacturasVencida()
        {
            return ReadBoolParam("PEDALERTFATVEN");
        }

        /// <summary>
        /// Alerta en pedidos cuando el cliente tiene una factura con mas de 60 dias y no le permite hacer un pedido
        /// </summary>
        /// <returns></returns>
        public bool GetParAlertaFacturas60dias()
        {
            return ReadBoolParam("PEDALERT60");
        }

        /// <summary>
        /// Autorizar para realizar un pedido en caso que el cliente tenga cheques diferidos pendientes de depositar
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosAutorizarChequediferido()
        {
            return ReadBoolParam("PEDAUTCHKDIF");
        }

        /// <summary>
        /// Este parametro definirá la cantidad días sobre la fecha actual para llenar la columna pedFechaEntrega
        /// </summary>
        /// <returns></returns>
        public int GetParPedidosDiasEntrega()
        {
            return ReadIntParam("PEDDIASENTREGA");
        }



        /// <summary>
        /// en ventas activa que se captura la cantidad de piezas
        /// </summary>
        /// <returns></returns>
        public bool GetParVentasCantidadPiezas()
        {
            return ReadBoolParam("VENCANTPIEZAS") && Arguments.Values.CurrentModule == Modules.VENTAS;
        }

        /// <summary>
        /// en ventas activa que se pueda usar la calculadora para saber de cuanto debe ser el monto del pedido a credito para que la app lo deje continuar
        /// </summary>
        /// <returns></returns>
        public bool GetParVentasCalculadoraDeNegociacion(bool fromOperaciones)
        {
            return ReadBoolParam("VENCALPAGO") && (fromOperaciones || Arguments.Values.CurrentModule == Modules.VENTAS);
        }

        public bool GetParConteoFisicoAjustarFaltante()
        {
            return ReadBoolParam("CONTFAJUSTAR") && Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && GetParCrearFacturaByConteoFisico();
        }

        /// <summary>
        /// Si esta activo oculta el lote y la fecha de vencimiento del lote
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParDevolucionesOcultarLoteYFechaVencimiento() { return ReadBoolParam("DEVOCULOTE"); }

        public bool GetParDevolucionesOcultarFechaVencimiento() { return ReadBoolParam("DEVNOFEC") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES; }

        /// <summary>
        /// Si esta activo busca si el cliente ha tenido cheques devueltos en el tiempo que coloque en parvalor (dias) ejemplo 1 año // parvalor : 365
        /// </summary>
        /// <returns>bool</returns>
        public int GetParCobrosChequesDevueltosNtiempo() { return ReadIntParam("COBALECKDEV"); }


        public bool GetParDevolucionesOcultarCantidadOferta() { return ReadBoolParam("DEVOCUCAOF"); }
        public bool GetParDevolucionesOcultarFactura() { return ReadBoolParam("DEVOCUFAC"); }

        // En pedidos habilita la hora para la entrega
        public bool GetHabilitarHoraenEntrega()
        {
            return ReadBoolParam("PEDHORA");
        }

        public bool GetParCuadresValidarVentasPendienteParaCerrar()
        {
            return ReadBoolParam("CUAVENTVAL");
        }

        /// <summary>
        /// Permite que la forma de pago efectivo no se utilice con alguna más
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParCobrosEfectivoSolo() { return Arguments.Values.CurrentModule == Modules.COBROS && ReadBoolParam("COBEFESOLO"); }

        /// <summary>
        /// Los depositos a Banco solo será el efectivo
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParDepositosBancoEfectivo() { return ReadBoolParam("DEPBANEFE"); }

        /// <summary>
        /// Si existen cheques devueltos sin saldar, no le permite hacer pedido
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParPedidosBloquearChequesDevueltos() { return ReadBoolParam("PEDBLOCKCKD"); }

        /// <summary>
        /// En recibos permite digitar el porcentaje de descuento que se le aplicara a una factura
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosPorcientoDescuentoDigitable() { return ReadBoolParam("RECEDITDESC"); }

        /// <summary>
        /// Si existen cheques devueltos sin saldar, no le permite hacer pedido
        /// </summary>
        /// <returns>bool</returns>
        public string GetParDepositosFormasPago() { return ReadStringParam("DEPFORPAGO"); }

        /// <summary>
        /// DEVFACPRECIO: Muestra el precio y calcula los montos en devoluciones por facturas
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParDevolucionesFacturaPrecioProducto() { return ReadBoolParam("DEVFACPRECIO"); }

        /// <summary>
        /// activa la funcionalidad de devolver factura completa en devoluciones.
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesDevolverFacturaCompleta()
        {
            return ReadBoolParam("DEVCONFACT") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES;
        }

        /// <summary>
        /// CONTEO FISICO OBLIGATORIO AL CERRAR CUADRE
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoFisicoObligatorio()
        {
            return ReadBoolParam("CONTFISOBL");
        }

        /// <summary>
        /// activas las opciones de desarrollador en la app
        /// </summary>
        /// <returns></returns>
        public bool GetParOpcionesDesarrollador()
        {
            return ReadBoolParam("DESAOPCIONES");
        }

        /// <summary>
        /// multi almacen con almacen de devolucion
        /// </summary>
        /// <returns></returns>
        public bool GetParMultialmacenConDevolucion()
        {
            return ReadBoolParam("VENMULTALMDEV");
        }

        /// <summary>
        /// almacen ranchero
        /// </summary>
        /// <returns></returns>
        public int GetParAlmacenVentaRanchera()
        {
            return ReadIntParam("VENRANCALM");
        }

        /// <summary>
        /// No muestra las visitas en la sincronizacion automatica 
        /// </summary>
        /// <returns></returns>
        public bool GetParNoNoticiasAUTOSYNC()
        {
            return ReadBoolParam("NONEWSSYNCAUTO");
        }

        /// <summary>
        /// en gastos al configurar el NCF en el combobox de tipos de ncf solo aparecen los que esten en este parametro
        /// </summary>
        /// <returns></returns>
        public string GetParGastosTiposNCFValidos()
        {
            return ReadStringParam("GASNCFTIPO");
        }


        /// <summary>
        /// Permite realizar recibos con faltante. el Parvalor es el limite maximo permitido
        /// el parametro moneda que recibe este Parametro se utiliza para filtrar el parametro definido segun la moneda base de cuentas por cobrar
        /// ejemplo : RECFAL-USD, RECFAL-DOP, RECFAL-EUR
        /// </summary>
        /// <returns></returns>
        public int GetParRecAceptarFaltante(string Moneda)
        {
            if (string.IsNullOrWhiteSpace(Moneda)) throw new Exception();
            return ReadIntParam("RECPERFAL-" + Moneda);
        }

        /// <summary>
        /// en ventas activa la captura de lotes, con parvalor 1 el lote es editable, con parvalor 2 el lote es seleccionable
        /// </summary>
        /// <returns></returns>
        public int GetParVentasLote()
        {
            return Arguments.Values.CurrentModule == Modules.VENTAS ? ReadIntParam("VENLOTE") : -1;
        }

        /// <summary>
        /// en cambios de mercancia permite seleccionar el lote
        /// parvalor = 1 se digita, parvalor = 2 se selecciona
        /// </summary>
        /// <returns></returns>
        public int GetParCambiosMercanciaLotes()
        {
            var result = -1;

            if (Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
            {
                result = ReadIntParam("CAMBLOTE");
            }

            //return ReadIntParam("CAMBLOTE") && Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA;

            return result;
        }

        /// <summary>
        /// Se utiliza para asignar una lista de precio para el cuadre 
        /// </summary>
        /// <returns></returns>
        public string GetParListaPreciosCuadre()
        {
            return ReadStringParam("INVLIPCODIGO");
        }

        /// <summary>
        /// que los conteo fisicos sean por almacen
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoFisicoPorAlmacen()
        {
            return ReadBoolParam("CONFALMID");
        }

        /// <summary>
        /// ORDENAR POR DEFECTO LOS PROUCTOS AL BUSCAR
        /// </summary>
        /// <returns></returns>
        public string GetParProdOrden()
        {
            return ReadStringParam("PRODORDEN");
        }
        /// <summary>
        /// Sector por defecto de un representante para tomar la imagen de la empresa en reportes  
        /// </summary>
        /// <returns></returns>
        public string GetParRepresentanteSectorPorDefecto()
        {
            return ReadStringParam("REPSECTOR");
        }

        /// <summary>
        /// en el detalle del pedido muestra la cantidad en quintales
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosMostrarQuintales()
        {
            return ReadBoolParam("PEDSUMQUIN") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// Usa un formato de letras pequeñas para la empreasa tambien reduce el logo
        /// </summary>
        /// <returns></returns>
        public bool GetParReducirImpresionEmpresa()
        {
            return ReadBoolParam("EMPREDIMP");
        }

        public bool GetParHistoricoFacturasEnOperaciones()
        {
            return ReadBoolParam("OPERHISTFACT");
        }

        /// <summary>
        /// Cuando el lote es obligatorio si este parametro esta activo no valida que exista en la tabla de ProductosLotes
        /// </summary>
        /// <returns></returns>
        public bool GetParProductoNoValidarLote()
        {
            return ReadBoolParam("PRONOVALLOTE");
        }

        /// <summary>
        /// sincroniza automaticamente si tiene mas de x registros pendientes
        /// el parvalor es 1|20 donde el numero de la izquierda es cuando sincronizara y el de la derecha es despues de que cantidad
        /// en este ejemplo 1 es despues de cerrar visita y si tienen mas de 20 registros pendientes
        /// </summary>
        /// <returns></returns>
        public AutomaticSync GetParSincronizacionAutomaticaByCantidadRegistros()
        {
            var raw = ReadStringParam("SYNCCANTTRA");

            try
            {

                if (raw.Contains("|"))
                {

                    var content = raw.Split('|');

                    if (int.TryParse(content[0], out int when) && int.TryParse(content[1], out int quantity))
                    {

                        return new AutomaticSync()
                        {
                            QuantityOfPendingToSync = quantity,
                            Moment = (MomentToSync)when
                        };

                    }

                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return null;
        }

        /// <summary>
        /// En recibos permite abonar un monto mayor que el pendiente a una factura
        /// </summary>
        /// <returns></returns>
        /* public bool GetParRecibosAbonarMontoMayor()
         {
             return ReadBoolParam("RECMONFACM ");
         }*/


        /// <summary>
        /// ABRIR Y CERRAR CUADRES POR MULTIPLE ALMACENES AGRUPADOS (POR LOS PARAMETROS QUE TIENEN ALMID)
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadrePorMultiplesAlmacenesAgrupados()
        {
            return ReadBoolParam("CUAMULTALMGRUP");
        }

        public List<RepresentantesParametros> GetAlmacenesAgrupados(string parReferencia)
        {
            try
            {
                return SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParReferencia, ParDescripcion, ParValor  from RepresentantesParametros " +
                    "where UPPER(ltrim(rtrim(ParReferencia))) like ? or UPPER(ltrim(rtrim(ParReferencia)))='VENRANCALM' order by ParDescripcion", new string[] { $"%{parReferencia}%" });
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return new List<RepresentantesParametros>();
            }
        }

        /// <summary>
        /// En el asunto del correo en vez de poner el usuario se coloca la empresa
        /// </summary>
        /// <returns></returns>
        public bool GetParAsuntoEnvio()
        {
            return ReadBoolParam("COMASUNTO");
        }

        /// <summary>
        /// Nota para colocarse en Formato de impresión Cotizaciones
        /// </summary>
        /// <returns></returns>
        public string GetParCotizacionesNota()
        {
            return ReadStringParam("COTNOTA");
        }

        /// <summary>
        /// Ofertas y descuentos en cotizaciones
        /// </summary>
        /// <returns></returns>
        public bool GetParCotizacionesOfertasyDescuentos()
        {
            return ReadBoolParam("COTOFEDES") && Arguments.Values.CurrentModule == Modules.COTIZACIONES;
        }

        /// <summary>
        /// Mostrar la lista de precio del cliente en la visita
        /// </summary>
        /// <returns></returns>
        public bool GetParShowListaPrecio()
        {
            return ReadBoolParam("SHOWLP");
        }

        /// <summary>
        /// en los formatos de impresión poner el logo mas pequeño y la inf. de la empresa en letra pequeña.
        /// </summary>
        /// <returns></returns>
        public string GetParPrintModifyLogo()
        {
            return ReadStringParam("MODFLOGO");
        }

        /// <summary>
        /// En conteo fisico activa el uso de lotes, con parvalor 1 el lote es editable, con parvalor 2 el lote es seleccionable
        /// </summary>
        /// <returns></returns>
        public int GetParConteosFisicosLotes()
        {
            return Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && GetParUsarMultiAlmacenes() ? ReadIntParam("CONTFISLOTE") : -1;
        }

        /// <summary>
        /// 1: en ventas al agregar un producto este se agrega con un lote automatico desde inventariosalmacenesrepresentantes
        /// 2: en ventas al agregar un producto este se agrega con un lote automatico desde EntregasRepartidorTransaccionesDetalle  
        /// </summary>
        /// <returns></returns>
        public int GetParVentasLotesAutomaticos()
        {
            return Arguments.Values.CurrentModule == Modules.VENTAS ? ReadIntParam("VENLOTAUTO") : -1;  
        }

        //public bool GetParVentasLotesAutomaticos()
        //{
        //    return ReadBoolParam("VENLOTAUTO") && Arguments.Values.CurrentModule == Modules.VENTAS;
        //}

        /// <summary>
        /// Aplicar notas de credito al monto total de la factura, no al monto sin itbis
        /// </summary>
        /// <returns></returns>
        public bool GetParAplicarNotaCreditoMontoTotal()
        {
            return ReadBoolParam("RECNCMTOTAL");
        }

        /// <summary>
        /// Cambiar la fuente de la impresion de la empresa
        /// </summary>
        /// <returns></returns>
        public string GetParChangeFontEmpresa()
        {
            return ReadStringParam("PRINTFONT");
        }

        /// <summary>
        /// Parametro para QUE LAS NOTAS DE CREDITOS NO REBAJEN EL DESCUENTO por pronto pago NOTCRENOREBDESPRONPAGO --- se cambio por este NCNOREBDESPPAG por que era muy largo
        /// </summary>
        /// <returns></returns>
        public bool GetParNotasCreditoNoRebajarDescuentoPorProntoPago()
        {
            return ReadBoolParam("NCNOREBDESPPAG");
        }

        /// <summary>
        /// Aceptar productos con cantidad 0 en conteo fisico
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoFisicoAceptarProductosCantidadCero() { return Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && ReadBoolParam("CONTFPRODCERO"); }

        /// <summary>
        /// Precio lista sin impuestos (Advalorem, selectivo, itbis)
        /// </summary>
        /// <returns></returns>
        public bool GetParPrecioSinImpuestos()
        {
            return ReadBoolParam("PEDPRODSINIMP");
        }

        //Redondea a dos cantidades las cantidades introducidas , las ofertas, y el detalle de porciento de una oferta
        public bool GetParRedondeoCantidadesDecimales()
        {
            return ReadBoolParam("PEDREDCANT");
        }

        public double GetParVentasPorcientoBalancePagoMinimo()
        {
            var value = ReadDoubleParam("VENPAGOMIN");

            //return Arguments.Values.CurrentModule == Modules.VENTAS ? value : -1;
            return value;
        }

        public double GetParVentasPorcientoAdicionalPedido()
        {
            var value = ReadDoubleParam("VENPEDADIC");

            //return Arguments.Values.CurrentModule == Modules.VENTAS ? value : -1;
            return value;
        }

        public double GetParVentasPorcientoAdicionalLimiteCredito()
        {
            var value = ReadDoubleParam("VENLCADIC");

            //return Arguments.Values.CurrentModule == Modules.VENTAS ? value : -1;
            return value;
        }
        public bool GetParPedidosOcultarDescuentoManualMonto()
        {
            return ReadBoolParam("PEDOCULDESMON");
        }

        public bool GetParCobrosVerDetalleDocumentos()
        {
            var result = true;

            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(ParValor, '') as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?",
                new string[] { "CUENTADET" });

            if (list != null && list.Count > 0)
            {
                if (list[0].ParValor.Trim().Equals("0"))
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// ParValores:
        /// 0: deshabilitado
        /// 1: todas las condiciones pago bloqueadas
        /// 2: todas las condiciones pago bloqueadas hacia abajo
        /// Nota: Si estoy en una venta desde un pedido carga la condicion del pedido sino la del cliente
        /// </summary>
        /// <returns></returns>
        public int GetParBloqueoCondicionPago()
        {
            if (Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                return ReadIntParam("PEDCONIDLOCK");
            }
            else if (Arguments.Values.CurrentModule == Modules.VENTAS)
            {
                return ReadIntParam("VENCONIDLOCK");
            }
            else if (Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                return ReadIntParam("COTCONIDLOCK");
            }
            else
            {
                return -1;
            }
        }

        public bool GetParOfertasEspecificasCliente()
        {
            return ReadBoolParam("PEDOFEESPCLI");
        }

        public bool GetParRedondearOfertasDecimalMayorA5()
        {
            return ReadBoolParam("PEDOFEREDEMA5");
        }

        public bool GetParPushMoneyPorPagar()
        {
            return ReadBoolParam("PMXP");
        }

        /// <summary>
        /// En la pantalla de operaciones selecciona automaticamente el sector que tenga entregas pendientes.
        /// </summary>
        /// <returns></returns>
        public bool GetParSeleccionarSectorAutomaticamenteSiTieneEntrega()
        {
            return ReadBoolParam("VISSECENTAUTO");
        }

        /// <summary>
        /// cambia el nombre del modulo de compras a pushmoney
        /// </summary>
        /// <returns></returns>
        public bool GetParCambiarNombreComprasPorPushMoney()
        {
            return ReadBoolParam("COMPAPUSHM");
        }

        public bool GetParVisitasFallidas()
        {
            var def = true;

            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { "VISFALLIDA" });

                if (list != null && list.Count > 0)
                {
                    def = !string.IsNullOrWhiteSpace(list[0].ParValor) && list[0].ParValor.Trim() == "1";
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return def;
        }

        /// <summary>
        /// buscar PRODUCTOS al escribir
        /// </summary>
        /// <returns></returns>
        public bool GetBuscarProductosAlEscribir()
        {
            return ReadBoolParam("SEARCHPRODWR");
        }
        /// <summary>
        /// orden de columnas en cobros
        /// 1- el vendedor esta al final
        /// 2- el vendedor esta al inicio
        /// </summary>
        /// <returns></returns>
        public int GetParOrdencolumnasCobros()
        {
            return ReadIntParam("CXCORDERCOLUM");
        }


        /// <summary>
        /// validar campo RNC o Cedula en SAC
        /// </summary>

        public bool GetValidarRncSAC()
        {
            return ReadBoolParam("SACVALRNC");
        }

        /// <summary>
        /// cARGAR LOTES DESDELA TABLA DE CLIENTESFACTURASPRODUCTOSLOTES
        /// </summary>
        /// <returns></returns>

        public bool GerParClientesFacturaProductosLotes()
        {
            return ReadBoolParam("DEVCLIFTPROLOT");
        }

        /// <summary>
        /// en el modulo de compras en el tab de forma de pago carga las formas de pago que esten definidas en este parametro,
        /// el parvalor es el fopID de la forma de pago separado por comas, por ejemplo 1,2,5,20
        /// </summary>
        /// <returns></returns>
        public string[] GetParComprasFormasPagoValidas()
        {
            var par = ReadStringParam("COMFOPID");

            if (!string.IsNullOrWhiteSpace(par))
            {
                return par.Split(',');
            }

            return null;
        }

        /// <summary>
        /// Parametro para imprimir la tasa de la tabla de recibos en el formato multimoneda
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosImpresionTasa()
        {
            return ReadBoolParam("RECIMPTASA");
        }

        /// <summary>
        /// activa poder sincronizar dentro de la visita
        /// </summary>
        /// <returns></returns>
        public bool GetParSincronizarDentroDeVisita()
        {
            return ReadBoolParam("VISSINC");
        }

        /// <summary>
        /// cuando se selecciona un cliente en la pantalla de clientes en vez de mostrar la ventana de opciones crea la visita directamente si se encuentra desactivado este parametro
        /// </summary>
        /// <returns></returns>
        public bool GetParMostrarVisitaMenu()
        {
            var result = true;

            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ? and ltrim(rtrim(ParValor)) = '0'", new string[] { "VISITAMENU" });

            if (list != null && list.Count > 0)
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// en compras activa que se capture la factura
        /// </summary>
        /// <returns></returns>
        public bool GetParComprasUsarFacturas()
        {
            return Arguments.Values.IsPushMoneyRotacion; //Arguments.Values.CurrentModule == Modules.COMPRAS && ReadBoolParam("COMFACT");
        }

        /// <summary>
        /// en recibos para cuando se salde una o mas facturas, se tome la tasa de la factura si la fecha es menor a la fecha de vencimiento de la factura
        /// en caso de no serlo se tomara la tasa de dia, si hay mas de una factura saldada se tomara la tasa mayor de todas las facturas saldadas.
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosTasaFacturas_O_TasaDia()
        {
            return ReadBoolParam("RECTASADIAOFT");
        }

        public bool GetParEntregasRepartidorUsarRowDetallado()
        {
            return Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR && ReadBoolParam("ENTROWDET");
        }

        public bool GetParEntregasRepartidorUsarRowDeProductosSinDialog()
        {
            return Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR && ReadBoolParam("ENTROWPRODEDIT");
        }

        public bool GetParConducesGuardarYImprimirDirectamente()
        {
            return (Arguments.Values.CurrentModule == Modules.CONDUCES && ReadBoolParam("CONGUARIMP"));
        }

        public bool GetParConsultarEntregas()
        {
            return ReadBoolParam("ENTREPCONSULT");
        }

        public bool GetParConducesUsarRowSinDialog()
        {
            return Arguments.Values.CurrentModule == Modules.CONDUCES && ReadBoolParam("CONROWPRODEDIT");
        }


        /// <summary>       
        /// En el modulo de prospecto excluye condiciones de pagos (ConReferencias) separadas por coma
        /// </summary>
        /// <returns></returns>
        public string[] GetParProspectoCondicionesPagosNOValidas()
        {
            var par = ReadStringParam("PRONOVALCONID");

            if (!string.IsNullOrWhiteSpace(par))
            {
                return par.Split(',');
            }

            return new string[] { };
        }

        /// <summary>
        /// Valida si una factura que se va a saldar tiene abono diferido y no permite saldarla
        /// </summary>
        /// <returns></returns>
        public bool GetParRecNoSaldoWithAbonoDiferido()

        {
            return ReadBoolParam("RECNOSALSIABDIF");
        }


        /// <summary>
        /// este parametro en las entregas al rebajar o sumar el inventario no toma en cuenta el lote
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasControlInventarioSinLotes()
        {
            return ReadBoolParam("ENTINVNOLOTE");
        }

        /// <summary>       
        /// Segun los valores que tenga, el valor a la izquiera de la coma (,) es El Ancho (W) 
        /// y a la derecha La Altura (H) que se imprimirá el logo en la impresión de los formatos.
        /// </summary>
        /// <returns></returns>
        public string[] GetParImpresionLogoSize()
        {
            var par = ReadStringParam("IMPLOGOSIZE");

            if (!string.IsNullOrWhiteSpace(par))
            {
                return par.Split(',');
            }

            return null;
        }

        /// <summary>
        /// Firma obligatoria del cliente en pedidos
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosFirmaObligatoria()
        {
            return ReadIntParam("PEDFIRMOBLI") == 1 || ReadIntParam("PEDFIRMOBLI") == 2 && new DS_Pedidos().ExistPedidosCreditosEnVisitas();
        }


        public bool GetParEntregasMultiples()
        {
            return ReadBoolParam("ENTMULTIPLE") && Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR;
        }

        /// <summary>
        /// En entregas repartidor las ofertas se dan todo o nada tanto el producto ofertado como por el que se da oferta, es decir,
        /// si el producto ofertado no se entrega completo no se entrega completo se cae tanto el ofertado como por el que se dio la oferta
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasOfertasTodoONada()
        {
            return ReadBoolParam("ENTOFECOMPLETA") && (Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR || Arguments.Values.CurrentModule == Modules.CONDUCES);
        }

        public string GetParProspectosListaPreciosDefault()
        {
            return ReadStringParam("PROSLISTDEF");
        }

        public bool GetParComprasRotacion() { return ReadBoolParam("PUSHMONEYROT"); }

        /// <summary>
        /// solo permite cerrar el cuadre si ha hecho deposito y tirado foto del comprobante
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadresCerrarSiTieneDeposito()
        {
            return ReadBoolParam("CUACERRSIDEP");
        }

        /// <summary>
        /// En depositos permite capturar fotos
        /// </summary>
        /// <returns></returns>
        public bool GetParDepositosCapturarFoto()
        {
            return ReadBoolParam("DEPFOTO");
        }

        /// <summary>
        /// para que se haga el conteo fisico por todos los almacenes 
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoFisicoMultiAlmacenAll()
        {
            return ReadBoolParam("COFALLMULTALM");
        }

        /// <summary>
        /// para que cuando se valla hacer el conteo fisico por alamcenes tome los almacenes para especificados por el parametro
        /// </summary>
        /// <returns></returns>
        public string GetParConteoFisicoAlmacenesParaContar()
        {
            return ReadStringParam("CONTALMS");
        }

        /// <summary>
        /// Id del almacen de melma (que es el usado en ventas)
        /// </summary>
        /// <returns></returns>
        public int GetParAlmacenIdParaMelma()
        {
            return ReadIntParam("CONTALMIDMEL");
        }

        /// <summary>
        /// Al guardar un entrega imprime automaticamente la cantidad de copias que diga este parametro
        /// </summary>
        /// <returns></returns>
        public int GetParEntregasImpresionAutomatica()
        {
            if (Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR)
            {
                return ReadIntParam("ENTRIMPAUTO");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Al guardar un conduce imprime automaticamente la cantidad de copias que diga este parametro
        /// </summary>
        /// <returns></returns>
        public int GetParConducesImpresionAutomatica()
        {
            if (Arguments.Values.CurrentModule == Modules.CONDUCES)
            {
                return ReadIntParam("CONIMPAUTO");
            }
            else
            {
                return -1;
            }
        }

        public bool GetParRecibosOrdenarDocumentoPorFechaEntrega()
        {
            return Arguments.Values.CurrentModule == Modules.COBROS && ReadBoolParam("RECORDFECHAENT");
        }

        /// <summary>
        /// Controla el monto minimo en pedidos por moneda
        /// </summary>
        /// <returns></returns>
        public double GetParPedidosMontoMinimo(string Moneda)
        {
            return ReadDoubleParam("PEDMONMIN-" + Moneda);
        }

        /// <summary>
        /// Este parametro va con relacion al parametro PEDMONMIN-" + Moneda, para determinar donde mostrar la validacion de monto minimo en pedidos
        /// Valor 1: muestra advertencia antes de ir al detalle
        /// Valor 2: muestra advertencia cuando se va a guardar el pedido definitivo, si es preliminar no valida
        /// </summary>
        /// <returns></returns>
        public int GetParPedidosValidaMontoMinimo()
        {
            var result = ReadIntParam("PEDVALMONMIN");

            if (result != -1)
            {
                return result;
            }
            else
            {
                return 1;
            }
        }

        public bool GetParDevolucionesProductoMotivoVencido()
        {
            return ReadBoolParam("DEVMOTVENFEC") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES;
        }

        //en recibos al intentar agregar un cheque la longitud del numero se ajusta al parvalor de este parametro
        public int GetParNumeroChequesLongitudMaxima()
        {
            return ReadIntParam("CHENUMLONG");
        }

        //al entrar a compras se cargan los productos automaticamente
        public bool GetParModulosCargasProductosAuto()
        {
            return (ReadBoolParam("COMLOADALL") && Arguments.Values.CurrentModule == Modules.COMPRAS)
                || (ReadBoolParam("PEDLOADALL") && Arguments.Values.CurrentModule == Modules.PEDIDOS)
                || (ReadBoolParam("VENLOADALL") && Arguments.Values.CurrentModule == Modules.VENTAS)
                || (ReadBoolParam("CAMLOADALL") && Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
                || (ReadBoolParam("DEVLOADALL") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
                || (ReadBoolParam("CONLOADALL") && Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS)
                || (ReadBoolParam("COTLOADALL") && Arguments.Values.CurrentModule == Modules.COTIZACIONES);
        }

        //en cotizaciones activa la consulta de ofertas
        public bool GetParCotizacionesConsultarOfertas()
        {
            return ReadBoolParam("COTCONOFE") && Arguments.Values.CurrentModule == Modules.COTIZACIONES;
        }

        /// <summary>
        /// en devoluciones carga el motivo por defecto.
        /// </summary>
        /// <returns></returns>
        public string GetParDevolucionesMotivoPorDefecto()
        {
            return ReadStringParam("DEVMOTDEF");
        }

        /// <summary>
        /// en devoluciones imprime la cantidad de copias que diga el parametro
        /// </summary>
        /// <returns></returns>
        public int GetParDevolucionesCopiasPorDefecto()
        {
            if (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES)
            {
                return ReadIntParam("DEVCOPIAS");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// en compras imprime la cantidad de copias que diga el parametro
        /// </summary>
        /// <returns></returns>
        public int GetParComprasCopiasPorDefecto()
        {
            if (Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                return ReadIntParam("PUSHCOPIAS");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// en recibos imprime la cantidad de copias que diga el parametro
        /// </summary>
        /// <returns></returns>
        public int GetParRecibosCopiasPorDefecto()
        {
            if (Arguments.Values.CurrentModule == Modules.COBROS)
            {
                return ReadIntParam("RECCOPIAS");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// en pushmoney no permite imprimir mas copias que la que diga el parametro
        /// </summary>
        /// <returns></returns>
        public int GetParComprasLimiteMaximoCopiasImpresion()
        {
            if (Arguments.Values.CurrentModule == Modules.COMPRAS)
            {
                return ReadIntParam("COMLIMTCOP");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Permite que la forma de pago tarjeta de credito no se utilice con alguna más
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParCobrosTarjetaCreditoSolo() { return Arguments.Values.CurrentModule == Modules.COBROS && ReadBoolParam("RECTARJSOLO"); }


        /// <summary>
        /// Seleccion de tipo de pedido obligatoria
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParTipoPedidoObligatorio() { return Arguments.Values.CurrentModule == Modules.PEDIDOS && ReadBoolParam("PEDTIPOOBLI"); }

        /// <summary>
        /// activa el modulo de quejas al servicio
        /// </summary>
        /// <returns></returns>
        public bool GetParQuejasServicio()
        {
            return ReadBoolParam("QUESERV");
        }
        /// <summary>
        /// Consulta transacciones busqueda por sector
        /// </summary>
        /// <returns></returns>

        public bool GetParConsultaTransaccionesUsarSectores()
        {
            return ReadBoolParam("CONTRASECTOR"); ;
        }

        /// <summary>
        /// en la pantalla de comentarios activa la captura de departamento
        /// </summary>
        /// <returns></returns>
        public bool GetParVisitasComentarioUsarDepartamento()
        {
            return ReadBoolParam("VISCOMDEP");
        }

        /// <summary>
        /// en pedidos muestra info crediticia del cliente, credito disponible, balance, monto venc.
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosMostrarInformacionCrediticiaCliente()
        {
            return Arguments.Values.CurrentModule == Modules.PEDIDOS && ReadBoolParam("PEDCLIDAT");
        }

        /// <summary>
        /// activa o desactiva el btn de clientes en el home de la app
        /// </summary>
        /// <returns></returns>
        public bool GetParHomeClientes()
        {
            var result = true;

            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(ParValor, '') as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?",
                new string[] { "CLIENTES" });

            if (list != null && list.Count > 0)
            {
                if (list[0].ParValor.Trim().Equals("0"))
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// permite guardar un recibo a partir de un pedido. El parvalor de este pedido debe ser igual al conId con el que esta funcionalidad se activara.
        /// </summary>
        /// <returns></returns>
        public int GetParPedidosConIdPrepago()
        {
            return ReadIntParamDifReturn("PEDPREPAGO");
        }

        /// <summary>
        /// en cargar mostrar las cantidades en cajas y unidades
        /// </summary>
        /// <returns></returns>
        public bool GetParCargasMostrarCajasUnidades()
        {
            return ReadBoolParam("CARCAJUNID");
        }

        /// <summary>
        /// en compras inactiva la captura de dependientes
        /// </summary>
        /// <returns></returns>
        public bool GetParComprasNoUsarDependiente()
        {
            return ReadBoolParam("COMNODEP");
        }

        /// <summary>
        /// en compras inactiva la captura de la forma de pago y crea una forma de pago automatica con forID igual al parvalor del parametro
        /// </summary>
        /// <returns></returns>
        public int GetParComprasFormaPagoAutomatica()
        {
            return ReadIntParam("COMFORIDAUTO");
        }

        /// <summary>
        /// en cuadres al abrir un cuadre inserta en Inventarios los productos definidos en la tabla VehiculosCapacidad
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadresVehiculosCapacidad()
        {
            return ReadBoolParam("CUAVEHCAP");
        }

        /// <summary>
        /// parametro para activar que los conduces se creen en base a devoluciones.
        /// </summary>
        /// <returns></returns>
        public bool GetParConducesDesdeDevoluciones()
        {
            return ReadBoolParam("CONFROMDEV");
        }

        /// <summary>
        /// parametro para sustituir la palabra sobrante por adelanto en el formato de recibo
        /// </summary>
        /// <returns></returns>
        public bool GetParSustituirSobrantePorAdelantoEnFormato()
        {
            return ReadBoolParam("RECADELANTO");
        }

        public bool GetParUsarCamaraLegacy()
        {
            return ReadBoolParam("CAMERALEGACY");
        }

        public string[] GetParModulosPrioridad()
        {

            var param = "";

            switch (Arguments.Values.CurrentModule)
            {
                case Modules.PEDIDOS:
                    param = "PEDPRIORIDAD";
                    break;
                case Modules.COTIZACIONES:
                    param = "COTPRIORIDAD";
                    break;
                default:
                    return null;
            }

            var par = ReadStringParam(param);

            if (!string.IsNullOrWhiteSpace(par))
            {
                return par.Split('|');
            }

            return null;
        }

        /// <summary>
        /// parametro para calcular la georeferencia por el tipo de visita 
        /// 1=Manual 
        /// 2=Automatico 
        /// 3=Bloquea por Distancia
        /// 4=Bloquea por Distancia sino tiene visita presencial en el dia
        /// </summary>
        /// <returns></returns>

        public int GetParCalcularSegunTipoVisita()
        {
            return ReadIntParam("VISTIPOCALC");
        }

        /// <summary>
        /// para evaluar la distancia que tiene el cliente y la visita que se esta creando cuando VISTIPOCALC sea tipo 2
        /// </summary>
        /// <returns></returns>
        public int GetParDistanciaVisita()
        {
            return ReadIntParam("VISDISTANCIA");
        }

        /// <summary>
        /// el ParValor es igual a un ConID. este solo permite que se elijan 2 condiciones de pago. la que tiene el cliente por defecto o la del parametro.las otras no se podran elegir en ventas
        /// </summary>
        /// <returns></returns>
        public int GetParVenConIDCandado()
        {
            return ReadIntParam("VENCONIDCAN");
        }


        /// <summary>
        /// Guardar RNC y Cedula en el Campo de CliRNC al hacer un SAC
        /// </summary>
        /// <returns></returns>
        public bool GetParSACCliRNCCedula()
        {
            return ReadBoolParam("SACRNCCED");
        }

        /// <summary>
        /// mostrar cantidad y unidaa en historico facturas
        /// </summary>
        /// <returns></returns>
        public bool GetParHistoricoFacturaCantidadUnidad()
        {
            return ReadBoolParam("HISFATCANUND");
        }


        /// <summary>
        /// Accede a sac desde clientes si la visita no tiene la geocalizacion del cliente
        /// </summary>
        /// <returns></returns>
        public int GetParGoToSACfromClientes()
        {
            return ReadIntParam("SACFROMCLI");
        }


        public string GetParMultiConIdFormaPagoContado()
        {
            var valorDefault = ReadStringParam("MULTIFPCON");

            return valorDefault;
        }

        /// <summary>
        /// mostrar condicionespago solo del cliente y de contado
        /// </summary>
        /// <returns></returns>
        public bool GetParCondicionesPagoClienteyContado()
        {
            return ReadBoolParam("CONDIDCLICONT");
        }

        /// <summary>
        /// precio total como hisprecio en historico factura
        /// </summary>
        /// <returns></returns>
        public bool GetParHistoricoFacturasTotalPrecioProducto()
        {
            return ReadBoolParam("HISFATPRETOTAL");
        }

        /// <summary>
        /// al entrar a recibo nuevo si la factura tiene menos de 30 dias le activa el indicador de itbis.
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosItbisMenos30Dias()
        {
            return ReadBoolParam("RECITBIS30");
        }

        /// <summary>
        /// No marcar los productos validos para descuentos, 
        /// este parametro es util cuando el query que marca los productos validos para descuentos tarda mucho
        /// </summary>
        /// <returns></returns>
        public bool GetParDescuentosProductosNoMarcarValidosParaDescuento()
        {
            return ReadBoolParam("DESPRONOMARCAR");
        }

        /// <summary>
        /// activa el modulo de traspasos
        /// </summary>
        /// <returns></returns>
        public bool GetParTraspasos()
        {
            return ReadBoolParam("TRASPASOS");
        }

        /// <summary>
        /// en impresion de cuadres mostrar las cantidades en cajas y unidades
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadresMostrarCajasUnidades()
        {
            return ReadBoolParam("CUACAJUNID");
        }

        /// <summary>
        /// En inventarios mostrar las cantidades en cajas y unidades
        /// </summary>
        /// <returns></returns>
        public bool GetParInventariosMostrarCajasUnidades()
        {
            return ReadBoolParam("INVCAJUNID");
        }

        /// <summary>
        /// activa el campo cantidad de canastos
        /// </summary>
        /// <returns></returns>
        public int GetParCaracteristicaCanasto()
        {
            return ReadIntParam("VenCanasto");
        }

        /// <summary>
        /// Devoluciones secuencia por sector
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesSecuenciaSector()
        {
            return ReadBoolParam("DEVSECSEC");
        }

        /// <summary>
        /// Devuelve si el usuario puede ser autorizado
        /// </summary>
        /// <returns></returns>
        public bool GetParVENAUTLIMCRE()
        {
            return ReadBoolParam("VENAUTLIMCRE");
        }

        /// <summary>
        /// al intentar abrir un cuadre pide la clave de un auditor
        /// </summary>
        /// <returns></returns>
        public bool GetParAperturarCuadrePorAuditor()
        {
            return ReadBoolParam("CUAAUDITOR");
        }

        //Cambia la descripcion de el list de vechiculos al abrir cuadre
        //1= Ficha - Referencia
        //2= Chassis
        //3 or Default= Ficha - Marca - Modelo
        public int GetParVehiculoDescripcion()
        {
            return ReadIntParam("VEHDESCRIPCION");
        }

        /// <summary>
        /// activa el modulo de operativos medicos
        /// </summary>
        /// <returns></returns>
        public bool GetParOperativosMedicos()
        {
            return ReadBoolParam("OPEMEDICOS");
        }

        //PARA OBTENER LA TASA EN CONSULTA TRANSACCIONES YA SEA DEL DIA O LA DEL RECIBO
        //1= TASA DEL DIA
        //2= TASA DE CREACION DEL RECIBO
        public int GetParTasaDelDiaODelRecibo()
        {
            return ReadIntParam("RECTASDIORECDI");
        }

        //PARA setear el tipo de comprobante por defecto en prospectos
        //
        //
        public string GetParProspectoIdTipoComprobanteDefault()
        {
            return ReadStringParam("PROSIDTIPCOM");
        }

        /// <summary>
        /// en pedidos permite la captura de colores y tamano de los productos
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosProductosColoresYTamanos()
        {
            return ReadBoolParam("PEDSIZECOLOR") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        //Validacion de Multiples Pedidos en un dia para un cliente
        //-1 - (Default)
        //0 - No permite mas de un pedido por dia por cliente
        //1 - Permite mas de un pedido por dia por cliente(Default)
        public int GetParPedidosMultiClientes()
        {
            return ReadIntParam("PEDMULTICLI");
        }

        //separar envios de images en visitas de 3 en 3 para modelos especificos
        //(Default)
        //Permite enviar images de 3 en 3
        public bool GetParVisitasCantidadImagenes()
        {
            return ReadBoolParam("VISCANTIMG");
        }

        /// <summary>
        /// activa el modulo de canastos
        /// </summary>
        /// <returns></returns>
        public bool GetParCanastos()
        {
            return ReadBoolParam("CANASTOS");
        }

        /// <summary>
        /// capturar canastos sin detalle
        /// </summary>
        /// <returns></returns>
        public bool GetParCanastosNoDetalle()
        {
            return ReadBoolParam("CAPCANASTO");
        }
        public bool GetParDescuentoSinRedondeo()
        {
            return ReadBoolParam("DESSINRE");
        }

        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProDescripcion3
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProDescripcion3()
        {
            return ReadStringParam("PROBUSAVADESC3");
        }

        /// <summary>
        /// Texto a mostrar en busqueda avanzada para Prodatos1
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProDatos1()
        {
            return ReadStringParam("PROBUSAVADATO1");
        }

        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProDescripcion2
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProDescripcion2()
        {
            return ReadStringParam("PROBUSAVADESC2");
        }


        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProDescripcion
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProDescripcion()
        {
            return ReadStringParam("PROBUSAVADESC");
        }
        /// <summary>
        /// Texto a mostrar en busqueda avanzada para Prodatos2
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProDatos2()
        {
            return ReadStringParam("PROBUSAVADATO2");
        }
        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProReferencia
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProReferencia()
        {
            return ReadStringParam("PROBUSAVAPROREF");
        }



        /// <summary>
        /// Texto a mostrar en busqueda avanzada para Productos.Cat3Id
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProCat3Id()
        {
            return ReadStringParam("PROBUSAVACAT3ID");
        }


        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProDescripcion1
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProDescripcion1()
        {
            return ReadStringParam("PROBUSAVADESC1");
        }

        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProCodigo
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProCodigo()
        {
            return ReadStringParam("PROBUSAVACOD");
        }


        /// <summary>
        /// Olculta ProReferencia en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaOcultarProReferencia()
        {
            return ReadBoolParam("BUSAVAOCUPROREF");
        }
        /// <summary>
        /// Olculta CAT3ID en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaOcultarCat3Id()
        {
            return ReadBoolParam("BUSAVAOCUCAT3ID");
        }

        /// <summary>
        /// Mostrar ProDescripcion3 en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaMostrarProDescripcion3()
        {
            return ReadBoolParam("BUSAVASHOPDESC3");
        }

        /// <summary>
        /// Mostrar ProDatos2 en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaMostrarProDatos2()
        {
            return ReadBoolParam("BUSAVASHOPDATO3");
        }

        /// <summary>
        /// Mostra condicion de pago en los datos de cliente de la visita
        /// </summary>
        /// <returns></returns>
        public bool GetParVisitaVerClienteCondicioPago()
        {
            return ReadBoolParam("VISVERCONPCLI");
        }
        /// <summary>
        /// Visitas Fallida emover tipo mensaje otros
        /// </summary>
        /// <returns></returns>
        public bool GetParVisitasFallidasRemoverMensajeOtros()
        {
            return ReadBoolParam("VISFALLREMOTRO");
        }

        /// <summary>
        /// Visitas Fallida emover tipo mensaje otros
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosObtenerMonedaDesdeTipoPedido()
        {
            return ReadBoolParam("PEDMONFROMTIPOP");
        }



        /// <summary>
        /// para que luego de conteofisico mande a crear el deposito antes de cerrar cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParDepositoconConteo()
        {
            return ReadBoolParam("DEPCONCUA");
        }

        /// <summary>
        /// valida que los ayudantes sean codigos validos, se deben de cargar todos los representantes en el dispositivo
        /// </summary>
        /// <returns></returns>
        public bool GetParValidaAyudantes()
        {
            return ReadBoolParam("CUAVALAYU");
        }

        /// <summary>
        /// No permite realizar ventas por fuera al repartidor aunque no tenga el parametro de multialmacen
        /// </summary>
        /// <returns></returns>
        public bool GetParNoVentasRancherasParaRepartidor()
        {
            return ReadBoolParam("NOVENRANCH");
        }

        /// <summary>
        /// En pedidos al intentar agregar un producto multiplica la cantidad por 12 si se tiene un check activado
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosCantidadesDocenasVenirActivo()
        {
            return ReadBoolParam("PEDCANTDOCE") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// Las ofertas salen con aplicando al Segmento del cliente, debe de estar llena la tabla ofertasdetalletiponegocios
        /// </summary>
        /// <returns></returns>
        public bool GetOfertasConSegmento()
        {
            return ReadBoolParam("OFESEGCLI");
        }

        public string GetParRecibosMontoSobrantes()
        {
            return ReadStringParam("RECSOBMAX");
        }


        /// <summary>
        /// activa el modulo de promociones
        /// </summary>
        /// <returns></returns>
        public bool GetParPromociones()
        {
            return ReadBoolParam("PROMOCION");
        }

        /// <summary>
        /// activa el modulo de entregas de mercancia
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasMercancia()
        {
            return ReadBoolParam("ENTMERC");
        }

        public bool GetParEntregasMercanciasPorFactura()
        {
            return ReadBoolParam("ENTMERFAC") && Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA;
        }

        /// <summary>
        /// limita la cantidad de productos semanal que se puede entragar en las entregas de mercancia
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasMercanciaLimiteEntregar()
        {
            return ReadBoolParam("ENTREGALIM") && Arguments.Values.CurrentModule == Modules.ENTREGASMERCANCIA;
        }

        public bool GetParEntregasPromocionesUsarCanastos()
        {
            return ReadBoolParam("PROMOCAN");
        }

        public bool GetParEliminarCopiasYOriginal()
        {
            return ReadBoolParam("ELICOORIMP");
        }
        public bool GetParTasaMonedas()
        {
            return ReadBoolParam("TASMONMAY");
        }

        /// <summary>
        /// en el modulo de cambios pide el motivo del cambio
        /// parvalor = 1, el motivo es general
        /// parvalor = 2, el motivo es por producto
        /// </summary>
        /// <returns></returns>
        public int GetParCambiosUsarMotivos()
        {
            var par = ReadIntParam("CAMMOTIVO");

            if (Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA)
            {
                return par;
            }

            return -1;
        }

        /// <summary>
        /// Mostrar ProColor en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaMostrarProColor()
        {
            return ReadBoolParam("BUSAVASHOPROCOL");
        }
        /// <summary>
        /// Mostrar ProPais en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaMostrarProPaisOrigen()
        {
            return ReadBoolParam("BUSAVASHOPROPAI");
        }
        /// <summary>
        /// Mostrar ProAnio en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaMostrarProAnioFabricacion()
        {
            return ReadBoolParam("BUSAVASHOPROANI");
        }
        /// <summary>
        /// Mostrar ProAnio en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaMostrarProMedida()
        {
            return ReadBoolParam("BUSAVASHOPROMED");
        }


        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProColor
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProColor()
        {
            return ReadStringParam("PROBUSAVAPROCOL");
        }

        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProPaisOrigen
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProPaisOrigen()
        {
            return ReadStringParam("PROBUSAVAPROPAI");
        }

        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProMedida
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProMedida()
        {
            return ReadStringParam("PROBUSAVAPROMED");
        }
        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProAnio
        /// </summary>
        /// <returns></returns>
        public string GetParBusquedaAvanzadaLabelProAnio()
        {
            return ReadStringParam("PROBUSAVAPRANI");
        }

        /// <summary>
        /// Texto a mostrar en busqueda avanzada para ProAnio
        /// </summary>
        /// <returns></returns>
        public int GetParTipoPedidoDefault()
        {
            return ReadIntParam("PEDTIPODEFAUL");
        }

        /// <summary>
        /// Olculta ProDescripcion2 en busqueda avanzada 
        /// </summary>
        /// <returns></returns>
        public bool GetParBusquedaAvanzadaOcultarProDescripcion2()
        {
            return ReadBoolParam("BUSAVAOCUPRODE2");
        }

        /// <summary>
        /// En el modulo de entregas repartidor no permite hacer entregas parciales a clientes de contado
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasRepartidorNoParcialClientesDeContado()
        {
            return ReadBoolParam("ENTNOPARCIALCON") && Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR;
        }

        /// <summary>
        /// Permite validar la cantidad para productos retormables
        /// </summary>
        /// <returns></returns>
        public bool GetParCantidadMaximaForReturn()
        {
            return ReadBoolParam("COMVALRET") && GetParConteosAlmacenReferecia().Contains(Arguments.Values.AlmRef);
        }

        /// <summary>
        /// La referencia de los almacenes para validar los productos retornables
        /// </summary>
        /// <returns></returns>
        public string GetParConteosAlmacenReferecia()
        {
            return ReadStringParam("CONALMREF");
        }

        /// <summary>
        /// En ventas permite ver la descripción ProDescripcion3
        /// </summary>
        /// <returns></returns>
        public bool GetParConteosProductosDescripcion()
        {
            return ReadBoolParam("CONPRODESCR") && Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS;
        }
        /// <summary>
        /// En ventas permite ver la descripcion ProDescripcion3
        /// </summary>
        /// <returns></returns>
        public bool GetParVentasProductosDescripcion()
        {
            return ReadBoolParam("VENPRODESCR") && Arguments.Values.CurrentModule == Modules.VENTAS;
        }

        /// <summary>
        /// En el modulo de Clientes permite ver el nombre comercial
        /// </summary>
        /// <returns></returns>
        public bool GetParShowCliNombreComercial()
        {
            return ReadBoolParam("SHOWCLINOMCOMER");
        }
        /// <summary>
        /// En el modulo de PEDIDOS permite ver la cantidad de docenas
        /// </summary>
        /// <returns></returns>
        public bool GetParShowCantidadDocenas()
        {
            return ReadBoolParam("SHOWCLINOMCOM");
        }
        public bool GetParPedidosReferenciaColoresYTamanosSoloNumeros()
        {
            return ReadBoolParam("PEDREFNUM") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// en pedidos y ventas muestra al buscar un producto el % de descuento que tiene si la cantidadInicial es 1
        /// </summary>
        /// <returns></returns>
        public bool GetParDescuentosProductosMostrarPreview()
        {
            return ReadBoolParam("DESPROPREVIEW");
        }

        public bool GetParMostrarConteosCiegos()
        {
            return ReadBoolParam("CONCIEGO");
        }
        public bool GetParMostrarFacturasVentasEnConteos()
        {
            return ReadBoolParam("MOFAVECONT");
        }

        public bool GetParNoAperturarCuadreSinCarga()
        {
            return ReadBoolParam("NOCUSINCAR");
        }

        //Mostrar NCF en formatos pdf cuentas por cobrar.
        public bool GetParNcf()
        {
            return ReadBoolParam("NCFSHOWINFA");
        }

        //Mostrar almacenes en la fila 16 de pedidos
        public bool GetParAlmInPed()
        {
            return ReadBoolParam("PARALMINPED") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        public int GetParCamposEspecificosForTiposCanales(string tipcanales = "")
        {
            return ReadIntParam("CAESPFORTIPC" + tipcanales);
        }
        public int GetParCamposEspecificosForCondicionPago(string tipcondpago = "")
        {
            return ReadIntParam("CAESPFORCONP" + tipcondpago);
        }
        public string GetParCamposEspecificosForEstatus(string estatus = "")
        {
            return ReadStringParam("CAEPFOREST" + estatus);
        }

        public string GetParCamposEspecificosForTipComprobante(string tipcomprobante = "")
        {
            return ReadStringParam("CAPFORCPROBA" + tipcomprobante);
        }

        public string GetParShowCamposEspecificos()
        {
            return ReadStringParam("CAMESFORSHOWESP");
        }

        /// <summary>
        /// activa la funcionalidad de visita virtual
        /// </summary>
        /// <returns></returns>
        public bool GetParVisitaVirtual()
        {
            var result = true;

            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros " +
                    "where UPPER(ltrim(rtrim(ParReferencia))) = ? ", new string[] { "VISITAVIRTUAL" });

                if (list != null && list.Count > 0 && !string.IsNullOrWhiteSpace(list[0].ParValor))
                {
                    result = list[0].ParValor.Trim().Equals("1");
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return result;
        }

        public int GetParVisitaDistanciaMinimaVisitaPresencial()
        {
            var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros " +
               "where UPPER(ltrim(rtrim(ParReferencia))) = ? ", new string[] { "VISDISTMIN" });

            if (list != null && list.Count > 0 && int.TryParse(list[0].ParValor, out int valor))
            {
                return valor;
            }

            // si no existe el valor VISDISTMIN validara si existe el valor VISDISTANCIA como el minimo de visita para presencial
            var list2 = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros " +
               "where UPPER(ltrim(rtrim(ParReferencia))) = ? ", new string[] { "VISDISTANCIA" });

            if (list2 != null && list2.Count > 0 && int.TryParse(list2[0].ParValor, out int valor2))
            {
                return valor2;
            }

            return 200; //metros
        }
        public bool GetParPedForValidInInv()
        {
            return ReadBoolParam("PEDFORVALIDINV") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }
        //No realiza devolucion en entrega facturas.
        public bool GetParNoDevolucionEnEntrega()
        {
            return ReadBoolParam("NODEVENT");
        }

        //Seleccionar motivo de devolucion en entrega de pedido parcial.
        public bool GetParMotDevolucionPedidoParcial()
        {
            return ReadBoolParam("MOTENTPAR");
        }

        /// <summary>
        /// en el modulo de entregas repartidor si la entrega es de contado lleva a la pantalla de recibos.
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasRepartidorGuardarReciboDeContado()
        {
            return ReadBoolParam("ENTRECCON") && Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR;
        }

        /// <summary>
        /// en el modulo de entregas repartidor si la entrega es de contado pide la firma.
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasFirmaDeContadoObligatoria()
        {
            return ReadBoolParam("ENTFIRMCONOB") && Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR;
        }

        /// <summary>
        /// dentro de la visita poder consultar transacciones
        /// </summary>
        /// <returns></returns>
        public bool GetParVisitasConsultarTransacciones()
        {
            return ReadBoolParam("VERTRANSCENVIS") && Arguments.Values.CurrentClient != null && Arguments.Values.CurrentVisSecuencia != -1;
        }
        public bool GetParDescuentosMaximosForPedidos()
        {
            return ReadBoolParam("LIPDESCMAX");
        }

        //Dias de No visualizacion de los pedidos a entregar
        public int GetDiasEntregasRepartidorTransaccionesVisibles()
        {
            var result = ReadIntParam("DIASRECHENT");

            if (result != -1)
            {
                return result;
            }
            else
            {
                return 7;
            }

        }

        public bool GetParNoCargasEntregaFactura() { return ReadBoolParam("ENTNOCARG"); }

        public bool GetParHuellaForLogin()
        {
            return ReadBoolParam("HUELLAFORLOGIN");
        }

        public string GetParMultimonedasSet()
        {
            return ReadStringParam("MULTIMONSET");
        }

        /// <summary>
        /// en impresion entregas transacciones mostrar las cantidades en cajas y unidades
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasTransaccionesMostrarCajasUnidades()
        {
            return ReadBoolParam("ENTCAJUNID");
        }
        /// <summary>
        /// activa la solicitud de exclusion de clientes
        /// </summary>
        /// <returns></returns>
        public bool GetParSolicitudExclusionClientes()
        {
            return ReadBoolParam("SOLEXCCLI");
        }

        public bool GetParLipCodigoClientes()
        {
            return ReadBoolParam("LIPCODIGOCLIENTES");
        }

        /// <summary>
        /// Permite traer el tipo de presupuestos de cajas a vender
        /// </summary>
        /// <returns></returns>
        public string GetParPresupuestosForGetPreTipo()
        {
            return ReadStringParam("PREFORTIP");
        }

        public string GetParCamposAdicionalesCamposObligatorios()
        {
            return ReadStringParam("CAMPADIOBL");
        }

        public int GetParFotosEncuestasCount()
        {
            return ReadIntParam("FOTENCCO");
        }
        public bool GetParAccionesNoObligatorias()
        {
            return ReadBoolParam("ACCIONESOBLI");
        }
        public string GetParPrefSec()
        {
            return ReadStringParam("RECDEVPREF");
        }

        /// <summary>
        /// en pedidos permite capturar la fecha de entrega por productos.
        /// </summary>
        /// <returns></returns>
        /*public bool GetParPedidosMultiFechaEntrega()
        {
            return (ReadBoolParam("PEDPRODMULDET") && Arguments.Values.CurrentModule == Modules.PEDIDOS) || (ReadBoolParam("COTPRODMULDET") && Arguments.Values.CurrentModule == Modules.COTIZACIONES);
        }*/

        /// <summary>
        /// en pedidos permite tomar la cantidad en inventario de la tabla de InventariosAlmacenes.
        /// </summary>
        /// <returns></returns>
        public bool GetParCantInvAlmacenes()
        {
            return ReadBoolParam("CANTINVALM");
        }
        public bool GetParShowOfertasAlAgregar()
        {
            return ReadBoolParam("SHOWOFEAGR");
        }

        //Procentaje de Retencion a aplicar automaticamente
        public double GetParRecibosPorcientoRetencion()
        {
            var result = ReadIntParam("RECRETAUTO");

            if (result != -1)
            {
                return result;
            }
            else
            {
                return 5;
            }

        }

        //Le suma +1 al ofefechafin en ofertas
        public bool GetOfertaConDia()
        {
            return ReadBoolParam("OFEMASDIA");
        }

        //Le suma +1 al desfechafin en ofertas
        public bool GetDescuentoConDia()
        {
            return ReadBoolParam("DESMASDIA");
        }


        public bool GetParProductosNoVendidosNewQuery()
        {
            return ReadBoolParam("PRONOVENQU");
        }
        public bool GetParCiclosAudRutasCli()
        {
            return ReadBoolParam("CICLOAUDRUT");
        }


        public bool GetParNoObligarFacturasEnPushMoney()
        {
            return ReadBoolParam("NOOBFACPUSH");
        }

        public bool GetParNoObligarHacerRecibosConSobrante()
        {
            return ReadBoolParam("NOOBRECSO");
        }

        public bool GetParVisitasResultados()
        {
            return ReadBoolParam("VISRESULTADO");
        }


        /// <summary>
        /// Presupuesto de clientes convertir a entero codigo cliente
        /// </summary>
        /// <returns></returns>
        public bool GetParPresupuestosConvertirAIntCodigoCliente()
        {
            return ReadBoolParam("PRECLICODASINT");
        }
        public bool GetParBonoToPickers()
        {
            return ReadBoolParam("BONTOPICK") && Arguments.Values.CurrentModule != Modules.COMPRAS;
        }

        //Se cambio el parametro VALPEDCRE por el parametro PEDLIMCRE y PEDFACVEN para separar validar factura vencida y limite credito
        //public bool GetParValidarPedSiTieneCredito(bool isventas = false)
        //{
        //    return (ReadBoolParam("VALPEDCRE") && Arguments.Values.CurrentModule == Modules.PEDIDOS) || (ReadBoolParam("VALVENCRE") && (Arguments.Values.CurrentModule == Modules.VENTAS || isventas));
        //}

        public bool GetParValidarVenSiTieneCredito(bool isventas = false)
        {
            return (ReadBoolParam("VALVENCRE") && (Arguments.Values.CurrentModule == Modules.VENTAS || isventas));
        }

        //Valida si tiene factura vencida y no permite entrar a pedidos
        public bool GetParValidarPedidoSiTieneFacturaVencida()
        {
            return (ReadBoolParam("PEDFACVEN") && Arguments.Values.CurrentModule == Modules.PEDIDOS);
        }
        public bool GetParValidarFechaVencimiento()
        {
            return ReadBoolParam("VALFECVEN");
        }

        /// <summary>
        /// PERMITE IMPRIMIR LAS IMAGENES DEL LOGO MAS RAPIDO Y PERMITE IMPRIMIR EN ESCPOS Y ZEBRA EL LOGO
        /// </summary>
        /// <returns></returns>
        public bool GetParImageForLogo()
        {
            return ReadBoolParam("IMGFORLOG");
        }

        /// <summary>
        /// no permite crear visitas si existen recibos sin depositar que excedean el parvalor de este parametro que es el limite de horas.
        /// </summary>
        /// <returns></returns>
        public double GetParRecibosCantidadHorasMaximasSinDepositar()
        {
            return GetParDepositos() ? ReadDoubleParam("RECDEPHORMAX") : -1;
        }

        /// <summary>
        /// al aceptar o rechazar una carga de inventario sincroniza
        /// </summary>
        /// <returns></returns>
        public bool GetParSincronizarAlFinalizarCargaInventario()
        {
            return ReadBoolParam("CARSYNCFIN");
        }

        /// <summary>
        /// en recibos si hay sobrante, este se le resta a la factura que tenga un descuento mayor
        /// </summary>
        /// <returns></returns>
        public double GetParRecibosMontoSobranteConvertirADescuento()
        {
            if (Arguments.Values.CurrentModule == Modules.COBROS)
            {
                return ReadDoubleParam("RECSOB2DESC");
            }
            else
            {
                return -1;
            }
        }
        public bool GetParMoverDevolucionDesdeEntrega()
        {
            return ReadBoolParam("ENTDEVFROMENT");
        }
        public bool GetParPedCantidadSugeridos()
        {
            return ReadBoolParam("PEDCANTSUG");
        }

        //visualizar la cantidad minima en los row 8 y 2
        public bool GetParProdShowCantMinima()
        {
            return ReadBoolParam("PROSHOMIN");
        }
        public bool GetParPedidosProductosUnidades()
        {
            return ReadBoolParam("PEDPROUND") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }
        public bool GetParPedidosProductosUnidadesForOrder()
        {
            return ReadBoolParam("PEDPROORDEN");
        }

        public int GetParPedidosDiasEntregaSinFeriado()
        {
            return ReadIntParam("PEDDIAENTSINFER");
        }

        public bool GetParDevolucionesConListaPrecios()
        {
            return ReadBoolParam("DEVCONLIP");

        }

        public bool GetParProductosUnidosConUnidadConListaPrecios()
        {
            return ReadBoolParam("PEDUNDCONPREC");

        }

        public int GetParCantidadDividirPrecioDevolucion()
        {
            return ReadIntParam("DEVCANTDIVPRE");
        }

        public bool GetParDevolucionSinItbis()
        {
            var result = ReadBoolParam("DEVSINITBIS");

            return result;

        }

        /// valida el total de cargas del detalle y cabecera = 1
        /// No valida el total de cargas del detalle y cabecera= 0
        /// valida el total de la cantidad de filas en cargas del detalle y cabecera = 2
        /// <returns></returns>
        public int GetParCargasNoValidaTotales()
        {
            return ReadIntParam("CARNOTOTAL");
        }

        public bool GetOfertasyDescuentosbyUnidadMedida()
        {
            return ReadBoolParam("OFEDESUND");
        }


        /// <summary>
        /// usar la carga inicial fast
        /// </summary>
        /// <returns></returns>
        public bool GetParCargaInicialFast()
        {
            return ReadBoolParam("CARINIFAST");
        }

        public string[] GetParVisitasTipoVisitas()
        {
            var parm = ReadStringParam("VISTIPO");

            if (!string.IsNullOrEmpty(parm))
            {
                return parm.Split(',');
            }
            return null;
        }

        public int GetTransaccionCantidadCopias(int titId)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select Cast(ParValor as int) as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { "COPIAS-IMP-" + titId.ToString() });

                if (list.Count > 0)
                {
                    return Convert.ToInt32(list[0].ParValor);
                }
                else
                {

                    list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(TitCopiasImpresion, '-1') as ParValor from TiposTransaccion where TitID = ?", new string[] { titId.ToString() });

                    if (list.Count > 0)
                    {
                        return Convert.ToInt32(list[0].ParValor);
                    }

                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return -1;
        }

        public double GetParPedidosRangoDescuentoMinimoEnPrecioModificado()
        {
            return ReadDoubleParam("PEDDESMINPRE");
        }

        public bool GetParPedidosSearchAutomatico()
        {
            return ReadBoolParam("PEDSEARCHAU");
        }

        public bool GetParPedidosSearchicloSemanaAnt()
        {
            return ReadBoolParam("RUTSEMANT");
        }

        public bool GetParInitPrinterManager()
        {
            return ReadBoolParam("INITPRINTER");
        }

        public bool GetParSolicitudActualizacionClienteDireccion()
        {
            return ReadBoolParam("SACD");
        }


        /// <summary>
        ///Forma de pago en tarjeta solo acepta pago en Pesos Dominicanos
        /// </summary>
        /// <returns></returns>
        public bool GetParTarjetaSoloPesosDominicano()
        {
            return ReadBoolParam("FORTARJDOP");
        }


        /// <summary>
        /// No genera recibo automatico en ventas por diferencia de conteo.
        /// </summary>
        /// <returns></returns>
        public bool GetParNoReciboAutomatico()
        {
            return ReadBoolParam("CONTNOREC");
        }

        /// <summary>
        /// No Redondeo en monto itbis
        /// </summary>
        /// <returns></returns>
        public bool GetParItbisSinRedondeo()
        {
            return ReadBoolParam("ITBISSINRE");
        }


        /// <summary>
        /// Conteo con almacen de devolucion y despacho
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoConAlmacenDespachoyDevolucion()
        {
            return ReadBoolParam("CONDESDEV");
        }

        /// <summary>
        /// mostrar productos solo en inventario en conteo fisico
        /// </summary>
        /// <returns></returns>
        public bool GetProductosEnInventarioEnConteoFisico()
        {
            return ReadBoolParam("CONPROINV");
        }
        public string GetParSacCampoObligatorios()
        {
            return ReadStringParam("SACCAMPOBLIG");
        }

        public string GetParShowTabsEspecificos()
        {
            return ReadStringParam("TABESFORSHOW");
        }

        public bool GetParHomeBtnProspectos()
        {
            return ReadBoolParam("HOMEPROSP");
        }
        public bool GetParNoObligarAuditorHaacerComentarios()
        {
            return ReadBoolParam("SACCOMENOOBLI");
        }
        public bool GetParMostrarClientesPosMayorAcero()
        {
            return ReadBoolParam("SACVERLIST");
        }

        public bool GetParMostrarDocenasEnAgregarProductos()
        {
            return ReadBoolParam("AGRVERDOC");
        }

        public bool GetParMostrarProductosEnInventarioParaInventarios()
        {
            return ReadBoolParam("INVMOSSOL") && Arguments.Values.CurrentModule == Modules.INVFISICO;
        }



        /// <summary>
        /// no permite cerrar el cuadre si hay entregas pendientes antes del conteo
        /// </summary>
        /// <returns></returns>
        public int GetParCuadresValidarEntregasPendienteParaCerrarAntesDelConteo()
        {
            return ReadIntParam("CUACIEVALCONT");
        }

        /// <summary>
        /// Redondeo en monto itbis total final
        /// </summary>
        /// <returns></returns>
        public bool GetParItbisRedondeoGeneral()
        {
            return ReadBoolParam("ITBISREGEN");
        }

        /// <summary>
        /// Redondeo en monto descuento total final
        /// </summary>
        /// <returns></returns>
        public bool GetParDescuentoRedondeoGeneral()
        {
            return ReadBoolParam("DESCREGEN");
        }

        public bool GetParRequisicionesInventario()
        {
            return ReadBoolParam("REQINV");
        }

        public int GetParCantidadDiasParaVencimiento()
        {
            return ReadIntParam("DEVMOTVEN");
        }


        /// <summary>
        /// Permite realizar recibos con sobrante. el Parvalor es el limite maximo permitido
        /// el parametro moneda que recibe este Parametro se utiliza para filtrar el parametro definido segun la moneda base de cuentas por cobrar
        /// ejemplo : RECPERSOB-USD, RECPERSOB-DOP, RECPERSOB-EUR
        /// </summary>
        /// <returns></returns>
        public int GetParRecAceptarSobrante(string Moneda)
        {
            if (string.IsNullOrWhiteSpace(Moneda)) throw new Exception();
            return ReadIntParam("RECPERSOB-" + Moneda);
        }


        /// <summary>
        /// Cargas permitidas x cuadre
        /// </summary>
        /// <returns></returns>
        public int GetParCargasPermitidasByCuadre()
        {
            return ReadIntParam("CARPERMCUA");
        }

        public bool GetParDigitaUlt4DigitosTarjeta()
        {
            return ReadBoolParam("FPTARJ4DIG");
        }

        public bool GetParShowRowInfoPedidosEntregar()
        {
            return ReadBoolParam("ROWPEDENTINF");
        }

        /// <summary>
        /// Al cerrar el cuadre valida que existan cargas aceptadas para entrar al conteo fisico
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadresCerrarValidandoCargaAceptada()
        {
            return ReadBoolParam("CONTVALCARGA");
        }

        /// <summary>
        /// toma el campo CarReferenciaEntrega para actualizar la entrega segun sea aceptada o cancelada
        /// </summary>
        /// <returns></returns>
        public bool GetParCargasConReferenciaEntrega()
        {
            return ReadBoolParam("CARCONENT");
        }

        public bool GetParHomeBtnCargasCancelar()
        {
            return ReadBoolParam("CARGACANCEL");
        }
        public bool GetParNoShowProInTemp()
        {
            return ReadBoolParam("INVNOSHTEMP") && Arguments.Values.CurrentModule == Modules.INVFISICO;
        }
        public bool GetParCatalogoProductos()
        {
            return ReadBoolParam("CATALOGOPROD");
        }

        public bool GetParCatalogoProductosVertical()
        {
            return ReadBoolParam("CATPRODVER");
        }
        public bool GetParTakeFromNCF2021()
        {
            return ReadBoolParam("NCFTAKE2021");
        }
        public int GetParTakeLastFacturasInDevoluciones()
        {
            return ReadIntParam("DEVFACTCANT");
        }


        /// <summary>
        /// no permite la venta ni pedidos a clientes diferentes a estatus activo
        /// </summary>
        /// <returns></returns>
        public bool GetParNoVenForProspecto()
        {
            return ReadBoolParam("VENNOPROSP");
        }
        public bool GetParNoAgruparEnVentas()
        {
            return ReadBoolParam("VENTAGR") && (Arguments.Values.CurrentModule == Modules.VENTAS || (Arguments.Values.CurrentModule == Modules.DEVOLUCIONES && GetParDevolucionCondicion()));
        }
        public bool GetParTotalUnitarioSinItbis()
        {
            return ReadBoolParam("TOTALNOIT");
        }

        public int GetParInventariosTomarCantidades()
        {
            return Arguments.Values.CurrentModule == Modules.INVFISICO ?  ReadIntParam("INVAREACANT") : -1;
        }

        public int GetParColocacionProductosTomarCantidades()
        {
            var parinvArea = GetParColocacionProductosCapturarArea();

            if(parinvArea == null)
            {
                return -1;
            }

            return Arguments.Values.CurrentModule == Modules.COLOCACIONMERCANCIAS && parinvArea == "D" ? ReadIntParam("COLPRODAREACANT") : -1;
        }

        public int GetParLimiteMaximoPromociones()
        {
            return ReadIntParam("PROMCANTMAX");
        }

        /// <summary>
        /// en el catalogo de producto solo muestra los productos que tengan imagen
        /// </summary>
        /// <returns></returns>
        public bool GetParCatalogoProductoSoloConImagen()
        {
            return ReadBoolParam("CATIMGOBL");
        }

        public double GetParPedidosDescuentoManualGeneral()
        {
            if (Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                return ReadDoubleParam("PEDDESCGEN");
            } else if (Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                return ReadDoubleParam("COTDESCGEN");
            } else
            {
                return -1;
            }
        }

        /// <summary>
        /// al momento de aplicar el descuento general a los productos no se los aplica si el producto en ProDato3 tiene una E
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosBloquearProductosDescuentoGeneralConE()
        {
            return ReadBoolParam("EXPRODESGEN");
        }


        /// <summary>
        /// si se tiene el parametro de descuento general y el descuento especificado es mayor al limite pide una autorizacion
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosDescuentoGeneralAutorizar()
        {
            return ReadBoolParam("PINDESCGEN");
        }

        /// <summary>
        /// activa el modulo de auditoria de precios
        /// </summary>
        /// <returns></returns>
        public bool GetParAuditoriaPrecios()
        {
            return ReadBoolParam("AUDITORIAPRECIO");
        }

        /// <summary>
        /// en cotizaciones si se tiene el parametro de oferta manual: COTOFEMANUAL, no permite que la oferta digitada exceda el % especificado en este parametro
        /// </summary>
        /// <returns></returns>
        public double GetParCotizacionesLimiteMaximoOfertaManual()
        {
            if (Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                return ReadDoubleParam("COTOFEMAX");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Permite Agregar entregas en cero
        /// </summary>
        /// <returns></returns>
        public bool GetParEntregasAgregarCero()
        {
            return ReadBoolParam("ENTAGRCER");
        }

        public string GetParEntregasMostrarMensaje()
        {
            return ReadStringParam("ENTMOSMEN");
        }

        public string GetParEntregasMostrarMensajeCreditoFiscal()
        {
            return ReadStringParam("ENTMOSMENFIS");
        }


        public bool GetParNotaCreditoPorDevolucion() { return ReadBoolParam("NCDEV"); }

        /// <summary>
        /// no permite cerrar el cuadre si hay clientes sin visitar
        /// </summary>
        /// <returns></returns>
        public string GetParCuadresValidarClientesVisitadosParaCerrarAntesDelConteo()
        {
            return ReadStringParam("CUAVALCIERCONT");
        }

        public double GetParRecAceptarFaltanteToSel()
        {
            return ReadDoubleParam("RECFALTANTE2DESC");
        }

        //public bool GetParSeleccionarVisitaEnCliente()
        //{
        //    return ReadBoolParam("CLISELVIS");
        //}

        public bool GetParPedidosMostrarPresupuesto()
        {
            return ReadBoolParam("PEDPRESU") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        public bool GetParConsultaInventarioFisico()
        {
            return ReadBoolParam("INVFCONSULT");
        }



        /// <summary>
        ///  Muestra el combo para seleccionar el cliente para el orden
        /// </summary>
        public bool GetParProspectoCliRutOrden()
        {
            return ReadBoolParam("PROCLIRUTORD");

        }

        /// <summary>
        ///  Muestra el combo para seleccionar el cliente para el orden
        /// </summary>
        public bool GetParSacCliRutOrden()
        {
            return ReadBoolParam("SACCLIRUTORD");

        }


        public bool GetParSectoresPendientes()
        {
            return ReadBoolParam("SECPENPER");

        }

        /// <summary>
        ///  No muestra precios en el row de productos 1
        /// </summary>
        public bool GetParNoMostrarProductos()
        {
            return ReadBoolParam("PRONOSHOPRE");

        }

        /// <summary>
        ///  No tomar el sector para dar los productos en ofertas
        /// </summary>
        public bool GetParNoDarOfertaXSector()
        {
            return ReadBoolParam("OFEDARXSECTOR");
        }

        public bool GetParPedidosIgnorarBloqueoProductos()
        {
            return ReadBoolParam("PEDBLQPRDANU") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// en la pantalla de agregar productos (Pedidos, devoluciones, cotizaciones, compras, etc) cambia las posiciones de los view de precio y cantidad 
        /// poniendo la cantidad arriba y el precio abajo.
        /// </summary>
        /// <returns></returns>
        public bool GetParCambiarPrecioPorCantidadEnOrdenDePosicion()
        {
            return ReadBoolParam("PRODCANTYPREC");
        }

        public bool GetParAuditoriaPreciosCapturarFacing()
        {
            return ReadBoolParam("AUDPFACING") && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS;
        }

        public bool GetParAuditoriaPrecioCapturarPresencia()
        {
            return ReadBoolParam("AUDPPRESENCIA") && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS;
        }

        public bool GetParDevolucionCondicion()
        {
            return ReadBoolParam("DEVCONDICION");
        }

        public bool GetParDevolucionesCondicionUnico() { return ReadBoolParam("DEVCONUNICO"); }//en devoluciones en el tab de configurar pide la condicion de los productos en la devolucion y solo deja agregar productos con una sola condicion

        //oculta el btn de conduces si el cliente no tiene K en clidatosOtros
        public bool GetParConducesAClientesNombrados()
        {
            return ReadBoolParam("CONCLINOMBRADOS");
        }

        public int GetParDevolucionesFacturasLimiteDias()
        {
            if (GetParDevolucionesProductosFacturas())
            {
                return ReadIntParam("DEVFACTDIAS");
            }
            else
            {
                return -1;
            }
        }

        public bool GetParInventarioFisicoCapturarFacing()
        {
            return ReadBoolParam("INFCNTFACING") && Arguments.Values.CurrentModule == Modules.INVFISICO;
        }

        public bool GetParRecPinAutorizacion()
        {
            return ReadBoolParam("RECIMPAUT");
        }

        public bool GetParPedPinAutorizacion()
        {
            return ReadBoolParam("PEDIMPAUT");
        }
        public bool GetParDevPinAutorizacion()
        {
            return ReadBoolParam("DEVIMPAUT");
        }

        public bool GetParPedidosConsultarVencimientosProductos()
        {
            return ReadBoolParam("PedConLot") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }


        /// <summary>
        /// al intentar cancelar carga pide la clave de un auditor
        /// </summary>
        /// <returns></returns>
        public bool GetParCancelarCargaPorAuditor()
        {
            return ReadBoolParam("CANCELCARAUDITOR");
        }

        public bool GetParNotaCreditoVerPagoContado() { return ReadBoolParam("NCVERCON"); }

        public bool GetParCobrosMuestralblBalance() { return ReadBoolParam("COBLBLBALANCE"); }

        /// <summary>
        /// mueve las cantidades del almacen de despacho al almacen de devolucion para contar un solo almacen
        /// </summary>
        /// <returns></returns>
        public bool GetParMoverCantidadesDespachoaDevolucion()
        {
            return ReadBoolParam("INVDESPTODEV");
        }

        /// <summary>
        /// visualiza inventario por lote en ventas rancheras
        /// </summary>
        /// <returns></returns>
        public bool GetParVerInventarioConLoteAutomatico()
        {
            return ReadBoolParam("VENSHOWINVLOT") && (Arguments.Values.CurrentModule == Modules.VENTAS
                || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA);
        }


        public bool GetParVentaRancheraConLote()
        {
            return ReadBoolParam("VENRANLOT") && Arguments.Values.CurrentModule == Modules.VENTAS;
        }

        /// <summary>
        /// elimina el temporal duplicado en conteos con lotes
        /// </summary>
        /// <returns></returns>
        public bool GetParEliminaTempEnConteoConLote()
        {
            return ReadBoolParam("CONLOTDELTEMP");
        }


        /// <summary>
        /// Conteo con mas de 2 almacenes
        /// </summary>
        /// <returns></returns>
        public bool GetParConteoConVariosAlmacenes()
        {
            return ReadBoolParam("CONALMACENES");
        }

        // Id de Almacenes que no se restaran cantidades  al realizar conteo fisico
        public string GetParNoCuadrarAlmacen()
        {
            return ReadStringParam("INVNOLIMP");
        }

        // Si en cantidades se utiliza monto por libras detalladas y no usa unidades
        public bool GetParAsignacionLotesByFechaVencimiento()
        {
            return ReadBoolParam("INVLOTFEVEN");
        }

        // REBAJA CANTIDAD TEMPORAL AL INVENTARIO  PARA DAR OFERTA A LOTE CORRESPONDIENTE
        public bool GetParRebajarCantidadTempParaOferta()
        {
            return ReadBoolParam("VENCANTINTEMP");
        }

        // No agrupara por lote en ventas
        public bool GetParVentasNoAgruparxLote()
        {
            return ReadBoolParam("VENTANOAGLOT") && Arguments.Values.CurrentModule == Modules.VENTAS; ;
        }

        // No Mostrar los Prospectos en el maestro de clientes
        public bool GetParListaClientesMostrarProspectos()
        {
            return ReadBoolParam("PROSVIS");
        }

        // No agrupara por lote en ventas
        public bool GetParVentasFaltantesPorAlmacen()
        {
            return ReadBoolParam("VENTAALMID");
        }

        // No salir de clientes, sin haber llenado cada artículo del inventario.
        public bool GetParClientesTodosInventarios()
        {
            return ReadBoolParam("CLITODINV");
        }

        // se selecciona el conid de la forma de pago para ventas.
        public int GetParRecElegirFormapagoParaVentas()
        {
            return ReadIntParam("RECFORMVEN");
        }


        public int GetParSegundoConIdFormaPagoContado()
        {
            return ReadIntParam("VENFORPAGO2");
        }


        /// <summary>
        /// para que luego de conteofisico mande a crear el deposito antes de cerrar cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParDepositoNoCerrarCuadre()
        {
            return ReadBoolParam("DEPNOCERRCUA");
        }

        // Agrupar Por Cantidades en el detalle de las transacciones.
        public bool GetParTransCantAgrupar()
        {
            return ReadBoolParam("TRANANAG");
        }

        // Autorizar para no tomar en cuenta los pedidos minimos.
        public bool GetParAutorizarParaPedidosMinimos()
        {
            return ReadBoolParam("PEDAUTMIN");
        }

        //Vendedor que va a contener todos los vendedores y clientes.
        public bool GetParVendedorContVend()
        {
            return ReadBoolParam("VENCONTVEN");
        }

        //Mostrar estados de cuentas en cuentas x cobrar.
        public bool GetParCxcShowEstadoDeCuentas()
        {
            return ReadBoolParam("CXCSHOESTCU");
        }

        public bool GetParConteoFisicoNoContarAlmacenesEnCero()
        {
            return ReadBoolParam("CONNOSININV");
        }

        /*validar productos con diferentes unmcodigo, no seran catalogados como 
         productos distintos a la hora de ser agregados*/
        public bool GetParProdUseUnmCodigo()
        {
            return ReadBoolParam("TRANPRODUNM") && (Arguments.Values.CurrentModule == Modules.PEDIDOS
                                                  || Arguments.Values.CurrentModule == Modules.COTIZACIONES
                                                  || Arguments.Values.CurrentModule == Modules.DEVOLUCIONES);
        }

        public bool GetParCambioMercanciaInsertarLotesParaRecivir()
        {
            return ReadBoolParam("CAMINLOT") && Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA;
        }

        public int GetFormatoResumenDeRecibos()
        {
            return ReadIntParam("FORMRESREC");
        }

        public bool GetParNoEntregarVentasParaciales()
        {
            return ReadBoolParam("VENTENTPAR") && Arguments.Values.CurrentModule == Modules.VENTAS;
        }

        public bool GetParClientesCambiarValorPorCanid()
        {
            return ReadBoolParam("CLICAMVALCAN");
        }
        public string GetParCamposEspecificosForTiPLocal(string local = "")
        {
            return ReadStringParam("CAPFORTIPLOC" + local);
        }

        public string GetParCamposEspecificosForTiPCliente(string cliente = "")
        {
            return ReadStringParam("CAPFORTIPCLI" + cliente);
        }

        public string GetParCamposEspecificosForTiPNegocios(string negocios = "")
        {
            return ReadStringParam("CAPFORTIPNEG" + negocios);
        }

        public bool GetParNoEntregasParaciales()
        {
            return ReadBoolParam("ENTNOTPAR") && Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR;
        }

        //No permitir abonos en cheques devueltos parvalor = 1
        public bool GetParNoAgregarAbonosCKD()
        {
            return ReadBoolParam("RECCKDNOABO");
        }

        /// <summary>
        /// descuentos en Devoluciones
        /// </summary>
        /// <returns></returns>
        public bool GetParDescuentosEnDevoluciones()
        {
            return ReadBoolParam("DEVDES") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES;
        }

        /// <summary>
        /// Trae historico facturas desde Cuentas x Cobrar
        /// </summary>
        /// <returns></returns>
        public bool GetParHistoricoFacturasFromCuentasxCobrar()
        {
            return ReadBoolParam("HISFATCXC");
        }

        /// <summary>
        /// imprime solo la linea de cambio del lote entregado
        /// </summary>
        /// <returns></returns>
        public bool GetParCambiosImprimirLoteEntregado()
        {
            return ReadBoolParam("CAMTIPOENT");
        }

        public int GetParFotoParaTransferenciaBancarias()
        {
            try
            {
                if (Arguments.Values.CurrentModule != Modules.COBROS)
                {
                    return -1;
                }

                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ifnull(ParValor, '0') as ParValor from RepresentantesParametros where UPPER(ltrim(rtrim(ParReferencia))) = ?", new string[] { "RECFOTOTRANS" });

                if (list != null && list.Count > 0)
                {
                    int.TryParse(list[0].ParValor.Trim(), out int result);

                    return result;
                }
                else
                {
                    return 1;
                }

            }
            catch (Exception) { }

            return 1;
        }


        /// <summary>
        /// En conteo fisico si el inventario es con lote, agrupa dichos lotes para que en conteo no se utilize la captura de lotes
        /// </summary>
        /// <returns></returns>
        public bool GetParConteosFisicosLotesAgrupados()
        {
            return Arguments.Values.CurrentModule == Modules.CONTEOSFISICOS && GetParUsarMultiAlmacenes() ? ReadBoolParam("CONTLOTEAG") : false;
        }

        /// <summary>
        /// Imprime facturas de faltantes al cierre del cuadre y no en la finalizacion de cada conteo fisico 
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadreImprimirFacturasxFaltantes()
        {
            return ReadBoolParam("CUAVENFALT");
        }

        /// <summary>
        /// en recibos permite cobrar todos los documentos pendientes con una misma moneda sin importar la moneda del documentos
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosusarMonedaUnica()
        {
            return ReadBoolParam("RECMONUNICA") && Arguments.Values.CurrentModule == Modules.COBROS;
        }

        public bool GetParHomeStandardMostrarReporte()
        {
            //REPOICON
            var result = true;

            try
            {
                var list = SqliteManager.GetInstance().Query<RepresentantesParametros>("select ParValor from RepresentantesParametros " +
                    "where UPPER(ltrim(rtrim(ParReferencia))) = ? ", new string[] { "REPOICON" });

                if (list != null && list.Count > 0 && !string.IsNullOrWhiteSpace(list[0].ParValor))
                {
                    result = list[0].ParValor.Trim().Equals("1");
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            if (GetParHomeBtnReportes())
            {
                result = false;
            }

            return result;
        }

        public bool GetParHomeStandardMostrarPresupuesto()
        {
            //REPOICON
            return ReadBoolParam("PRESUICON");
        }


        // Carga la RutaVisitasFecha desde EntregasRepartidorRutaVisitasFecha
        public bool GetParRutaVisitasFechaFromEntregasRepartidorRutaVisitasFecha()
        {
            return ReadBoolParam("RUTAVISENT");
        }

        // Tiempo entre una llamada al gps y otra
        public int GetParVisitaGPSDelay()
        {
            return ReadIntParam("VISGPSDELAY");
        }

        /// <summary>
        /// en devoluciones carga la condicion por defecto.
        /// </summary>
        /// <returns></returns>
        public string GetParDevolucionesCondicionPorDefecto()
        {
            return ReadStringParam("DEVCONDEF");
        }


        /// <summary>
        /// Este parametro habilita la aplicacion de cargas con estatus 7
        /// </summary>
        /// <returns></returns>
        public bool GetParCargasAplicacionAutomaticas()
        {
            return ReadBoolParam("CARAPAUTO");
        }

        public bool GetParPedidosMostrarCantidadConfirmada()
        {
            return ReadBoolParam("PEDCANTCONFIRM");
        }

        public bool GetParSacCapturarHorario()
        {
            return ReadBoolParam("SACCLIHORARIO");
        }

        public int GetParVisitasSeleccionarTipoManual()
        {
            int value = ReadIntParam("VISTIPOMANUAL");

            if (value == -1)
            {
                value = 0;
            }

            return value;
        }

        //Sincronizacion automatica al entrar a la visita
        public bool GetSyncAutoInVisita() { return ReadBoolParam("SYNCAUTOVIS"); }

        public int GetParConvertirCotizacionesAPedidos()
        {
            return ReadIntParam("COTCONV2PED");
        }
        public bool GetParNoTomarEnCuentaChequesDiferidos()
        {
            return ReadBoolParam("NOLIMTCHDIF");
        }
        public bool GetParPedidosOfertasEnInventarios()
        {
            return ReadBoolParam("PEDOFEINV") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// la distancia a la que el gps se actualiza
        /// </summary>
        /// <returns></returns>
        public double GetParGpsDistanciaMinimaActualizacion()
        {
            var result = ReadDoubleParam("GPSDISTACT");

            if(result <= 0)
            {
                result = 15;
            }

            return result;
        }
        public bool GetParPedidosVisualizarXClientesDetalles()
        {
            return ReadBoolParam("PEDCLIGRPP") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }
        public bool GetParRecNoAgregarVariasFormPagTrans()
        {
            return ReadBoolParam("RECFORMTRANS");
        }

        public bool GetParDirrecionDelVendedor()
        {
            return ReadBoolParam("DIRVENDEDOR");
        }

        public bool GetParNCPorDescuentoProntoPagoImpresion() { return ReadBoolParam("NCDPPIMP"); }

        /// <summary>
        /// Establece la formas de pago validas para todos los clientes
        /// </summary>
        /// <returns>bool</returns>
        public string GetParFormasPagoValidasGeneral() { return ReadStringParam("FORPAGOGEN"); }

        /// <summary>
        /// Convierte balances en Pesos a Dolares
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParConvertirBalanceADolares() { return ReadBoolParam("CXCBALDOLAR"); }

        // Autorizar para dar precio por debajo del minimo establecido.
        public bool GetParAutorizarParaPedidosPrecioMinimo()
        {
            return ReadBoolParam("PEDAUTPREMIN");
        }

        public int GetParGastosCantidadFotoObligatoria()
        {
            return ReadIntParam("GASIMGOBLIG");
        }

        /// <summary>
        /// al intentar cerrar un cuadre se valida el minimo y maximo kilometraje permitido del vehiculo, usando la columna VehCapacidadCombGls de vehiculos
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadresValidarCantidadKmPromedioPorDia()
        {
            return ReadBoolParam("CUAVEHKMXDIA");
        }
        public bool GetParClientesMostrarUrba()
        {
            return ReadBoolParam("CLIROWFORMAT");
        }
        
        /// <summary>
        /// activa el modulo de colocacion de mercancias
        /// </summary>
        /// <returns></returns>
        public bool GetParColocacionProductos()
        {
            return ReadBoolParam("COLPRODUCTOS");
        }
        
        /// <summary>
        /// 1- Permite al Auditor Loguearse en la pantalla principal
        /// 0- No Permite Loguearse en la pantalla principal
        /// </summary>
        /// <returns></returns>
        public bool GetParPermitLoginForAud()
        {
            return ReadBoolParam("AUDLOGIN");
        }

        public int GetParInventariosTomarCantidadesSinDetalle()
        {
            return Arguments.Values.CurrentModule == Modules.INVFISICO ? ReadIntParam("INVAREACANTDET") : -1;
        }

        public bool GetParDeleteOfertasInTemp()
        {
            return ReadBoolParam("OFEDELTEMP");
        }

        /// <summary>
        /// en ventas si se tiene el parametro de bloquear ventas si tiene facturas vencidas, pide una autorizacion para dejar continuar
        /// </summary>
        /// <returns></returns>
        public bool GetParVentasAutorizarVentaConFacturasVencidas()
        {
            return ReadBoolParam("VENAUTFACTVENC");
        }
        
        public bool GetParQuitarDescuentoVisible()
        {
            return ReadBoolParam("RECQUITARDESC");
        }

        /// <summary>
        /// convierte los documentos a una sola moneda basandose en la tasa de la factura
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosMonedaUnicaConTasaDocumento()
        {
            return ReadBoolParam("RECMONTASADOC") && Arguments.Values.CurrentModule == Modules.COBROS;
        }

        /// <summary>
        /// al crear una transaccion se recuerda el nombre del cliente con una alerta
        /// </summary>
        /// <returns></returns>
        public bool GetParTransaccionesAlertaConfirmarCliente()
        {
            return ReadBoolParam("TRANSCONFCLI");
        }

        public bool GetParPrecioSinRedondeo()
        {
            return ReadBoolParam("PRESINRE");
        }

        //Procentaje de maximo para Diferencia Cambiaria a aplicar automaticamente
        public double GetParRecibosPorcientoLimiteDiferenciaCambiaria()
        {
            return ReadDoubleParam("RECPRCCDIFC");
        }

        //limite maximo para digitar Redondeo en Recibo
        public double GetParRecibosLimiteRedondeo()
        {
            return ReadDoubleParam("RECLIMRED");
        }

        /// <summary>
        /// al cerrar un cuadre elimina del inventario los productos del cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParEliminarInventarioEnCeroAlCerrarCuadre()
        {
            return ReadBoolParam("CUAINVELM");
        }

        public bool GetParNoticiasEnOperaciones( )
        {
            return ReadBoolParam("NOTICIAINOP");
        }


        /// <summary>
        /// la app coje la orientacion de la pantalla que diga el sensor
        /// </summary>
        /// <returns></returns>
        public bool GetParOrientationPantallaSensor()
        {
            return ReadBoolParam("SCREENROT");
        }

        /// <summary>
        /// Unidad de Medida especifica para productos en Devoluciones
        /// </summary>
        /// <returns>si no devuevle nada no afecta el flujo</returns>
        public string GetParUnmCodigoEspecificaDevoluciones()
        {
            return ReadStringParam("DEVUNMCODIGO");
        }


        /// <summary>
        ///  muestra precio en devoluciones 
        /// </summary>
        public bool GetParDevolucionesConPrecio()
        {
            return ReadBoolParam("DEVCONPRE");

        }

        /// <summary>
        ///  No permite visitar un cliente fuera de la ruta diaria 
        /// </summary>
        public bool GetParNoVisitarClienteFueraRuta()
        {
            return ReadBoolParam("VISNORUTACLI");

        }

        public int GetParValidarKilometrajeEnCuadres()
        {
            return ReadIntParam("CUAVALKM");
        }

        public bool GetParImprimirVendedor()
        {
            return ReadBoolParam("VENIMPVEN");

        }

        /// <summary>
        /// Autorizacion al anular una devolucion
        /// </summary>
        /// <returns></returns>
        public bool GetParAutorizacionAnularDevolucion()
        {
            return ReadBoolParam("DEVAUTANU");
        }

        /// <summary>
        /// Autorizacion al editar una devolucion
        /// </summary>
        /// <returns></returns>
        public bool GetParAutorizacionEditarDevolucion()
        {
            return ReadBoolParam("DEVAUTEDIT");
        }

        /// <summary>
        /// indicara cuales centro de distribucion son permitidos para un representante en especifico en pedidos, 
        /// el parvalor seran los ID de los centros de distribucion separado por comas, por ejemplo 1,2,5,20
        /// </summary>
        /// <returns></returns>
        public string[] GetParFiltroCentroDistribucion()
        {
            var par = ReadStringParam("PEDCEDID");

            if (!string.IsNullOrWhiteSpace(par))
            {
                return par.Split(',');
            }

            return null;
        }

        //No permitir abonos en recibos, oculta la opcion
        public bool GetParNoAgregarAbonos()
        {
            return ReadBoolParam("RECNOABO");
        }

        //Pide autorizacion para aplicar una nota de credito
        public bool GetParRecAplicarNCByAutorizacion()
        {
            return ReadBoolParam("RECAUTNC");
        }

        public bool GetParNoValidarMontoCeroConBonoToPickers()
        {
            return ReadBoolParam("RECNOMONBON");
        }


        /// <summary>
        /// En recibos a los documentos de una moneda en especifico permite cobrar la forma de pago con una sola moneda,,
        /// el parvalor es el tipo de moneda de documento y la moneda de la forma de pago separado por | , 
        /// Ejemplo1: DOP|DOP = permite cobrar documentos en DOP solo con formas de pago en moneda DOP
        /// Ejemplo2: DOP|USD = permite cobrar documentos en DOP solo con formas de pago en moneda USD
        /// </summary>
        /// <returns></returns>
        public string[] GetParRecibosFormaPagoMonedaUnica()
        {
            var par = ReadStringParam("RECFORMONUNICA");

            if (!string.IsNullOrWhiteSpace(par))
            {
                return par.Split('|');
            }

            return null;
        }
        /// <summary>
        /// Parametro para tomar la nota de impresion por la forma de pago y no el formato de impresion.
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosNotasPorFormaPago()
        {
            return ReadBoolParam("RECNOTABYFORP");
        }

        public bool GetParOcultarImagenInFocus()
        {
            return ReadBoolParam("NOIMGFOCUS");
        }
        public bool GetParRecDetalleV2()
        {
            return ReadBoolParam("RECDETALLEV2");
        }

        public string GetParMesesByUsomultiples()
        {
            var result = ReadStringParam("PREMESESUSU");

            if (string.IsNullOrEmpty(result))
            {
                result = "MESES";
            }

            return result;
        }

        /// <summary>
        /// Calcula monto con Recibos Anulados en Deposito
        /// </summary>
        /// <returns>bool</returns>
        public bool GetParDepositosConRecibosAnulados() { return ReadBoolParam("DEPRECANUL"); }

        /// <summary>
        /// Autorizacion y comentario obligatorio al anular una venta
        /// </summary>
        /// <returns></returns>
        public bool GetParAutorizacionAnularVentaComentarioObligatorio()
        {
            return ReadBoolParam("VENAUTANUCOM");
        }

        /// <summary>
        /// Autorizacion  al anular una venta
        /// </summary>
        /// <returns></returns>
        public bool GetParAutorizacionAnularVenta()
        {
            return ReadBoolParam("VENAUTANU");
        }

        /// <summary>
        /// cantidad de libras a convertir en reporte de pedidos  
        /// </summary>
        /// <returns></returns>
        public bool GetParReportePedidosConvertir125Libras()
        {
            return ReadBoolParam("REPPEDCONV");
        }

        /// <summary>
        /// habilita numero de orden en configuracion y exige que sea obligatorio si el cliente tiene caracteristica 'Y - No. Orden obligatorio'
        /// </summary>
        /// <returns></returns>
        public bool GetNumOrdenObligatorio()
        {
            if(Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                return ReadBoolParam("PEDORDENOBLI");
            }
            else if (Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                return ReadBoolParam("COTORDENOBLI");
            }
            else if (Arguments.Values.CurrentModule == Modules.VENTAS)
            {
                return ReadBoolParam("VENORDENOBLI");
            }

            return false;
        }

        /// <summary>
        /// trae tipos de mensajes sin otros
        /// </summary>
        /// <returns></returns>
        public bool GetParTipoMensajeSinOTROS()
        {
            return ReadBoolParam("TIPMENNOOTROS");
        }

        /// <summary>
        /// Parametro para reducir del inventario la cantidad del conduce, cuando no tenemos el parametro de multialmacenes
        /// </summary>
        /// <returns></returns>
        public bool GetParCondrestarinventario()
        {
            return ReadBoolParam("CONRESINV");
        }

        /// <summary>
        /// Crear pedido al cliente por diferencia en inventario fisico de conciliacion
        /// </summary>
        /// <returns></returns>
        public bool GetParCrearPedidoByInventarioFisico()
        {
            return ReadBoolParam("PEDINVF") && Arguments.Values.CurrentModule == Modules.INVFISICO;
        }


        public bool GetParAuditoriaPrecioCapturarPrecioOferta()
        {
            return ReadBoolParam("AUDPPRECIOOFE") && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS;
        }

        public bool GetParAuditoriaPrecioCapturarCaras()
        {
            return ReadBoolParam("AUDPCARAS") && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS;
        }

        public int GetParTipoPedidoParaInventarioFisico()
        {
            var result = ReadIntParam("PEDTIPOINV");

            if (result > 0)
            {
                return result;
            }

            return 1;
        }

        public int GetFormatoImpresionInventarioFisicosPDF()
        {
            var value = ReadStringParam("FORMINVFISPDF");

            return string.IsNullOrEmpty(value) ? 0 : int.Parse(value);
        }

        public double GetDescuentoMaximoByRepresentante()
        {
            var value = ReadStringParam("REPDESMAX");

            return string.IsNullOrEmpty(value) ? -1 : double.Parse(value);
        }

        public bool GetParMostrarDescuentosMaximos()
        {
            return ReadBoolParam("DESCMAXVISIBLE");
        }
        public bool GetParAuditoriaPrecioCapturarCarasPorPresencia()
        {
            return ReadBoolParam("AUDPCARASBYPRES") && Arguments.Values.CurrentModule == Modules.AUDITORIAPRECIOS;
        }

        /// <summary>
        /// Habilita el agregar productos al pedido desde los presupuestos que contengan en el tipo la palabra PROD y el ProID en el query
        /// </summary>
        /// <returns></returns>
        public bool GetParPresupuestosAgregaPedidos()
        {
            return ReadBoolParam("PRESADDPED");
        }

        /// <summary>
        /// Habilita la consulta de oferta general
        /// </summary>
        /// <returns></returns>
        public bool GetParConsultarOfertasGenerales()
        {
            return ReadBoolParam("OFERGEN");
        }

        /// <summary>
        /// Carga el combo de Facturas en Devoluciones desde HistoricosFacturas
        /// </summary>
        /// <returns></returns>
        public bool GetParCargarComboFacturasFromHistoricoFacturas() { return ReadBoolParam("DEVFACFROMHIST"); }

        /// <summary>
        /// En Pedidos Combina el ProDescripcion y ProDescripcion1
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosProductosCombinarDescripcion()
        {
            return ReadBoolParam("PEDPROCOMBDESCR") && Arguments.Values.CurrentModule == Modules.PEDIDOS;
        }

        /// <summary>
        /// Mostrar Campo ProDescripcion3
        /// 1: Muestra el Label de Equivalencia en Modal Detalle Producto
        /// 2: Muestra el Label de Equivalencia en Modal Detalle Producto y en Agregar Productos
        /// </summary>
        /// <returns></returns>
        public int GetProDescripcion3ProductoPed()
        {
            return ReadIntParam("PEDPRODES3");
        }

        /// <summary>
        /// en devoluciones habilita en On el switch de seleccion de factura por defecto
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesSwitchSeleccionFactura()
        {
            return ReadBoolParam("DEVSELFAC") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES;
        }

        /// <summary>
        /// en devoluciones al elegir un producto muestra un combobox para elegir la factura
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesNoValidaLimitexFactura()
        {
            return ReadBoolParam("DEVNOVALFACT") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES;
        }

        /// <summary>
        /// usar AreaCtrolCredit para gasso gasso
        /// </summary>
        /// <returns></returns>
        public bool GetParRecibosAreaCtrolCredit()
        {
            return ReadBoolParam("AREACTRCR");
        }

        /// <summary>
        /// En Inventario fisico activa el uso de lotes, con parvalor 1 el lote es editable, con parvalor 2 el lote es seleccionable
        /// </summary>
        /// <returns></returns>
        public int GetParInventarioFisicosLotes()
        {
            return Arguments.Values.CurrentModule == Modules.INVFISICO ? ReadIntParam("INVFISLOTE") : -1;
        }

        /// <summary>
        ///En devoluciones cuando se selecciona una factura desde historicoFacturas segmenta las cantidades normales y las ofertas
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesOfertasSeparadas() { return ReadBoolParam("DEVOFESEP") && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES; }

        /// <summary>
        ///Ofertas manuales con descuento 100%
        /// </summary>
        /// <returns></returns>
        public bool GetParOfertasManualesConDescuento100Porciento() { return ReadBoolParam("OFEMAN100"); }

        /// <summary>
        ///no puede cerrar el cuadre con cargas pendientes, siempre y cuando las cargas tengan fecha menor o igual al cuadre
        /// </summary>
        /// <returns></returns>
        public bool GetParCuadresNoCerrarConCargas() { return ReadBoolParam("CARCUAACEP"); }

        /// <summary>
        /// Permite validar la cantidad para productos retormables
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesNoValidaMotivo()
        {
            return ReadBoolParam("DEVNOMOTIVO") ;
        }
        public bool GetParMoatrarAlertaConfiguracion()
        {
            return ReadBoolParam("PEDDPPCONF") ;
        }

        /// <summary>
        /// Calcula y maneja Flete en Pedidos
        /// </summary>
        /// <returns></returns>
        /// 
        public bool GetCalculaFlete()
        {
            if (Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                return ReadBoolParam("PEDFLETE");
            }

            return false;
        }

        /// <summary>
        /// Monto Divisor del Flete
        /// </summary>
        /// <returns></returns>
        /// 
        public double GetMontoDivisorFlete()
        {
            var value = ReadStringParam("FLETEDIV");

            return string.IsNullOrEmpty(value) ? 0 : double.Parse(value);
        }

        /// <summary>
        /// 1: recuerda las lineas de usomultiple PEDLINRECORDAR de los productos que no fueron ingresados en el pedido
        /// 2: recuerda las lineas de usomultiple PEDLINRECORDAR de los productos que no fueron buscados y no existen en tabla PedidosBusqueda
        /// 3: bloquea el pedido si las lineas colocadas en usomultiple PEDLINRECORDAR no fueron buscadas y no existen en tabla PedidosBusqueda 
        /// </summary>
        /// <returns></returns>
        /// 

        public int GetLineasRecordar()
        {
            if (Arguments.Values.CurrentModule == Modules.PEDIDOS)
            {
                return ReadIntParam("PEDLINRECORDAR");
            }
            else if (Arguments.Values.CurrentModule == Modules.COTIZACIONES)
            {
                return ReadIntParam("COTLINRECORDAR");
            }
            else if (Arguments.Values.CurrentModule == Modules.VENTAS)
            {
                return ReadIntParam("VENLINRECORDAR");
            }

            return -1;
        }

        /// <summary>
        /// No permite guardar como preliminar un pedido Definitivo
        /// </summary>
        /// <returns></returns>
        public bool GetParPedidosNoCambiarDefinitivoAPreliminar()
        {
            return ReadBoolParam("PEDNODEFTOPRE");
        }

        //Cuando se tiene el Parametro DESCMANUAL si se desea que el % manual colocado no se recalcule por dias establecidos
        public bool GetParDescuentoManualNoValidaDias() { return ReadBoolParam("DESCMANNODIAS"); }

        public bool GetParVentaGeneraXmlFirmado() { return ReadBoolParam("VENGENXML"); }


        /// <summary>
        /// No Redondear Cantidad Detalle
        /// </summary>
        /// <returns></returns>
        public bool GetParCantidadRealSinRedondeo() { return ReadBoolParam("CANTREALSINRE"); }

        public string GetParCliTipoCliente1() { return ReadStringParam("CLITIPOCLIENTE1"); }
        public string GetParCliTipoCliente2() { return ReadStringParam("CLITIPOCLIENTE2"); }
        public string GetParCliTipoCliente3() { return ReadStringParam("CLITIPOCLIENTE3"); }

        public string GetParCliCat1() { return ReadStringParam("CLICAT1"); }
        public string GetParCliCat2() { return ReadStringParam("CLICAT2"); }
        public string GetParCliCat3() { return ReadStringParam("CLICAT3"); }

        public bool GetParVentasNoRebajaInventario() { return ReadBoolParam("VENSININV"); }

        public bool GetParConvertirCotizacionesAVentas()
        {
            return ReadBoolParam("COTCONV2VEN");
        }
        
        public bool GetParTaresActualizarAuto()
        {
            return ReadBoolParam("TARACTAUT");
        }


        public int GetParClientesMostrarCategoriasYTiposEnOperaciones()
        {
            return ReadIntParam("CLISHOWCATTIP");
        }

        public bool GetParOfertasUnificadas()
        {
            return ReadBoolParam("OFEUNIFICADA") && (Arguments.Values.CurrentModule == Modules.PEDIDOS || Arguments.Values.CurrentModule == Modules.VENTAS);
        }


        public bool GetParConteoFisicoUsaUnidadMedidaByListaPrecio()
        {
            return ReadBoolParam("CONFUNMLISTA");
        }

        /// <summary>
        /// en devoluciones permite la captura de productos adicionales a la factura seleccionada
        /// </summary>
        /// <returns></returns>
        public bool GetParDevolucionesPermitirCapturaProdAdicionales()
        {
            return ReadBoolParam("DEVPRODADIC");
        }

        public bool GetCodeBar()
        {
            return ReadBoolParam("TRACODIGOBARRA");
        }

        /// <summary>
        /// no permite mas de 1 requisicion por dia
        /// </summary>
        /// <returns></returns>
        /// 
        public bool GetParRequisicionInventarioUnaxDia()
        {
            return ReadBoolParam("REQINVDIA");
        }

        /// <summary>
        /// no muestra informacion de la empresa en la impresion ni en pdf
        /// </summary>
        /// <returns></returns>
        public bool GetParEmpresaNoMostrar()
        {
            return ReadBoolParam("NOINFOEMPRESA");
        }

        /// <summary>
        /// muestra varios inventarios SD y LV en row de productos
        /// </summary>
        /// <returns></returns>
        public bool GetParMostrarVariosInventariosEnRow()
        {
            return ReadBoolParam("SHOWINVMULROW");
        }

    }
}


