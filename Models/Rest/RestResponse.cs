namespace ASP_P42.Models.Rest
{
    public class RestResponse
    {
        public RestStatus Status { get; set; } = RestStatus.Ok;
        public RestMeta Meta { get; set; } = new();
        public Object? Data { get; set; }
    }
}
