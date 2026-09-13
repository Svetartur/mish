namespace ASP_P42.Models.Admin
{
    public class AdminGroupViewModel
    {
        public AdminAddGroupFormModel? FormModel { get; set; }
        public List<Data.Entities.ProductGroup> Groups { get; set; } = [];
    }
}
