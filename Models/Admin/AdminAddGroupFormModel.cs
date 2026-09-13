using Microsoft.AspNetCore.Mvc;

namespace ASP_P42.Models.Admin
{
    public class AdminAddGroupFormModel
    {
        [FromForm(Name = "group-parent")]
        public Guid? ParentId { get; set; } = null!;
        
        [FromForm(Name = "group-name")]
        public String Name { get; set; } = null!;
        
        [FromForm(Name = "group-description")]
        public String Description { get; set; } = null!;
        
        [FromForm(Name = "group-slug")]
        public String Slug { get; set; } = null!;

        [FromForm(Name = "group-img")]
        public IFormFile Image { get; set; } = null!;

        [FromForm(Name = "group-hidden")]
        public int IsHidden { get; set; } = 0;

        [FromForm(Name = "group-order")]
        public int Order { get; set; }
    }
}
