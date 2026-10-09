namespace ASP_P42.Models.Rest
{
    public class RestMeta
    {
        public String ApiName { get; set; } = String.Empty;
        public long ServerTime { get; set; } = (DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks) / 10000;
        public String DataType { get; set; } = "application/json";
        public String[] Manipulations { get; set; } = [];
        public int? CacheTime { get; set; }
        public Dictionary<String, String> Links { get; set; } = [];
        public RestMetaPagination? Pagination { get; set; }
        public Dictionary<String, Object> Parameters { get; set; } = [];
    }
}
