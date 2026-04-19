using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMove.Glms.Web.Models;

public abstract class Contract
{
    public int Id { get; set; }
    
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Draft"; // Supported: Draft, Active, On Hold, Expired
    
    [Required]
    [StringLength(50)]
    public string ServiceLevel { get; set; }
    
    // The relative path to the uploaded PDF agreement
    [StringLength(255)]
    [Display(Name = "Signed Agreement (PDF)")]
    public string? SignedAgreementPdfPath { get; set; }
    
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
