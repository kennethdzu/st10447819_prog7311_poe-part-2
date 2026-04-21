namespace TechMove.Glms.Web.Services;

using TechMove.Glms.Web.Models;


/// Makes new contracts based on the type.

public interface IContractFactory
{

    /// Creates the correct concrete Contract subtype based on the supplied type name.
 
    Contract Create(string contractType);
}
