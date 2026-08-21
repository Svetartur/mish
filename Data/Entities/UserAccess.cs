namespace ASP_P42.Data.Entities
{
    public class UserAccess
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public String Login { get; set; } = null!;
        public String Salt { get; set; } = null!;
        public String Dk { get; set; } = null!;

        public UserData UserData { get; set; } = null!;
        public UserRole UserRole { get; set; } = null!;
    }
}
