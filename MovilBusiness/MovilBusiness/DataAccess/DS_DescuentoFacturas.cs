
using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovilBusiness.DataAccess
{
    public class DS_DescuentoFacturas : DS_Controller
    {
        private DS_TiposTransaccionesCXC myTipTran;
        private DS_CuentasxCobrar myCxc;
        private static DS_DescuentoFacturas Instance;
        public DS_DescuentoFacturas()
        {
            myTipTran = new DS_TiposTransaccionesCXC();
            myCxc = new DS_CuentasxCobrar();
        }

        public static DS_DescuentoFacturas GetInstance()
        {
            if (Instance == null)
            {
                Instance = new DS_DescuentoFacturas();
            }

            return Instance;
        }

        //este metodo retorna el montototal de descuento de una factura, si quieres que este monto venga calculado tomando en cuenta una NC aun sin aplicar le pones los dos ultimos parametros, otherside null
        public DescFactura GetMontoDescuentoFactura(double ForceDescPorciento, RecibosDocumentosTemp Factura) { return GetMontoDescuentoFactura(Factura, -1, ForceDescPorciento, -1); }
        public DescFactura GetMontoDescuentoFactura(RecibosDocumentosTemp Factura) { return GetMontoDescuentoFactura(Factura, -1, -1, -1); }
        public DescFactura GetMontoDescuentoFactura(RecibosDocumentosTemp Factura, bool fromAbono) { return GetMontoDescuentoFactura(Factura, -1, -1, -1, fromAbono); }
        public DescFactura GetMontoDescuentoFactura(RecibosDocumentosTemp Factura, /*RecibosDocumentosTemp NCAplicar,*/ double ValorAplicarNC) { return GetMontoDescuentoFactura(Factura, -1, -1, ValorAplicarNC); }
        public DescFactura GetMontoDescuentoFactura(RecibosDocumentosTemp Factura, int Dias) { return GetMontoDescuentoFactura(Factura, Dias, -1, -1); }
        public DescFactura GetMontoDescuentoFactura(RecibosDocumentosTemp Factura, int Dias, /*RecibosDocumentosTemp NCAAplicar,*/ double ForceDescPorciento, double ValorAplicarNC, bool fromAbono = false, bool isForAgregarFormaPago = false)
        {
            if(Arguments.Values.CurrentModule == Enums.Modules.RECONCILIACION)
            {
                return new DescFactura();
            }

            if (!myTipTran.GetTipoTransaccionAplicaDescuento(Factura.Sigla))
            {
                return new DescFactura();
            }

            var ncaplicaparaDescuento = myTipTran.GetTipoTransaccionAplicaDescuento("NC");

            var NotasCreditosAplicadas = new DS_Recibos().GetNotasCreditoAplicadasByFactura(Factura.Referencia);

            if (Dias != -1)
            {
                Factura.Dias = Dias;
            }

            double MontoNcAplicadas = 0;
            double MontoNcAplicadasSinItbis = 0;

            foreach (var nc in NotasCreditosAplicadas)
            {
                MontoNcAplicadas += Math.Abs(nc.ValorAplicado);
                MontoNcAplicadasSinItbis += Math.Abs(nc.MontoSinItbisNC);
            }

            if (myParametro.GetParDescuentoManual() <= 0 && !myParametro.GetParRecibosDescuentoFacturasSemiAutomatico() && myTipTran.ExistsSigla(Factura.Sigla))
            {
                //var list = GetDescuentoFactura(Factura.Referencia, Factura.Dias, Factura.DescPorciento, MontoNcAplicadas, Factura.AutSecuencia, isForAgregarFormaPago);
                var list = GetDescuentoFactura(Factura.Referencia, Factura.DiasChequeDif, Factura.DescPorciento, MontoNcAplicadas, Factura.AutSecuencia, isForAgregarFormaPago);

                if (list.Count == 0)
                {
                    return new DescFactura();
                }

                var Descuento = list[0];
                if (Descuento.DeFPorciento == 0 && Descuento.DefDescuento == 0)
                {
                    return new DescFactura();
                }

                if (ValorAplicarNC != -1)
                {
                    Descuento.DeFPorciento = Factura.DescPorciento;
                }

                if (ForceDescPorciento != -1)
                {
                    Descuento.DeFPorciento = ForceDescPorciento;
                }

                double Monto;
                if (fromAbono)
                {
                    Monto = Factura.Aplicado - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + MontoNcAplicadas);
                }
                else
                {
                    if (!Descuento.DefIndicadorItbis && !Factura.DefIndicadorItbis)
                    {
                        if (MontoNcAplicadas > 0)
                        {
                            if (ValorAplicarNC == -1)
                                ValorAplicarNC = 0;
                        }
                        if (!ncaplicaparaDescuento)
                        {
                            Monto = Factura.MontoSinItbis;
                        }
                        else
                        {
                            //Monto = Factura.MontoSinItbis - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + MontoNcAplicadas);
                            Monto = Factura.MontoSinItbis - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + MontoNcAplicadasSinItbis);
                        }

                        /*if (MontoNcAplicadas > 0 && ValorAplicarNC == 0)
                        {
                            ValorAplicarNC = -1;
                        }*/
                        // Monto = myParametro.GetParDescuentoAbonos() ? Factura.Balance : Factura.MontoSinItbis - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + Math.Abs(Factura.CxcNotaCredito) + MontoNcAplicadas);
                    }
                    else
                    {
                        if (!ncaplicaparaDescuento)
                        {
                            Monto = Factura.MontoSinItbis;
                        }
                        else
                        {
                            Monto = Factura.MontoTotal - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + MontoNcAplicadas);
                        }
                    }
                }


                double MontoDescuento;
                if (Monto > 0)
                {
                    if (Descuento.DeFPorciento == 0)
                    {

                        MontoDescuento = Descuento.DefDescuento;
                        Descuento.DeFPorciento = Math.Round((Descuento.DefDescuento / Monto) * 100, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        MontoDescuento = myParametro.GetParDescuentoAbonos() && fromAbono ? (Monto / (1 - (Descuento.DeFPorciento / 100)) * (Descuento.DeFPorciento / 100)) * (Factura.MontoSinItbis / Factura.MontoTotal) : Monto * (Descuento.DeFPorciento / 100);
                        //  MontoDescuento = myParametro.GetParDescuentoAbonos() ? ((Factura.MontoSinItbis / Factura.MontoTotal) * Monto) * (Descuento.DeFPorciento / 100) : Monto * (Descuento.DeFPorciento / 100);
                        //MontoDescuento = Monto * (Descuento.DeFPorciento / 100);
                    }
                }
                else
                {
                    MontoDescuento = 0;
                    Descuento.DeFPorciento = 0;
                }

                DescFactura desc = new DescFactura
                {
                    DescuentoValor = Descuento.DefDescuento > 0 ? Descuento.DefDescuento : Math.Abs(MontoDescuento),
                    DescPorciento = Descuento.DeFPorciento,
                    IndicadorItbis = Descuento.DefIndicadorItbis || Factura.DefIndicadorItbis,
                    DefDescuento = Descuento.DefDescuento,
                };

                return desc;

            }
            else if (myParametro.GetParDescuentoManual() > 0 && (Factura.DescPorciento > 0 || ForceDescPorciento != -1))
            {
                //Se agrega para validar que el descuento manual esta dentro de fecha si es con cheque futurista
                var list = GetDescuentoFactura(Factura.Referencia, Factura.DiasChequeDif, 0, MontoNcAplicadas, Factura.AutSecuencia, isForAgregarFormaPago);

                if (list.Count == 0)
                {
                    return new DescFactura();
                }

                if (ValorAplicarNC == -1)
                    ValorAplicarNC = 0;

                double Monto;
                if (!Factura.DefIndicadorItbis && !Factura.DefIndicadorItbis)
                {
                    //Monto = Factura.MontoSinItbis - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + MontoNcAplicadas);
                    Monto = Factura.MontoSinItbis - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + MontoNcAplicadasSinItbis);
                }
                else
                {
                    Monto = Factura.MontoTotal - (ValorAplicarNC == -1 ? 0 : ValorAplicarNC + MontoNcAplicadas);
                }

                var porciento = Factura.DescPorciento;

                if (ForceDescPorciento != -1)
                {
                    porciento = ForceDescPorciento;
                }

                double MontoDescuento;
                if (Monto > 0)
                {
                    if (porciento == 0)
                    {
                        MontoDescuento = 0;
                    }
                    else
                    {
                        MontoDescuento = Monto * (porciento / 100);
                    }
                }
                else
                {
                    MontoDescuento = 0;
                    porciento = 0;
                }

                DescFactura desc = new DescFactura
                {
                    DescuentoValor = Math.Abs(MontoDescuento),
                    DescPorciento = porciento,
                    IndicadorItbis = Factura.DefIndicadorItbis
                };

                return desc;
            }
            
            return new DescFactura();
        }

        private List<DescuentoFacturas> GetDescuentoFactura(string Referencia, int dias, double DescAplicado, double MontoNcAplicadas = 0, int AutSecuencia = 0, bool isForAgregarFormaPago = false)
        {
            string DescuentoAplicado = "Union all Select DescPorciento, DefIndicadorItbis,0 as DeFDescuento From RecibosDocumentosTemp where Referencia = '" + Referencia + "' ";

            var query = "select DeFPorciento, DefIndicadorItbis, DeFDescuento from DescuentoFacturas " +
           "where CXCReferencia = ? and " + dias.ToString() + " between cast(DeFDiaInicial as integer) and cast(DeFDiaFinal as integer)" +
           " " + (DescAplicado > 0 && (MontoNcAplicadas == 0 || AutSecuencia > 0) ? DescuentoAplicado : "") + " "+(isForAgregarFormaPago ? " order by DeFDiaFinal DESC " : " order by DeFPorciento DESC ") +" ";

            return SqliteManager.GetInstance().Query<DescuentoFacturas>(query, new string[] { Referencia });
        }

        public List<double> GetPorcientoDescuentoDisponible(string Referencia, int dias)
        {
            var result = new List<double>();
            try
            {
                var list = SqliteManager.GetInstance().Query<DescuentoFacturas>("select distinct DeFPorciento from DescuentoFacturas " +
                "where CXCReferencia = ? and (" + dias.ToString() + " < cast(DeFDiaInicial as integer)  or " + dias.ToString() + " <= cast(DeFDiaFinal as integer))  order by DefDiaInicial", new string[] { Referencia });

                if (list != null)
                {
                    foreach (var desc in list)
                    {
                        result.Add(desc.DeFPorciento);
                    }
                }
            }

            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return result;
        }

        public int GetIndicadorItbisDescuentoDisponible(string Referencia, int dias)
        {
            bool result = false;
            try
            {
                var list = SqliteManager.GetInstance().Query<DescuentoFacturas>("select DefIndicadorItbis from DescuentoFacturas " +
                "where CXCReferencia = ? and " + dias.ToString() + " BETWEEN DeFDiaInicial and DeFDiaFinal  order by DefDiaInicial", new string[] { Referencia });

                if (list != null)
                {
                   result = list[0].DefIndicadorItbis;
                }
            }

            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return result ? 1 : 0;
        }

        public List<double> GetPorcientoDescuentoPin(string cxcReferencia)
        {
            var result = new List<double>();

            var list = SqliteManager.GetInstance().Query<DescuentoFacturas>("select distinct DeFPorciento from DescuentoFacturas where ltrim(rtrim(CXCReferencia)) = ? " +
                "order by DeFPorciento desc", new string[] { cxcReferencia.Trim() });

            double porciento = 0;

            if (list != null && list.Count > 0)
            {
                porciento = (double)list.Max(x => x.DeFPorciento);
            }

            var porcientoParametro = myParametro.GetParRecibosPorcientoDescuentoDisponibleParaFacturas();

            if (porcientoParametro > 0)
            {
                switch (porcientoParametro)
                {
                    case 1:
                        for (int i = 0; i <= porciento; i++)
                        {
                            result.Add(i);
                        }
                        break;
                    case 2:
                        for (int i = (int)porciento; i >= 0; i--)
                        {
                            result.Add(i);
                        }
                        break;
                    case 3:
                        for (int i = (int)porciento; i >= 0; i--)
                        {
                            result.Add(i);
                        }

                        if ((int)porciento > 0)
                        {
                            result = result.OrderByDescending(x => x).ToList();
                            if (!result.Contains(myParametro.GetDescuentoManualAdicional()))
                            {
                                result.Add(myParametro.GetDescuentoManualAdicional());
                            }

                            if (!result.Contains(myParametro.Get2doDescuentoManualAdicional()))
                            {
                                result.Add(myParametro.Get2doDescuentoManualAdicional());
                            }
                        }
                        
                        break;
                }

            }
            else
            {
                if (list != null)
                {
                    foreach (var desc in list)
                    {
                        result.Add((int)desc.DeFPorciento);
                    }
                }
            }

            return result;

        }

        public List<double> GetPorcientoDescuentoPinDou(string cxcReferencia)
        {
            var result = new List<double>();

            var list = SqliteManager.GetInstance().Query<DescuentoFacturas>("select distinct DeFPorciento from DescuentoFacturas where ltrim(rtrim(CXCReferencia)) = ? " +
                "order by DeFPorciento desc", new string[] { cxcReferencia.Trim() });

            double porciento = 0;

            if (list != null && list.Count > 0)
            {
                porciento = list.Max(x => x.DeFPorciento);
            }

                if (list != null)
                {
                    foreach (var desc in list)
                    {
                        result.Add(desc.DeFPorciento);
                    }
                }

            return result;

        }

        public bool IsValidDescuentoFactura(string Referencia, int dias)
        {
            var query = "select 1 from DescuentoFacturas " +
           "where CXCReferencia = ? and " + dias.ToString() + " between cast(DeFDiaInicial as integer) and cast(DeFDiaFinal as integer)";

            return SqliteManager.GetInstance().Query<DescuentoFacturas>(query, new string[] { Referencia }).Count > 0;
        }

        public List<DescuentoFacturas> GetDescuentoFacturaByCXCReferencia(string Referencia, double cxcBalance, double montoADescuento = 0.0)
        {
            var query = $"select DeFDiaInicial, DeFDiaFinal, Case When ifnull(DefDescuento,0) = 0  Then round(({montoADescuento} * ifnull(DeFPorciento,0)) / 100, 2)  Else DefDescuento End as DefDescuento, " +
                $" {cxcBalance} - Case When ifnull(DefDescuento,0) = 0  Then round(({montoADescuento} * ifnull(DeFPorciento,0)) / 100, 2) Else DefDescuento End as MontoTotal, c.CxcFecha " +
                $" from DescuentoFacturas d " +
                $" Inner Join CuentasxCobrar c on c.CxcReferencia = d.CxcReferencia " +
           "where d.CxcReferencia = ? order by DeFDiaInicial asc ";

            return SqliteManager.GetInstance().Query<DescuentoFacturas>(query, new string[] { Referencia });
        }
    }
}
