using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GraniteHouse.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Display(Name ="User Name")]
        public string UserNameId { get; set; }

        [ForeignKey("UserNameId")]
        public virtual AuthenticationUser User { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [NotMapped]
        [Required]
        public DateTime OrderTime { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Phone]
        [Required]
        public string CustomerPhone { get; set; }

        [EmailAddress]
        [Required]
        public string Customermail { get; set; }

        public bool IsConfirmed { get; set; }

    }
}
