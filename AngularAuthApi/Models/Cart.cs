using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AngularAuthApi.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }
        public int Quantity { get; set; }
        public float TotalAmount { get; set; }
        public int price { get; set; }
        public string productname { get; set; }

        public User UserModel { get; set; }

        [ForeignKey("ProductModel")]
        public int ProductModel_ProductId { get; set; }

        public Product ProductModel { get; set; }
    }
}

