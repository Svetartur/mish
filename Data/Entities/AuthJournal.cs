namespace ASP_P42.Data.Entities
{
    public class AuthJournal
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }
        public String Login { get; set; } = null!;
        public String Dk { get; set; } = null!;
        public bool IsOk { get; set; }
    }
}
