using MovilBusiness.Abstraction;
using MovilBusiness.Configuration;
using MovilBusiness.Enums;
using MovilBusiness.Internal;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.model.Internal.Structs.Args;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MovilBusiness.DataAccess
{
    public class DS_Recibos : DS_Controller
    {
        private DS_DescuentoFacturas myDesFac;
        private DS_TiposTransaccionesCXC myTipTran;
        private DS_Clientes myCli;

        public DS_Recibos()
        {
            myDesFac = new DS_DescuentoFacturas();
            myTipTran = new DS_TiposTransaccionesCXC();
            myCli = new DS_Clientes();
        }

        /** devuelve el monto total de todos los recibos que no esten anulador del cliente*/
        public double GetMontoTotalRecibosByCliId(int Id, string MonCodigo = null, string fecha = null /*yyyy-MM-dd*/)
        {
            //Prueba
            var SectorCondition = "";

            if (myParametro.GetParRecibosPorSector())
            {
                SectorCondition = " and AreaCtrlCredit = '" + Arguments.Values.CurrentSector?.AreaCtrlCredit + "'";

                if (myParametro.GetParAreaCrtlCreditoClienteSubString())
                {
                    SectorCondition = " and SUBSTR(AreaCtrlCredit, 1, 2) = '" + Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) + "'";
                }
            }

            if (!string.IsNullOrWhiteSpace(MonCodigo))
            {
                SectorCondition += " and trim(upper(MonCodigo)) = trim(upper('" + MonCodigo + "')) ";
            }

            if (!string.IsNullOrEmpty(fecha))
            {
                SectorCondition += " and RecFecha like '"+fecha+"%'";
            }

            var list = SqliteManager.GetInstance().Query<Recibos>("select CAST(SUM(RecMontoEfectivo + RecMontoCheque + " +
                "RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento - RecMontoSobrante) AS REAL) as RecTotal from Recibos " +
                "where CliID = ? and RecEstatus <> 0 " + SectorCondition, new string[] { Id.ToString() });

            if (list.Count > 0)
            {
                return list[0].RecTotal;
            }

            return 0;
        }

        public double GetMontoTotalRecibosByCliIdSinChequeDiferido(int Id, string MonCodigo = null, string fecha = null /*yyyy-MM-dd*/)
        {
            var SectorCondition = "";

            if (myParametro.GetParRecibosPorSector())
            {
                SectorCondition = " and AreaCtrlCredit = '" + Arguments.Values.CurrentSector.AreaCtrlCredit + "'";

                if (myParametro.GetParAreaCrtlCreditoClienteSubString())
                {
                    SectorCondition = " and SUBSTR(AreaCtrlCredit, 1, 2) = '" + Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) + "'";
                }
            }

            if (!string.IsNullOrWhiteSpace(MonCodigo))
            {
                SectorCondition += " and trim(upper(MonCodigo)) = trim(upper('" + MonCodigo + "')) ";
            }

            if (!string.IsNullOrEmpty(fecha))
            {
                SectorCondition += " and RecFecha like '" + fecha + "%'";
            }

            var list = SqliteManager.GetInstance().Query<Recibos>("select CAST(SUM(RecMontoEfectivo + RecMontoCheque + " +
                "RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento - RecMontoSobrante) AS REAL) as RecTotal from Recibos " +
                "where CliID = ? and RecEstatus <> 0 " + SectorCondition, new string[] { Id.ToString() });

            if (list.Count > 0)
            {
                return list[0].RecTotal;
            }

            return 0;
        }

        public double GetMontoTotalRecibosByCliIdyFecha(int Id, string MonCodigo = null, string fecha = null /*yyyy-MM-dd*/)
        {
            var SectorCondition = "";

            if (myParametro.GetParRecibosPorSector())
            {
                SectorCondition = " and AreaCtrlCredit = '" + Arguments.Values.CurrentSector.AreaCtrlCredit + "'";

                if (myParametro.GetParAreaCrtlCreditoClienteSubString())
                {
                    SectorCondition = " and SUBSTR(AreaCtrlCredit, 1, 2) = '" + Arguments.Values.CurrentSector.AreaCtrlCredit.Substring(0, 2) + "'";
                }
            }

            if (!string.IsNullOrWhiteSpace(MonCodigo))
            {
                SectorCondition += " and trim(upper(MonCodigo)) = trim(upper('" + MonCodigo + "')) ";
            }

            if (!string.IsNullOrEmpty(fecha))
            {
                SectorCondition += " and RecFecha like '" + fecha + "%'";
            }

            var list = SqliteManager.GetInstance().Query<Recibos>("select CAST(SUM(RecMontoEfectivo + RecMontoCheque + " +
                "RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento - RecMontoSobrante) AS REAL) as RecTotal from Recibos " +
                "where CliID = " + Id.ToString() + " and RecEstatus <> 0 " + SectorCondition + " " +
                "Union select CAST(SUM(RecMontoEfectivo + RecMontoCheque + " +
                "RecMontoChequef + RecMontoTransferencia + RecMontoTarjeta + RecMontoDescuento - RecMontoSobrante) AS REAL) as RecTotal from RecibosConfirmados " +
                "where CliID = " + Id.ToString() + " and RecEstatus <> 0 " + SectorCondition, new string[] { });

            if (list.Count > 0)
            {
                var Rectotal = 0.00;
                foreach (var rcb in list)
                {
                    Rectotal += rcb.RecTotal;
                }
                return Rectotal;
            }

            return 0.00;
        }

        public void ActualizarDepSecuencia(int recSecuencia, int depSecuencia, string rowguid)
        {
            Hash rec = new Hash("Recibos");
            rec.Add("DepSecuencia", depSecuencia);
            rec.Add("UsuInicioSesion", "mdsoft");
            rec.ExecuteUpdate("rowguid = '" + rowguid + "' ");
            //rec.ExecuteUpdate("RecSecuencia = " + recSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");


            rec = new Hash("RecibosConfirmados");
            rec.Add("DepSecuencia", depSecuencia);
            rec.Add("UsuInicioSesion", "mdsoft");
            rec.ExecuteUpdate("rowguid = '" + rowguid + "' ");
            //rec.ExecuteUpdate("RecSecuencia = " + recSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'");
        }

        public void ClearTemps()
        {
            SqliteManager.GetInstance().Execute("delete from RecibosDocumentosTemp");
            SqliteManager.GetInstance().Execute("delete from FormasPagoTemp");
            SqliteManager.GetInstance().Execute("delete from DocumentosAplicadosTemp");
        }

        public void ClearFormasPago()
        {
            SqliteManager.GetInstance().Execute("Delete from FormasPagoTemp");
        }

        public void ActualizarFechaDescuentoFacturasInTemp(string Fecha)
        {
            SqliteManager.GetInstance().Execute("Update RecibosDocumentosTemp set FechaDescuento = ? where UPPER(Sigla) <> 'NC'", new string[] { Fecha });
        }

        public void SaldarFacturaInTemp(RecibosDocumentosTemp Factura, bool NoDarDescuento, double tasa) { SaldarFacturaInTemp(Factura, NoDarDescuento, null, true, tasa); }
        public void SaldarFacturaInTemp(RecibosDocumentosTemp Factura, bool NoDarDescuento) { SaldarFacturaInTemp(Factura, NoDarDescuento, null); }
        public void SaldarFacturaInTemp(RecibosDocumentosTemp Factura, DescFactura Descuento) { SaldarFacturaInTemp(Factura, false, Descuento); }
        public void SaldarFacturaInTemp(RecibosDocumentosTemp Factura, bool NoDarDescuento, DescFactura Descuento, bool recalcularDescuento = true, double tasa = 0)
        {
            DescFactura descuentoFactura = Descuento;


            if(Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                descuentoFactura = new DescFactura();
            }else if (descuentoFactura == null)
            {
                descuentoFactura = myDesFac.GetMontoDescuentoFactura(Factura);
            }

            if (myParametro.GetParDescuentoAbonos() && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
            {
                var montoSujetoDescuento = 0.0;

                if (descuentoFactura.IndicadorItbis)
                {
                    montoSujetoDescuento = (Factura.MontoTotal - Math.Abs(Factura.Credito) - Math.Abs(Factura.CxcNotaCredito));
                }
                else
                {
                    montoSujetoDescuento = (Factura.MontoSinItbis - Math.Abs(Factura.CreditoSinItbis) - Math.Abs(Factura.CxcNotaCredito));
                }

                descuentoFactura.DescuentoValor = GetMontoDescuentoParaAbono(Factura, descuentoFactura.DescPorciento, montoSujetoDescuento, true);
            }

            string calcularDescuento = "1";

            if (NoDarDescuento)
            {
                descuentoFactura.DescPorciento = 0;
                descuentoFactura.DescuentoValor = 0;
                calcularDescuento = "0";
            }

            var updateTasa = "";
            if (tasa != 0)
            {
                updateTasa = ", Tasa = "+tasa;
            }

            var cxc = new DS_CuentasxCobrar().GetCuentaByReferencia(Factura.Referencia, Factura.Documento);
            var desmonte = 0.00;
            if (cxc != null && Factura.CalcularDesmonte)
            {
                desmonte = cxc.cxcDesmonte;
            }

            double aplicado = (Factura.Balance - Math.Round(descuentoFactura.DescuentoValor, 2) - desmonte) - (Math.Abs(Factura.Credito) + Math.Abs(Factura.CxcNotaCredito));

            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Desmonte= " + desmonte + ", Estado = 'Saldo', Aplicado = " + aplicado + ", Pendiente = 0, " +
                "Descuento = " + Math.Round(descuentoFactura.DescuentoValor, 2).ToString() + ", DescPorciento = " + descuentoFactura.DescPorciento + ", " +
                "AutSecuencia = " +Factura.AutSecuencia+ ", CalcularDesc = '" + calcularDescuento + "' " + updateTasa +" "+
                "where ltrim(rtrim(Documento)) = ? and ltrim(rtrim(Referencia)) = ?", new string[] { Factura.Documento, Factura.Referencia });

            if (!NoDarDescuento && descuentoFactura.DescPorciento == 0 && descuentoFactura.DescuentoValor == 0 && recalcularDescuento && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
            {
                CalcularDescuentosFacturasInTemp(Factura);
            }

        }

        public void EstRecibos(/*int recSecuencia, string recTipo*/ string rowguid,int est = 0)
        {
            //string where = " and ltrim(rtrim(ifnull(RecTipo,''))) = '" + recTipo + "' ";

            Hash ped = new Hash("Recibos");
            ped.Add("RecEstatus", est);
            ped.Add("UsuInicioSesion", /*Arguments.CurrentUser.RepCodigo*/"mdsoft");
            //ped.ExecuteUpdate("RecSecuencia = " + recSecuencia + " and DepSecuencia = 0 and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where);

            if (est == 0)
            {
                if (new DS_SuscriptoresCambios().UpdateCambioEstadoInsertByRowguid(rowguid, est))
                {
                    ped.SaveScriptForServer = false;
                }
            }

            ped.ExecuteUpdate("rowguid = '" + rowguid + "'");


            if (myParametro.GetParRecibosNCPorDescuentoProntoPago() == 1)
            {
                var desc = new Hash("NCDPP");
                desc.Add("NCDEstatus", est);
                desc.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                desc.Add("NCDFechaActualizacion", Functions.CurrentDate());
                desc.ExecuteUpdate("rowguid = '" + rowguid + "'");
                //desc.ExecuteUpdate("RecSecuencia = " + recSecuencia + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " + where);
            }
        }

        private void ActualizarNcfDpp(string NCFDPP, string rowguid)
        {
            Hash n = new Hash(" " + (myParametro.GetParTakeFromNCF2021() ? "RepresentantesDetalleNCF2021" : "RepresentantesDetalleNCF2018") + " ");
            n.Add("ReDNCFActual", NCFDPP, true);
            n.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            n.ExecuteUpdate("rowguid = '" + rowguid + "'");
        }

        public void AbonarFacturaInTemp(RecibosDocumentosTemp Factura, double Aplicado, bool noCalcularDescuentoFromDialog = false, DescFactura descuentoFactura = null)
        {
            double Pendiente = Factura.Balance - Aplicado;

            // double aplicado = (Factura.Balance - descuentoFactura.DescuentoValor) - (Math.Abs(Factura.Credito) + Math.Abs(Factura.CxcNotaCredito));

            SqliteManager.GetInstance().Execute("UPDATE RecibosDocumentosTemp SET Estado = 'Abono', " +
                "Aplicado = " + Aplicado + ", Descuento = 0.0, DescPorciento = 0.0, Desmonte = 0.0, Pendiente = " + Pendiente + " " +
                "WHERE LTRIM(RTRIM(Documento)) = '" + Factura.Documento.Trim() + "' and LTRIM(RTRIM(Referencia)) = '" + Factura.Referencia.Trim() + "'");

            if (myParametro.GetParDescuentoAbonos() && Arguments.Values.CurrentModule != Modules.RECONCILIACION)
            {
                Factura.Aplicado = Aplicado;

                if (descuentoFactura == null)
                {
                    descuentoFactura = myDesFac.GetMontoDescuentoFactura(Factura, true);

                    descuentoFactura.DescuentoValor = GetMontoDescuentoParaAbono(Factura, descuentoFactura.DescPorciento, Factura.Aplicado, false);
                }

                string calcularDescuento = "1";

                if (noCalcularDescuentoFromDialog)
                {
                    descuentoFactura.DescPorciento = 0;
                    descuentoFactura.DescuentoValor = 0;
                    calcularDescuento = "0";
                }

                Pendiente = (Factura.Balance - descuentoFactura.DescuentoValor) - (Math.Abs(Factura.Credito) + Math.Abs(Factura.CxcNotaCredito)) - Aplicado;

                if(Pendiente < 0)
                {
                    Pendiente = 0;
                }

                SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Pendiente = " + Pendiente + ", " +
                    "Descuento = " + descuentoFactura.DescuentoValor.ToString() + ", DescPorciento = " + descuentoFactura.DescPorciento + ", " +
                    "AutSecuencia = 0, CalcularDesc = '" + calcularDescuento + "' " +
                    "where ltrim(rtrim(Documento)) = ? and ltrim(rtrim(Referencia)) = ?", new string[] { Factura.Documento, Factura.Referencia });
            }
        }

        public void EliminarFacturaInTemp(RecibosDocumentosTemp data)
        {
            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Estado = 'Pendiente', " +
                "Aplicado = 0, Pendiente = Balance, Descuento = 0, Desmonte = 0, Credito = 0, CreditoSinItbis = 0 where ltrim(rtrim(Documento)) = ? " +
                "and ltrim(rtrim(Referencia)) = ?", new string[] { data.Documento, data.Referencia });

            //CalcularDescuentosFacturasInTemp(data, true);
            //if (data.CXCNCF) {
                var list = GetNotasCreditoAplicadasByFactura(data.Referencia);

                foreach (var nc in list)
                {
                   var ncIsFraccionada = NotaCreditoEstaFraccionada(nc.NCReferencia);

                    string estadoNC = ncIsFraccionada ? "Aplicada" : "Pendiente";

                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Estado = ?, " +
                        "Pendiente = (abs(Pendiente) + abs(abs(" + nc.ValorAplicado + ") * Origen)), " +
                        "Aplicado = round(cast(abs(Aplicado) as real) - (cast(abs(" + nc.ValorAplicado + ") as real)), 2), DescPorciento = 0 " +
                        "where ltrim(rtrim(Referencia)) = ?", new string[] { estadoNC, nc.NCReferencia });

                    DeleteNcAplicadaInTemp(nc.NCReferencia, data.Referencia);
                }
           // }

            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Descuento = 0.0, Pendiente = " +
                "(Balance - 0.0 - Aplicado - Credito), Estado = case when Credito = 0.0 and Aplicado = 0.0 then 'Pendiente' else 'Abono' end " +
                "where Referencia = ?", new string[] { data.Referencia });

        }

        public void AgregarFormaPago(FormasPagoTemp model)
        {
            SqliteManager.GetInstance().Insert(model);

            if (model.Futurista == "Si")
            {
                SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set FechaChequeDif = '" + model.Fecha + "'");
            }
        }

        public bool NotaCreditoEstaFraccionada(string ReferenciaNC)
        {
            return SqliteManager.GetInstance().Query<DocumentosAplicadosTemp>("select NCReferencia from DocumentosAplicadosTemp where ltrim(rtrim(upper(NCReferencia))) = ? ", new string[] { ReferenciaNC.Trim().ToUpper() }).Count > 1 && myParametro.GetParRecibosSplitNotasDeCredito(ReferenciaNC, out _);
        }

        public List<FormasPagoTemp> GetFormasPagoInTemp(bool diferido = false)
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select FormaPago, Banco, NoCheque, Futurista, Fecha, Valor, Tasa, Prima, " +
                "rowguid, RefSecuencia, MonCodigo, AutSecuencia, ForID, BanID from FormasPagoTemp where 1=1 " + (diferido ? " and trim(upper(Futurista)) = 'SI'" : ""), new string[] { });
        }

        public bool IsFormasPagoInTemp()
        {
            return GetFormasPagoInTemp().Count > 0;
        }

        private bool ExistsFormaPagoByBancoYNumero(string banco, string numero, string formaPago)
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ltrim(rtrim(FormaPago)) = '" + formaPago + "' and ltrim(rtrim(NoCheque)) = '" + numero.Trim() + "' and ltrim(rtrim(Banco)) = '" + banco.Trim() + "'", new string[] { }).Count > 0;
        }

        public bool ExistsChk(string banco, string numero)
        {
            return ExistsFormaPagoByBancoYNumero(banco, numero, "Cheque");
        }

        public bool ExistsFacturasSaldadasConDescuento()
        {
            var list = SqliteManager.GetInstance().Query<Totales>("select 1 as Total from RecibosDocumentosTemp where Estado = 'Saldo' and Sigla <> 'NC' and ifnull(DescPorciento, 0) > 0 limit 1", new string[] { });

            return list != null && list.Count > 0;
        }

        public bool ExistsFormasDePagoDiferentesAChkDiferidos()
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where Futurista = 'No' and ForID not in (8,9) ", new string[] { }).Count > 0;
        }

        public bool ExistsChkDiferidos(bool sameDate = true, string fecha = null)
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where Futurista = 'Si' " + (fecha != null ? " and substr(Fecha, 1, 10) " + (sameDate ? "=" : "<>") + " '" + fecha.Substring(0, 10) + "'" : ""), new string[] { }).Count > 0;
        }

        public bool ExistsFormaEfectivo(bool sameDate = true, string fecha = null)
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ForID = 1 ", new string[] { }).Count > 0;
        }

        public bool ExistsFormaPago(bool sameDate = true, string fecha = null)
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp", new string[] { }).Count > 0;
        }

        public bool ExistsFormaDePago(string FormaPago, string moncodigo = "")
        {
            string Moneda = "";
            if (!string.IsNullOrWhiteSpace(moncodigo))
            {
                Moneda = "And MonCodigo = '" + moncodigo + "'";
            }
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ltrim(rtrim(FormaPago)) = ? " + Moneda + "", new string[] { FormaPago.Trim(), moncodigo.Trim() }).Count > 0;
        }

        public bool ExistsMaximoFormaDePago(int formaPago, string moncodigo = "", int FopCantidadPermitida = 1)
        {
            string Moneda = "";
            if (!string.IsNullOrWhiteSpace(moncodigo))
            {
                Moneda = "And MonCodigo = '" + moncodigo + "'";
            }
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ForID = ? " + Moneda + "", new string[] { formaPago.ToString(), moncodigo.Trim() }).Count >= FopCantidadPermitida;
        }

        public bool ExistsFormaDePagoMonedaDiferente(string moncodigo)
        {
            string Moneda = "";
            
            Moneda = " MonCodigo <> '" + moncodigo + "' ";
            
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where  " + Moneda + "", new string[] {  moncodigo.Trim() }).Count > 0;
        }

        public int GetLastRefSecuenciaInTemp()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("select max(cast(ifnull(RefSecuencia, 0) as Integer)) as RefSecuencia from FormasPagoTemp ", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].RefSecuencia;
            }

            return 0;
        }

        public bool ExistsFormaPagoTransferencia(string banco, string numero)
        {
            return ExistsFormaPagoByBancoYNumero(banco, numero, "Transferencia");
        }

        public List<FormasPagoTemp> GetTotalesFormasPagoInTempByTipo(int formaId)
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("SELECT SUM(Prima) as Valor, FormaPago, ForID FROM FormasPagoTemp where ForID = " + formaId + " group by FormaPago", new string[] { });
        }

        public RecibosResumen GetResumenFormasPagoInTemp()
        {
            var result = new RecibosResumen();

            var list = SqliteManager.GetInstance().Query<FormasPagoTemp>("SELECT ifnull(SUM(cast(Prima as real)), 0.0) as Valor, FormaPago, ForID FROM FormasPagoTemp group by FormaPago", new string[] { });

            foreach (var rec in list)
            {
                switch (rec.FormaPago)
                {
                    case "Cheque":
                        result.Cheques = rec.Valor;
                        break;
                    case "Efectivo":
                        result.Efectivo = rec.Valor;
                        break;
                    case "Tarjeta de crédito":
                        result.TarjCredito = rec.Valor;
                        break;
                    case "Transferencia":
                        result.Transferencias = rec.Valor;
                        break;
                    case "Orden pago":
                        result.OrdenPago = rec.Valor;
                        break;
                    case "Retención":
                        result.Retencion = rec.Valor;
                        break;
                    case "Diferencia Cambiaria":
                        result.DiferenciaCambiaria = rec.Valor;
                        break;
                    case "Redondeo":
                        result.Redondeo = rec.Valor;
                        break;
                }
            }

            result.Descuentos = GetMontoDescuentoTotalInTemp();
            result.NotasCredito = GetMontoNCTotalAplicado();

            var sobrante = GetTotalAPagar();

            result.Sobrante = sobrante;
            result.Facturas = GetTotalFacturasAbonadasSaldadasInTemp();
            result.SobranteRaw = sobrante;

            return result;
        }

        public double GetTotalAPagar(string fechaChkDiferido = null, bool isRecVerificarDesc = false)
        {
            double totalAplicado = 0;
            double totalFormaPago = 0;

            bool isfechaChkDiferido = !string.IsNullOrWhiteSpace(fechaChkDiferido);
            //string where = isfechaChkDiferido && isRecVerificarDesc ? " and RecVerificarDesc = 0 " : "";
            //var aplicado = SqliteManager.GetInstance().Query<FormasPagoTemp>("select ifnull(SUM(cast(Aplicado as real)), 0.0) as Valor, ifnull(SUM(case when Estado = 'Saldo' " + where + " then cast(Descuento as real) else 0.0 end), 0.0) as Tasa from RecibosDocumentosTemp where Origen = '1'", new string[] { });

            string where = isfechaChkDiferido && isRecVerificarDesc ? " and ifnull(RecVerificarDesc,0) = 0 " : "";

            var aplicado = SqliteManager.GetInstance().Query<FormasPagoTemp>("select ifnull(SUM(cast(Aplicado as real)), 0.0) as Valor, ifnull(SUM(case when Estado = 'Saldo'  then cast(Descuento as real) else 0.0 end), 0.0) as Tasa from RecibosDocumentosTemp where Origen = '1'", new string[] { });

            if (aplicado.Count > 0)
            {
                //quito el descuento normal, y le aplico el nuevo descuento por chk diferido para saber el total real a pagar.
                totalAplicado = Math.Round(aplicado[0].Valor + (isfechaChkDiferido ? aplicado[0].Tasa : 0) - (isfechaChkDiferido ? GetTotalDescuentoParaChkDiferidoSaldos(fechaChkDiferido) : 0), 2);
            }

            var formapago = SqliteManager.GetInstance().Query<FormasPagoTemp>("select ifnull(SUM(cast(ifnull(Prima,0.0) as real)), 0.0) as Prima from FormasPagoTemp", new string[] { });

            if (formapago.Count > 0)
            {
                totalFormaPago = Math.Round(formapago[0].Prima, 2);
            }

            //var recnosobrante = myParametro.GetParRecibosNoSobrante();

            //if (recnosobrante.Length > 0 && recnosobrante[0] == "1")
            //{
            //    totalPagar = totalAplicado - totalFormaPago;
            //}
            //else
            //{
            //    totalPagar = totalFormaPago - totalAplicado;
            //}

            return totalFormaPago - totalAplicado;

        }

        public bool Verificar()
        {
            var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>
               ("select 1 from RecibosDocumentosTemp where Origen = '1' and DefIndicadorNoRestantes = 1 ");

            SqliteManager.GetInstance().Execute("Update RecibosDocumentosTemp set DefIndicadorNoRestantes = 0 ");

            return list != null && list.Count > 0;
        }

        public double GetTotal(string fechaChkDiferido = null)
        {
            double totalPagar = 0;

            var aplicado = SqliteManager.GetInstance().Query<FormasPagoTemp>("select ifnull(SUM(cast(Balance as real)), 0.0) as Valor, ifnull(SUM(case when Estado = 'Saldo' then cast(Descuento as real) else 0.0 end), 0.0) as Tasa from RecibosDocumentosTemp where Origen = '1'", new string[] { });

            if (aplicado.Count > 0)
            {
                totalPagar = Math.Round(aplicado[0].Valor + (!string.IsNullOrWhiteSpace(fechaChkDiferido) ? aplicado[0].Tasa : 0) - (!string.IsNullOrWhiteSpace(fechaChkDiferido) ? GetTotalDescuentoParaChkDiferidoSaldos(fechaChkDiferido) : 0), 2);
            }

            return totalPagar;

        }

        public double GetMontoTotal(string fechaChkDiferido = null)
        {
            double totalPagar = 0;

            var aplicado = SqliteManager.GetInstance().Query<FormasPagoTemp>("select ifnull(SUM(cast(MontoTotal as real)), 0.0) as Valor, ifnull(SUM(case when Estado = 'Saldo' then cast(Descuento as real) else 0.0 end), 0.0) as Tasa from RecibosDocumentosTemp where Origen = '1' and ifnull(Aplicado,0) > 0 ", new string[] { });

            if (aplicado.Count > 0)
            {
                totalPagar = Math.Round(aplicado[0].Valor + (!string.IsNullOrWhiteSpace(fechaChkDiferido) ? aplicado[0].Tasa : 0) - (!string.IsNullOrWhiteSpace(fechaChkDiferido) ? GetTotalDescuentoParaChkDiferidoSaldos(fechaChkDiferido) : 0), 2);
            }

            return totalPagar;

        }

        public double GetRecibosSobrante(int recSecuencia, string recTipo, bool confirmado)
        {
           // double totalPagar = 0;
            double totalAplicado = 0;
            double totalFormaPago = 0;

            var aplicado = SqliteManager.GetInstance().Query<RecibosAplicacion>("select ifnull(SUM(cast(RecValor as real)), 0.0) as RecValor, " +
                "ifnull(SUM(RecDescuento), 0.0) as RecDescuento from " + (confirmado?"RecibosAplicacionConfirmados":"RecibosAplicacion") + " where " +
                "RecSecuencia = ? and trim(upper(RecTipo)) = ? and RepCodigo = ?", 
                new string[] { recSecuencia.ToString(), recTipo.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (aplicado.Count > 0)
            {
                totalAplicado = Math.Round(aplicado[0].RecValor,2);
            }

            var formapago = SqliteManager.GetInstance().Query<RecibosFormaPago>("select ifnull(SUM(cast(RecPrima as real)), 0.0) as RecPrima " +
                "from "+ (confirmado?"RecibosFormaPagoConfirmados":"RecibosFormaPago") + " " +
                "where RecSecuencia = ? and trim(upper(RecTipo)) = ? and RepCodigo = ? and ForID != 3 ", 
                new string[] { recSecuencia.ToString(), recTipo.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (formapago.Count > 0)
            {
                totalFormaPago = Math.Round(formapago[0].RecPrima, 2);
            }
            //var recnosobrante = myParametro.GetParRecibosNoSobrante();
            //if (recnosobrante.Length > 0 && recnosobrante[0] == "1")
            //{
            //    totalPagar = totalAplicado - totalFormaPago;
            //}
            //else
            //{
            //    totalPagar = totalFormaPago - totalAplicado;
            //}

            return totalFormaPago - totalAplicado;

        }


        public List<RecibosDocumentosTemp> GetDocumentsInTempByEstado(string Estado) { return GetDocumentsInTemp(false, Estado, false); }
        public List<RecibosDocumentosTemp> GetDocumentsInTemp() { return GetDocumentsInTemp(false, null, false); }
        public List<RecibosDocumentosTemp> GetDocumentsInTemp(bool onlyAvailableForNC, bool ncHasSplit) { return GetDocumentsInTemp(onlyAvailableForNC, null, false, ncHasSplit); }
        public List<RecibosDocumentosTemp> GetDocumentsInTemp(bool onlyAvailableForNC, string Estado, bool withCalcDesc = false, bool NCHasSplit = false)
        {
            var x = DS_RepresentantesParametros.GetInstance().GetParAplicarNotaCreditoMontoTotal();
            string sql = "select CalcularDesmonte, Desmonte, AplicaDescuento,Fecha, Documento, Referencia, ifnull(Sigla, '') as Sigla, cxcComentario, CAST(replace(strftime('%d-%m-%Y', SUBSTR(FechaEntrega,1,10)),' ','') as varchar) as FechaEntrega, CxcColor,  " +
                    "Aplicado, Descuento, MontoTotal, Balance, Pendiente, CXCNCF, Estado, replace(Credito, ',', '') as Credito, replace(CreditoSinItbis, ',', '') as CreditoSinItbis, FechaIngles, " +
                    "cast(Origen as integer) as Origen, MontoSinItbis, DescPorciento, DefIndicadorItbis, AutSecuencia, FechaDescuento, FechaChequeDif, Dias, " +
                    "julianday(Cast(strftime('%Y-%m-%d',DATETIME('NOW', 'localtime'),'" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(FechaVencimiento, FechaSinFormatear),1,10)) as DiasVencido, " +
                    "DescuentoFactura, Clasificacion, FechaVencimiento, CalcularDesc, Retencion, " +
                    "case when Referencia in (select FacturaReferencia from DocumentosAplicadosTemp) then 1 else 0 end as IndicadorNotaCreditoAplicada, " +
                    "CxcNotaCredito, ifnull((select CXCNCFAfectado from CuentasxCobrar where CxcReferencia = RecibosDocumentosTemp.Referencia ), '') as CXCNCFAfectado, "+ ( x ? "Documento ||' - '|| Aplicado " : "Documento ||' - '|| Balance ||' - '|| round(Aplicado,2) ") + " as DocumentoMonto, " +
                    " ifnull((select cxcTasa from CuentasxCobrar where CxcReferencia = RecibosDocumentosTemp.Referencia),0) as Tasa, RecVerificarDesc, RepCodigo " +
                    "from RecibosDocumentosTemp " +
                    "where 1=1 ";

            if (onlyAvailableForNC || Estado != null)
            {
                sql += " and Origen = 1";

                if (Estado != null)
                {
                    sql += " and upper(Estado) = '" + Estado.ToUpper() + "'";
                }
                else if (myParametro.GetParRecibosAplicarNCaTodasFacturas() || Arguments.Values.CurrentModule == Modules.RECONCILIACION)
                {
                    sql += " and upper(Estado) in ('SALDO', 'ABONO', 'PENDIENTE')";
                }
                else
                {
                    sql += " and upper(Estado) in ('SALDO'" + (myParametro.GetParRecibosNCaFacturasConAbono() ? ", 'ABONO'" : "") + ")";
                }

                if (NCHasSplit)
                {
                    //sql += " and (abs(Credito) < abs(Aplicado)) ";
                    sql += " and (abs(Credito) < abs(Balance)) ";
                }
            }

            if (withCalcDesc)
            {
                sql += " and ifnull(CalcularDesc, 1) = 1 ";
            }

            //string orderBy = " order by Fecha asc ";
            //string orderBy = " order by CAST(Dias as int) DESC, Documento ASC ";

            var orderBy = " order by cast(julianday(Fecha) as integer) asc";

            if (myParametro.GetParRecibosImportadoraLaPlaza())
            {
                orderBy = " order by julianday(Cast(strftime('%Y-%m-%d','now','" + Functions.GetDiferenciaHorariaSqlite() + " hours') as Varchar)) - julianday(SUBSTR(ifnull(FechaVencimiento, Fecha),1,10)) ASC ";
            }

            if (myParametro.GetParRecibosOrdenarDocumentoPorFechaEntrega())
            {
                orderBy = " order by FechaEntrega ASC, Documento ASC ";
            }

            sql += " " + orderBy;

            return SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(sql, new string[] { });
        }

        public List<Compras> GetPushMoneyDepositados(int depsencuencia)
        {
           
                    string sql = "SELECT CD.ComSecuencia,   SUM( (((CD.ComCantidad * CD.ComPrecio) - ((CD.ComCantidad * CD.ComPrecio) * CD.ComDescuento)) * (1 + (CD.ComItbis/100) )) )  as Monto, cl.CliNombre as CliNombre FROM Compras C "
                            + "Inner join ComprasDetalle CD on C.ComSecuencia = CD.ComSecuencia "
                            + "left join clientes cl on cl.cliID = c.Cliid "
                            + "Where C.DepSecuencia = " + depsencuencia + " "
                            + "Group by CD.ComSecuencia";

            return SqliteManager.GetInstance().Query<Compras>(sql, new string[] { });

        }
        

        public void AplicarNotaCredito(RecibosDocumentosTemp NC, RecibosDocumentosTemp Factura, double valorAplicarManual = -1)
        {
            var valorAplicado = Math.Abs(NC.Pendiente);
            var valorSinItbis = Math.Abs(NC.MontoSinItbis);

            if (myParametro.GetParRecibosSplitNotasDeCredito(NC.Referencia, out _) && valorAplicarManual != -1)
            {
                valorAplicado = Math.Abs(valorAplicarManual);
                valorSinItbis = Math.Abs(valorAplicarManual);
            }

            double Credito = (Math.Abs(Factura.Credito) + Math.Abs(Factura.CxcNotaCredito)) + Math.Abs(valorAplicado);
            double Pendiente = Factura.Pendiente - Math.Abs(valorAplicado);
            double aplicado;

            if(Math.Abs(Credito) > Math.Abs(Factura.Balance))
            {
                valorAplicado = Math.Abs(Factura.Balance) - (Math.Abs(Factura.Credito) + Math.Abs(Factura.CxcNotaCredito));

                if(valorAplicado <= 0)
                {
                    throw new Exception("Este documento a sido saldado completamente, no es posible aplicar la Nota de Credito");
                }
                //valorSinItbis = Factura.Pendiente;  //debo de sacarlo del pendiente - %itbis porque sino el descuento me lo dara mal ya que lo dara del monto total de la NC sin itbis, cuando lo que aplique puede ser menor ya que el monto de la factura pendiente es menor al pendiente de la NC
                Credito = (Math.Abs(Factura.Credito) + Math.Abs(Factura.CxcNotaCredito)) + Math.Abs(valorAplicado);
                Pendiente = Factura.Pendiente - Math.Abs(valorAplicado);
            }

            if (myParametro.GetParRecibosSplitNotasDeCredito(NC.Referencia, out _))
            {
                if (NC.MontoTotal != NC.MontoSinItbis)
                {
                    var porcientoItbis = Math.Abs(1.0 - ((NC.MontoSinItbis / NC.MontoTotal)));

                    var montoItbisAplicado = NC.Pendiente * porcientoItbis;

                    valorSinItbis = NC.Pendiente - montoItbisAplicado;
                }
                
            }
            else
            {
                if (NC.MontoTotal != valorSinItbis)
                {
                    var porcientoItbis = Math.Abs(1.0 - ((valorSinItbis / NC.MontoTotal)));

                    var montoItbisAplicado = NC.Pendiente * porcientoItbis;

                    valorSinItbis = NC.Pendiente - montoItbisAplicado;
                }
            }
            
            DescFactura descuentoFactura = myDesFac.GetMontoDescuentoFactura(Factura, !Factura.DefIndicadorItbis ? valorSinItbis : valorAplicado);

            if (Pendiente < 0)
            {
                Pendiente = 0;
            }

            var valorFinalparaaplicar = Factura.Balance - Factura.Desmonte - descuentoFactura.DescuentoValor - Credito;

            if (Factura.Estado == "Saldo")
            {
                aplicado = valorFinalparaaplicar;
                if (aplicado < 0) { aplicado = 0; }
            }else
            {
                aplicado = valorFinalparaaplicar < Factura.Aplicado ? valorFinalparaaplicar : Factura.Aplicado;
            }

            SqliteManager.GetInstance().Execute("Update RecibosDocumentosTemp set Estado = 'Aplicada', " +
                "Pendiente = abs(round(CAST((( CAST(ifnull(abs(Pendiente), 0.0) AS REAL) - CAST((" + valorAplicado + ") AS REAL) )  * Origen) AS REAL), 2))," +
                "Aplicado = round((CAST(Aplicado AS REAL) + CAST(" + valorAplicado + " AS REAL) ), 2) where ltrim(rtrim(Documento)) = ? and Referencia = ? ", 
                new string[] { NC.Documento.Trim(), NC.Referencia.Trim() });

            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Pendiente = round(CAST(" + Pendiente + " AS REAL), 2)," +
                ""+(valorFinalparaaplicar < Factura.Aplicado ? "RecMontoNcDevolver = "+ Factura.Aplicado + ", " : " ") +""+
                "Aplicado = round(CAST(" + aplicado + " AS REAL), 2), DescPorciento = round(CAST(" + descuentoFactura.DescPorciento + " AS REAL), 2),  Descuento = round(CAST(" + descuentoFactura.DescuentoValor + " AS REAL), 2)," +
                "Credito = round(CAST( " + (Math.Abs(Factura.Credito) + Math.Abs(valorAplicado)) + " AS REAL), 2), " +
                "CreditoSinItbis = round(CAST(CAST(ifnull(abs(CreditoSinItbis), 0.0) AS REAL) + CAST(" + valorSinItbis + " AS REAL) AS REAL), 2) " +
                "where ltrim(rtrim(Documento)) = ? and Referencia = ?", new string[] { Factura.Documento.Trim(), Factura.Referencia.Trim() });
            //"where ltrim(rtrim(Documento)) like ?", new string[] { Factura.Documento.Trim() });

            DocumentosAplicadosTemp nc = new DocumentosAplicadosTemp();
            nc.FacturaReferencia = Factura.Referencia;
            nc.NCReferencia = NC.Referencia;
            nc.SiglaFactura = Factura.Sigla;
            nc.ValorAplicado = Math.Abs(valorAplicado);
            nc.FacturaFecha = Factura.FechaSinFormatear;
            nc.FacturaDocumento = Factura.Documento;
            nc.MontoSinItbisNC = valorSinItbis;

            SqliteManager.GetInstance().InsertOrReplace(nc);
        }

        public RecibosDocumentosTemp GetFacturaConAplicadoMayor(double Aplicado, string NCF = "")
        {
            List<RecibosDocumentosTemp> list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select Fecha, Documento, Referencia, ifnull(Sigla, '') as Sigla, " +
                    "Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, replace(Credito, ',', '') as Credito, FechaIngles, " +
                    "cast(Origen as integer) as Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, FechaChequeDif, Dias, " +
                    "DescuentoFactura, Clasificacion, FechaVencimiento, CalcularDesc, Retencion, " +
                    "case when Referencia in (select FacturaReferencia from DocumentosAplicadosTemp) then 1 else 0 end as IndicadorNotaCreditoAplicada, ifnull((select CXCNCFAfectado from CuentasxCobrar where CxcReferencia = RecibosDocumentosTemp.Referencia ), '') as CXCNCFAfectado, CXCNCF, Desmonte " +
                    "from RecibosDocumentosTemp where Origen = 1 and Estado = 'Saldo' and cast(Aplicado as real) >= ? " +
                    " order by Aplicado desc", new string[] { Aplicado.ToString() });

            if (list != null && list.Count > 0)
            {
                if (myParametro.GetParNotaCreditoAutoFactura())
                {
                    var fact = list.FirstOrDefault(f => f.CXCNCF == NCF);
                    if (fact != null)
                    {
                        return fact;
                    }
                }
                else
                {
                    return list[0];
                }
             
            }

            return null;
        }


        public RecibosDocumentosTemp GetFacturaAsociadaNotaCredito(string cxcReferencia)
        {
            List<RecibosDocumentosTemp> list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select Fecha, Documento, Referencia, ifnull(Sigla, '') as Sigla, " +
                    "Aplicado, Descuento, MontoTotal, Balance, Pendiente, Estado, replace(Credito, ',', '') as Credito, FechaIngles, " +
                    "cast(Origen as integer) as Origen, MontoSinItbis, DescPorciento, AutSecuencia, FechaDescuento, FechaChequeDif, Dias, " +
                    "DescuentoFactura, Clasificacion, FechaVencimiento, CalcularDesc, Retencion, " +
                    "case when Referencia in (select FacturaReferencia from DocumentosAplicadosTemp) then 1 else 0 end as IndicadorNotaCreditoAplicada, ifnull((select CXCNCFAfectado from CuentasxCobrar where CxcReferencia = RecibosDocumentosTemp.Referencia ), '') as CXCNCFAfectado, CXCNCF, Desmonte " +
                    "from RecibosDocumentosTemp where Origen = 1  and Referencia >= ? "
                    , new string[] { cxcReferencia });

            if (list != null && list.Count > 0)
            {
                 
                    return list[0];
                 
            }

            return null;
        }

        public List<DocumentosAplicadosTemp> GetNotasCreditoAplicadasByFactura(string FacturaReferencia)
        {
            return SqliteManager.GetInstance().Query<DocumentosAplicadosTemp>("select NCReferencia, Abs(ValorAplicado) as ValorAplicado, " +
                "Abs(MontoSinItbisNC) as MontoSinItbisNC, FacturaReferencia, SiglaFactura, AutID from DocumentosAplicadosTemp where FacturaReferencia = ?", new string[] { FacturaReferencia });
        }

        public List<DocumentosAplicadosTemp> GetNotasCreditoAplicadasByFacturaWitoutNC(string FacturaReferencia)
        {
            return SqliteManager.GetInstance().Query<DocumentosAplicadosTemp>("select NCReferencia, Abs(ValorAplicado) as ValorAplicado, " +
                "Abs(MontoSinItbisNC) as MontoSinItbisNC, FacturaReferencia, SiglaFactura, AutID from DocumentosAplicadosTemp where FacturaReferencia = ? and NCReferencia not like '%NC%' ", new string[] { FacturaReferencia });
        }

        public void EliminarNCInTemp(RecibosDocumentosTemp NC)
        {
            List<DocumentosAplicadosTemp> docs = SqliteManager.GetInstance().Query<DocumentosAplicadosTemp>("select NCReferencia, ValorAplicado, FacturaReferencia, SiglaFactura, AutID, MontoSinItbisNC from DocumentosAplicadosTemp where NCReferencia = ?", new string[] { NC.Referencia });

            double totalMontoAplicado = 0;
            foreach (var notaCredito in docs)
            {
                totalMontoAplicado += Math.Abs(notaCredito.ValorAplicado);

                SqliteManager.GetInstance().Execute("Update RecibosDocumentosTemp set Pendiente = case when Estado = 'Abono' then (abs(Pendiente) + abs(" + Math.Abs(notaCredito.ValorAplicado) + ")) else Pendiente end, " +
                    "Credito = case when (Abs(Credito) - " + Math.Abs(notaCredito.ValorAplicado) + ") >= 0 then (Abs(Credito) - " + Math.Abs(notaCredito.ValorAplicado) + ") else 0 end, " +
                    "CreditoSinItbis = case when (Abs(CreditoSinItbis) - " + Math.Abs(notaCredito.MontoSinItbisNC) + ") >= 0 then (Abs(CreditoSinItbis) - " + Math.Abs(notaCredito.MontoSinItbisNC) + ") else 0 end, " +
                    "" +(!myParametro.GetParAplicarNotaCreditoMontoTotal() ? " Aplicado = case when Estado = 'Saldo' then (Aplicado + " + Math.Abs(notaCredito.ValorAplicado) + ") else Aplicado end " : " Aplicado = case when Estado = 'Abono' And (abs(Aplicado - RecMontoNcDevolver) + abs(Pendiente) + " + Math.Abs(notaCredito.ValorAplicado) + ") != Balance then RecMontoNcDevolver else (case when Estado = 'Saldo' then (Aplicado + " + Math.Abs(notaCredito.ValorAplicado) + ") else Aplicado end) end ") + " " +
                    "where Referencia = ?", new string[] { notaCredito.FacturaReferencia });

                //select Documento, Referencia, MontoSinItbis, Credito, DescPorciento
                List<RecibosDocumentosTemp> list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select Documento, Referencia, MontoSinItbis, Credito, DescPorciento, Descuento, Aplicado, Desmonte, CreditoSinItbis, ifnull(DefIndicadorItbis,0) from RecibosDocumentosTemp where trim(Referencia) = ?", new string[] { notaCredito.FacturaReferencia });

                foreach (var factura in list)
                {
                    factura.Aplicado += factura.Aplicado > 0 ? factura.Descuento : factura.MontoSinItbis;
                    //double descuentoValor = ((factura.Aplicado - Math.Abs(factura.Credito)) * factura.DescPorciento) / 100;
                    double descuentoValor = 0.00;
                    if (!factura.DefIndicadorItbis)
                    {
                        descuentoValor = (factura.MontoSinItbis - Math.Abs(factura.CreditoSinItbis)) * (factura.DescPorciento / 100);
                    }
                    else
                    {
                        descuentoValor = (factura.Aplicado + factura.Desmonte) * (factura.DescPorciento / 100);
                    } 

                    factura.Aplicado -= descuentoValor;

                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Descuento = '" + descuentoValor + "', Aplicado = " + factura.Aplicado + " where Referencia = ? and Documento = ?", new string[] { factura.Referencia, factura.Documento });
                }
            }

            DeleteNcAplicadaInTemp(NC.Referencia);

            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Estado = 'Pendiente', Pendiente = (abs(Pendiente) + abs(abs(" + totalMontoAplicado + ") * Origen)), " +
                "Aplicado = 0, DescPorciento = 0 where ltrim(rtrim(Referencia)) = ? and ltrim(rtrim(Documento)) = ?", new string[] { NC.Referencia, NC.Documento });
        }

        private void DeleteNcAplicadaInTemp(string NCReferencia) { DeleteNcAplicadaInTemp(NCReferencia, null); }
        private void DeleteNcAplicadaInTemp(string NCReferencia, string facturaReferencia)
        {
            var sql = "delete from DocumentosAplicadosTemp where NCReferencia = ?";

            if (facturaReferencia != null)
            {
                sql += " and FacturaReferencia = '" + facturaReferencia + "'";
            }
            SqliteManager.GetInstance().Execute(sql, new string[] { NCReferencia });
        }

        public void CalcularDescuentosFacturasInTemp() { CalcularDescuentosFacturasInTemp(null); }
        public void CalcularDescuentosFacturasInTemp(RecibosDocumentosTemp Factura)
        {
            string Dias = "RecibosDocumentosTemp.Dias";
            string FechaChkFuturista = GetFechaChequeFuturista();

            var parDescuentoaAbonos = myParametro.GetParDescuentoAbonos();

            var whereFactura = "";

            if(Factura != null)
            {
                whereFactura = " and ltrim(rtrim(Documento)) = '" + Factura.Documento + "' and ltrim(rtrim(Referencia)) = '" + Factura.Referencia + "' ";
            }
                        
            if (Factura != null)
            {
                int DiasParaDescuento = Factura.Dias;
                if (FechaChkFuturista != null)
                {
                    DateTime dt = DateTime.ParseExact(Factura.Fecha, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    string fechafactura = dt.ToString("yyyy-MM-dd");
                    DateTime date = DateTime.Parse(FechaChkFuturista);
                    DateTime facFecha = DateTime.Parse(fechafactura);
                    DiasParaDescuento = Math.Abs((int)(facFecha - date).TotalDays);
                }

                DescFactura descuentoFactura = myDesFac.GetMontoDescuentoFactura(Factura, DiasParaDescuento);
                //MontoCreditoAplicadoFactura CreditoAplicado = GetMontoTotalCreditoAplicadoFactura(Factura);

                var descPorcientoFactura = Factura.DescPorciento;
                if ((descuentoFactura.DescPorciento > descPorcientoFactura ) || myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas() == 3 && descuentoFactura.DescPorciento > 0 && (descPorcientoFactura == myParametro.GetDescuentoManualAdicional() || descPorcientoFactura == myParametro.Get2doDescuentoManualAdicional())
                    || (descPorcientoFactura > descuentoFactura.DescPorciento && (Factura.AutSecuencia > 0 || myParametro.GetParDescuentoManualNoValidaDias())))
                {
                    descuentoFactura.DescPorciento = descPorcientoFactura;
                    descuentoFactura.DescuentoValor = Factura.Descuento;

                    var aplicado = 0.0;

                    if (descuentoFactura.IndicadorItbis)
                    {

                        aplicado = (Factura.MontoTotal - Math.Abs(Factura.Credito) - Math.Abs(Factura.CxcNotaCredito));
                    }
                    else
                    {
                        aplicado = (Factura.MontoSinItbis - Math.Abs(Factura.CreditoSinItbis) - Math.Abs(Factura.CxcNotaCredito));
                    }

                    if (aplicado > 0 && descuentoFactura.DescPorciento > 0)
                    {
                        descuentoFactura.DescuentoValor = Math.Round(aplicado * (descuentoFactura.DescPorciento / 100), 2);
                    }
                    else
                    {
                        descuentoFactura.DescuentoValor = 0;
                        descuentoFactura.DescPorciento = 0;
                    }
                }

                if (parDescuentoaAbonos)
                {
                    double montoParaDescuento;

                    if (Factura.Estado == "Abono")
                    {
                        montoParaDescuento = Factura.Aplicado;
                    }
                    else
                    {
                        if (descuentoFactura.IndicadorItbis)
                        {
                            montoParaDescuento = (Factura.MontoTotal - Math.Abs(Factura.Credito) - Math.Abs(Factura.CxcNotaCredito));
                        }
                        else
                        {
                            montoParaDescuento = (Factura.MontoSinItbis - Math.Abs(Factura.CreditoSinItbis) - Math.Abs(Factura.CxcNotaCredito));
                        }
                    }

                    descuentoFactura.DescuentoValor = GetMontoDescuentoParaAbono(Factura, descuentoFactura.DescPorciento, montoParaDescuento, Factura.Estado != "Abono");
                }
                //else
                //{
                //    var montoParaDescuento = 0.0;

                //    if (descuentoFactura.IndicadorItbis)
                //    {

                //        montoParaDescuento = (Factura.MontoTotal - Math.Abs(CreditoAplicado.Credito));
                //    }
                //    else
                //    {
                //        montoParaDescuento = (Factura.MontoSinItbis - Math.Abs(CreditoAplicado.CreditoSinItbis));
                //    }

                //    if (montoParaDescuento > 0 && descuentoFactura.DescPorciento > 0)
                //    {
                //        descuentoFactura.DescuentoValor = descuentoFactura.DefDescuento > 0 ? descuentoFactura.DefDescuento : Math.Round(montoParaDescuento * (descuentoFactura.DescPorciento / 100), 2);
                //    }
                //    else
                //    {
                //        descuentoFactura.DescuentoValor = 0;
                //        descuentoFactura.DescPorciento = 0;
                //    }

                //}

                var where = "";

                if (!parDescuentoaAbonos)
                {
                    where = " and Estado <> 'Abono' ";
                }

                SqliteManager.GetInstance().Execute("Update RecibosDocumentosTemp set Descuento = " + descuentoFactura.DescuentoValor.ToString() + ", DescPorciento = " + descuentoFactura.DescPorciento.ToString() + " " +
                    "where ltrim(rtrim(Documento)) = ? and ltrim(rtrim(Referencia)) = ? " + where, new string[] { Factura.Documento, Factura.Referencia });
            }
            else
            {
                if (FechaChkFuturista != null)
                {
                    DateTime date = DateTime.Parse(FechaChkFuturista);
                    Dias = "(RecibosDocumentosTemp.Dias + " + (DateTime.Now - date).TotalDays + ")";
                }
                 
                var list = GetDocumentsInTemp(false, Estado: "Saldo", withCalcDesc:true);

                if (parDescuentoaAbonos)
                {
                    list.AddRange(GetDocumentsInTemp(false, Estado: "Abono", withCalcDesc: true));
                }

                foreach(var factura in list)
                {

                    if (!myParametro.GetParRecibosItbisMenos30Dias())
                    {
                        // actualiza el DefIndicadorItbis
                        var defIndicadorItbis = DS_DescuentoFacturas.GetInstance().GetIndicadorItbisDescuentoDisponible(factura.Referencia, int.Parse(factura.Dias.ToString()));
                        SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set DefIndicadorItbis = " + defIndicadorItbis + " where Referencia = '" + factura.Referencia + "' and ifnull(Origen, 0) > 0");
                        factura.DefIndicadorItbis = defIndicadorItbis == 1 ? true : false;
                    }

                    var descuentoFactura = myDesFac.GetMontoDescuentoFactura(factura, factura.Estado == "Abono");
                    //MontoCreditoAplicadoFactura CreditoAplicado = GetMontoTotalCreditoAplicadoFactura(factura);
                    var descPorcientoFactura = factura.DescPorciento;
                    if ((descuentoFactura.DescPorciento > descPorcientoFactura) || myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas() == 3 && descuentoFactura.DescPorciento > 0 && (descPorcientoFactura == myParametro.GetDescuentoManualAdicional() || descPorcientoFactura == myParametro.Get2doDescuentoManualAdicional())
                        || (descPorcientoFactura > descuentoFactura.DescPorciento && (factura.AutSecuencia > 0 || myParametro.GetParDescuentoManualNoValidaDias())))
                    {
                        descuentoFactura.DescPorciento = descPorcientoFactura;
                        descuentoFactura.DescuentoValor = factura.Descuento;

                        var aplicado = 0.0;

                        if (descuentoFactura.IndicadorItbis)
                        {

                            aplicado = (factura.MontoTotal - Math.Abs(factura.Credito) - Math.Abs(factura.CxcNotaCredito));
                        }
                        else
                        {
                            aplicado = (factura.MontoSinItbis - Math.Abs(factura.CreditoSinItbis) - Math.Abs(factura.CxcNotaCredito));
                        }

                        if (aplicado > 0 && descuentoFactura.DescPorciento > 0)
                        {
                            descuentoFactura.DescuentoValor = Math.Round(aplicado * (descuentoFactura.DescPorciento / 100), 2);
                        }
                        else
                        {
                            descuentoFactura.DescuentoValor = 0;
                            descuentoFactura.DescPorciento = 0;
                        }
                    }


                    if (parDescuentoaAbonos)
                    {
                        var montoParaDescuento = 0.0;

                        if (factura.Estado == "Abono")
                        {
                            montoParaDescuento = factura.Aplicado;
                        }
                        else
                        {
                            if (descuentoFactura.IndicadorItbis)
                            {
                                montoParaDescuento = (factura.MontoTotal - Math.Abs(factura.Credito) - Math.Abs(factura.CxcNotaCredito));
                            }
                            else
                            {
                                montoParaDescuento = (factura.MontoSinItbis - Math.Abs(factura.CreditoSinItbis) - Math.Abs(factura.CxcNotaCredito));
                            }
                        }

                        descuentoFactura.DescuentoValor = GetMontoDescuentoParaAbono(factura, descuentoFactura.DescPorciento, montoParaDescuento, factura.Estado != "Abono");
                    }
                    //else
                    //{
                    //    var montoParaDescuento = 0.0;

                    //    if (descuentoFactura.IndicadorItbis)
                    //    {

                    //        montoParaDescuento = (factura.MontoTotal - Math.Abs(CreditoAplicado.Credito));
                    //    }
                    //    else
                    //    {
                    //        montoParaDescuento = (factura.MontoSinItbis - Math.Abs(CreditoAplicado.CreditoSinItbis));
                    //    }

                    //    if (montoParaDescuento > 0 && descuentoFactura.DescPorciento > 0)
                    //    {
                    //        descuentoFactura.DescuentoValor = descuentoFactura.DefDescuento > 0 ? descuentoFactura.DefDescuento : Math.Round(montoParaDescuento * (descuentoFactura.DescPorciento / 100), 2);
                    //    }
                    //    else
                    //    {
                    //        descuentoFactura.DescuentoValor = 0;
                    //        descuentoFactura.DescPorciento = 0;
                    //    }

                    //}

                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp " +
                        "set Descuento = " + descuentoFactura.DescuentoValor + ", " +
                        "DescPorciento = " + descuentoFactura.DescPorciento + " where ltrim(rtrim(Documento)) = ? and ltrim(rtrim(Referencia)) = ?", 
                        new string[] { factura.Documento, factura.Referencia });

                    if(factura.Estado == "Abono")
                    {
                        var pendiente = (factura.Balance - descuentoFactura.DescuentoValor) - (Math.Abs(factura.Credito) + Math.Abs(factura.CxcNotaCredito)) - factura.Aplicado;

                        if (pendiente < 0)
                        {
                            pendiente = 0;
                        }

                        SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Pendiente = " + pendiente + " where Estado = 'Abono' and Origen = 1 and CalcularDesc = 1 and ltrim(rtrim(Documento)) = '" + factura.Documento + "' and ltrim(rtrim(Referencia)) = '" +factura.Referencia + "' ", new string[] { });
                        //SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Aplicado = Aplicado + Descuento where Estado = 'Abono' and Origen = 1 and CalcularDesc = '1' " + " and ltrim(rtrim(Documento)) = '" + factura.Documento + "' and ltrim(rtrim(Referencia)) = '" +factura.Referencia + "' ", new string[] { });
                        // SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Pendiente = Balance - Aplicado where Estado = 'Abono' and Origen = 1 and CalcularDesc = '1' " + " and ltrim(rtrim(Documento)) = '" + factura.Documento + "' and ltrim(rtrim(Referencia)) = '" + factura.Referencia + "' ", new string[] { });
                    }
                }

                /*SqliteManager.GetInstance().Execute("UPDATE RecibosDocumentosTemp SET Descuento = case when CalcularDesc = '1' then IFNULL((" +
                                "SELECT Case when df.DeFPorciento > 0 then CASE WHEN df.DefIndicadorItbis = 0 THEN " +
                                "(RecibosDocumentosTemp.MontoSinItbis - (abs(RecibosDocumentosTemp.Credito) + abs(CxcNotaCredito))) * (df.DeFPorciento / 100.0) " +
                                "ELSE (RecibosDocumentosTemp.MontoTotal - (abs(RecibosDocumentosTemp.Credito) + abs(CxcNotaCredito))) * (df.DeFPorciento / 100.0) END else df.DefDescuento end " +
                                "FROM DescuentoFacturas df " +
                                "WHERE LTRIM(RTRIM(df.CxcReferencia)) = LTRIM(RTRIM(RecibosDocumentosTemp.Referencia)) " +
                                "AND " + Dias + " BETWEEN df.DeFDiaInicial AND df.DeFDiaFinal), 0.0) else 0 end where 1=1 " + where, new string[] { });

                SqliteManager.GetInstance().Execute("UPDATE RecibosDocumentosTemp SET DescPorciento = case when CalcularDesc = '1' then IFNULL((" +
                           "SELECT df.DeFPorciento FROM DescuentoFacturas df " +
                           "WHERE LTRIM(RTRIM(df.CxcReferencia)) = LTRIM(RTRIM(RecibosDocumentosTemp.Referencia)) " +
                           "AND " + Dias + " BETWEEN df.DeFDiaInicial AND df.DeFDiaFinal), 0.0) else 0 end where 1=1 " + where, new string[] { });*/

            }

            var sql =
                    "UPDATE RecibosDocumentosTemp SET Aplicado = case when Estado = 'Saldo' then Balance - ifnull(Descuento, 0.0) - ifnull(Desmonte, 0.0) - abs(ifnull(Credito, 0.0)) - abs(ifnull(CxcNotaCredito, 0.0)) else Aplicado end,  Pendiente = case when Estado = 'Saldo' then 0.0 else Balance - abs(Credito) - abs(Descuento) - abs(CxcNotaCredito) end where Origen = 1 ";

            if (Factura != null)
            {
                sql += whereFactura;
            }

            SqliteManager.GetInstance().Execute(sql);

            if (Factura != null)
            {
                SqliteManager.GetInstance().Execute("UPDATE RecibosDocumentosTemp SET Pendiente = 0 WHERE LTRIM(RTRIM(Documento)) = '" + Factura.Documento + "' AND LTRIM(RTRIM(Referencia)) = '" + Factura.Referencia + "' AND Estado <> 'Abono'");
            }

        }
        /** se hace asi porque solo se puede agregar un cheque futurista a la vez*/
        private string GetFechaChequeFuturista()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("select Fecha from FormasPagoTemp where Futurista = ?", new string[] { "Si" });

            if (list != null && list.Count > 0)
            {
                return list[0].Fecha;
            }

            return null;
        }

        public void ActualizarIndicadorDescuentoItbisInTemp()
        {
            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set DefIndicadorItbis = 1 where trim(Referencia) " +
                "in (select trim(cxcReferencia) from DescuentoFacturas where ifnull(DefIndicadorItbis, 0) = 1)", new string[] { });
        }

        private double GetTotalDescuentoParaChkDiferidoSaldos(string fecha)
        {
            var parDescuentoEnAbonos = myParametro.GetParDescuentoAbonos();

            var list = GetDocumentsInTemp(false, "Saldo", true);
                        
            double total = 0.0;

            DateTime.TryParse(fecha, out DateTime fechaDiferido);

            foreach (var Factura in list)
            {
                var fechaFact = DateTime.ParseExact(Factura.Fecha, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var dias = (fechaDiferido - fechaFact).TotalDays;
                dias = Math.Truncate(dias);

                var porcDescuentoFactura = Factura.DescPorciento;

                if (!myParametro.GetParRecibosItbisMenos30Dias())
                {
                    //Si es diferido se actualiza el DefIndicadorItbis
                    var defIndicadorItbis = DS_DescuentoFacturas.GetInstance().GetIndicadorItbisDescuentoDisponible(Factura.Referencia, int.Parse(dias.ToString()));
                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set DefIndicadorItbis = " + defIndicadorItbis + " where Referencia = '" + Factura.Referencia + "' and ifnull(Origen, 0) > 0");
                    Factura.DefIndicadorItbis = defIndicadorItbis == 1 ? true : false;
                }

                //En el uso de descuento manual se quita el que se iguale a Cero ya que no se recalcula automaticamente
                if (myParametro.GetParDescuentoManual() <= 0)
                {
                    Factura.DescPorciento = 0;
                    //Factura.Descuento = 0;
                }
 
                DescFactura DescuentoFactura = myDesFac.GetMontoDescuentoFactura(Factura, (int)dias, -1, -1, false);
                //MontoCreditoAplicadoFactura CreditoAplicado = GetMontoTotalCreditoAplicadoFactura(Factura);

                if((DescuentoFactura.DescPorciento > porcDescuentoFactura) || myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas() == 3 && DescuentoFactura.DescPorciento > 0 && (porcDescuentoFactura == myParametro.GetDescuentoManualAdicional() || porcDescuentoFactura == myParametro.Get2doDescuentoManualAdicional())
                    || (porcDescuentoFactura > DescuentoFactura.DescPorciento && (Factura.AutSecuencia > 0 || myParametro.GetParDescuentoManualNoValidaDias())))
                {
                    DescuentoFactura.DescPorciento = porcDescuentoFactura;
                    DescuentoFactura.DescuentoValor = Factura.Descuento;

                    var aplicado = 0.0;

                    if (DescuentoFactura.IndicadorItbis)
                    {

                        aplicado = (Factura.MontoTotal - Math.Abs(Factura.Credito) - Math.Abs(Factura.CxcNotaCredito));
                    }
                    else
                    {
                        aplicado = (Factura.MontoSinItbis - Math.Abs(Factura.CreditoSinItbis) - Math.Abs(Factura.CxcNotaCredito));
                    }

                    if (aplicado > 0 && DescuentoFactura.DescPorciento > 0)
                    {
                        DescuentoFactura.DescuentoValor = Math.Round(aplicado * (DescuentoFactura.DescPorciento / 100), 2);
                    }
                    else
                    {
                        DescuentoFactura.DescuentoValor = 0;
                        DescuentoFactura.DescPorciento = 0;
                    }
                }

                if (parDescuentoEnAbonos)
                {
                    var montoParaDescuento = 0.0;

                    if (DescuentoFactura.IndicadorItbis)
                    {
                        montoParaDescuento = (Factura.MontoTotal - Math.Abs(Factura.Credito) - Math.Abs(Factura.CxcNotaCredito));
                    }
                    else
                    {
                        montoParaDescuento = (Factura.MontoSinItbis - Math.Abs(Factura.CreditoSinItbis) - Math.Abs(Factura.CxcNotaCredito));
                    }

                    DescuentoFactura.DescuentoValor = GetMontoDescuentoParaAbono(Factura, DescuentoFactura.DescPorciento, montoParaDescuento, Factura.Estado != "Abono");
                }
                //else
                //{
                //    var montoParaDescuento = 0.0;

                //    if (DescuentoFactura.IndicadorItbis)
                //    {
                        
                //        montoParaDescuento = (Factura.MontoTotal - Math.Abs(CreditoAplicado.Credito));
                //    }
                //    else
                //    {
                //        montoParaDescuento = (Factura.MontoSinItbis - Math.Abs(CreditoAplicado.CreditoSinItbis));
                //    }

                //    if (montoParaDescuento > 0 && DescuentoFactura.DescPorciento > 0)
                //    {
                //        DescuentoFactura.DescuentoValor = DescuentoFactura.DefDescuento > 0 ? DescuentoFactura.DefDescuento : Math.Round(montoParaDescuento * (DescuentoFactura.DescPorciento / 100), 2);
                //    }
                //    else
                //    {
                //        DescuentoFactura.DescuentoValor = 0;
                //        DescuentoFactura.DescPorciento = 0;
                //    }

                //}

                total += DescuentoFactura.DescuentoValor;
            }
            
            return total;
        }

        public void CalcularDescuentoChkDiferidoADocumentosSaldados(bool Editing = false, bool withCalcDesc = false, bool isForAgregarFormaPago = false, string datechfdif = null)
        {
            if (!ExistsChkDiferidos() && string.IsNullOrEmpty(datechfdif) && !Editing || Arguments.Values.CurrentModule == Modules.RECONCILIACION)
            {
                return;
            }
            //var list = GetDocumentsInTemp(false, "Saldo", withCalcDesc).Where(f => !f.RecVerificarDesc).ToList();
            var list = GetDocumentsInTemp(false, "Saldo", withCalcDesc).ToList();

            var parDescuentoEnAbonos = myParametro.GetParDescuentoAbonos();

            if (parDescuentoEnAbonos)
            {
                list.AddRange(GetDocumentsInTemp(false, "Abono", withCalcDesc));
            }

            if (!ExistsChkDiferidos() && string.IsNullOrEmpty(datechfdif))
            {
                return;
            }

            DateTime fechaDiferido = string.IsNullOrEmpty(datechfdif)? DateTime.Parse(GetFormasPagoInTemp(true).FirstOrDefault().Fecha)
                : DateTime.Parse(datechfdif);

            foreach (var Factura in list)
            {
                var fechaFact = DateTime.ParseExact(Factura.FechaEntrega, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var dias = (fechaDiferido - fechaFact).TotalDays;

                var descPorcientoFactura = Factura.DescPorciento;

                //En el uso de descuento manual se quita el que se iguale a Cero ya que no se recalcula automaticamente
                if (myParametro.GetParDescuentoManual() <= 0)
                {
                    Factura.DescPorciento = 0;
                }

                if (!myParametro.GetParRecibosItbisMenos30Dias())
                {
                    //Si es diferido se actualiza el DefIndicadorItbis
                    var defIndicadorItbis = DS_DescuentoFacturas.GetInstance().GetIndicadorItbisDescuentoDisponible(Factura.Referencia, int.Parse(dias.ToString()));
                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set DefIndicadorItbis = " + defIndicadorItbis + " where Referencia = '" + Factura.Referencia + "' and ifnull(Origen, 0) > 0");
                    Factura.DefIndicadorItbis = defIndicadorItbis == 1 ? true : false;
                }

                DescFactura DescuentoFactura = myDesFac.GetMontoDescuentoFactura(Factura, (int)dias, -1,-1, fromAbono:Factura.Estado == "Abono", isForAgregarFormaPago: isForAgregarFormaPago);
                //MontoCreditoAplicadoFactura CreditoAplicado = GetMontoTotalCreditoAplicadoFactura(Factura);
                if ((DescuentoFactura.DescPorciento > descPorcientoFactura && string.IsNullOrEmpty(datechfdif)) || myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas() == 3 && DescuentoFactura.DescPorciento > 0 && (descPorcientoFactura == myParametro.GetDescuentoManualAdicional() || descPorcientoFactura == myParametro.Get2doDescuentoManualAdicional())
                    || (descPorcientoFactura > DescuentoFactura.DescPorciento && (Factura.AutSecuencia > 0 || myParametro.GetParDescuentoManualNoValidaDias())))
                {
                    DescuentoFactura.DescPorciento = descPorcientoFactura;
                    DescuentoFactura.DescuentoValor = Factura.Descuento;

                    var aplicado = 0.0;

                    if (DescuentoFactura.IndicadorItbis)
                    {

                        aplicado = (Factura.MontoTotal - Math.Abs(Factura.Credito) - Math.Abs(Factura.CxcNotaCredito));
                    }
                    else
                    {
                        aplicado = (Factura.MontoSinItbis - Math.Abs(Factura.CreditoSinItbis) - Math.Abs(Factura.CxcNotaCredito));
                    }

                    if (aplicado > 0 && DescuentoFactura.DescPorciento > 0)
                    {
                        DescuentoFactura.DescuentoValor =  Math.Round(aplicado * (DescuentoFactura.DescPorciento / 100), 2);
                    }
                    else
                    {
                        DescuentoFactura.DescuentoValor = 0;
                        DescuentoFactura.DescPorciento = 0;
                    }
                }

                if (parDescuentoEnAbonos)
                {
                    var aplicado = 0.0;

                    if (Factura.Estado == "Abono")
                    {
                        aplicado = Factura.Aplicado;
                    }
                    else
                    {
                        if (DescuentoFactura.IndicadorItbis)
                        {
                            aplicado = (Factura.MontoTotal - Math.Abs(Factura.Credito) - Math.Abs(Factura.CxcNotaCredito));
                        }
                        else
                        {
                            aplicado = (Factura.MontoSinItbis - Math.Abs(Factura.CreditoSinItbis) - Math.Abs(Factura.CxcNotaCredito));
                        }
                    }

                    DescuentoFactura.DescuentoValor = GetMontoDescuentoParaAbono(Factura, DescuentoFactura.DescPorciento, aplicado, Factura.Estado != "Abono");
                }
                //else
                //{
                //    var aplicado = 0.0;

                //    if (DescuentoFactura.IndicadorItbis)
                //    {

                //        aplicado = (Factura.MontoTotal - Math.Abs(CreditoAplicado.Credito));
                //    }
                //    else
                //    {
                //        aplicado = (Factura.MontoSinItbis - Math.Abs(CreditoAplicado.CreditoSinItbis));
                //    }

                //    if (aplicado > 0 && DescuentoFactura.DescPorciento > 0)
                //    {
                //        DescuentoFactura.DescuentoValor = DescuentoFactura.DefDescuento > 0 ? DescuentoFactura.DefDescuento : Math.Round(aplicado * (DescuentoFactura.DescPorciento / 100), 2);
                //    }
                //    else
                //    {
                //        DescuentoFactura.DescuentoValor = 0;
                //        DescuentoFactura.DescPorciento = 0;
                //    }

                //}

                if (Factura.Estado == "Saldo")
                {
                    SaldarFacturaInTemp(Factura, false, DescuentoFactura, false);
                }
                else if (Factura.Estado == "Abono")
                {
                    var pendiente = (Factura.Balance - DescuentoFactura.DescuentoValor) - (Math.Abs(Factura.Credito) + Math.Abs(Factura.CxcNotaCredito)) - Factura.Aplicado;

                    if (pendiente < 0)
                    {
                        pendiente = 0;
                    }

                    SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set Pendiente = "+pendiente+", Descuento = " + DescuentoFactura.DescuentoValor.ToString() + ", DescPorciento = " + DescuentoFactura.DescPorciento + ", AutSecuencia = 0, CalcularDesc = 1 " +
                    "where ltrim(rtrim(Documento)) = ? and ltrim(rtrim(Referencia)) = ?", new string[] { Factura.Documento, Factura.Referencia });
                }
            }
        }

        public double GetMontoADescuentoFactura(RecibosDocumentosTemp Factura)
        {
            var ncaplicaparaDescuento = myTipTran.GetTipoTransaccionAplicaDescuento("NC");
            List<DocumentosAplicadosTemp> NCs = new List<DocumentosAplicadosTemp>();
            if (ncaplicaparaDescuento)
            {
                NCs = GetNotasCreditoAplicadasByFactura(Factura.Referencia);
            }
            //{
            //    NCs = GetNotasCreditoAplicadasByFacturaWitoutNC(Factura.Referencia);
            //}
            //else



            //List<DocumentosAplicadosTemp> NCs = GetNotasCreditoAplicadasByFactura(Factura.Referencia);

            double CreditoSinItbis = 0, CreditoConItbis = 0;
            double Monto;

            foreach (var nc in NCs)
            {
                CreditoConItbis += Math.Abs(nc.ValorAplicado);
                CreditoSinItbis += Math.Abs(nc.MontoSinItbisNC);
            }
            
            if (Factura.DefIndicadorItbis)
            {
                Monto= Factura.MontoTotal - Math.Abs(Factura.CxcNotaCredito) - CreditoConItbis;
            }
            else
            {
                Monto= Factura.MontoSinItbis - CreditoSinItbis - Math.Abs(Factura.CxcNotaCredito);
            }

            if (Monto > 0)
            {
                return Monto;
            }
            else
            {
                return 0;
            }
        }

        public MontoCreditoAplicadoFactura GetMontoTotalCreditoAplicadoFactura(RecibosDocumentosTemp Factura)
        {
            var ncaplicaparaDescuento = myTipTran.GetTipoTransaccionAplicaDescuento("NC");
            List<DocumentosAplicadosTemp> NCs = new List<DocumentosAplicadosTemp>();
            if (ncaplicaparaDescuento)
            { 
                NCs = GetNotasCreditoAplicadasByFactura(Factura.Referencia);
            }
            double CreditoSinItbis = 0, CreditoConItbis = 0;

            //List<DocumentosAplicadosTemp> NCs = GetNotasCreditoAplicadasByFactura(Factura.Referencia);

            foreach (var nc in NCs)
            {
                CreditoConItbis += Math.Abs(nc.ValorAplicado);
                CreditoSinItbis += Math.Abs(nc.MontoSinItbisNC);
            }

            MontoCreditoAplicadoFactura result = new MontoCreditoAplicadoFactura();
            result.Credito = CreditoConItbis;
            result.CreditoSinItbis = CreditoSinItbis;

            return result;
        }

        public void UpdateIndicadorCalcularDescuentoInTemp(string FacturaReferencia, bool CalcularDescuento, bool RecVerificarDesc = false)
        {
            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set CalcularDesc = " + (CalcularDescuento ? "1" : "0") + " , RecVerificarDesc = " + (RecVerificarDesc ? "1" : "0") + " where trim(Referencia) = trim(?)", new string[] { FacturaReferencia });
        }

        public void UpdateIndicadorCalcularDesmonteInTemp(string FacturaReferencia, bool CalcularDesmonte, bool RecVerificarDesmonte = false)
        {
            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set CalcularDesmonte = " + (CalcularDesmonte ? "1" : "0") + " , RecVerificarDesmonte = " + (RecVerificarDesmonte ? "1" : "0") + " where trim(Referencia) = trim(?)", new string[] { FacturaReferencia });
        }

        public void UpdateAutSecuenciaFacturaInTemp(string Referencia, int AutId)
        {

            var where = "";

            if (!string.IsNullOrWhiteSpace(Referencia))
            {
                where = " and trim(Referencia) = trim('" + Referencia + "') ";
            }

            SqliteManager.GetInstance().Execute("Update RecibosDocumentosTemp set AutSecuencia = ? where Origen = '1' " + where, new string[] { AutId.ToString(), Referencia });
        }

        public void UpdateDescuentoConDescuentoInTemp(string Referencia, bool descConImpuesto)
        {
            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set DefIndicadorItbis = " + (descConImpuesto ? "1" : "0") + " where trim(Referencia) = trim(?)", new string[] { Referencia });
        }

        public bool HaySobrantesConAbonos()
        {
            decimal totalFacturas = Math.Round(Convert.ToDecimal(GetTotalFacturasAbonadasSaldadasInTemp()),2);
            decimal totalbalance = Math.Round(Convert.ToDecimal(GetTotalBalanceInTemp()), 2);
            bool hayFacturasAbonadasConPendientes = GetTotalFacturasAbonadasConPendientesInTemp() > 0;

            if (((totalbalance > totalFacturas) || (totalbalance < totalFacturas)) && hayFacturasAbonadasConPendientes)
            { //hay sobrante o faltante con abonos
                return true;
            }

            return false;
        }

        private double GetTotalFacturasAbonadasSaldadasInTemp()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("SELECT (SUM(Descuento) + SUM(Aplicado) + SUM(Credito)) as Valor FROM RecibosDocumentosTemp WHERE Origen = '1' and UPPER(Estado) IN ('SALDO', 'ABONO')", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].Valor;
            }

            return 0;
        }

        private double GetTotalFacturasAbonadasConPendientesInTemp()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("SELECT (SUM(Descuento) + SUM(Aplicado) + SUM(Credito)) as Valor FROM RecibosDocumentosTemp WHERE Origen = '1' and UPPER(Estado) IN ('ABONO') and cast(Pendiente as REAL) > 0.0", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].Valor;
            }

            return 0;
        }

        private double GetRecibosTotalDocumentos(int recSecuencia, string recTipo, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<RecibosAplicacion>("SELECT SUM(ifnull(RecValor, 0.0)) as RecValor FROM "+(confirmado ? "RecibosAplicacionConfirmados" : "RecibosAplicacion") +" WHERE " +
                "RecSecuencia = ? and trim(upper(RecTipo)) = ? and RepCodigo = ? ", 
                new string[] { recSecuencia.ToString(), recTipo.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim() });

            if (list != null && list.Count > 0)
            {
                return list[0].RecValor;
            }

            return 0;
        }

        private double GetTotalBalanceInTemp()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("SELECT SUM(Prima) as Valor FROM FormasPagoTemp", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].Valor;
            }

            return 0;
        }

        public List<RecibosAplicacion> GetNotasCreditosAplicadasByRecibo(string cxcReferencia, int recSecuencia, bool ReciboConfirmados)
        {
            return SqliteManager.GetInstance().Query<RecibosAplicacion>("select RefNumeroAutorizacion as CXCReferencia, cxcDocumento as CxCDocumento, abs(RefValor) as RecValor, 'NC' as CxcSigla, RecTasa, MonCodigo " +
                "from " + (ReciboConfirmados ? "RecibosFormaPagoConfirmados a " : "RecibosFormaPago a ") + " where ForID = 3 and ltrim(rtrim(CXCReferencia)) = ? and RecSecuencia = ? and ltrim(rtrim(RepCodigo)) = ?", new string[] { cxcReferencia.Trim(), recSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }

        private List<RecibosDocumentosTemp> GetDocumentosAplicados()
        {
            return SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select * from RecibosDocumentosTemp where Estado in ('Abono', 'Saldo') and ifnull(Origen, -1) <> -1", new string[] { });
        }
        
        public string[] GetDocumentosAplicadosForSelectFac()
        {
            return SqliteManager.GetInstance().
                Query<RecibosDocumentosTemp>(@"select Documento from RecibosDocumentosTemp where Estado in 
                ('Abono', 'Saldo') and ifnull(Origen, -1) <> -1 and Descuento > 0").
                Select(r => r.Documento).ToArray();
        }

        public void UpdateRecibosDocumentosTempForSelect(string recibodoc, double descuento, bool issobrante)
        {
            SqliteManager.GetInstance()
                .Execute($@"Update RecibosDocumentosTemp set Descuento = Descuento {(issobrante ? " - " : " + ")} 
                   {Math.Abs(descuento)}, DescPorciento = DescPorciento {(issobrante? "-" : "+")} round(({Math.Abs(descuento)} / Aplicado) * 100.0, 2),
                   DefIndicadorNoRestantes = 1, Aplicado = Aplicado {(issobrante ? " + " : " - ")} {Math.Abs(descuento)}
                   where Documento = '{recibodoc}'");
        }

        private List<FormasPagoTemp> GetFormasPagoAgregadas()
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select * from FormasPagoTemp", new string[] { });
        }

        public List<DocumentosAplicadosTemp> GetNotasCreditoAplicadas()
        {
            string query = @"select NCReferencia, ValorAplicado, MontoSinItbisNC, FacturaReferencia, FacturaDocumento, c.MonCodigo as NCMonCodigo, 
                             FacturaFecha, SiglaFactura, AutID, c.CxcDocumento as NCDocumento from DocumentosAplicadosTemp d  
                             inner join CuentasxCobrar c on c.CXCReferencia = d.NCReferencia  
			                 inner join TiposTransaccionesCxc ttc on  trim(UPPER(ttc.ttcSigla)) = trim(UPPER(c.CxcSigla)) 
                             and ttc.ttcOrigen  = -1";
            return SqliteManager.GetInstance().Query<DocumentosAplicadosTemp>(query, new string[] { });

            //return SqliteManager.GetInstance().Query<DocumentosAplicadosTemp>("select NCReferencia, ValorAplicado, MontoSinItbisNC, FacturaReferencia, FacturaDocumento, " +
            //    "FacturaFecha, SiglaFactura, AutID, c.CxcDocumento as NCDocumento from DocumentosAplicadosTemp d " +
            //    "inner join CuentasxCobrar c on c.CXCReferencia = d.NCReferencia and c.CxcSigla IN('NC','RCA')", new string[] { });
        }

        public bool HayDocumentosAplicadosInTemp()
        {
            var DocumentosAplicadosInTemp = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select * from RecibosDocumentosTemp where Origen = 1 and Estado in ('Abono', 'Saldo') limit 1").FirstOrDefault();

            return DocumentosAplicadosInTemp != null;
        }

        public bool HayFormasDePagoAgregadasInTemp()
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp limit 1").FirstOrDefault()!= null;
        }

        public bool HayFormasDePagoAgregadasRetencion()
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ForID = 5 ").FirstOrDefault()!= null;
        }

        public bool HayFormasDePagoAgregadasDiferenciaCambiaria()
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ForID = 8 ", new string[] { }).Count > 0;
        }


        public double GetMontoTotalFormaPagoByName(string name) { return GetMontoTotalFormaPagoByForId(-1, name, null); }
        private double GetMontoTotalFormaPagoByName(string name, string futurista) { return GetMontoTotalFormaPagoByForId(-1, name, futurista); }
        private double GetMontoTotalFormaPagoByForId(int forId) { return GetMontoTotalFormaPagoByForId(forId, null, null); }
        private double GetMontoTotalFormaPagoByForId(int forId, string formaPago, string futurista)
        {
           var sql = "SELECT SUM(Prima) as Valor FROM FormasPagoTemp WHERE ";

            if (forId == -1 && formaPago != null)
            {
                sql += " upper(FormaPago) = '" + formaPago.ToUpper() + "'";
            }
            else
            {
                sql += " ForID = " + forId.ToString();
            }

            if (futurista != null)
            {
                sql += " and upper(Futurista) = '" + futurista.ToUpper() + "'";
            }

            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>(sql, new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].Valor;
            }

            return 0;
        }

        private double GetMontoTotalDescuento()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("SELECT SUM(Descuento) as Valor FROM RecibosDocumentosTemp WHERE Origen = '1' and UPPER(Estado) = 'SALDO'", new string[] { });

            if (list != null && list.Count > 0)
            {
                return Math.Round(list[0].Valor,2);
            }

            return 0;
        }

        private double GetMontoTotalDesmonte()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("SELECT SUM(Desmonte) as Valor FROM RecibosDocumentosTemp WHERE Origen = '1' and UPPER(Estado) = 'SALDO'", new string[] { });

            if (list != null && list.Count > 0)
            {
                return Math.Round(list[0].Valor, 2);
            }

            return 0;
        }

        public List<CuentasxCobrar> GetCxCNCFByReference(string cxcReferencia)
        {
            return SqliteManager.GetInstance().Query<CuentasxCobrar>("select ifnull(CxCNCF,'') CxCNCF, CxCFecha, cxcComentario from CuentasxCobrar where ltrim(rtrim(CXCReferencia)) = ?", new string[] { cxcReferencia.Trim() });
        }
        public double GetMontoTotalFormasPago()
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("select SUM(Prima) as Valor from FormasPagoTemp", new string[] { });

            if (list != null && list.Count > 0)
            {
                return list[0].Valor;
            }

            return 0;
        }

        public int GetNextSecuenciaRecibos()
        {
            int RecSecuencia;
            int RecSecuenciaParams = myParametro.GetParRecibosSecuenciaPorSector();

            string AreaCtrlsubto = Arguments.Values.CurrentSector != null && Arguments.Values.CurrentClient != null ? myCli.GetareaCtrlCreditOfClienteDetalle(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo).Substring(0, 2) : "";

            if (RecSecuenciaParams >= 1 && myParametro.GetParRecibosPorSector())
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + (RecSecuenciaParams == 2 ? AreaCtrlsubto : Arguments.Values.CurrentSector.SecCodigo));
            }
            else
            {
                RecSecuencia = myParametro.GetParRecibosRecTipoChkDiferidos() && ExistsChkDiferidos()
                    ? DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-3")
                    : DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
            }

            return RecSecuencia;
        }

        public int GuardarRecibo(string recNumero, Monedas moneda, bool IsEditing = false, int EditingResecuencia = -1, string forceRecTipo = null, bool fromCopy= false)
        {
            var RecTipo = "2";
            var RecStatus = 1;
            int RecSecuencia;
            int RecSecuenciaParams = myParametro.GetParRecibosSecuenciaPorSector();

            string AreaCtrlsubfor = "", AreaCtrlsubto = "";
            if (myParametro.GetParAreaCrtlCreditoClienteSubString() && Arguments.Values.CurrentSector != null && Arguments.Values.CurrentClient != null)
            {
                string areaSub = myCli.GetareaCtrlCreditOfClienteDetalle(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo);
                if (!string.IsNullOrEmpty(areaSub))
                {

                    AreaCtrlsubfor = areaSub.Length > 1 ? areaSub.Length == 2 ? areaSub : areaSub.Substring(2, 2) : "";
                    AreaCtrlsubto = areaSub.Length > 1 ? areaSub.Length == 2 ? areaSub : areaSub.Substring(0, 2) : "";
                }
            }

            if (RecSecuenciaParams == 1 /*&& myParametro.GetParRecibosPorSector()*/)
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + Arguments.Values.CurrentSector.SecCodigo);
                RecTipo = Arguments.Values.CurrentSector.SecCodigo;
            }else if(RecSecuenciaParams == 2)
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-" + AreaCtrlsubto);
                RecTipo = Arguments.Values.CurrentSector.SecCodigo;
            }
            else
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");
            }

            if (myParametro.GetParRecibosRecTipoBySector() && Arguments.Values.CurrentSector != null)
            {
                RecTipo = Arguments.Values.CurrentSector.SecCodigo;
            }

            if (Arguments.Values.CurrenRecDocumentosTemp != null)
            {
                if (Arguments.Values.CurrenRecDocumentosTemp.RecTipo == 1 && !myParametro.GetParRecibosRecTipoChkDiferidos() && !ExistsChkDiferidos())
                {
                    RecTipo = Arguments.Values.CurrenRecDocumentosTemp.RecTipo.ToString();
                }
            }
           
            if (myParametro.GetParRecFormOrdenPago())
            {
                foreach (var fp in GetFormasPagoAgregadas())
                {
                    RecTipo = fp.ForID == 18 ? "3" : "2";
                    RecStatus = fp.ForID == 18 ? 3 : 1;
                }
            }

            if (myParametro.GetParRecibosRecTipoChkDiferidos() && ExistsChkDiferidos())
            {
                RecSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos-3");
                RecTipo = "3";//"2";
            }else if (Arguments.Values.CurrentModule == Modules.VENTAS || Arguments.Values.CurrentModule == Modules.ENTREGASREPARTIDOR)
            {
                RecTipo = "1";
            }

            if (!string.IsNullOrWhiteSpace(forceRecTipo))
            {
                RecTipo = forceRecTipo;
            }

            int lastSecuenciaVisitas = 0;

            if (IsEditing)
            {
                lastSecuenciaVisitas = DS_RepresentantesSecuencias.GetLastSecuencia("Visitas");
            }
            if (!IsEditing && !fromCopy)
            {
                new DS_Visitas().ActualizarVisitaEfectiva(Arguments.Values.CurrentVisSecuencia);
            }

            var rec = new Hash("Recibos");
            rec.Add("RecEstatus", IsEditing ? 9 : RecStatus);
            rec.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
            if (!myParametro.GetParRecibosAreaCtrolCredit())
                rec.Add("RecTipo", RecTipo);
            else
                rec.Add("RecTipo", AreaCtrlsubto);
            rec.Add("RecSecuencia", IsEditing ? EditingResecuencia : RecSecuencia);
            rec.Add("CliID", Arguments.Values.CurrentClient.CliID);
            rec.Add("RecFecha", Functions.CurrentDate());
            rec.Add("RecNumero", recNumero);            
            rec.Add("RecMontoNcr", Math.Abs(GetMontoNCTotalAplicado()));
            var montoDescuento = GetMontoTotalDescuento();
            rec.Add("RecMontoDescuento", montoDescuento);
            var montoDesmonte = GetMontoTotalDesmonte();
            rec.Add("RecTotalDescuentoDesmonte", montoDesmonte);
            rec.Add("RecMontoEfectivo", GetMontoTotalFormaPagoByName("EFECTIVO"));
            rec.Add("RecMontoCheque", GetMontoTotalFormaPagoByName("CHEQUE", "No"));
            rec.Add("RecMontoChequeF", GetMontoTotalFormaPagoByName("CHEQUE", "Si"));
            rec.Add("RecMontoTransferencia", GetMontoTotalFormaPagoByName("TRANSFERENCIA"));
            rec.Add("RecMontoTarjeta", GetMontoTotalFormaPagoByName("TARJETA"));

            var montoSobrante = Verificar() ? 0 : GetTotalAPagar();
            
            rec.Add("RecMontoSobrante", montoSobrante);

            rec.Add("CuaSecuencia", Arguments.Values.CurrentCuaSecuencia);
            rec.Add("VisSecuencia", IsEditing ? lastSecuenciaVisitas : Arguments.Values.CurrentVisSecuencia);
            rec.Add("DepSecuencia", 0);
            rec.Add("RecRetencion", GetMontoTotalFormaPagoByName("RETENCION"));

            rec.Add("RecDivision", AreaCtrlsubto);

            if(!IsEditing)
            {
                rec.Add("SecCodigo", (Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.SecCodigo : ""));
            }
            
            if(myParametro.GetParAreaCrtlCreditoClienteSubString() && Arguments.Values.CurrentSector.AreaCtrlCredit != null && Arguments.Values.CurrentSector.AreaCtrlCredit.Length > 1)
            {
                rec.Add("AreactrlCredit", AreaCtrlsubto + AreaCtrlsubfor);
            }
            else 
            {
                rec.Add("AreactrlCredit", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : "");
            }
            rec.Add("MonCodigo", moneda != null ? moneda.MonCodigo : "");
            rec.Add("OrvCodigo", Arguments.Values.CurrentSector != null ? myCli.GetofvCodigoAndorvCodigo(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo).Item1 :"");
            rec.Add("OfvCodigo", Arguments.Values.CurrentSector != null ? myCli.GetofvCodigoAndorvCodigo(Arguments.Values.CurrentClient.CliID, Arguments.Values.CurrentSector.SecCodigo).Item2 : "");
            rec.Add("RecCantidadImpresion", 0);            
            rec.Add("RecTotal", GetMontoTotalFormasPago());
            rec.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            rec.Add("RecFechaActualizacion", Functions.CurrentDate());
            string rowguid = Guid.NewGuid().ToString();
            rec.Add("rowguid", rowguid);
            rec.Add("mbVersion", Functions.AppVersion);
            rec.Add("RecTasa", moneda != null ? moneda.MonTasa : 1);

            double MontoTotalAplicado = 0.0;
            double MontoTotalDescuento = 0.0;
            
            //rec.Add("PHONE_ID", "");
            if (IsEditing)
            {
                var recibo = GetReciboBySecuencia(EditingResecuencia, false);

                if (recibo != null)
                {
                    rec.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { IsText = true, Value = recibo.rowguid } }, true);
                }

               // rec.ExecuteUpdate("ltrim(rtrim(Repcodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' And RecSecuencia = " + EditingResecuencia + " ");
            }
            else { rec.ExecuteInsert(); }

            int reaSecuencia = 0;
            bool parNCdpp = myParametro.GetParRecibosNCPorDescuentoProntoPago() == 1;

            var parDescuentoEnAbonos = myParametro.GetParDescuentoAbonos();

            double RecItbis = 0.0;

            var montoTotal = 0.0;
            var montoSinItbisTotal = 0.0;
            var parMontoSobranteParaDescuento = myParametro.GetParRecibosMontoSobranteConvertirADescuento();

            var restarSobranteADescuento = false;

            foreach (var Aplicado in GetDocumentosAplicados())
            {
                RecItbis = Aplicado.MontoTotal > 0 ? Aplicado.MontoItbis * (Aplicado.Aplicado / Aplicado.MontoTotal) : 0;
                var ap = new Hash("RecibosAplicacion");
                ap.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                if(!myParametro.GetParRecibosAreaCtrolCredit())
                   ap.Add("RecTipo", RecTipo);
                else
                   ap.Add("RecTipo", AreaCtrlsubto);

                ap.Add("RecSecuencia", IsEditing ? EditingResecuencia : RecSecuencia);
                reaSecuencia++;
                ap.Add("ReaSecuencia", reaSecuencia);
                if(!IsEditing)
                {
                    ap.Add("SocCodigo", AreaCtrlsubto);
                }                
                ap.Add("CXCReferencia", Aplicado.Referencia != null ? Aplicado.Referencia : "");
                ap.Add("RecValor", Math.Abs(Aplicado.Aplicado));
                ap.Add("RecItbis", RecItbis);
                
                MontoTotalAplicado += Aplicado.Aplicado;

                if(!parDescuentoEnAbonos && Aplicado.Descuento > Aplicado.Balance)
                {
                    Aplicado.Descuento = Aplicado.Balance;
                    MontoTotalDescuento += Aplicado.Balance;
                }

                ap.Add("RecIndicadorSaldo", Aplicado.Estado == "Saldo" || (Aplicado.Estado == "Abono" && (Aplicado.Aplicado + Aplicado.Descuento) >= Aplicado.Balance) ? 1 : 0);


                if (parMontoSobranteParaDescuento > 0.0 && !restarSobranteADescuento && montoSobrante >= parMontoSobranteParaDescuento && Aplicado.Descuento >= montoSobrante)
                {
                    Aplicado.Descuento -= montoSobrante;
                    Aplicado.DescPorciento = Math.Round((Aplicado.Descuento / Aplicado.Aplicado) * 100.0, 2);
                    restarSobranteADescuento = true;
                }
               
                ap.Add("RecDescuento", Math.Round(Aplicado.Descuento,2));
                ap.Add("RecDescuentoDesmonte", Math.Round(Aplicado.Desmonte, 2));
                MontoTotalDescuento += Aplicado.Descuento;
                ap.Add("CxcSigla", Aplicado.Sigla);
                ap.Add("repCodigo2", Aplicado.RepCodigo);
                ap.Add("AutID", Aplicado.AutSecuencia);
                ap.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                ap.Add("RecFechaActualizacion", Functions.CurrentDate());
                ap.Add("rowguid", Guid.NewGuid().ToString());
                ap.Add("RecPorcDescuento", Aplicado.DescPorciento);
                ap.Add("RecMontoADescuento", GetMontoADescuentoFactura(Aplicado));
                ap.Add("CxCDocumento", Aplicado.Documento);
                ap.Add("DefIndicadorItbis", Aplicado.DefIndicadorItbis);
                ap.Add("cliid", Arguments.Values.CurrentClient.CliID);
                ap.Add("CxcBalance", Aplicado.Balance);
                ap.Add("MonCodigo", Aplicado.MonCodigo);
                Monedas tasa = SqliteManager.GetInstance().Query<Monedas>("SELECT * FROM Monedas where monCodigo = ? ", new string[] { Aplicado.MonCodigo }).FirstOrDefault();

                ap.Add("RecTasa", myParametro.GetParRecibosMonedaUnicaConTasaDocumento() ? Aplicado.TasaDocumento : tasa?.MonTasa);

                if (parNCdpp && Aplicado.Descuento > 0.0 && !IsEditing)
                {
                    var ncdSecuencia = DS_RepresentantesSecuencias.GetLastSecuencia("NCDPP");

                    var des = new Hash("NCDPP");
                    des.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                    des.Add("NCDSecuencia", ncdSecuencia);
                    des.Add("NCDFecha", Functions.CurrentDate());
                    des.Add("RecTipo", RecTipo);
                    des.Add("RecSecuencia", IsEditing ? EditingResecuencia : RecSecuencia);
                    des.Add("CxcReferencia", Aplicado.Referencia != null ? Aplicado.Referencia : "");
                    des.Add("CxcDocumento", Aplicado.Documento);
                    des.Add("CxCNCFAfectado", Aplicado.CXCNCF);
                    des.Add("NCDMonto", Aplicado.Descuento);
                    des.Add("NCDItbis", Math.Abs(((1.0 - (Aplicado.MontoTotal / Aplicado.MontoSinItbis)) * 100.0)).ToString("N2"));//tengo la formula
                    des.Add("CliID", Arguments.Values.CurrentClient.CliID);

                    var ncf = new DS_Clientes().GetSiguienteNCF(Arguments.Values.CurrentClient, forNC:true);

                    if (ncf == null)
                    {
                        throw new Exception("No tienes secuencia de NCF definidas para las Notas de Creditos por pronto pago");
                    }

                    des.Add("NCDNCF", ncf.NCFCompleto);
                    ActualizarNcfDpp(ncf.Secuencia.ToString(), ncf.rowguid);
                    des.Add("NCDEstatus", 1);
                    des.Add("NCDFechaActualizacion", Functions.CurrentDate());
                    des.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                    des.Add("rowguid", Guid.NewGuid().ToString());
                    des.ExecuteInsert();

                    DS_RepresentantesSecuencias.UpdateSecuencia("NCDPP", ncdSecuencia);
                }

                var recValorSinImp = 0.0;

                if (Aplicado.MontoTotal > 0)
                {
                    recValorSinImp = Aplicado.Aplicado * Aplicado.MontoSinItbis / Aplicado.MontoTotal;
                }

                montoTotal += Aplicado.MontoTotal;
                montoSinItbisTotal += recValorSinImp;

                ap.Add("RecValorSinImpuesto", recValorSinImp);
                if (IsEditing) {
                    ap.ExecuteUpdate("ltrim(rtrim(Repcodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' And RecSecuencia = " + EditingResecuencia + " " + (IsEditing ? "And ReaSecuencia = " + reaSecuencia + " " : ""));
                } else {
                    ap.ExecuteInsert();
                }

            }

            if (restarSobranteADescuento)
            {
                var recUpdate = new Hash("Recibos");
                recUpdate.Add("RecMontoDescuento", Math.Round(montoDescuento - montoSobrante));

                var recibo = GetReciboBySecuencia(RecSecuencia, false);

                if (recibo != null)
                {
                    recUpdate.ExecuteUpdate(new string[] { "rowguid" }, new Model.Internal.DbUpdateValue[] { new Model.Internal.DbUpdateValue() { IsText = true, Value = recibo.rowguid } }, true);
                }
            }

            double MontoTotalEfectivo = 0;
            double MontoTotalCheque = 0;
            double MontoTotalChequeFuturista = 0;
            double MontoTotalTransferencia = 0;
            double MontoTotalTarjeta = 0;

            var myTranImg = new DS_TransaccionesImagenes();

            var secuencia = DS_RepresentantesSecuencias.GetLastSecuencia("Recibos");


            if (IsEditing)
            {
                Hash UpdateOP = new Hash("RecibosFormaPago");
                UpdateOP.Add("RefValor", 0);
                UpdateOP.Add("RecPrima", 0);
                UpdateOP.ExecuteUpdate("ltrim(rtrim(Repcodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' And RecSecuencia = " + EditingResecuencia + " ");
            }
            int RefSecuenciaConOP = 0;
            if (IsEditing)
            {
                RefSecuenciaConOP = GetOPformasPago(EditingResecuencia) + 1;
            }

            foreach (var fp in GetFormasPagoAgregadas())
            {

                Hash pago = new Hash("RecibosFormaPago");
                pago.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);

                if (!myParametro.GetParRecibosAreaCtrolCredit())
                    pago.Add("RecTipo", RecTipo);
                else
                    pago.Add("RecTipo", AreaCtrlsubto);

                pago.Add("RecSecuencia", IsEditing ? EditingResecuencia : RecSecuencia);
                pago.Add("RefSecuencia", IsEditing ? RefSecuenciaConOP : fp.RefSecuencia);
                RefSecuenciaConOP++;
                pago.Add("AutSecuencia", fp.AutSecuencia);
                pago.Add("ForID", fp.ForID);
                pago.Add("BanID", fp.BanID);
                pago.Add("RefTarjetaPV", fp.TipTarjeta);

                myTranImg.MarkToSendToServer("RecibosFormaPago", secuencia + "|" + fp.RefSecuencia, false);

                switch (fp.ForID)
                {
                    case 1: //efectivo
                        MontoTotalEfectivo += fp.Valor;
                        break;
                    case 2: //cheque
                        if (fp.Futurista == "Si")
                        {
                            MontoTotalChequeFuturista += fp.Valor;
                        }
                        else
                        {
                            MontoTotalCheque += fp.Valor;
                        }
                        break;
                    case 3: //notas de creditos
                        break;
                    case 4: //transferencia
                        MontoTotalTransferencia += fp.Valor;
                        break;
                    case 5: //retencion
                        break;
                    case 6: //tarjeta
                        MontoTotalTarjeta += fp.Valor;
                        break;
                    case 18: //orden pago
                        break;
                }

                pago.Add("RefNumeroCheque", fp.NoCheque);
                pago.Add("RefFecha", fp.Fecha == null || fp.Fecha.Trim().Length == 0 ? Functions.CurrentDate() : fp.Fecha);
                pago.Add("RefIndicadorDiferido", fp.Futurista == "Si" ? 1 : 0);
                pago.Add("RefNumeroAutorizacion", 0);
                pago.Add("RefValor", fp.Valor);
                pago.Add("CXCReferencia", "");
                pago.Add("cliid", Arguments.Values.CurrentClient.CliID);

                //if de multimoneda
                pago.Add("RecPrima", fp.Prima);
                pago.Add("MonCodigo", fp.MonCodigo);

                pago.Add("RecTasa", fp.Tasa);

                if(!IsEditing)
                {
                    if (!myParametro.GetParRecibosAreaCtrolCredit())
                        pago.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                    else
                        pago.Add("SocCodigo", AreaCtrlsubto);                    
                }               

                pago.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                pago.Add("RecFechaActualizacion", Functions.CurrentDate());
                pago.Add("rowguid", Guid.NewGuid().ToString());
                pago.ExecuteInsert();
            }

            var refSecuencia = GetLastRefSecuenciaInTemp() + 1;

            foreach (var nc in GetNotasCreditoAplicadas())
            {
                Hash Credito = new Hash("RecibosFormaPago");
                Credito.Add("RepCodigo", Arguments.CurrentUser.RepCodigo);
                Credito.Add("RecTipo", RecTipo);
                Credito.Add("RecSecuencia", RecSecuencia);
                Credito.Add("RefSecuencia", refSecuencia); refSecuencia++;
                Credito.Add("ForID", 3);
                Credito.Add("BanID", 0);
                Credito.Add("cliid", Arguments.Values.CurrentClient.CliID);
                Credito.Add("RefNumeroCheque", 0);
                Credito.Add("RefFecha", Functions.CurrentDate());
                Credito.Add("RefIndicadorDiferido", 0);
                Credito.Add("RefNumeroAutorizacion", nc.NCReferencia);
                Credito.Add("RefValor", Math.Abs(nc.ValorAplicado));
                Credito.Add("CXCReferencia", nc.FacturaReferencia);
                Credito.Add("RecPrima", 0);
                Credito.Add("RecTasa", 0);
                Credito.Add("MonCodigo", nc.NCMonCodigo);

                if (!IsEditing)
                {
                    if (!myParametro.GetParRecibosAreaCtrolCredit())
                        Credito.Add("SocCodigo", Arguments.Values.CurrentSector != null ? Arguments.Values.CurrentSector.AreaCtrlCredit : null);
                    else
                        Credito.Add("SocCodigo", AreaCtrlsubto);
                }                
                Credito.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
                Credito.Add("RecFechaActualizacion", Functions.CurrentDate());
                Credito.Add("rowguid", Guid.NewGuid().ToString());
                Credito.Add("cxcDocumento", nc.NCDocumento);
                Credito.Add("RefFechaDocumento", nc.FacturaFecha);
                Credito.ExecuteInsert();
                
            }

            var updateRec = new Hash("Recibos");
            updateRec.Add("RecCantidadDetalleAplicacion", reaSecuencia);
            updateRec.Add("RecCantidadDetalleFormaPago", IsEditing ? RefSecuenciaConOP - 1 : refSecuencia - 1);
            updateRec.ExecuteUpdate($"rowguid = '{rowguid}'");

            if (myParametro.GetParRecibosValidarMontoCabeceraVSdetalle())
            {
                if (Math.Abs(Convert.ToDouble(rec["recmontoefectivo"])) - MontoTotalEfectivo > 1)
                {
                    throw new Exception("El monto efectivo no concuerda");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontocheque"])) - MontoTotalCheque > 1)
                {
                    throw new Exception("El monto de cheque no concuerda");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontochequef"])) - MontoTotalChequeFuturista > 1)
                {
                    throw new Exception("El monto de cheque futurista no concuerda");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontotransferencia"])) - MontoTotalTransferencia > 1)
                {
                    throw new Exception("El monto de transferencia no concuerda");
                }

                if (Math.Abs(Convert.ToDecimal(rec["rectotal"])) - Convert.ToDecimal(MontoTotalAplicado) > 0)
                {
                    throw new Exception("Hay una diferencia entre el total de la cabecera y el detalle del recibo");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontodescuento"])) - MontoTotalDescuento > 1)
                {
                    throw new Exception("hay una diferencia entre el descuento de la cabecera y el detalle del recibo");
                }

                if (Math.Abs(Convert.ToDouble(rec["recmontotarjeta"])) - MontoTotalTarjeta > 1)
                {
                    throw new Exception("El monto de tarjeta no concuerda");
                }
            }

            if (IsEditing)
            {
                DS_RepresentantesSecuencias.UpdateSecuencia("Visita", lastSecuenciaVisitas);

            }
            else
            {
                if (myParametro.GetParRecibosRecTipoChkDiferidos() && ExistsChkDiferidos())
                {
                    DS_RepresentantesSecuencias.UpdateSecuencia("Recibos-3", RecSecuencia); 
                }

                if (RecSecuenciaParams > 0)
                {
                    DS_RepresentantesSecuencias.UpdateSecuencia("Recibos-" + (RecSecuenciaParams == 2 ? AreaCtrlsubto
                        : Arguments.Values.CurrentSector.SecCodigo), RecSecuencia);
                }
                else
                {

                    DS_RepresentantesSecuencias.UpdateSecuencia("Recibos", RecSecuencia);
                }
                
            }

            if (DS_RepresentantesParametros.GetInstance().GetParVisitasResultados())
            {
                ActualizarVisitasResultados();
            }

            ClearTemps();

            return IsEditing ? EditingResecuencia : RecSecuencia;
        }

        private void ActualizarVisitasResultados()
        {
            var list = SqliteManager.GetInstance().Query<VisitasResultados>("select '' as VisComentario, 3 as TitID, count(*) as VisCantidadTransacciones, " +
                "sum(a.RecValor) as VisMontoTotal, sum(a.RecValorSinImpuesto) as VisMontoSinItbis " +
                "from Recibos r " +
                "inner join RecibosAplicacion a on a.RepCodigo = r.RepCodigo and a.RecSecuencia = r.RecSecuencia and r.RecTipo = a.RecTipo " +
                "where r.VisSecuencia = ? and r.RepCodigo = '"+Arguments.CurrentUser.RepCodigo.Trim()+"'", 
                new string[] { Arguments.Values.CurrentVisSecuencia.ToString() });

            if(list != null && list.Count > 0)
            {
                new DS_Visitas().GuardarVisitasResultados(list[0]);
            }
        }

        public void EliminarFormaPagoInTemp(FormasPagoTemp formaPago)
        {
            SqliteManager.GetInstance().Execute("delete from FormasPagoTemp where FormaPago = ? and rowguid = ?", new string[] { formaPago.FormaPago, formaPago.rowguid });

            if (formaPago.Futurista.Trim().ToUpper() == "SI")
            {
                SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set FechaChequeDif = Null where Origen = 1");
            }
        }

        public void EliminarTodasFormaPagoInTemp()
        {
            SqliteManager.GetInstance().Execute("delete from FormasPagoTemp", new string[] { });

        }

        public Recibos GetReciboBySecuencia(int RecSecuencia, bool reciboConfirmado)
        {
            var list = SqliteManager.GetInstance().Query<Recibos>("select c.CliSector as CliSector, c.RepCodigo as CliRepCodigo, RecSecuencia,RecTipo, ifnull(R.RecTipo, '') as RecTipo, R.SecCodigo, c.CliID , R.ofvCodigo, RecCantidadImpresion,c.CliIndicadorPresentacion as CliIndicadorPresentacion, RecFecha, CliCodigo, CliNombre, " + (reciboConfirmado ? 1 : 0) + " as Confirmado, RecEstatus, " +
                "RecMontoSobrante,ifnull(C.CliContacto, '') as CliContacto,ifnull(C.CliCalle, '') as CliCalle, ifnull(C.CliUrbanizacion, '') as CliUrbanizacion, ifnull(C.CliRNC, '') as CliRNC, ifnull(C.CliTelefono, '') as CliTelefono, VisSecuencia, c.RepCodigo as RepVendedor, " +
                "r.MonCodigo " + (myParametro.GetParRecibosPuedeCobrarEnVariasMonedas() ? ",RecTasa" : "") + ", m.MonSigla as MonSigla, r.rowguid as rowguid from " + (reciboConfirmado ? "RecibosConfirmados" : "Recibos") + " r inner join Clientes c on c.CliID = r.CliID " +
                "Left join Monedas m on m.Moncodigo = r.MonCodigo "+
                "where RecSecuencia = ? and r.RepCodigo = ? ", new string[] { RecSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public Recibos GetReciboByNumeroYTipo(string recNumero, string recTipo)
        {
            var list = SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia,RecTipo, ifnull(R.RecTipo, '') as RecTipo, R.SecCodigo, RecCantidadImpresion, RecFecha, CliCodigo, CliNombre, 0 as Confirmado, RecEstatus, " +
                "RecMontoSobrante,ifnull(C.CliContacto, '') as CliContacto,ifnull(C.CliCalle, '') as CliCalle, ifnull(C.CliUrbanizacion, '') as CliUrbanizacion, ifnull(C.CliRNC, '') as CliRNC, ifnull(C.CliTelefono, '') as CliTelefono, VisSecuencia, c.RepCodigo as RepVendedor, " +
                "r.MonCodigo " + (myParametro.GetParRecibosPuedeCobrarEnVariasMonedas() ? ",RecTasa" : "") + ", m.MonSigla as MonSigla, r.rowguid as rowguid from Recibos r inner join Clientes c on c.CliID = r.CliID " +
                "Left join Monedas m on m.Moncodigo = r.MonCodigo " +
                "where RecNumero = ? and r.RepCodigo = ? and RecTipo = ? ", new string[] { recNumero, Arguments.CurrentUser.RepCodigo, recTipo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<Recibos> GetRecibosBySecuencias(List<Recibos>recSecuencias, bool confirmados)
        {
            if (recSecuencias == null || recSecuencias.Count == 0)
            {
                return new List<Recibos>();
            }

            /*string where = "";

            foreach(var rec in recSecuencias)
            {
                where += (!string.IsNullOrWhiteSpace(where) ? " or " : "") + "(r.RecSecuencia = " + rec.RecSecuencia + " and r.RecTipo = '"+rec.RecTipo+"')";
            }

            where += ") order by RecSecuencia desc";*/

            List<Recibos> list = list = SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia, ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '') as RecFecha, CliCodigo||'-'||CliNombre as CliNombre, r.CliID as CliID, RecTipo, " + (confirmados ? "1" : "0") + " as confirmado, r.RecEstatus as RecEstatus " +
                " from " + (confirmados ? "RecibosConfirmados" : "Recibos") + " r inner join Clientes c on c.CliID = r.CliID " +
                "where r.RepCodigo = ? order by RecSecuencia desc", new string[] { Arguments.CurrentUser.RepCodigo });

            var RecSecuencia = recSecuencias.Select(r => r.RecSecuencia);
            var RecTipo = recSecuencias.Select(r => r.RecTipo);

            return list.Where(r => RecSecuencia.Contains(r.RecSecuencia) && RecTipo.Contains(r.RecTipo)).ToList();
        }

        public Recibos GetReciboBySecuencia_OLD(int RecSecuencia, bool reciboConfirmado)
        {
            List<Recibos> list = SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia,RecTipo, R.SecCodigo, RecCantidadImpresion, RecFecha, CliCodigo, CliNombre, " + (reciboConfirmado ? 1 : 0) + " as Confirmado, RecEstatus, " +
                "RecMontoSobrante,r.MonCodigo " + (myParametro.GetParRecibosPuedeCobrarEnVariasMonedas() ? ",RecTasa" : "") + " from " + (reciboConfirmado ? "RecibosConfirmados" : "Recibos") + " r inner join Clientes c on c.CliID = r.CliID " +
                "where RecSecuencia = ? and r.RepCodigo = ? ", new string[] { RecSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }


        public void ActualizarCantidadImpresion(/*int recSecuencia, string recTipo, */string rowguid)
        {
            Hash map = new Hash("Recibos")
            {
                SaveScriptForServer = false
            };
            map.Add("RecCantidadImpresion", "RecCantidadImpresion + 1", true);
            map.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            /////POR QUE HABIAN 2 UPDATE EN ESTE METODO PARA LA MISMA TABLA DE RECIBOS????
           // map.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and RecSecuencia = " + recSecuencia + " and ltrim(rtrim(ifnull(RecTipo, ''))) = '" + recTipo.Trim() + "'");
            map.SaveScriptForServer = true;
           // map.ExecuteUpdate("RepCodigo = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and RecSecuencia = " + recSecuencia + " and ifnull(RecTipo, '') = '" + recTipo.Trim() + "'");
            map.ExecuteUpdate("rowguid = '"+ rowguid + "'");


            Hash map2 = new Hash("RecibosConfirmados");
            map2.Add("RecCantidadImpresion", "RecCantidadImpresion + 1", true);
            map2.Add("UsuInicioSesion", Arguments.CurrentUser.RepCodigo);
            //map2.ExecuteUpdate("ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' and RecSecuencia = " + recSecuencia + " and ifnull(RecTipo, '') = '" + recTipo.Trim() + "'");
            map2.ExecuteUpdate("rowguid = '" + rowguid + "'");

        }

        public List<RecibosAplicacion> GetRecibosAplicacionBySecuencia(int RecSecuencia, bool ReciboConfirmados, string recTipo = null, bool withNC = false)
        {
            string query = "select ifnull(a.CxCDocumento, '') as CxCDocumento, ifnull(a.CxcBalance,0.0) as CxcBalance, a.CXCReferencia, a.RecValor as RecValor, RecDescuento, " +
                "a.RecValorSinImpuesto as RecValorSinImpuesto, a.RecItbis as RecItbis, a.CxcSigla as CxcSigla, ifnull(cc.cxcFechaVencimiento, '') as CXCFechaVencimiento, ifnull(cc.cxcFecha, '') as CXCFecha, RecIndicadorSaldo, a.RecPorcDescuento as RecPorcDescuento " +
               "from " + (ReciboConfirmados ? "RecibosAplicacionConfirmados a " : "RecibosAplicacion a ") + "LEFT JOIN CuentasXCobrar cc ON a.CXCReferencia = cc.CXCReferencia and a.CxcSigla= cc.CxcSigla " +
               "where a.RecSecuencia = "+ RecSecuencia.ToString()  + " and a.RepCodigo = '"+ Arguments.CurrentUser.RepCodigo + "' " + (!string.IsNullOrWhiteSpace(recTipo) ? " and trim(upper(a.RecTipo)) = '" + recTipo.Trim().ToUpper() + "' " : "") + (!withNC ? " and a.CxCSigla <> 'NC' " : "");


            return SqliteManager.GetInstance().Query<RecibosAplicacion>(query,
               new string[] {});
        }

        public List<RecibosAplicacion> GetRecibosAplicacionConDesmonteBySecuencia(int RecSecuencia, bool ReciboConfirmados, string recTipo = null, bool withNC = false)
        {
            string query = "select ifnull(a.CxCDocumento, '') as CxCDocumento, ifnull(a.CxcBalance,0.0) as CxcBalance, a.CXCReferencia, a.RecValor as RecValor, RecDescuento, " +
                "a.RecValorSinImpuesto as RecValorSinImpuesto, a.RecItbis as RecItbis, a.CxcSigla as CxcSigla, ifnull(cc.cxcFechaVencimiento, '') as CXCFechaVencimiento, ifnull(cc.cxcFecha, '') as CXCFecha, RecIndicadorSaldo, a.RecPorcDescuento as RecPorcDescuento, a.RecDescuentoDesmonte " +
               "from " + (ReciboConfirmados ? "RecibosAplicacionConfirmados a " : "RecibosAplicacion a ") + "LEFT JOIN CuentasXCobrar cc ON a.CXCReferencia = cc.CXCReferencia and a.CxcSigla= cc.CxcSigla " +
               "where a.RecSecuencia = " + RecSecuencia.ToString() + " and a.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " + (!string.IsNullOrWhiteSpace(recTipo) ? " and trim(upper(a.RecTipo)) = '" + recTipo.Trim().ToUpper() + "' " : "") + (!withNC ? " and a.CxCSigla <> 'NC' " : "");


            return SqliteManager.GetInstance().Query<RecibosAplicacion>(query,
               new string[] { });
        }

        public List<RecibosAplicacion> GetRecibosAplicacionBySecuencia2(int RecSecuencia, bool ReciboConfirmados, string recTipo = null, bool withNC = false)
        {
            string query = "select ifnull(a.CxCDocumento, '') as CxCDocumento, ifnull(a.CxcBalance,0.0) as CxcBalance, a.CXCReferencia, a.RecValor as RecValor, RecDescuento, " +
                "a.RecValorSinImpuesto as RecValorSinImpuesto, a.RecItbis as RecItbis, a.CxcSigla as CxcSigla, ifnull(cc.cxcFechaVencimiento, '') as CXCFechaVencimiento, ifnull(cc.cxcFecha, '') as CXCFecha, RecIndicadorSaldo, a.MonCodigo as MonCodigo, a.RecTasa as RecTasa, a.RecPorcDescuento as RecPorcDescuento " +
               "from " + (ReciboConfirmados ? "RecibosAplicacionConfirmados a " : "RecibosAplicacion a ") + "LEFT JOIN CuentasXCobrar cc ON a.CXCReferencia = cc.CXCReferencia " +
               "where a.RecSecuencia = " + RecSecuencia.ToString() + " and a.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " + (!string.IsNullOrWhiteSpace(recTipo) ? " and trim(upper(a.RecTipo)) = '" + recTipo.Trim().ToUpper() + "' " : "") + (!withNC ? " and a.CxCSigla <> 'NC' " : "");


            return SqliteManager.GetInstance().Query<RecibosAplicacion>(query,
               new string[] { });
        }

        public double GetMontoTotalNCByRecibos(int RecSecuencia, bool ReciboConfirmado)
        {
            List<RecibosAplicacion> list = SqliteManager.GetInstance().Query<RecibosAplicacion>("select sum(RefValor) as RecValor from " +
                (ReciboConfirmado ? "RecibosFormaPagoConfirmados" : "RecibosFormaPago") + " f inner join " + (ReciboConfirmado ? "RecibosConfirmados" : "Recibos") + " r on r.RepCodigo = f.RepCodigo and r.RecSecuencia = f.RecSecuencia " +
                "WHERE ForID = 3 and f.RepCodigo = ? and f.RecSecuencia = ?", new string[] { Arguments.CurrentUser.RepCodigo, RecSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0].RecValor;
            }

            return 0;
        }


        public RecibosFormaPago GetRecibosIsDiferido(int recSecuencia, string rectipo, bool reciboConfirmado = false)
        {
            List<RecibosFormaPago> list = SqliteManager.GetInstance().Query<RecibosFormaPago>("select RefIndicadorDiferido " +
                "from " + (reciboConfirmado ? "RecibosFormaPagoConfirmados" : "RecibosFormaPago") + " R " +
                "LEFT JOIN Bancos B ON B.BanID = R.BanID " +
                "LEFT JOIN CuentasBancarias C on CuBID = R.BanID AND ForID = 4 " +
                "where RecSecuencia = ? and RepCodigo = ? " + (!string.IsNullOrWhiteSpace(rectipo) ? " and trim(upper(R.RecTipo)) = '" + rectipo.Trim().ToUpper() + "' " : "") + " AND ForID <> 3 order by RefSecuencia",
                new string[] { recSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<RecibosFormaPago> GetRecibosFormasPagoBySecuencia(int RecSecuencia, bool reciboConfirmado, string recTipo = null)
        {
            return SqliteManager.GetInstance().Query<RecibosFormaPago>("select ifnull(CuBNombre, ifnull(ltrim(rtrim(BanNombre)), '')) as BanNombre, ForID, RefNumeroCheque, RefValor, " +
                "RefIndicadorDiferido, strftime('%d-%m-%Y', RefFecha) as RefFecha, RecTasa, RecPrima, RefNumeroAutorizacion from " + (reciboConfirmado ? "RecibosFormaPagoConfirmados" : "RecibosFormaPago") + " R " +
                "LEFT JOIN Bancos B ON B.BanID = R.BanID " +
                "LEFT JOIN CuentasBancarias C on CuBID = R.BanID AND ForID = 4 " +
                "where RecSecuencia = ? and R.RepCodigo = ? "+(!string.IsNullOrWhiteSpace(recTipo) ? " and trim(upper(R.RecTipo)) = '"+recTipo.Trim().ToUpper()+"' " : "")+" AND ForID <> 3 order by RefSecuencia", 
                new string[] { RecSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<RecibosFormaPago> GetRecibosFormasPagoBySecuenciaYmonedas(int RecSecuencia, bool reciboConfirmado, string moncodigo)
        {
            string WhereMoncodigo = "";
            if (!string.IsNullOrWhiteSpace(moncodigo))
            {
                WhereMoncodigo = "AND R.Moncodigo = '" + moncodigo + "'";
            }

            return SqliteManager.GetInstance().Query<RecibosFormaPago>("select Distinct ifnull(CuBNombre, ifnull(ltrim(rtrim(BanNombre)), '')) as BanNombre, ForID, RefNumeroCheque, RefValor, " +
                "RefIndicadorDiferido, strftime('%d-%m-%Y', RefFecha) as RefFecha, RefNumeroAutorizacion, RecPrima, r.RecTasa from " + (reciboConfirmado ? "RecibosFormaPagoConfirmados" : "RecibosFormaPago") + " R " +
                "LEFT JOIN Bancos B ON B.BanID = R.BanID " +
                "LEFT JOIN CuentasBancarias C on CuBID = R.BanID AND ForID = 4 " +
                "where RecSecuencia = ? and RepCodigo = ? AND ForID <> 3 " + WhereMoncodigo + " order by RefSecuencia", new string[] { RecSecuencia.ToString(), Arguments.CurrentUser.RepCodigo });
        }

        public List<Recibos> GetRecibosByDeposito(int depSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia, RecMontoEfectivo , RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from Recibos r inner join Clientes c on c.CliID = r.CliID where DepSecuencia = " + depSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'  AND RecEstatus <> 0 " +
                "union select RecSecuencia, RecMontoEfectivo , RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from RecibosConfirmados r inner join Clientes c on c.CliID = r.CliID " +
                "where DepSecuencia = " + depSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' AND RecEstatus <> 0 order by RecSecuencia", new string[] { });
        }
        
  
        public List<Recibos> GetRecibosByDepositoToShowAnulado(int depSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia, RecMontoEfectivo , RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from Recibos r inner join Clientes c on c.CliID = r.CliID where DepSecuencia = " + depSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                "union select RecSecuencia, RecMontoEfectivo , RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from RecibosConfirmados r inner join Clientes c on c.CliID = r.CliID " +
                "where DepSecuencia = " + depSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' order by RecSecuencia", new string[] { });
        }

        public List<RecibosAplicacion> GetRecibosFacturasByDeposito(int recsecuencia)
        {
            return SqliteManager.GetInstance().Query<RecibosAplicacion>("SELECT CXCReferencia, CxcDocumento , RecDescuento FROM RecibosAplicacion WHERE RecSecuencia = " + recsecuencia + " AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " +
                "union select CXCReferencia, CxcDocumento, RecDescuento FROM RecibosAplicacionConfirmados WHERE RecSecuencia = " + recsecuencia + " AND RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' ", new string[] { });
        }

        public List<Recibos> GetRecibosByDepositoAnulados(int depSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia, RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from Recibos r inner join Clientes c on c.CliID = r.CliID where DepSecuencia = " + depSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'  AND RecEstatus = 0 " +
                "union select RecSecuencia, RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from RecibosConfirmados r inner join Clientes c on c.CliID = r.CliID " +
                "where DepSecuencia = " + depSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'  AND RecEstatus = 0 order by RecSecuencia", new string[] { });
        }

        public List<Recibos> GetRecibosByCuadreAnulados(int cuaSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia, RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from Recibos r inner join Clientes c on c.CliID = r.CliID where CuaSecuencia = " + cuaSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'  AND RecEstatus = 0 " +
                "union select RecSecuencia, RecTotal, RecEstatus, RecTipo, RecMontoTransferencia, RecRetencion, c.CliID, CliNombre, c.CliCodigo " +
                "from RecibosConfirmados r inner join Clientes c on c.CliID = r.CliID " +
                "where CuaSecuencia = " + cuaSecuencia + " and ltrim(rtrim(r.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "'  AND RecEstatus = 0 order by RecSecuencia", new string[] { }).ToList();
        }
        public double GetMontoDescuentoTotalInTemp()
        {
            try
            {
                List<RecibosDocumentosTemp> list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("SELECT SUM(Descuento) as Descuento FROM RecibosDocumentosTemp WHERE Origen = '1' and UPPER(Estado) in ('SALDO' " + (myParametro.GetParDescuentoAbonos() ? ",'ABONO'" : "") + " )", new string[] { });

                if (list != null && list.Count > 0)
                {
                    return list[0].Descuento;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        private double GetRecibosDescuento(int recSecuencia, string recTipo, bool confirmado)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<RecibosAplicacion>("SELECT SUM(ifnull(RecDescuento, 0.0)) as RecDescuento FROM "+(confirmado?"RecibosAplicacionConfirmados":"RecibosAplicacion")+" " +
                    "WHERE RecSecuencia = ? and RepCodigo = ? and trim(upper(RecTipo)) = ?", 
                    new string[] { recSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim(), recTipo.Trim().ToUpper() });

                if (list != null && list.Count > 0)
                {
                    return list[0].RecDescuento;
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public double GetMontoNCTotalAplicado()
        {
            try
            {
                List<DocumentosAplicadosTemp> list = SqliteManager.GetInstance().Query<DocumentosAplicadosTemp>("select Sum(ValorAplicado) as ValorAplicado from DocumentosAplicadosTemp", new string[] { });

                if (list != null && list.Count > 0)
                {
                    return Math.Abs(list[0].ValorAplicado);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        private double GetRecibosMontoNCAplicadas(int recSecuencia, string recTipo, bool confirmado)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<RecibosFormaPago>("select Sum(abs(ifnull(RefValor, 0.0))) as RefValor from " + (confirmado?"RecibosFormaPagoConfirmados":"RecibosFormaPago") + " " +
                    "where ForID = 3 and RecSecuencia = ? and trim(upper(RecTipo)) = ? and RepCodigo = ?", 
                    new string[] { recSecuencia.ToString(), recTipo.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim() });

                if (list != null && list.Count > 0)
                {
                    return Math.Abs(list[0].RefValor);
                }

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return 0;
        }

        public bool HayNotasCreditoFraccionadasConPendiente()
        {
            return SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select 1 as ValorAplicado from RecibosDocumentosTemp where Origen = -1 and ltrim(rtrim(Estado)) = 'Aplicada' and abs(Pendiente) > 0", new string[] { }).Count > 0;
        }

        public bool PermiteRecibosSplitNotasDeCredito()
        {
            var list = SqliteManager.GetInstance().Query<TiposTransaccionesCXC>("select upper(ifnull(t.ttcCaracteristicas, '')) as ttcCaracteristicas from TiposTransaccionesCxc t " +
                "where t.ttcSigla='NC' and (upper(ifnull(t.ttcCaracteristicas, '')) like '%P%' or upper(ifnull(t.ttcCaracteristicas, '')) like '%S%') limit 1", new string[] { });

            return list != null && list.Count > 0;
        }

        public bool HayNotasCreditosAplicadas()
        {
            return SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select 1 as ValorAplicado from RecibosDocumentosTemp where Origen = -1 and ltrim(rtrim(Estado)) = 'Aplicada'", new string[] { }).Count > 0;
        }

        public RecibosAplicacion GetReciboAplicacionByCxcDocumento(string cxcDocumento, bool confirmado)
        {
            var list = SqliteManager.GetInstance().Query<RecibosAplicacion>("select RecSecuencia from " + (confirmado ? "RecibosAplicacionConfirmados" : "RecibosAplicacion") + " where ltrim(rtrim(upper(CxcDocumento))) = ?", new string[] { cxcDocumento.Trim().ToUpper() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<Sectores> GetSociedadesByRecibosSinDepositar(string monCodigo = null)
        {
            string whereMonCodigo = "";

            if (!string.IsNullOrWhiteSpace(monCodigo))
            {
                whereMonCodigo = " and upper(trim(r.MonCodigo)) = trim(upper('" + monCodigo + "')) ";
            }

            var query = "select ifnull(MonCodigo, '') as MonCodigo, ifnull(substr(AreaCtrlCredit,1,2), '') as SecCodigo, substr(AreaCtrlCredit, 1,2) || '                  ' || ifnull(MonCodigo, '') || '        ' || min(RecSecuencia) || '-' ||  max(RecSecuencia) as SecDescripcion, min(RecSecuencia) || '-' ||  max(RecSecuencia) as LipCodigo from ( " +
                    "SELECT  distinct r.AreaCtrlCredit as AreaCtrlCredit, RecibosFormaPago.MonCodigo as MonCodigo, r.RecSecuencia as RecSecuencia " +
                    "FROM Recibos r " +
                    "inner join RecibosFormaPago on r.RecSecuencia =  RecibosFormaPago.RecSecuencia " +
                    "Where (r.DepSecuencia = 0 or r.DepSecuencia is null) " + whereMonCodigo + " and trim(r.repcodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "union all " +
                    "SELECT  distinct r.AreaCtrlCredit as AreaCtrlCredit, RecibosFormaPagoConfirmados.MonCodigo as MonCodigo, r.RecSecuencia as RecSecuencia " +
                    "FROM RecibosConfirmados r " +
                    "inner join RecibosFormaPagoConfirmados on r.RecSecuencia =  RecibosFormaPagoConfirmados.RecSecuencia " +
                    "Where (r.DepSecuencia = 0 or r.DepSecuencia is null) " + whereMonCodigo + " and trim(r.repcodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    ") tabla GROUP by AreaCtrlCredit ";

            return SqliteManager.GetInstance().Query<Sectores>(query, new string[] { });
        }


        public void InsertReciboInTemp(int transaccionID, bool forEditing = false)
        {
            ClearTemps();

            var query = "Select RecFecha as Fecha ,CxcDocumento as Documento,CxcReferencia as Referencia, ifnull(CxcSigla, '') as Sigla, " + (forEditing ? "ra.RecValor as Aplicado" : "r.RecTotal as Aplicado") + " ,RecDescuento as Descuento, " +
                " r.RecTotal as Balance,1 as Origen,r.RecTotal as MontoSinItbis, ra.RecPorcDescuento as DescPorciento, ra.AutID as AutSecuencia,'Saldo' as Estado, 0 as Pendiente, 1 as CalcularDesc,ra.DefIndicadorItbis as DefIndicadorItbis, " +
                " ifnull(replace(cast(julianday('now', '" + Functions.GetDiferenciaHorariaSqlite() + " hours') - julianday(replace(RecFecha, 'T', ' ')) as integer),' ', ''), '') as Dias, ra.CxcReferencia as CXCNCF, r.MonCodigo as MonCodigo, ifnull(r.recTasa, 0) as Tasa from recibos r " +
                "Inner Join recibosAplicacion ra on r.RecSecuencia = ra.RecSecuencia and r.Repcodigo = ra.RepCodigo Where r.RepCodigo = ? and r.RecSecuencia = ? and RecEstatus=3 ";

            var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), transaccionID.ToString() });

            SqliteManager.GetInstance().InsertAll(list);
        }

        public bool GetReciboTieneNCPorDpp(int recSecuencia)
        {
            return SqliteManager.GetInstance().Query<Recibos>("select RecSecuencia from NCDPP " +
                "where trim(RepCodigo) = ? and RecSecuencia = ? limit 1", new string[] { Arguments.CurrentUser.RepCodigo.Trim(), recSecuencia.ToString() }).Count > 0;
        }



        public List<NCDPP> GetNCDppRecibos(int recSecuencia)
        {
            string query = "select NCDSecuencia, RecTipo, CxcDocumento, NCDMonto, NCDItbis, NCDNCF, c.CliNombre as CliNombre, c.CliCodigo as CliCodigo ,RecSecuencia , CxCNCFAfectado, NCDFecha " +
           "from NCDPP n " +
           "inner join Clientes c on c.CliID = n.CliID " +
           "where n.RecSecuencia = ? and trim(n.RepCodigo) = ? ";

            return SqliteManager.GetInstance().Query<NCDPP>(query, new string[] { recSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });
        }


        public NCDPP GetNCDppRecibosPdf(int recSecuencia, int ncdSecuencia)
        {
            List<NCDPP> list = SqliteManager.GetInstance().Query<NCDPP>("select NCDSecuencia, RecTipo, n.CxcDocumento as CxcDocumento, NCDMonto, NCDItbis, NCDNCF, c.CliNombre as CliNombre, c.CliCodigo as CliCodigo ,RecSecuencia , CxCNCFAfectado, NCDFecha " +
            "from NCDPP n " +
            "inner join Clientes c on c.CliID = n.CliID " +
            "where n.RecSecuencia = ? and n.NCDSecuencia = ? and trim(n.RepCodigo) = ? ", new string[] { recSecuencia.ToString(), ncdSecuencia.ToString(), Arguments.CurrentUser.RepCodigo.Trim() });


            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }



        public int GetOPformasPago(int recSecuencia)
        {
            List<FormasPagoTemp> list = SqliteManager.GetInstance().Query<FormasPagoTemp>("select max(cast(ifnull(RefSecuencia, 0) as Integer)) as RefSecuencia from recibosformapago Where RecSecuencia = ?", new string[] { recSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0].RefSecuencia;
            }

            return 0;
        }


        public RecibosFormaPago GetForID(int recSecuencia, string rectipo, bool reciboConfirmado = false)
        {
            List<RecibosFormaPago> list = SqliteManager.GetInstance().Query<RecibosFormaPago>("select ForID from "+ (reciboConfirmado ? "RecibosFormaPagoConfirmados" : "RecibosFormaPago") +" Where RecSecuencia = ? And RecTipo = ?", new string[] { recSecuencia.ToString(), rectipo });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public List<Monedas> GetMonedasFromRecibos(int recSecuencia, bool confirmado)
        {
            string sql = "Select Distinct r.MonCodigo, MonNombre,MonTasa from "+(confirmado? " RecibosFormaPagoConfirmados " : "RecibosFormaPago") + " R " +
                       "  Inner Join Monedas m On R.Moncodigo = m.Moncodigo Where RecSecuencia = ? ";
            var list = SqliteManager.GetInstance().Query<Monedas>(sql, new string[] { recSecuencia.ToString() });


            return list;
        }

        public Monedas MonedasGetTasaFromRecibos(int recSecuencia, bool confirmado)
        {
            string sql = "Select Distinct r.MonCodigo, MonNombre,r.RecTasa as MonTasa from " + (confirmado ? " RecibosConfirmados " : "Recibos") + " R " +
                       "  Inner Join Monedas m On R.Moncodigo = m.Moncodigo Where RecSecuencia = ? ";
            var list = SqliteManager.GetInstance().Query<Monedas>(sql, new string[] { recSecuencia.ToString() });

            if (list != null && list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

        public int GetRecTipo(int recSecuencia)
        {
            string sql = "Select RecTipo from RecibosDocumentosTemp " +
                       " Where RecSecuencia = " + recSecuencia;
            var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(sql, new string[] { });

            int RecTipo = 2;
            foreach (var rectipo in list)
            {
                RecTipo = rectipo.RecTipo;
            }

            return RecTipo;
        }

        public void UpdateRefValorRecDocTemp(int RefSecuencia, int RecSecuencia)
        {
            string query = "Update RecibosDocumentosTemp set Aplicado = ifnull((Select RefValor from RecibosFormaPago where RefSecuencia = ? and RecSecuencia = ?), 0.0) ";
            SqliteManager.GetInstance().Execute(query, new string[] { RefSecuencia.ToString(), RecSecuencia.ToString() });
        }

        public List<RecibosDocumentosTemp> GetNotaCreditoAutomaticaParaFactura(string Documento, string NCF )
        {
             
            string sql = "Select cxcDocumento as Documento, cxcBalance as Balance, cxcBalance as Pendiente, cxcReferencia as Referencia, cxcMontoSinItbis as MontoSinItbis, cxcMontoTotal as MontoTotal, CXCNCFAfectado as CXCNCFAfectado  from CuentasXCobrar where CXCNCFAfectado = '" + NCF + "' and cxcsigla = 'NC' and CliId = " + Arguments.Values.CurrentClient.CliID + " /*and CXCDocumento = '" + Documento + "'*/";
            return SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(sql, new string[] { });//.FirstOrDefault();

        }

        public bool ifExistOldFact()
        {
            try
            {
                //var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select count(*), ifnull(Dias,0) as Dias from recibosDocumentosTemp where Estado = 'Saldo' and sigla = 'FAT' " +
                //            " and dias < (Select dias from RecibosDocumentosTemp where estado = 'Pendiente' and sigla = 'FAT')", new string[] { }).FirstOrDefault();

                var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select count(*), ifnull(Dias,0) as Dias from recibosDocumentosTemp where Estado in('Abono', 'Saldo') and sigla = 'FAT' " +
                           " and dias < (Select dias from RecibosDocumentosTemp where estado = 'Pendiente' and sigla = 'FAT')", new string[] { }).FirstOrDefault();

                if (list.Dias > 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        public double GetAplicadoInTemp(string referencia, string documento)
        {
            try
            {

                var item = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select Aplicado from RecibosDocumentosTemp where Referencia = ? and Documento = ?", 
                    new string[] { referencia, documento }).FirstOrDefault();

                if(item != null)
                {
                    return item.Aplicado;
                }

            }catch(Exception){

            }

            return 0;
        }

        public double GetMontoDescuentoParaAbono(RecibosDocumentosTemp CurrentFactura, double PorcDescuento, double aplicado, bool saldando)
        {
            if (saldando)
            {
                var raw = (CurrentFactura.MontoSinItbis / CurrentFactura.MontoTotal);

                var balance = CurrentFactura.Balance;

                var desc = PorcDescuento / 100.0;

                var valorSinDesc = raw * balance;

                var result = valorSinDesc * desc;

                return Math.Round(result, 2);
            }
            else
            {

                var Monto = aplicado - (CurrentFactura.CxcNotaCredito + CurrentFactura.Credito);
                var raw = Monto / (1.0 - (PorcDescuento / 100.0));
                raw = raw * (PorcDescuento / 100.0);
                var porcItbis = CurrentFactura.MontoSinItbis / CurrentFactura.MontoTotal;//Math.Round(1.0 + (1.0 - (CurrentFactura.MontoSinItbis / CurrentFactura.MontoTotal)), 2);
                raw = raw * porcItbis;
                return Math.Round(raw, 2);

                //Se comento el calculo estaba mal
                //var Monto = aplicado - (CurrentFactura.CxcNotaCredito + CurrentFactura.Credito);
                //var porcItbis = (CurrentFactura.MontoSinItbis / CurrentFactura.MontoTotal);
                //var desc = PorcDescuento / 100.0;
                //var valorSinDesc = porcItbis * Monto;
                //var result = valorSinDesc * desc;
                //return Math.Round(result, 2);

                //Formula de referencia
                //var result = ((Monto / 1.18) / (Monto - (Monto * (PorcDescuento / 100.0)))) * (Monto * (PorcDescuento / 100.0));
            }
        }

        public bool GetChequesDiferidosSinDespositar()
        {

            DateTime datetoday = DateTime.Now;
            var item = SqliteManager.GetInstance().Query<Recibos>("Select r.RecSecuencia from Recibos r INNER JOIN RecibosFormaPago rfp ON r.RecSecuencia = rfp.RecSecuencia AND r.RepCodigo = rfp.RepCodigo " +
            "where rfp.RefIndicadorDiferido = 1 and RefFecha > '"+datetoday.ToString("yyyy-MM-dd")+"' and r.RecEstatus <> 0 and  r.Repcodigo = '" + Arguments.CurrentUser.RepCodigo + "' and r.Cliid = " + Arguments.Values.CurrentClient.CliID + " " +
            "UNION ALL " +
            "Select r.RecSecuencia from RecibosConfirmados r INNER JOIN RecibosFormaPago rfp ON r.RecSecuencia = rfp.RecSecuencia AND r.RepCodigo = rfp.RepCodigo " +
            "where rfp.RefIndicadorDiferido = 1 and RefFecha > '" + datetoday.ToString("yyyy-MM-dd") + "' and r.RecEstatus <> 0 and r.Repcodigo = '" + Arguments.CurrentUser.RepCodigo + "' and r.cliid = " + Arguments.Values.CurrentClient.CliID + " ",
                    new string[] {});
            return item.Count > 1;
        }

        public RecibosResumen GetResumenRecibos(int recSecuencia, bool Confirmados, string recTipo)
        {
            var result = new RecibosResumen();

            var list = SqliteManager.GetInstance().Query<RecibosFormaPago>("SELECT ifnull(SUM(cast(RecPrima as real)), 0.0) as RecPrima, ForID " +
                "FROM "+(Confirmados?"RecibosFormaPagoConfirmados":"RecibosFormaPago")+" where RecSecuencia = ? and ForID != 3 " +
                "and trim(upper(RecTipo)) = ? and RepCodigo = ? group by ForID", 
                new string[] { recSecuencia.ToString(), recTipo.Trim().ToUpper(), Arguments.CurrentUser.RepCodigo.Trim() });

            foreach (var rec in list)
            {
                switch (rec.FormaPago)
                {
                    case "Cheque":
                        result.Cheques = rec.RecPrima;
                        break;
                    case "Efectivo":
                        result.Efectivo = rec.RecPrima;
                        break;
                    case "Tarjeta de crédito":
                        result.TarjCredito = rec.RecPrima;
                        break;
                    case "Transferencia":
                        result.Transferencias = rec.RecPrima;
                        break;
                    case "Orden pago":
                        result.OrdenPago = rec.RecPrima;
                        break;
                    case "Retención":
                        result.Retencion = rec.RecPrima;
                        break;
                    case "Diferencia Cambiaria":
                        result.DiferenciaCambiaria = rec.RecPrima;
                        break;
                    case "Redondeo":
                        result.Redondeo = rec.RecPrima;
                        break;
                }
            }

            result.Descuentos = GetRecibosDescuento(recSecuencia, recTipo, Confirmados);
            result.NotasCredito = GetRecibosMontoNCAplicadas(recSecuencia, recTipo, Confirmados);
            result.Sobrante = Math.Abs(GetRecibosSobrante(recSecuencia, recTipo, Confirmados));
            result.Facturas = GetRecibosTotalDocumentos(recSecuencia, recTipo, Confirmados);

            return result;
        }

        public string GetComentarioRecibo(int recsecuencia)
        {
            try
            {
                var list = SqliteManager.GetInstance().Query<Mensajes>("SELECT MenDescripcion from Mensajes where TraSecuencia = " + recsecuencia + " " +
                    "and repcodigo = '" + Arguments.CurrentUser.RepCodigo + "' ", new string[] { });

                if (list.Count > 0 && list != null)
                {
                    return list[0].MenDescripcion;
                }
            }
            catch (Exception)
            {

            }

            return "";
        }

        public bool ExistAbonadoDiferido(string cxcReferencia)
        {
            var list = SqliteManager.GetInstance().Query<RecibosAplicacion>("Select 1 from RecibosAplicacion RA " +
                    "Inner join RecibosFormaPago RF ON RA.RecSecuencia = RF.RecSecuencia where RA.CxcReferencia = '" + cxcReferencia + "'  AND RF.RefIndicadorDiferido = 1 " +
                    "And RecIndicadorSaldo = 0 ", new string[] { });

            if (list.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool ExistsFormasDePagoDiferentesATarjetaCredito()
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ForID != 6", new string[] { }).Count > 0;
        }

        public Clientes GetClientesForRepcodigo(string clicodigo)
        {
            return SqliteManager.GetInstance().Query<Clientes>("SELECT Repcodigo from Clientes WHERE cliid ='" + clicodigo + "' ", new string[] { }).FirstOrDefault();
        }

        public bool ExistsFormaTarjetaCredito(bool sameDate = true, string fecha = null)
        {
            return SqliteManager.GetInstance().Query<FormasPagoTemp>("select 1 as Valor from FormasPagoTemp where ForID = 6 ", new string[] { }).Count > 0;
        }

        public Monedas GetMonedaReciboOrdenPago(int transaccionID, int Tasa = 1)
        {
            var query = "select m.MonCodigo, m.MonNombre, m.MonSigla, "+(Tasa == 1 ? " m.MonTasa as MonTasa " : " r.RecTasa as MonTasa " )+" from Recibos r " +
                "inner join Monedas m on trim(upper(m.MonCodigo)) = trim(upper(r.MonCodigo))  " +
                "Inner Join recibosAplicacion ra on r.RecSecuencia = ra.RecSecuencia and r.Repcodigo = ra.RepCodigo Where r.RepCodigo = ? and r.RecSecuencia = ? and RecEstatus=3 ";

            var moneda = SqliteManager.GetInstance().Query<Monedas>(query, new string[] { Arguments.CurrentUser.RepCodigo.Trim(), transaccionID.ToString() });


            return moneda[0];
        }

        public double GetMaxTasaAplicada(string referencia)
        {
            try
            {

                var item = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select max(Tasa) as Tasa from RecibosDocumentosTemp  ",
                    new string[] {}).FirstOrDefault();

                if (item != null)
                {
                    return item.Tasa;
                }

            }
            catch (Exception)
            {

            }

            return 0;
        }

        public bool ReciboTieneAplicaciones(int RecSecuencia)
        {
            var item = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>("select RecSecuencia from RecibosAplicacion " +
                "where RecSecuencia = ? ",
                new string[] { RecSecuencia.ToString() });

            if (item != null && item.Count > 0)
            {
                return true;
            }

            return false;
        }

        public double GetRecibosAplicacionByRecValor(int RecSecuencia, bool ReciboConfirmados, string recTipo = null, bool withNC = false)
        {
            string query = "select sum(a.RecValor) " +
               "from " + (ReciboConfirmados ? "RecibosAplicacionConfirmados a " : "RecibosAplicacion a ") + "LEFT JOIN CuentasXCobrar cc ON a.CXCReferencia = cc.CXCReferencia " +
               "where a.RecSecuencia = " + RecSecuencia.ToString() + " and a.RepCodigo = '" + Arguments.CurrentUser.RepCodigo + "' " + (!string.IsNullOrWhiteSpace(recTipo) ? " and trim(upper(a.RecTipo)) = '" + recTipo.Trim().ToUpper() + "' " : "") + (!withNC ? " and a.CxCSigla <> 'NC' " : "");

            RecibosAplicacion result = SqliteManager.GetInstance().Query<RecibosAplicacion>(query,
               new string[] { }).FirstOrDefault();

            return result != null? result.RecValor : 0;
        }

        public double GetMontoPendienteInTemp()
        {
            var sql = "select sum(ifnull(Pendiente, 0.0)) as Pendiente from RecibosDocumentosTemp " +
                "where Origen = '1' and cast(Pendiente as REAL) > 0.0 and trim(Estado) !='Saldo'";

            var result = 0.0;

            var list = SqliteManager.GetInstance().Query<RecibosDocumentosTemp>(sql, new string[] { });

            if(list != null && list.Count > 0)
            {
                result = list[0].Pendiente;
            }

            return result;
        }
        public string getNombreEstatus(int RecEstado)
        {
           string query = "Select estdescripcion from estados where UPPER(EstTabla) = UPPER('RECIBOS') and estestado = '" + RecEstado + "'";
           var list = SqliteManager.GetInstance().Query<Estados>(query, new string[] { });
           return list != null && list.Count > 0 ? list[0].EstDescripcion : "";
        }

        public string CrearNoReferencia(string RepCodigo, int Secuencia)
        {
            string Rep, CuaSec;
            Rep = RepCodigo;
            CuaSec = RellenaCeros(5, Secuencia);
            return (Rep + CuaSec).ToString();
        }

        public string RellenaCeros(int Ceros, int Numero)
        {
            string R = "";
            int C = 0;
            C = Ceros - Numero.ToString().Length;
            if (C < 0)
            {
                return "0";
            }
            for (int i = 1; i <= C; i++)
            {
                R = R + "0";
            }
            R = R + Numero.ToString();

            return R;
        }
        public bool HayRecibosSinDepositarExcedenHoras(double horas)
        {
            if (horas > 0)
            {
                DateTime dt = DateTime.Now.Date;
                var dt1 = String.Format("{0:s}", dt);
                var sql = "select r.RecSecuencia, rf.RefFecha from Recibos r " +
                    "Inner join RecibosFormaPago rf on r.rectipo = rf.rectipo and r.RecSecuencia = rf.RecSecuencia and r.RepCodigo = rf.RepCodigo " +
                    "where trim(r.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and ifnull(DepSecuencia, 0) = 0 and ((rf.RefIndicadorDiferido = 1 AND (CAST(replace(DATE(rf.RefFecha), '-', '') AS INTEGER) <= CAST(replace('" + dt1 + "', '-', '') AS INTEGER)) ) OR rf.RefIndicadorDiferido<>1) " +
                    "union all " +
                    "select r.RecSecuencia, rfc.RefFecha " +
                    "from RecibosConfirmados r Inner join RecibosFormaPagoconfirmados rfc on r.rectipo = rfc.rectipo " +
                    " and r.RecSecuencia = rfc.RecSecuencia and r.RepCodigo = rfc.RepCodigo " +
                    "where trim(r.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo.Trim() + "' " +
                    "and ifnull(DepSecuencia, 0) = 0 and ((rfc.RefIndicadorDiferido = 1 AND (CAST(replace(DATE(rfc.RefFecha), '-', '') AS INTEGER) <= CAST(replace('" + dt1 + "', '-', '') AS INTEGER)) ) OR rfc.RefIndicadorDiferido<>1) ";
                
                var list = SqliteManager.GetInstance().Query<Recibos>(sql, new string[] { });

                list = list.Where(p => DateTime.Parse(p.RefFecha).Date < DateTime.Now.Date).ToList();
                foreach (var rec in list)
                {
                    var datetimer = DateTime.Parse(rec.RefFecha).Date;
                    datetimer = datetimer.AddHours(23).AddMinutes(59);
                    TimeSpan time = DateTime.Now - datetimer;
                    if (Math.Abs(time.TotalHours) >= horas)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void DeletePairValuesInRecibos()
        {
            SqliteManager.GetInstance().Execute("DELETE from RecibosAplicacion where upper(rowguid) in (SELECT upper(rowguid) from RecibosAplicacionConfirmados)");
            SqliteManager.GetInstance().Execute("DELETE from RecibosFormaPago where upper(rowguid) in (SELECT upper(rowguid) from recibosformapagoconfirmados)");
            SqliteManager.GetInstance().Execute("DELETE from recibos where upper(rowguid) in (SELECT upper(rowguid) from RecibosConfirmados)");
        }

        public bool GetFormasPagoAgregadasForValid(out string[] recnosobrante)
        {
            recnosobrante = myParametro.GetParRecibosNoSobrante();

            //if (recnosobrante.Length <= 1)
            //    return true;

            string result = "";
            int num = 0;
            int recsobranteresult = recnosobrante.Skip(1).ToArray().Length;
            foreach (var item in recnosobrante.Skip(1))
            {
                num++;
                if(num == recsobranteresult)                
                    result += item;                
                else
                    result += item + ",";
            }

            var list = SqliteManager.GetInstance().Query<FormasPagoTemp>
                ($"select 1 from FormasPagoTemp where ForID in({result})");

            return SqliteManager.GetInstance().Query<FormasPagoTemp>
                ($"select 1 from FormasPagoTemp")?.Count > 1 ? list?.Count == recsobranteresult : list?.Count > 0;
        }

        public void UpdateDateRec(string Fecha)
        {
            SqliteManager.GetInstance().Execute("update RecibosDocumentosTemp set FechaChequeDif = '" + Fecha + "'");
        }
        
        public Recibos GetRecFormPago(int recsecuencia)
        {
           return SqliteManager.GetInstance().Query<Recibos>("SELECT RecMontoEfectivo,RecMontoCheque, RecMontoChequeF, RecMontoTransferencia from Recibos where recsecuencia = " + recsecuencia + "" +
               " union " +
               "SELECT RecMontoEfectivo,RecMontoCheque, RecMontoChequeF, RecMontoTransferencia from RecibosConfirmados where recsecuencia = " + recsecuencia + " ").FirstOrDefault();
        }

    }
}
