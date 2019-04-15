using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cfNetDemo.lib
{
    public class Models
    {
        public string odatametadata { get; set; }
        public Item[] value { get; set; }
        public string odatanextLink { get; set; }
    }

    public class Item
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public float QuantityOnStock { get; set; }
        public float QuantityOrderedFromVendors { get; set; }
        public float QuantityOrderedByCustomers { get; set; }
    }

    public class ItemDB
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Integrated { get; set; }

        public ItemDB(string Code, String Name, Boolean Integrated)
        {
            this.Code = Code;
            this.Name = Name;
            this.Integrated = Integrated;

        }
       
    }

}
