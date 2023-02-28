using System.ComponentModel.DataAnnotations;

namespace AngularAuthApi.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string ProName { get; set; }
        public string PictureUrl { get; set; }

       
        public DateTime OrderDate { get; set; }

     
        public float AmountPaid { get; set; }

       
        public string ModeOfPayment { get; set; }

       public string OrderStatus { get; set; }

        public Cart CartModel { get; set; }
    }
}
