namespace Domain.DTos
{
    public class Jwt
    {
        public string Secretkey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double ExpiryDurationInMinutes { get; set; }
    }
}
