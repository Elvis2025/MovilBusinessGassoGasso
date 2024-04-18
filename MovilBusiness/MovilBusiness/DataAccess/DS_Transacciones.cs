using MovilBusiness.Configuration;
using MovilBusiness.model;
using MovilBusiness.model.Internal;
using MovilBusiness.Utils;
using System;
using System.Collections.Generic;

namespace MovilBusiness.DataAccess
{
    public class DS_Transacciones : DS_Controller
    {

        public List<Transaccion> GetByNameAndEstatus(string transaccionNombre, string Estado, int CliId = -1, int traSecuencia = -1, string secCodigo = null, bool forFecha = false, string serachValue = "", string filtro = "" )
        {

            string diasPermitidos = "365";
            string query = "Select ";
           
            string whereCliente = CliId != -1 ? " and c.CliID = " + CliId : "";

            switch (filtro)
            {
                case "Nombre":
                    whereCliente += " and c.CliNombre  LIKE '%" + serachValue + "%'";
                    break;
                case "Código":
                    whereCliente += "and CliCodigo Like '%" + serachValue + "%'";
                    break;

                case "Código barra": 
                  
                    break;
                case "Combinada":
                    whereCliente += "and CliNombre LIKE '%" + serachValue + "%'  OR CliCodigo LIKE '%" + serachValue + "%'";
                    break;

            }
            //Modificar where cuando se vaya a implementar en las demas transacciones
            if (!string.IsNullOrWhiteSpace(secCodigo) && (transaccionNombre.ToUpper() == "PEDIDOS" || transaccionNombre.ToUpper().StartsWith("ENTREGASTRANSACCIONES")))
            {
                whereCliente += " And p.SecCodigo = '" + secCodigo + "'";
            }
            string joinSector = "";
            if (DS_RepresentantesParametros.GetInstance().GetParSectores()>0 && (transaccionNombre.ToUpper() == "PEDIDOS" || transaccionNombre.ToUpper() == "ENTREGASTRANSACCIONES" || transaccionNombre.ToUpper() == "ENTREGASTRANSACCIONESCONFIRMADOS"))
            {
                joinSector = " Inner Join Sectores s on p.SecCodigo= s.SecCodigo ";
            }
            List<Sectores> Sectores = new List<Sectores>();
            bool usarSectoresEntregas = DS_RepresentantesParametros.GetInstance().GetParConsultaTransaccionesUsarSectores();
            if (usarSectoresEntregas)
            {
                Sectores = new DS_Sectores().GetSectores();               
            }

                   
            if (usarSectoresEntregas && Sectores.Count  > 0 && transaccionNombre.ToUpper().StartsWith("ENTREGASTRANSACCIONES"))
            {
                joinSector = " Inner Join Sectores s on s.SecCodigo= p.SecCodigo ";
            }

            var parDiasTransaccionesVisibles = DS_RepresentantesParametros.GetInstance().GetDiasTransaccionesVisibles();
            if (parDiasTransaccionesVisibles > 0)
            {
                diasPermitidos = parDiasTransaccionesVisibles.ToString();
            }

            switch (transaccionNombre.ToUpper())
            {
                case "PEDIDOS":
                    query += "PedSecuencia as TransaccionID, p.PedEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus," : "") + " p.PedFecha as Fecha, '['||p.RepCodigo||'],['||p.PedSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  c.CliID as CliID, PedSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(PedFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       " " + (DS_RepresentantesParametros.GetInstance().GetParSectores() > 0 ? "SecDescripcion,1 as UseSector," : "") + " cast(replace(cast(julianday(datetime('now')) - julianday(PedFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion ,  p.SecCodigo from Pedidos p " +
                       "inner join Clientes c on c.CliID = p.CliID " + joinSector + " " +
                       "left join Estados h on h.EstTabla = 'Pedidos' and h.EstEstado = p.PedEstatus " +
                       "where " + (traSecuencia != -1 ? "PedSecuencia = " + traSecuencia + " and " : "") + " " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(PedFecha,1,10)),' ','' ), '')" : "PedEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(PedFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by PedSecuencia desc";
                    break;
                case "AUDITORIASPRECIOS":
                    query += "AudSecuencia as TransaccionID, p.AudEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus," : "") + " p.AudFecha as Fecha, '['||p.RepCodigo||'],['||p.AudSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  c.CliID as CliID, AudSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(AudFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       " cast(replace(cast(julianday(datetime('now')) - julianday(AudFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from AuditoriasPrecios p " +
                       "inner join Clientes c on c.CliID = p.CliID " + joinSector + " " +
                       "left join Estados h on h.EstTabla = 'AuditoriasPrecios' and h.EstEstado = p.AudEstatus " +
                       "where " + (traSecuencia != -1 ? "AudSecuencia = " + traSecuencia + " and " : "") + " " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(AudFecha,1,10)),' ','' ), '')" : "AudEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(AudFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by AudSecuencia desc";
                    break;
                case "SOLICITUDEXCLUSIONCLIENTES":
                    query += "SolSecuencia as TransaccionID, p.SolEstado as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus," : "") + " p.SolFecha as Fecha, '['||p.RepCodigo||'],['||p.SolSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  c.CliID as CliID, SolSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(SolFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       " cast(replace(cast(julianday(datetime('now')) - julianday(SolFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from SolicitudExclusionClientes p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'SolicitudExclusionClientes' and h.EstEstado = p.SolEstado " +
                       "where " + (traSecuencia != -1 ? "SolSecuencia = " + traSecuencia + " and " : "") + " " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(SolFecha,1,10)),' ','' ), '')" : "SolEstado") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(SolFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by SolSecuencia desc";
                    break;
                case "SOLICITUDACTUALIZACIONCLIENTEDIRECCION":
                    query += "SolSecuencia as TransaccionID, p.SolEstado as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus," : "") + " p.SolFecha as Fecha, '['||p.RepCodigo||'],['||p.SolSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  c.CliID as CliID, SolSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(SolFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       " cast(replace(cast(julianday(datetime('now')) - julianday(SolFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from SolicitudActualizacionClienteDireccion p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'SolicitudActualizacionClienteDireccion' and h.EstEstado = p.SolEstado " +
                       "where " + (traSecuencia != -1 ? "SolSecuencia = " + traSecuencia + " and " : "") + " " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(SolFecha,1,10)),' ','' ), '')" : "SolEstado") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(SolFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by SolSecuencia desc";
                    break;
                case "ENTREGASTRANSACCIONES":
                    query += "EntSecuencia as TransaccionID, p.EntEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.EntSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  p.EntFecha as Fecha, p.CliID as CliID, EntSecuencia||' - '||ltrim(rtrim(c.CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '') as TransacionDescripcion " +
                         " " + (usarSectoresEntregas && Sectores.Count > 0 ? ", SecDescripcion,1 as UseSector" : "") +
                        " from EntregasTransacciones p " +
                        "inner join Clientes c on c.CliID = p.CliID " + joinSector + " " +
                        "left join Estados h on h.EstTabla = 'EntregasTransacciones' and h.EstEstado = p.EntEstatus " +
                       "where ltrim(rtrim(p.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '')" : "EntEstatus") + " = ? " + whereCliente + (traSecuencia != -1 ? " and EntSecuencia = " + traSecuencia : "") + " " +
                       "order by EntSecuencia desc";
                    break;
                case "ENTREGASTRANSACCIONESCONFIRMADOS":
                    query += "EntSecuencia as TransaccionID, p.EntEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.EntSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  p.EntFecha as Fecha, p.CliID as CliID, EntSecuencia||' - '||ltrim(rtrim(c.CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '') as TransacionDescripcion " +
                          " " + (usarSectoresEntregas && Sectores.Count > 0 ? ", SecDescripcion,1 as UseSector" : "") +
                        " from EntregasTransaccionesConfirmados p " +
                        "inner join Clientes c on c.CliID = p.CliID " + joinSector + " " +
                        "left join Estados h on h.EstTabla = 'EntregasTransaccionesConfirmados' and h.EstEstado = p.EntEstatus " +
                       "where ltrim(rtrim(p.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '')" : "EntEstatus") + " = ? " + whereCliente + (traSecuencia != -1 ? " and EntSecuencia = " + traSecuencia : "") + " " +
                       "order by EntSecuencia desc";
                    break;
                case "ENTREGASREPARTIDOR":
                    query += "EnrSecuencia as TransaccionID, e.EnrEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus, " : "") + " '['||e.RepCodigo||'],['||e.EnrSecuencia||']' as TraKey, e.RepCodigo as RepCodigo,  e.EnrFecha as Fecha, -1 as CliID, ifnull(EnrNumeroERP, '')||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EnrFecha,1,10)),' ','' ), '') as TransacionDescripcion " +
                        " from EntregasRepartidor e " +
                        "left join Estados h on h.EstTabla = 'EntregasRepartidor' and h.EstEstado = e.EnrEstatus " +
                       "where ltrim(rtrim(e.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EnrFecha,1,10)),' ','' ), '')" : "EnrEstatus") + " = ? " + (traSecuencia != -1 ? " and EnrNumeroERP = " + traSecuencia : "") + " " +
                       "order by EnrSecuencia desc";
                    break;
                case "ENTREGASREPARTIDORCONFIRMADOS":
                    query += "EnrSecuencia as TransaccionID, e.EnrEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus, " : "") + " '['||e.RepCodigo||'],['||e.EnrSecuencia||']' as TraKey, e.RepCodigo as RepCodigo,  e.EnrFecha as Fecha, -1 as CliID, EnrSecuencia||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EnrFecha,1,10)),' ','' ), '') as TransacionDescripcion " +
                        " from EntregasRepartidorConfirmados e " +
                        "left join Estados h on h.EstTabla = 'EntregasRepartidorConfirmados' and h.EstEstado = e.EnrEstatus " +
                       "where ltrim(rtrim(e.RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EnrFecha,1,10)),' ','' ), '')" : "EnrEstatus") + " = ? " + (traSecuencia != -1 ? " and EnrSecuencia = " + traSecuencia : "") + " " +
                       "order by EnrSecuencia desc";
                    break;
                case "CLIENTES": //prospectos
                    query += "CliID as TransaccionID, c.CliEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||CliID||']' as TraKey, c.CliFechaActualizacion as Fecha, CliID as CliID, CliID||' - '||ltrim(rtrim(CliNombre)) as TransacionDescripcion,SecCodigo " +
                        "from Clientes c " +
                        "left join Estados h on h.EstTabla = 'Clientes' and h.EstEstado = CliEstatus " +
                        "where CliDatosOtros like '%P%' " + whereCliente + " and ltrim(rtrim(RepCodigo)) = '" + Arguments.CurrentUser.RepCodigo + "' and " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CliFechaActualizacion,1,10)),' ','' ), '')" : "CliEstatus") + " = ? " + (traSecuencia != -1 ? " and CliID = " + traSecuencia : "") + " " +
                        "order by CliNombre";
                    break;
                case "PEDIDOSCONFIRMADOS":
                    query += "PedSecuencia as TransaccionID, p.PedEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.PedSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  p.PedFecha as Fecha, c.CliID as CliID, PedSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(PedFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(PedFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion,p.SecCodigo as Seccodigo, p.PedNumeroERP as NumeroERP, 1 as IsPedConfirmados from PedidosConfirmados p " +
                        "inner join Clientes c on c.CliID = p.CliID " +
                        "left join Estados h on h.EstTabla = 'PedidosConfirmados' and h.EstEstado = PedEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(PedFecha,1,10)),' ','' ), '')" : "PedEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and PedSecuencia = " + traSecuencia + " " : "") +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(PedFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by PedSecuencia desc";
                    break;
                case "COMPRAS":
                    query += "ComSecuencia as TransaccionID, p.ComEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.ComSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  p.ComFecha as Fecha, c.CliID as CliID, ComSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       "cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, p.SecCodigo as Seccodigo from Compras p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'Compras' and h.EstEstado = ComEstatus " +
                       "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '')" : "ComEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and ComSecuencia = " + traSecuencia + " " : "") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer)) < " + diasPermitidos + "  and p.ComTipo == 1 " +
                       " order by ComSecuencia desc";
                    break;
                case "COMPRASCONFIRMADOS":
                    query += "ComSecuencia as TransaccionID, p.ComEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.ComSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  p.ComFecha as Fecha, c.CliID as CliID, ComSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       "cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, p.SecCodigo as Seccodigo from ComprasConfirmados p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'ComprasConfirmados' and h.EstEstado = ComEstatus " +
                       "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '')" : "ComEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and ComSecuencia = " + traSecuencia : "") + "" +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " and p.ComTipo == 1 " +
                       " order by ComSecuencia desc";
                    break;
                case "COMPRASPUSHMONEY":
                    Arguments.Values.IsPushMoneyRotacion = true;
                    query += "ComSecuencia as TransaccionID, p.ComEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.ComSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  p.ComFecha as Fecha, c.CliID as CliID, ComSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       "cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, p.SecCodigo as Seccodigo from Compras p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'Compras' and h.EstEstado = ComEstatus " +
                       "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '')" : "ComEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and ComSecuencia = " + traSecuencia + " " : "") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer)) < " + diasPermitidos + "  and p.ComTipo == 2 " +
                       " order by ComSecuencia desc";
                    break;
                case "COMPRASPUSHMONEYCONFIRMADOS":
                    Arguments.Values.IsPushMoneyRotacion = true;
                    query += "ComSecuencia as TransaccionID, p.ComEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.ComSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  p.ComFecha as Fecha, c.CliID as CliID, ComSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       "cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, p.SecCodigo as Seccodigo from ComprasConfirmados p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'ComprasConfirmados' and h.EstEstado = ComEstatus " +
                       "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ComFecha,1,10)),' ','' ), '')" : "ComEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and ComSecuencia = " + traSecuencia : "") + "" +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(ComFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " p.ComTipo == 2 " +
                       " order by ComSecuencia desc";
                    break;
                case "COTIZACIONES":
                    query += "CotSecuencia as TransaccionID, p.CotEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.CotSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  CotFecha as Fecha, c.CliID as CliID, CotSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CotFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       "cast(replace(cast(julianday(datetime('now')) - julianday(CotFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion,p.SecCodigo as Seccodigo from Cotizaciones p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'Cotizaciones' and h.EstEstado = CotEstatus " +
                       "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CotFecha,1,10)),' ','' ), '')" : "CotEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and CotSecuencia = " + traSecuencia : "") + "" +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(CotFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by CotSecuencia desc";
                    break;
                case "COTIZACIONESCONFIRMADOS":
                    query += "CotSecuencia as TransaccionID, p.CotEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.CotSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  CotFecha as Fecha, c.CliID as CliID, CotSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CotFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       "cast(replace(cast(julianday(datetime('now')) - julianday(CotFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion,p.SecCodigo as Seccodigo from CotizacionesConfirmados p " +
                       "inner join Clientes c on c.CliID = p.CliID " +
                       "left join Estados h on h.EstTabla = 'CotizacionesConfirmados' and h.EstEstado = CotEstatus " +
                       "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CotFecha,1,10)),' ','' ), '')" : "CotEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and CotSecuencia = " + traSecuencia : "") + "" +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(CotFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by CotSecuencia desc";
                    break;
                case "RECIBOS":
                    query += "RecSecuencia as TransaccionID, r.RecEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||r.RepCodigo||'],['||r.RecTipo||'],['||r.RecSecuencia||']' as TraKey, r.RepCodigo as RepCodigo,  RecFecha as Fecha, c.CliID as CliID, ifnull(RecTipo, '') as DataField, RecSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(RecFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, r.SecCodigo as Seccodigo, ifnull(RecTasa, 0) as RecTasa from Recibos r " +
                        "inner join Clientes c on c.CliID = r.CliID " +
                        "left join Estados h on h.EstTabla = 'Recibos' and h.EstEstado = r.RecEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '')" : "RecEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(r.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and RecSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by RecSecuencia desc";
                    break;
                case "RECIBOSCONFIRMADOS":
                    query += "RecSecuencia as TransaccionID, r.RecEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||r.RepCodigo||'],['||r.RecTipo||'],['||r.RecSecuencia||']' as TraKey, r.RepCodigo as RepCodigo,  RecFecha as Fecha, ifnull(RecTipo,'') as DataField, c.CliID as CliID, RecSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(RecFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                               "cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, r.SecCodigo as Seccodigo from RecibosConfirmados r " +
                               "inner join Clientes c on c.CliID = r.CliID " +
                               "left join Estados h on h.EstTabla = 'RecibosConfirmados' and h.EstEstado = RecEstatus " +
                               "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '')" : "RecEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(r.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and RecSecuencia = " + traSecuencia : "") + "" +
                               " and (cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                               " order by RecSecuencia desc";
                    break;
                case "PUSHMONEYPAGOS":
                    query += "pusSecuencia as TransaccionID, r.pusEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||r.RepCodigo||'],['||r.pusSecuencia||']' as TraKey, r.RepCodigo as RepCodigo,  pusFecha as Fecha, r.rowguid as DataField, c.CliID as CliID, pusSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(pusFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(pusFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, r.SecCodigo as Seccodigo from PushMoneyPagos r " +
                        "inner join Clientes c on c.CliID = r.CliID " +
                        "left join Estados h on h.EstTabla = 'PushMoneyPagos' and h.EstEstado = r.pusEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(pusFecha,1,10)),' ','' ), '')" : "pusEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(r.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and pusSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(pusFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by pusSecuencia desc";
                    break;
                case "RECONCILIACIONES":
                    query += "RecSecuencia as TransaccionID, r.RecEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||r.RepCodigo||'],['||r.RecTipo||'],['||r.RecSecuencia||']' as TraKey, r.RepCodigo as RepCodigo,  RecFecha as Fecha, ifnull(RecTipo,'') as DataField, c.CliID as CliID, RecSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(RecFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                              "cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, r.SecCodigo as Seccodigo from Reconciliaciones r " +
                              "inner join Clientes c on c.CliID = r.CliID " +
                              "left join Estados h on h.EstTabla = 'Reconciliaciones' and h.EstEstado = RecEstatus " +
                              "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '')" : "RecEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(r.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and RecSecuencia = " + traSecuencia : "") + "" +
                              " and (cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                              " order by RecSecuencia desc";
                    break;
                case "RECONCILIACIONESCONFIRMADOS":
                    query += "RecSecuencia as TransaccionID, r.RecEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||r.RepCodigo||'],['||r.RecTipo||'],['||r.RecSecuencia||']' as TraKey, r.RepCodigo as RepCodigo,  RecFecha as Fecha, ifnull(RecTipo,'') as DataField, c.CliID as CliID, RecSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(RecFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                              "cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, r.SecCodigo as Seccodigo from ReconciliacionesConfirmados r " +
                              "inner join Clientes c on c.CliID = r.CliID " +
                              "left join Estados h on h.EstTabla = 'Reconciliaciones' and h.EstEstado = RecEstatus " +
                              "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(RecFecha,1,10)),' ','' ), '')" : "RecEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(r.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and RecSecuencia = " + traSecuencia : "") + "" +
                              " and (cast(replace(cast(julianday(datetime('now')) - julianday(RecFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                              " order by RecSecuencia desc";
                    break;
                case "DEVOLUCIONES":
                    query += "DevSecuencia as TransaccionID, d.DevEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||d.RepCodigo||'],['||DevSecuencia||']' as TraKey, d.RepCodigo as RepCodigo,  DevFecha as Fecha, c.CliID as CliID, DevSecuencia||' - '||ifnull(ltrim(rtrim(CliNombre)), '')||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(DevFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(DevFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, d.SecCodigo as Seccodigo from Devoluciones d " +
                        "inner join Clientes c on c.CliID = d.CliID " +
                        "left join Estados h on h.EstTabla = 'Devoluciones' and h.EstEstado = DevEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DevFecha,1,10)),' ','' ), '')" : "DevEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(d.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and DevSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(DevFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by DevSecuencia desc";
                    break;
                case "DEVOLUCIONESCONFIRMADAS":
                    query += "DevSecuencia as TransaccionID, d.DevEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||d.RepCodigo||'],['||DevSecuencia||']' as TraKey, d.RepCodigo as RepCodigo,  DevFecha as Fecha, c.CliID as CliID,DevSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(DevFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(DevFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion,  d.SecCodigo as Seccodigo from DevolucionesConfirmadas d " +
                        "inner join Clientes c on c.CliID = d.CliID " +
                        "left join Estados h on h.EstTabla = 'DevolucionesConfirmadas' and h.EstEstado = DevEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DevFecha,1,10)),' ','' ), '')" : "DevEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(d.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and DevSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(DevFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by DevSecuencia desc";
                    break;
                case "DEPOSITOS":
                    query += "DepSecuencia as TransaccionID, d.DepEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||d.RepCodigo||'],['||d.DepSecuencia||']' as TraKey, d.RepCodigo as RepCodigo,  DepFecha as Fecha, DepSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(DepFecha, 1,10)), ' ', ''), '') as TransacionDescripcion," +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, d.SocCodigo as Seccodigo " +
                        "from Depositos d " +
                        "left join Estados h on h.EstTabla = 'Depositos' and h.EstEstado = DepEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DepFecha,1,10)),' ','' ), '')" : "DepEstatus") + " = ? and trim(d.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and DepSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by DepSecuencia desc";
                    break;
                case "DEPOSITOSCONFIRMADOS":
                    query += "DepSecuencia as TransaccionID, DepEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||d.RepCodigo||'],['||d.DepSecuencia||']' as TraKey, d.RepCodigo as RepCodigo,  DepFecha as Fecha, DepSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(DepFecha, 1,10)), ' ', ''), '') as TransacionDescripcion," +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion,d.SocCodigo as Seccodigo " +
                        "from DepositosConfirmados d " +
                        "left join Estados h on h.EstTabla = 'DepositosConfirmados' and h.EstEstado = DepEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DepFecha,1,10)),' ','' ), '')" : "DepEstatus") + " = ? and trim(d.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and DepSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by DepSecuencia desc";
                    break;
                case "DEPOSITOSCOMPRAS":
                    query += "DepSecuencia as TransaccionID, DepEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||d.RepCodigo||'],['||d.DepSecuencia||']' as TraKey, d.RepCodigo as RepCodigo,  DepFecha as Fecha, DepSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(DepFecha, 1,10)), ' ', ''), '') as TransacionDescripcion," +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from DepositosCompras d " +
                        "left join Estados h on h.EstTabla = 'DepositosCompras' and h.EstEstado = DepEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DepFecha,1,10)),' ','' ), '')" : "DepEstatus") + " = ? and trim(d.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and DepSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by DepSecuencia desc";
                    break;
                case "DEPOSITOSCOMPRASCONFIRMADOS":
                    query += "DepSecuencia as TransaccionID, d.DepEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||d.RepCodigo||'],['||d.DepSecuencia||']' as TraKey, d.RepCodigo as RepCodigo,  DepFecha as Fecha, DepSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(DepFecha, 1,10)), ' ', ''), '') as TransacionDescripcion," +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from DepositosComprasConfirmados d " +
                        "left join Estados h on h.EstTabla = 'DepositosComprasConfirmados' and h.EstEstado = DepEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(DepFecha,1,10)),' ','' ), '')" : "DepEstatus") + " = ? and trim(d.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and DepSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(DepFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by DepSecuencia desc";
                    break;
                case "VISITAS":
                    query += "VisSecuencia as TransaccionID, v.VisEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||v.RepCodigo||'],['||v.VisSecuencia||']' as TraKey, v.RepCodigo as RepCodigo,  VisFechaEntrada as Fecha, c.CliID as CliID,VisSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(VisFechaEntrada, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(VisFechaEntrada) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from Visitas v " +
                        "left join Estados h on h.EstTabla = 'Visitas' and h.EstEstado = VisEstatus " +
                        "inner join Clientes c on c.CliID = v.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(VisFechaEntrada,1,10)),' ','' ), '')" : "VisEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(v.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and VisSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(VisFechaEntrada) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by VisSecuencia desc";
                    break;
                case "VISITASCONFIRMADOS":
                    query += "VisSecuencia as TransaccionID, v.VisEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||v.RepCodigo||'],['||v.VisSecuencia||']' as TraKey, v.RepCodigo as RepCodigo,  VisFechaEntrada as Fecha, c.CliID as CliID,VisSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(VisFechaEntrada, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(VisFechaEntrada) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from VisitasConfirmados v " +
                        "left join Estados h on h.EstTabla = 'VisitasConfirmados' and h.EstEstado = v.VisEstatus " +
                        "inner join Clientes c on c.CliID = v.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(VisFechaEntrada,1,10)),' ','' ), '')" : "VisEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(v.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and VisSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(VisFechaEntrada) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by VisSecuencia desc";
                    break;
                case "ENTREGASDOCUMENTOS":
                    query += "EntSecuencia as TransaccionID, v.EntEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||v.RepCodigo||'],['||v.EntSecuencia||']' as TraKey, v.RepCodigo as RepCodigo,  EntFecha as Fecha, c.CliID as CliID,EntSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(EntFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from EntregasDocumentos v " +
                        "inner join Clientes c on c.CliID = v.CliID " +
                        "left join Estados h on h.EstTabla = 'EntregasDocumentos' and h.EstEstado = v.EntEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '')" : "EntEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(v.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and EntSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by EntSecuencia desc";
                    break;
                case "ENTREGASDOCUMENTOSCONFIRMADOS":
                    query += "EntSecuencia as TransaccionID, v.EntEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||v.RepCodigo||'],['||v.EntSecuencia||']' as TraKey, v.RepCodigo as RepCodigo,  EntFecha as Fecha, c.CliID as CliID,EntSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(EntFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from EntregasDocumentosConfirmados v " +
                        "left join Estados h on h.EstTabla = 'EntregasDocumentosConfirmados' and h.EstEstado = v.EntEstatus " +
                        "inner join Clientes c on c.CliID = v.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '')" : "EntEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(v.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and EntSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by EntSecuencia desc";
                    break;
                case "MUESTRAS":
                    query += "MueSecuencia as TransaccionID, v.MueEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, "+(forFecha ? " 1 as ShowEstatus, " : "")+" '['||v.RepCodigo||'],['||v.MueSecuencia||']' as TraKey, v.RepCodigo as RepCodigo,  MueFecha as Fecha, c.CLIID as CliID, MueSecuencia||' - '||ltrim(rtrim(c.CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(MueFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(MueFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from Muestras v " +
                        "inner join Clientes c on c.CliID = v.CLIID " +
                        "inner join Estados h on h.EstTabla = 'Muestras' and h.EstEstado = v.MueEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(MueFecha,1,10)),' ','' ), '') = '"+Estado+"'" : "ifnull(MueEstatus, 1) = " + Estado) + " and trim(v.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + whereCliente + " " + (traSecuencia != -1 ? " and MueSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(MueFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by MueSecuencia desc";
                    break;
                case "MUESTRASCONFIRMADOS":
                    query += "MueSecuencia as TransaccionID, v.MueEstatus as Estatus, v.EstDescripcion as EstatusDescripcion, "+(forFecha ? " 1 as ShowEstatus, " : "")+" '['||v.RepCodigo||'],['||v.MueSecuencia||']' as TraKey, v.RepCodigo as RepCodigo,  MueFecha as Fecha, c.CLIID as CliID, MueSecuencia||' - '||ltrim(rtrim(c.CliNombre))||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(MueFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(MueFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from MuestrasConfirmados v " +
                        "inner join Clientes c on c.CliID = v.CLIID " +
                        "inner join Estados h on h.EstTabla = 'Muestras' and h.EstEstado = v.MueEstatus " +
                        " where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(MueFecha,1,10)),' ','' ), '') = '"+Estado+"'" : "ifnull(MueEstatus, 1) = " + Estado) + " and trim(v.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + whereCliente + " " + (traSecuencia != -1 ? " and MueSecuencia = " + traSecuencia : "") + "" +
                         " and (cast(replace(cast(julianday(datetime('now')) - julianday(MueFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by MueSecuencia desc";
                    break;
                case "CUADRES":
                    query += "CuaSecuencia as TransaccionID, CuaEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.CuaSecuencia||']' as TraKey, c.RepCodigo as RepCodigo,  CuaFechaInicio as Fecha, CuaSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(CuaFechaInicio, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(CuaFechaInicio) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from Cuadres c " +
                        "left join Estados h on h.EstTabla = 'Cuadres' and h.EstEstado = CuaEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CuaFechaInicio,1,10)),' ','' ), '')" : "CuaEstatus") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and CuaSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(CuaFechaInicio) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by CuaSecuencia desc";
                    break;
                case "VENTAS":
                    query += "VenSecuencia as TransaccionID, p.VenEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.VenSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  VenFecha as Fecha, c.CliID as CliID, VenSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(VenFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                      "cast(replace(cast(julianday(datetime('now')) - julianday(VenFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                      "from Ventas p " +
                      "left join Estados h on h.EstTabla = 'Ventas' and h.EstEstado = VenEstatus " +
                      "inner join Clientes c on c.CliID = p.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(VenFecha,1,10)),' ','' ), '')" : "VenEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and VenSecuencia = " + traSecuencia : "") + "" +
                      " and (cast(replace(cast(julianday(datetime('now')) - julianday(VenFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                      " order by VenSecuencia desc";
                    break;
                case "VENTASCONFIRMADOS":
                    query += "VenSecuencia as TransaccionID, p.VenEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.VenSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  VenFecha as Fecha, c.CliID as CliID, VenSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(VenFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(VenFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from VentasConfirmados p " +
                        "left join Estados h on h.EstTabla = 'Ventas' and h.EstEstado = VenEstatus " +
                        "inner join Clientes c on c.CliID = p.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(VenFecha,1,10)),' ','' ), '')" : "VenEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and VenSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(VenFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by VenSecuencia desc";
                    break;
                case "CARGAS":
                    query += "CarSecuencia as TransaccionID, c.CarEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.CarSecuencia||']' as TraKey, c.RepCodigo as RepCodigo,  CarFecha as Fecha, CarSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(CarFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(CarFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from Cargas c " +
                        "left join Estados h on h.EstTabla = 'Cargas' and h.EstEstado = CarEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CarFecha,1,10)),' ','' ), '')" : "CarEstatus") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and CarSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(CarFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by CarSecuencia desc";
                    break;
                case "REQUISICIONESINVENTARIO":
                    query += "ReqSecuencia as TransaccionID, c.ReqEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.ReqSecuencia||']' as TraKey, c.RepCodigo as RepCodigo,  ReqFecha as Fecha, ReqSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(ReqFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(ReqFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from RequisicionesInventario c " +
                        "left join Estados h on h.EstTabla = 'RequisicionesInventario' and h.EstEstado = ReqEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ReqFecha,1,10)),' ','' ), '')" : "ReqEstatus") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and ReqSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(ReqFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by ReqSecuencia desc";
                    break;
                case "OPERATIVOS":
                    query += "OpeID as TransaccionID, c.OpeEstado as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.OpeID||']' as TraKey, OpeFecha as Fecha, OpeID||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(OpeFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(OpeFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from Operativos c " +
                        "left join Estados h on h.EstTabla = 'Operativos' and h.EstEstado = OpeEstado " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(OpeFecha,1,10)),' ','' ), '')" : "OpeEstado") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and OpeID = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(OpeFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by OpeID desc";
                    break;
                case "GASTOS":
                    query += " GasSecuencia as TransaccionID, G.GasEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||G.RepCodigo||'],['||G.GasSecuencia||']' as TraKey, G.RepCodigo as RepCodigo,  GasFecha as Fecha, GasSecuencia || ' - ' || 'Fecha: ' || ifnull(replace(strftime('%d-%m-%Y', SUBSTR(GasFecha, 1, 10)), ' ', ''), '')   || ' - ' || ifnull(UM.Descripcion, '')  as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(GasFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from Gastos G " +
                        "Inner join UsosMultiples UM on G.GasTipo = UM.CodigoUso and UM.CodigoGrupo = 'TIPOGASTOS' " +
                        "left join Estados h on h.EstTabla = 'Gastos' and h.EstEstado = GasEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(GasFecha,1,10)),' ','' ), '')" : "GasEstatus") + " = ? and trim(G.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and GasSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(GasFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by GasSecuencia desc";
                    break;
                case "GASTOSCONFIRMADOS":
                    query += " GasSecuencia as TransaccionID, G.GasEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||G.RepCodigo||'],['||G.GasSecuencia||']' as TraKey, G.RepCodigo as RepCodigo,  GasFecha as Fecha, GasSecuencia || ' - ' || 'Fecha: ' || ifnull(replace(strftime('%d-%m-%Y', SUBSTR(GasFecha, 1, 10)), ' ', ''), '')  || ' - ' || ifnull(UM.Descripcion, '')  as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(GasFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from GASTOSCONFIRMADOS G " +
                        "Inner join UsosMultiples UM on G.GasTipo = UM.CodigoUso and UM.CodigoGrupo = 'TIPOGASTOS' " +
                        "left join Estados h on h.EstTabla = 'GastosConfirmados' and h.EstEstado = GasEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(GasFecha,1,10)),' ','' ), '')" : "GasEstatus") + " = ? and trim(G.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and GasSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(GasFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by GasSecuencia desc";
                    break;
                case "NCDPP":
                    query += "RecSecuencia as TransaccionID, NCDEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.NCDSecuencia||']' as TraKey, c.RepCodigo as RepCodigo,  NCDFecha as Fecha, 'Rec. #:' || RecSecuencia || ' - ' ||  NCDSecuencia   || ' - ' || 'Fecha: ' || ifnull(replace(strftime('%d-%m-%Y', SUBSTR(NCDFecha, 1, 10)), ' ', ''), '')   as TransacionDescripcion, " +
                             "cast(replace(cast(julianday(datetime('now')) - julianday(NCDFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion, c.NCDSecuencia as TransaccionIDNCDPP " +
                             "from NCDPP c " +
                             "left join Estados h on h.EstTabla = 'NCDPP' and h.EstEstado = c.NCDEstatus " +
                             "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(NCDFecha,1,10)),' ','' ), '')" : "NCDEstatus") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and NCDSecuencia = " + traSecuencia : "") + "" +
                             " and (cast(replace(cast(julianday(datetime('now')) - julianday(NCDFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                             " order by NCDSecuencia desc";
                    break;
                case "CONTEOSFISICOS":
                    query += "ConSecuencia as TransaccionID, ConEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.ConSecuencia||']' as TraKey, c.RepCodigo as RepCodigo,  ConFecha as Fecha, ConSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(ConFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from ConteosFisicos c " +
                        "left join Estados h on h.EstTabla = 'ConteosFisicos' and h.EstEstado = ConEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ConFecha,1,10)),' ','' ), '')" : "ConEstatus") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and ConSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by ConSecuencia desc";
                    break;
                case "CONTEOSFISICOSCONFIRMADOS":
                    query += "ConSecuencia as TransaccionID, ConEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.ConSecuencia||']' as TraKey, c.RepCodigo as RepCodigo,  ConFecha as Fecha, ConSecuencia||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(ConFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from ConteosFisicosConfirmados c " +
                        "left join Estados h on h.EstTabla = 'ConteosFisicos' and h.EstEstado = ConEstatus " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ConFecha,1,10)),' ','' ), '')" : "ConEstatus") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and ConSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by ConSecuencia desc";
                    break;
                case "TRANSFERENCIASALMACENES":
                    query += "TraID as TransaccionID, TraEstado as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||c.RepCodigo||'],['||c.TraID||']' as TraKey, TraFecha as Fecha, TraID||' - '||'Fecha: '||ifnull(replace(strftime('%d-%m-%Y', SUBSTR(TraFecha, 1,10)), ' ', ''), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(TraFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from TransferenciasAlmacenes c " +
                        "left join Estados h on h.EstTabla = 'TransferenciasAlmacenes' and h.EstEstado = TraEstado " +
                        "where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(TraFecha,1,10)),' ','' ), '')" : "TraEstado") + " = ? and trim(c.RepCodigo) = '" + Arguments.CurrentUser.RepCodigo + "' " + (traSecuencia != -1 ? " and TraID = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(TraFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by TraID desc";
                    break;
                case "CONDUCES":
                    query += "ConSecuencia as TransaccionID, p.ConEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.ConSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  ConFecha as Fecha, c.CliID as CliID, ConSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ConFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                      "cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                      "from Conduces p " +
                      "left join Estados h on h.EstTabla = 'Conduces' and h.EstEstado = ConEstatus " +
                      "inner join Clientes c on c.CliID = p.SupID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ConFecha,1,10)),' ','' ), '')" : "ConEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and ConSecuencia = " + traSecuencia : "") + "" +
                      " and (cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                      " order by ConSecuencia desc";
                    break;
                case "CONDUCESCONFIRMADOS":
                    query += "distinct ConSecuencia as TransaccionID, p.ConEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.ConSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  ConFecha as Fecha, c.CliID as CliID, ConSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ConFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from ConducesConfirmados p " +
                        "left join Estados h on h.EstTabla = 'ConducesConfirmados' and h.EstEstado = ConEstatus " +
                        "inner join Clientes c on c.CliID = p.SupID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ConFecha,1,10)),' ','' ), '')" : "ConEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and ConSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(ConFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by ConSecuencia desc";
                    break;
                case "CAMBIOS":
                    query += "CamSecuencia as TransaccionID, p.CamEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.CamSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  CamFecha as Fecha, c.CliID as CliID, CamSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CamFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                      "cast(replace(cast(julianday(datetime('now')) - julianday(CamFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                      "from Cambios p " +
                      "left join Estados h on h.EstTabla = 'Cambios' and h.EstEstado = CamEstatus " +
                      "inner join Clientes c on c.CliID = p.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CamFecha,1,10)),' ','' ), '')" : "CamEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and CamSecuencia = " + traSecuencia : "") + "" +
                      " and (cast(replace(cast(julianday(datetime('now')) - julianday(CamFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                      " order by CamSecuencia desc";
                    break;
                case "CAMBIOSCONFIRMADOS":
                    query += "CamSecuencia as TransaccionID, p.CamEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.CamSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  CamFecha as Fecha, c.CliID as CliID, CamSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CamFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                        "cast(replace(cast(julianday(datetime('now')) - julianday(CamFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                        "from CambiosConfirmados p " +
                        "left join Estados h on h.EstTabla = 'CambiosConfirmados' and h.EstEstado = CamEstatus " +
                        "inner join Clientes c on c.CliID = p.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(CamFecha,1,10)),' ','' ), '')" : "CamEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and CamSecuencia = " + traSecuencia : "") + "" +
                        " and (cast(replace(cast(julianday(datetime('now')) - julianday(CamFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                        " order by CamSecuencia desc";
                    break;
                case "QUEJASSERVICIO":
                    query += "QueSecuencia as TransaccionID, p.QueEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.QueSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  QueFecha as Fecha, c.CliID as CliID, QueSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(QueFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                      "cast(replace(cast(julianday(datetime('now')) - julianday(QueFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                      "from QuejasServicio p " +
                      "left join Estados h on h.EstTabla = 'QuejasServicio' and h.EstEstado = QueEstatus " +
                      "inner join Clientes c on c.CliID = p.CliID where " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(QueFecha,1,10)),' ','' ), '')" : "QueEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and QueSecuencia = " + traSecuencia : "") + "" +
                      " and (cast(replace(cast(julianday(datetime('now')) - julianday(QueFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                      " order by QueSecuencia desc";
                    break;
                case "ENTREGAS DE CANASTOS":
                    query += "TraSecuencia as TransaccionID, p.TraEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.TraSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  TraFecha as Fecha, c.CliID as CliID, TraSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(TraFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                      "cast(replace(cast(julianday(datetime('now')) - julianday(TraFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                      "from TransaccionesCanastos p " +
                      "left join Estados h on h.EstTabla = 'TransaccionesCanastos' and h.EstEstado = TraEstatus " +
                      "inner join Clientes c on c.CliID = p.CliID where p.TitOrigen = -1 and  " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(TraFecha,1,10)),' ','' ), '')" : "TraEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and TraSecuencia = " + traSecuencia : "") + "" +
                      " and (cast(replace(cast(julianday(datetime('now')) - julianday(TraFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                      " order by TraSecuencia desc";
                    break;
                case "RECEPCIÓN DE CANASTOS":
                    query += "TraSecuencia as TransaccionID, p.TraEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.TraSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  TraFecha as Fecha, c.CliID as CliID, TraSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(TraFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                      "cast(replace(cast(julianday(datetime('now')) - julianday(TraFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                      "from TransaccionesCanastos p " +
                      "left join Estados h on h.EstTabla = 'TransaccionesCanastos' and h.EstEstado = TraEstatus " +
                      "inner join Clientes c on c.CliID = p.CliID where p.TitOrigen = 1 and  " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(TraFecha,1,10)),' ','' ), '')" : "TraEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and TraSecuencia = " + traSecuencia : "") + "" +
                      " and (cast(replace(cast(julianday(datetime('now')) - julianday(TraFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                      " order by TraSecuencia desc";
                    break;
                case "PROMOCIONES":
                    query += "EntSecuencia as TransaccionID, p.EntEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.EntSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  EntFecha as Fecha, c.CliID as CliID, EntSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                      "cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                      "from Entregas p " +
                      "left join Estados h on h.EstTabla = 'Promociones' and h.EstEstado = EntEstatus " +
                      "inner join Clientes c on c.CliID = p.CliID where p.EntTipo = 19 and  " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '')" : "EntEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and EntSecuencia = " + traSecuencia : "") + "" +
                      " and (cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                      " order by EntSecuencia desc";
                    break;
                case "ENTREGAS":
                    query += "EntSecuencia as TransaccionID, p.EntEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? " 1 as ShowEstatus, " : "") + " '['||p.RepCodigo||'],['||p.EntSecuencia||']' as TraKey, p.RepCodigo as RepCodigo,  EntFecha as Fecha, c.CliID as CliID, EntSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       "cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion " +
                       "from Entregas p " +
                       "left join Estados h on h.EstTabla = 'Entregas' and h.EstEstado = EntEstatus " +
                       "inner join Clientes c on c.CliID = p.CliID where p.EntTipo = 17 and  " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(EntFecha,1,10)),' ','' ), '')" : "EntEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(p.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") + (traSecuencia != -1 ? " and EntSecuencia = " + traSecuencia : "") + "" +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(EntFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by EntSecuencia desc";
                    break;
                case "INVENTARIOFISICO":
                    query += "InvSecuencia as TransaccionID, i.InvEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus," : "") + " i.infFecha as Fecha, '['||i.RepCodigo||'],['||i.InvSecuencia||']' as TraKey, i.RepCodigo as RepCodigo,  c.CliID as CliID, InvSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(infFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       " cast(replace(cast(julianday(datetime('now')) - julianday(infFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from InventarioFisico i " +
                       "inner join Clientes c on c.CliID = i.CliID " + " " +
                       "left join Estados h on h.EstTabla = 'InventarioFisico' and h.EstEstado = i.InvEstatus " +
                       "where " + (traSecuencia != -1 ? "InvSecuencia = " + traSecuencia + " and " : "") + " " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(infFecha,1,10)),' ','' ), '')" : "InvEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(i.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(infFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by InvSecuencia desc";
                    break;
                case "COLOCACIONPRODUCTOS":
                    query += "ColSecuencia as TransaccionID, i.ColEstatus as Estatus, h.EstDescripcion as EstatusDescripcion, " + (forFecha ? "1 as ShowEstatus," : "") + " i.ColFecha as Fecha, '['||i.RepCodigo||'],['||i.ColSecuencia||']' as TraKey, i.RepCodigo as RepCodigo,  c.CliID as CliID, ColSecuencia||' - '||ltrim(rtrim(CliNombre))||' - '||'Fecha: '||ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ColFecha,1,10)),' ','' ), '') as TransacionDescripcion, " +
                       " cast(replace(cast(julianday(datetime('now')) - julianday(ColFecha) as integer),' ', '') as integer) as CantidadDiasDesdeCreacion from ColocacionProductos i " +
                       "inner join Clientes c on c.CliID = i.CliID " + " " +
                       "left join Estados h on h.EstTabla = 'ColocacionProductos' and h.EstEstado = i.ColEstatus " +
                       "where " + (traSecuencia != -1 ? "ColSecuencia = " + traSecuencia + " and " : "") + " " + (forFecha ? "ifnull(replace( strftime('%d-%m-%Y', SUBSTR(ColFecha,1,10)),' ','' ), '')" : "ColEstatus") + " = ? " + whereCliente + (myParametro.GetParVendedorContVend() ? " " : " and trim(i.Repcodigo) = '" + Arguments.CurrentUser.RepCodigo + "' ") +
                       " and (cast(replace(cast(julianday(datetime('now')) - julianday(ColFecha) as integer),' ', '') as integer)) < " + diasPermitidos + " " +
                       " order by ColSecuencia desc";
                    break;
            }


            if (query == "Select ")
            {
                return new List<Transaccion>();
            }

            return SqliteManager.GetInstance().Query<Transaccion>(query, new string[] { Estado });
        }

        internal IEnumerable<Transaccion> GetByNameAndEstatus(string v1, string value, string v2, int traSecuencia, string sector, bool isForFecha)
        {
            throw new NotImplementedException();
        }
    }
}
