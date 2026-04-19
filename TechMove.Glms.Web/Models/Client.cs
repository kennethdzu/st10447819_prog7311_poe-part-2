using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechMove.Glms.Web.Models;

public class Client
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    [StringLength(200)]
    public string ContactDetails { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Region { get; set; }
    
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
