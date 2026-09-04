using System.Text.Json.Serialization;

namespace ASP_P42.Models.User
{
    public class UserSignupFormModel
    {
        [JsonPropertyName("name")]
        public String FullName { get; set; } = null!;
        
        [JsonPropertyName("login")]
        public String Login { get; set; } = null!;
        
        [JsonPropertyName("email")]
        public String Email { get; set; } = null!;
        
        [JsonPropertyName("phone")]
        public String? Phone { get; set; }
        
        [JsonPropertyName("password")]
        public String Password { get; set; } = null!;
        
        [JsonPropertyName("repeat")]
        public String Repeat { get; set; } = null!;
        
        [JsonPropertyName("isAgree")]
        public bool IsAgree { get; set; }
    }
}
