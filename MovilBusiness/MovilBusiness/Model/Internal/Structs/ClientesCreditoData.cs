using MovilBusiness.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace MovilBusiness.model.Internal.structs
{
    public struct ClientesCreditoData
    {
        public double LimiteCredito { get; set; }
        public double Balance { get; set; }
        public bool IndicadorCredito { get; set; }

        public double MontoVencido { get; set; }
        public string CliNombre { get; set; }
        public double BalanceSoloChequeDif { get; set; }
        public double BalanceSinChequeDif { get => Balance ; }
        public double CreditoDisponible { get => DS_RepresentantesParametros.GetInstance().GetParCobrosMuestralblBalance() ? LimiteCredito - (Balance + BalanceSoloChequeDif) : LimiteCredito - Balance; }

        public double TotalCxc { get => LimiteCredito - (LimiteCredito - (Balance + BalanceSoloChequeDif)); }
    }
}
