using System.ComponentModel.DataAnnotations;

namespace EMSSolution.Models
{
    public class Designation
    {
        public int iMasterid { get; set; }

        public string sName { get; set; } = string.Empty;
        
        public string sCode { get; set; } = string.Empty;
        public string sDescription { get; set; } = string.Empty;
    }
}
