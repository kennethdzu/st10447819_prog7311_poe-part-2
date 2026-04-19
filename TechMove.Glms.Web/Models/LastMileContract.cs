using System.ComponentModel.DataAnnotations;

namespace TechMove.Glms.Web.Models;

public class LastMileContract : Contract
{
    [Display(Name = "Delivery Radius (km)")]
    public double DeliveryRadius { get; set; }
}
