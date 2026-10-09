namespace ASP_P42.Models.Rest
{
    public record RestProductGroup
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public String Name { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;
        public String Slug { get; set; } = String.Empty;
        public String? ImageUrl { get; set; }
        public int IsHidden { get; set; }
        public int OrderInPrice { get; set; }
        public List<RestProductGroup> Children { get; set; } = [];
    }
}
