using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPC_Admin.Models
{
    public class UpdateStockViewModel
    {
        public int StockUpdateID { get; set; }
        public string DrugName { get; set; }
        public int CurrentQty { get; set; }
        public decimal PricePerUnit { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}