using System;
using TechMove.Glms.Web.Models;

namespace TechMove.Glms.Web.Services
{
    public class ContractFactory : IContractFactory
    {
        public Contract Create(string contractType)
        {
            if (contractType == "Freight")
            {
                FreightContract fc = new FreightContract();
                fc.WeightLimit = 1000;
                fc.Route = "Standard";
                return fc;
            }
            else if (contractType == "Warehousing")
            {
                WarehousingContract wc = new WarehousingContract();
                wc.Capacity = 500;
                wc.TemperatureZone = "Ambient";
                return wc;
            }
            else if (contractType == "LastMile")
            {
                LastMileContract lm = new LastMileContract();
                lm.DeliveryRadius = 50;
                return lm;
            }
            else
            {
                throw new ArgumentException("Unknown contract type: " + contractType, "contractType");
            }
        }
    }
}
