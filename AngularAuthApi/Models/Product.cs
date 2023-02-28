using System.ComponentModel.DataAnnotations;

namespace AngularAuthApi.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string Image { get; set; }

        public float Price { get; set; }
    }
}
