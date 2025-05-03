using SPC_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPC_Admin.Models
{
    public class StockAndDrugViewModel
    {
        public List<StockUpdate> StockUpdates { get; set; }
        public List<Drug> Drugs { get; set; }
    }
}