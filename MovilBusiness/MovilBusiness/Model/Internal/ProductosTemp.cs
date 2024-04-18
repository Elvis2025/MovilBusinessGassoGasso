using MovilBusiness.Configuration;
using MovilBusiness.DataAccess;
using MovilBusiness.Enums;
using MovilBusiness.Model;
using MovilBusiness.Model.Internal;
using MovilBusiness.Utils;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;

namespace MovilBusiness.model.Internal
{
    public class ProductosTemp
    {
        [PrimaryKey] public string rowguid { get; set; }
        public int TitID { get; set; }
        public int ProID { get; set; }
        public bool IndicadorOferta { get; set; }
        public bool IndicadorDescuento { get; set; }
        public double Cantidad { get; set; }
        public double CantidadDetalle { get; set; }
        public int CantidadPiezas { get; set; }

        public int CantidadConfirmada { get; set; }
        public bool ShowCantidadConfirmada { get; set; }

        public double CantidadDetalleR { get; set; }
        public double Precio { get; set; }
        public double Itbis { get; set; }
        public double Selectivo { get; set; }
        public double AdValorem { get; set; }
        public double Descuento { get; set; }
        public double DesPorciento { get; set; }
        public double DesPorcientoManual { get; set; }
        public int OfeID { get; set; }
        public string UnmCodigo { get; set; }
        public double InvCantidad { get; set; }
        public int InvCantidadDetalle { get; set; }
        public double ConCantidad { get; set; }
        public int ConCantidadDetalle { get; set; }
        public string ProCodigo { get; set; }
        public string Descripcion { get; set; }
        public bool IndicadorDetalle { get; set; }
        public int ProUnidades { get; set; }
        public string Accion { get; set; }
        public string Lote { get; set; }
        public string LoteEntregado { get; set; }
        public string LoteRecibido { get; set; }
        public string Documento { get; set; }
        public double CantidadOferta { get; set; } //usado en devoluciones
        public string FechaVencimiento { get; set; }
        public string FechaAntesVencimiento { get; set; }
        public string FechaDespuesVencimiento { get; set; }
        public int MotIdDevolucion { get; set; }
        public string MotDescripcion { get; set; }
        public int InvAreaId { get; set; } = -1;
        public string infLote { get; set; }
        public double ProPrecio3 { get; set; }
        public double PrecioTemp { get; set; }
        public int ProIDOferta { get; set; }
        public string ProReferencia { get; set; }
        public double LipPrecioMinimo { get; set; }
        public double ValorOfertaManual { get; set; }
        public double LipPrecioSugerido { get; set; }
        public string ProDatos3 { get; set; }
        public double PrecioCaja { get; set; }
        public bool IndicadorPromocion { get; set; }
        public string ProPosicion { get; set; }//DATE
        public int Posicion { get; set; }

        public double ProCantidadMultiploVenta { get; set; }
        public double ProCantidadMaxVenta { get; set; }
        public double ProCantidadMinVenta { get; set; }
        public double ProDescuentoMaximo { get; set; }
        public double PrecioSaved { get; set; }
        public int CantidadMaximaOferta { get; set; }
        public double OfeCantidadMaximaTransaccion { get; set; }
        public double CantidadEntrega { get; set; }
        public string DesIdsAplicados { get; set; } //aqui se guardan los DesID de losf descuento que aplicaron para este producto, se guardan separador por |
        public string AlmDescripcion { get; set; }
        public double ofeCantidadRebajaVenta { get; set; }

        public int AlmID { get; set; }

        public string RepSupervisor { get; set; }
        public double CantidadToCalcular { get; set; }
        public double PedMontoTotal { get; set; }
        public double PedMontoITBIS { get; set; }
        public double PedMontoSinITBIS { get; set; }
        public double PedTotalItbis { get; set; }
        public double PedTotalDescuento { get; set; }

