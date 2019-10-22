using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraniteHouse.Models.ModelViews
{
    public class SSShoppingCartViewModel
    {
        public List<Products> Products { get; set; }
        public Order Order { get; set; }
    }
}
