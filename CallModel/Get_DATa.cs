using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppEWMFlag.CallModel
{

        public class Item
        {
        public string MATNR { get; set; }
        public string SERIAL { get; set; }
        public string CREATEUTC { get; set; }
        }

        public class RootObject
        {
            public List<Item> item { get; set; }
        }
    
   
}
