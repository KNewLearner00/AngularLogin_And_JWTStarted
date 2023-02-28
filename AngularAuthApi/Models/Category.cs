using System.ComponentModel.DataAnnotations;

namespace AngularAuthApi.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }
    }
}
