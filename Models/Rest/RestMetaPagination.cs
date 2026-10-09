namespace ASP_P42.Models.Rest
{
    public class RestMetaPagination
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
    }
}
