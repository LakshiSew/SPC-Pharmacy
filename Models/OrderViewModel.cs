using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPC_Pharmacy.Models
{
    public class OrderViewModel
    {

        public int OrderID { get; set; }
        public string PharmacyName { get; set; }
        public string DrugName { get; set; }
        public int Quantity { get; set; }
        public Nullable<System.DateTime> OrderDate { get; set; }
        public string Status { get; set; }
    }
}