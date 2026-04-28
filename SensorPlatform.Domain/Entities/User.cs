namespace SensorPlatform.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
