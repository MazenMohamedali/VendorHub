using System.ComponentModel.DataAnnotations;
using VendorHub.Validation;

namespace VendorHub.DTOs.ProductDto
{
    public class ProductAddEditBase 
    {

        [DataType(DataType.Date)]
        [Display(Name = "Production Date")]
        public DateTime? ProductionDate { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Expiry Date")]
        [DateGreaterThan("ProductionDate", ErrorMessage = "Expiry must be later than Production Date")]
        public DateTime? ExpireDate { get; set; }
    }
}
