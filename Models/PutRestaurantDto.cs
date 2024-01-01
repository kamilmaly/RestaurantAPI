using System.ComponentModel.DataAnnotations;

namespace RestaurantAPI.Models
{
    public class PutRestaurantDto
    {
        [Required]
        [MaxLength(25)]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool HasDelivery { get; set; }
    }
}