        public bool UseAttribute1 { get; set; }
        public bool UseAttribute2 { get; set; }

        public string ProAtributo1 { get; set; }
        public string ProAtributo1Desc { get; set; }
        public string ProAtributo2 { get; set; }
        public string ProAtributo2Desc { get; set; }

        /// <summary>
        /// 0 = Recibiendo mercancia
        /// 1 = entregando mercancia
        /// </summary>
        public int TipoCambio { get; set; }
        public ProductosTemp Copy()
        {
            return (ProductosTemp)MemberwiseClone();
        }

        [Ignore] public string CantidadUnidades { get => Cantidad + "/" + CantidadDetalle; }
        [Ignore] public string CantidadManualUnidades { get => CantidadManual + "/" + CantidadManualDetalle; }
        [Ignore] public bool ShowUnidades { get => (InvCantidadDetalle > 0 && Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS && DS_RepresentantesParametros.GetInstance().GetShowInventarioAlmacenesEnPedidos())
                || (InvCantidadDetalle > 0 && Arguments.Values.CurrentModule == Enums.Modules.COTIZACIONES && DS_RepresentantesParametros.GetInstance().GetShowInventarioAlmacenesEnCotizaciones())
                || (InvCantidadDetalle > 0 && (Arguments.Values.CurrentModule == Enums.Modules.VENTAS || Arguments.Values.CurrentModule == Enums.Modules.CONTEOSFISICOS)); }

        [Ignore] public string InvCantidadUnidades { get => InvCantidad + "/" + InvCantidadDetalle; }
        [Ignore] public bool NoShowUnidades { get => (InvCantidadDetalle > 0 && InvCantidad == 0 && Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS && DS_RepresentantesParametros.GetInstance().GetShowInventarioAlmacenesEnPedidos()) || (InvCantidadDetalle > 0 && InvCantidad == 0 && Arguments.Values.CurrentModule == Enums.Modules.COTIZACIONES && DS_RepresentantesParametros.GetInstance().GetShowInventarioAlmacenesEnCotizaciones())
                || (InvCantidadDetalle > 0 && InvCantidad == 0 && Arguments.Values.CurrentModule == Enums.Modules.VENTAS
                || (InvCantidadDetalle == 0 && InvCantidad > 0 && (Arguments.Values.CurrentModule == Enums.Modules.VENTAS || Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS)))
                || (InvCantidadDetalle == 0 && InvCantidad >= 0 && Arguments.Values.CurrentModule == Enums.Modules.PRODUCTOS)
                || (InvCantidadDetalle == 0 && InvCantidad >= 0 && Arguments.Values.CurrentModule == Enums.Modules.COTIZACIONES); }

        //[Ignore] public double PrecioNeto { get { return Precio > 0.0 ? (Precio * (Itbis / 100.0)) + (Precio - Descuento) : 0; } }
        [Ignore] public double PrecioNeto { get { return Precio > 0.0 ? ((Precio + AdValorem + Selectivo) - Descuento) * (Itbis / 100.0 + 1.0) : 0; } }
        [Ignore] public double PrecioBruto { get { return (Precio + AdValorem + Selectivo);/* (DS_RepresentantesParametros.GetInstance().GetParPrecioSinImpuestos() && Precio > 0.0 ? (Precio > 0.0 ? Precio + AdValorem + Selectivo : 0) : Precio); */} }
        [Ignore] public double PrecioNeg { get { return PrecioTemp > 0.0 ? PrecioTemp + AdValorem + Selectivo : 0; } }
        [Ignore] public string PrecioSugerido { get { return LipPrecioSugerido.ToString(); } }
        [Ignore] public double PrecioUnitario { get { return double.IsNaN(PrecioNeto / ProUnidades) || double.IsInfinity(PrecioNeto / ProUnidades) ? 0 : PrecioNeto / ProUnidades; } }
        [Ignore] public double PrecioCajaNeto { get => (PrecioCaja + Selectivo + AdValorem) * ((Itbis / 100.0) + 1.0); }
        [Ignore] public double PrecioConDescuento { get => (Precio + AdValorem + Selectivo) - Descuento; }

