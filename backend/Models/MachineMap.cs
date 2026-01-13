using System.ComponentModel.DataAnnotations.Schema;

namespace EMSSolution.Models
{
    public class MachineMap
    {
        public int iMasterid { get; set; }

        public string IPAddress { get; set; } = string.Empty;
        [Column("machineId")]
        public string? MachineId { get; set; }
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
