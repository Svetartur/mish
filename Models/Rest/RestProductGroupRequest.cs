namespace ASP_P42.Models.Rest
{
    public class RestProductGroupRequest
    {
        public Guid? ParentId { get; set; }
        public String Name { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;
        public String Slug { get; set; } = String.Empty;
        public String? ImageUrl { get; set; }
        public int IsHidden { get; set; }
        public int OrderInPrice { get; set; } = 100000;
    }
}
