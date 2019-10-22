using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraniteHouse.Models.ModelViews
{
    public class DetailsOrderViewModel
    {
        public Order Order { get; set; }
        public List<AuthenticationUser> Users { get; set; }
        public List<Products> Products { get; set; }
    }
}
