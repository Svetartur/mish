using Microsoft.AspNetCore.Mvc;

namespace ASP_P42.Models.Admin
{
    public class AdminAddProductFormModel
    {
        [FromForm(Name = "product-group")]
        public Guid GroupId { get; set; }

        [FromForm(Name = "product-id")]
        public Guid? ProductId { get; set; }

        [FromForm(Name = "product-name")]
        public String Name { get; set; } = null!;

        [FromForm(Name = "product-description")]
        public String? Description { get; set; } = null!;

        [FromForm(Name = "product-slug")]
        public String? Slug { get; set; } = null!;

        [FromForm(Name = "product-img")]
        public IFormFile? Image { get; set; } = null!;

        [FromForm(Name = "product-hidden")]
        public int IsHidden { get; set; } = 0;

        [FromForm(Name = "product-order")]
        public int Order { get; set; }        

        [FromForm(Name = "product-stock")]
        public int Stock { get; set; }        

        [FromForm(Name = "product-price")]
        public double Price { get; set; }
    }
}
