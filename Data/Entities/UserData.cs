namespace ASP_P42.Data.Entities
{
    public class UserData
    {
        public Guid Id { get; set; }
        public String FullName { get; set; } = null!;
        public String Email { get; set; } = null!;
        public String? Phone { get; set; } = null!;
        public DateTime Birthdate { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<UserAccess> Accesses { get; set; } = [];
    }
}