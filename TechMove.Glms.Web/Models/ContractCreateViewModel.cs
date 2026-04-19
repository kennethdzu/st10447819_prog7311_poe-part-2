using System;
using System.ComponentModel.DataAnnotations;

namespace TechMove.Glms.Web.Models;

public class ContractCreateViewModel
{
    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Draft";
    
    [Required]
    [StringLength(50)]
    [Display(Name = "Service Level")]
    public string ServiceLevel { get; set; }
}
