using SPC_Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPC_Admin.Models
{
    public class AddTenderViewModel
    {
        public int TenderID { get; set; }
        public string DrugName { get; set; } // Selected drug name from dropdown
        public int QuantityRequired { get; set; }
        public DateTime ClosingDate { get; set; }
        public string Status { get; set; }
        public DateTime? ActionDate { get; set; }
        public List<StockUpdate> Drugs { get; set; }
    }
}