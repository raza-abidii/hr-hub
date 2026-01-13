namespace EMSSolution.Models
{
    public class BiometricRequest
    {
        public string ipAddress { get; set; } = string.Empty;
        public int machineId { get; set; }
        public int portNo { get; set; }
    }
}
