namespace Domain.Models
{
    public class RevokedToken
    {
        public int Id { get; set; }
        public string Jti { get; set; } // unique token ID
        public DateTime ExpirationDate { get; set; }
    }
}