        [Ignore] public string CantidadConUnidades { get { return (DS_RepresentantesParametros.GetInstance().GetParRedondeoCantidadesDecimales() ? Math.Round(Cantidad, 2) + (CantidadDetalle > 0 ? "/" + CantidadDetalle : "") : Cantidad + (CantidadDetalle > 0 ? "/" + CantidadDetalle : "")); } }
        [Ignore] public string CantidadDetalleRevenimiento { get => CantidadDetalleR.ToString(); }
        [Ignore] public string DescuentoFormatted { get => Descuento.ToString("N2") + " (" + DesPorciento.ToString("N2") + "%)"; }
        [Ignore] public string CajasUnidades { get => (int)(Cantidad / (ProUnidades > 0 ? ProUnidades : 1)) + "/" + (int)(Cantidad % (ProUnidades > 0 ? ProUnidades : 1)); }
        [Ignore] public string ProCodigoLabel { get => "Cód: " + ProCodigo; }
        [Ignore] public string lblOfertaPaqueteCajetilla { get; set; }
        [Ignore] public bool ShowFactura { get => Arguments.Values.CurrentModule == Enums.Modules.COMPRAS && !string.IsNullOrWhiteSpace(Documento); }
        [Ignore] public bool UsaCondicion { get => DS_RepresentantesParametros.GetInstance().GetParDevolucionCondicion() && Arguments.Values.CurrentModule == Modules.DEVOLUCIONES; }

        public bool UseInvArea { get; set; }
        public string InvAreaDescr { get; set; }
        public string ProDescripcion2 { get; set; } //referencia 2
        public string ProDescripcion1 { get; set; } //marca
        public string ProDescripcion3 { get; set; } //descripcion 2
        public string ProDatos1 { get; set; } //referencia 1
        public string ProDatos2 { get; set; }

        [Ignore]
        public string DescripcionElectiva
        {
            get
            {
                var descr = DS_RepresentantesParametros.GetInstance();

                if (descr.GetParConteosProductosDescripcion() || descr.GetParVentasProductosDescripcion())
                {
                    return ProDescripcion3;
                }

                if (descr.GetParPedidosProductosCombinarDescripcion())
                {
                    return Descripcion + ' ' + ProDescripcion1;
                }

                return Descripcion;
            }
        }

        public string ProImg { get; set; } //viene de la tabla de productos

        [Ignore] public string ProImage { 
            get 
            {
                string proCodigo = ProCodigo.Replace("-", "");

                switch (DS_RepresentantesParametros.GetInstance().
                    GetParPedidosVisualizarFotoEnDetalleProductos())
                {
                    case 1:
                        if (File.Exists(Arguments.Values.RutaImagenes + proCodigo + ".png"))
                            return Arguments.Values.RutaImagenes + proCodigo + ".png";
                        if (File.Exists(Arguments.Values.RutaImagenes + proCodigo + ".jpg"))
                            return Arguments.Values.RutaImagenes + proCodigo + ".jpg";
                        break;
                    case 2:
                        if(!string.IsNullOrWhiteSpace(ProImg))
                            return Functions.IsUrlValid(ProImg) ? ProImg :
                                Arguments.Values.RutaImagenes + ProImg.Substring(1, ProImg.Length - 1);                        
                        break;
                }

                if(Arguments.Values.CurrentModule != Modules.PEDIDOS)
                {
                    if (File.Exists(Arguments.Values.RutaImagenes + proCodigo + ".png"))
                    {
                        return Arguments.Values.RutaImagenes + proCodigo + ".png";
                    }
                    else if (File.Exists(Arguments.Values.RutaImagenes + proCodigo + ".jpg"))
                    {
                        return Arguments.Values.RutaImagenes + proCodigo + ".jpg";
                    }else if (!string.IsNullOrWhiteSpace(ProImg))
                    {
                        if (Functions.IsUrlValid(ProImg))
                        {
                            return ProImg;
                        }
                        else
                        {
                            return Arguments.Values.RutaImagenes + ProImg.Substring(1, ProImg.Length - 1);
                        }
                    }
                }
                return "-1";
            }
        }
        public bool IndicadorEliminar { get; set; }

