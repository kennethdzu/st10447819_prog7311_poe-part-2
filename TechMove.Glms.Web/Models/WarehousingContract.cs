using System.ComponentModel.DataAnnotations;

namespace TechMove.Glms.Web.Models;

public class WarehousingContract : Contract
{
    [Display(Name = "Capacity (m\\u00b3)")] // Measurement in cubic meters
    public int Capacity { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Temperature Zone")]
    public string? TemperatureZone { get; set; }
}
