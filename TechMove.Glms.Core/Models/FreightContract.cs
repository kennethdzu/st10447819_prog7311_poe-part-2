using System.ComponentModel.DataAnnotations;

namespace TechMove.Glms.Web.Models;

public class FreightContract : Contract
{
    [Display(Name = "Weight Limit (kg)")]
    public double WeightLimit { get; set; }
    
    [StringLength(200)]
    public string? Route { get; set; }
}
