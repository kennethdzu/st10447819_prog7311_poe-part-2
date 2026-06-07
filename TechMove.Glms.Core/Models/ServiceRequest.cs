using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.Glms.Web.Models;

public class ServiceRequest
{
    public int Id { get; set; }
    
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Cost (ZAR)")]
    public decimal Cost { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending";
}