        //[Ignore] public bool ShowDescripcionDetallada {get => DS_RepresentantesParametros.GetInstance().GetShowProductoDetallado() && AlmDescripcion != null; } 
        [Ignore] public bool ShowTotalLinea { get => Cantidad > 0; }
        [Ignore] public bool ShowSubTotalLinea { get => Cantidad > 0; }
        [Ignore] public double Total { get => PrecioNeto * Cantidad; }//detalle/prounidades * cantidad
        [Ignore] public double TotalProducto { get => (PrecioNeto * Cantidad) - ((DesPorciento / 100) * Cantidad); } //Falta que le reste el descuento
        [Ignore] public double TotalBalance { get => PrecioNeto * (InvCantidad + (InvCantidadDetalle * ProUnidades)); }
        [Ignore] public double TotalDescuento { get => (DesPorciento / 100) * Cantidad; }
        [Ignore] public double SubTotal { get => PrecioBruto * Cantidad; }
        [Ignore] public bool UsePrecioVenta { get => PrecioTemp > 0; }
        [Ignore] public bool isnotCompra { get => (Arguments.Values.CurrentModule != Enums.Modules.COMPRAS && Arguments.Values.CurrentModule != Enums.Modules.PRODUCTOS); }
        [Ignore] public bool isProductos { get => Arguments.Values.CurrentModule == Enums.Modules.PRODUCTOS; }
        [Ignore] public string ShowProIDoferta { get => DS_RepresentantesParametros.GetInstance().GetParProIDOferta() ? ":" + OfeID.ToString() : ""; }
        [Ignore] public bool ShowRevenimiento { get => ((Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS || Arguments.Values.CurrentModule == Enums.Modules.COTIZACIONES) && DS_RepresentantesParametros.GetInstance().GetParRevenimiento()); }
        [JsonIgnore] public bool ShowCantidad { get; set; } = true;
        [JsonIgnore] public bool ShowDescuento { get; set; } = false;
        [JsonIgnore] [Ignore] public string OfeDescripcion { get; set; }
        [JsonIgnore] [Ignore] public string DesDescripcion { get; set; }

        [JsonIgnore] [Ignore] public string LabelInventario { get => Arguments.Values.CurrentModule == Enums.Modules.VENTAS ? "Inventario: " : "Inventario Alm: "; }
        [Ignore] public bool ShowBtnInventarios { get => (InvCantidad > 0 && Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS && DS_RepresentantesParametros.GetInstance().GetShowInventarioAlmacenesEnPedidos()) || (InvCantidad > 0 && Arguments.Values.CurrentModule == Enums.Modules.COTIZACIONES && DS_RepresentantesParametros.GetInstance().GetShowInventarioAlmacenesEnCotizaciones()); }
        [JsonIgnore] [Ignore] public bool ShowProductosComboBtn { get => (Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS || Arguments.Values.CurrentModule == Enums.Modules.VENTAS) && !string.IsNullOrWhiteSpace(ProDatos3) && ProDatos3.Contains("x"); }
        [JsonIgnore] [Ignore] public bool ShowLayoutExtras { get => ShowProductosComboBtn || IndicadorOferta || ShowBtnInventarios || IndicadorDescuento || IndicadorPromocion; }
        [JsonIgnore] [Ignore] public bool UsaLote { get => !string.IsNullOrWhiteSpace(ProDatos3) && ProDatos3.ToUpper().Contains("L") && (Arguments.Values.CurrentModule == Enums.Modules.VENTAS || Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS || Arguments.Values.CurrentModule == Enums.Modules.CONTEOSFISICOS || Arguments.Values.CurrentModule == Enums.Modules.INVFISICO || Arguments.Values.CurrentModule == Modules.CAMBIOSMERCANCIA); }
        [JsonIgnore] [Ignore] public bool ShowAlmacenes => DS_RepresentantesParametros.GetInstance().GetParAlmInPed();
        [JsonIgnore] [Ignore] public string Almacenes => ShowAlmacenes ? new DS_inventariosAlmacenes().GetInventarioDisponibleByProductosIdForAlm(ProID) : "";

