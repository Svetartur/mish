namespace ASP_P42.Models.Home.Models
{
    public class HomeModelsViewModel
    {
        public String PageTitle { get; set; } = null!;
        public String Intro { get; set; } = null!;
        public String ClassificationHeader { get; set; } = null!;
        public String ExampleHeader { get; set; } = null!;
        public List<String> ClassificationList { get; set; } = [];
        public List<String> ExampleList { get; set; } = [];
    }
}