        public string LipRangoPrecioMinimo { get; set; }

        public double CantidadManual { get; set; }
        public int CantidadManualDetalle { get; set; }
        public string ProDescripcionOferta { get; set; }

        public bool IndicadorPresencia { get; set; }
        [Ignore][JsonIgnore]public string PresenciaDesc { get => IndicadorPresencia ? "Si" : "No"; }

        [Ignore] public bool ShowReferencia { get => Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS || Arguments.Values.CurrentModule == Enums.Modules.DEVOLUCIONES && DS_RepresentantesParametros.GetInstance().GetReferenciaProductoPed(); }
        [Ignore] public bool ShowCentroDist { get => Arguments.Values.CurrentModule == Enums.Modules.PEDIDOS && !string.IsNullOrEmpty(CedCodigo);}

        public int ConID { get; set; }

        public double PrecioMoneda { get; set; }
        public double LipDescuento { get; set; }
        public string ProColor { get; set; }
        public string ProPaisOrigen { get; set; }
        public string ProAnio { get; set; }
        public string ProMedida { get; set; }
        public bool IndicadorDocena { get; set; }
        public bool UsaLoteP { get; set; }
        public bool ShowImagesPro { get; set; } = true;
        public bool ShowImages { get => !string.IsNullOrWhiteSpace(ProImage) && ShowImagesPro; }

        public bool TieneDescuentoEscala { get; set; } = false;
        public bool ShowDescuentoPreview { get; set; } = false;

        public string ProCodigoDescripcion
        {
            get => ProCodigo + "-" + Descripcion;
        }

        public string PedFechaEntrega { get; set; }
        public string CedCodigo { get; set; }
        public string CedDescripcion { get; set; }
        public double PedCantidad { get; set; }
        public double CliCanTidadMinima { get; set; }
        public double? CantidadAlm { get; set; } = null;
        public double? CanTidadGond { get; set; } = null;
        public double? CanTidadTramo { get; set; } = null;
        public double? UnidadAlm { get; set; } = null;
        public double? UnidadGond { get; set; } = null;
        public string SecCodigo { get; set; }
        public string CatCodigo { get; set; }
        public string MarCodigo { get; set; }
        public bool VerPreciosProductos { get; set; } = true;
        public string DevCondicion { get; set; }
        public string DevDescripcion { get; set; }

        public int CantidadFacing { get; set; }

        public bool IndicadorOfertaForShow { get; set; }
        public bool CheckValueForInvFis { get; set; }
        public bool CheckDetailsForInvFis { get; set; }
        public bool RadioButtonNotEnabled { get; set; } = true;

        [Ignore][JsonIgnore] public PinchArgs pinchArgs { get; set; }

        public double PrecioOferta { get; set; }

        public double Caras { get; set; }

        public string invLote { get; set; }

        public double ProPeso { get; set; }
        public double PedFlete { get; set; }

        public int Linea { get; set; }

        public double PedOfeCantidad { get; set; }

        public string OfeCaracteristica { get; set; }

        public double PrecioBase { get; set; }
        public double DesPorcientoOriginal { get; set; }

        public int EnrSecuencia { get; set; }
        public int TraSecuencia { get; set; }
    }
}
