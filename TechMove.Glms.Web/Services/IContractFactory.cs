namespace TechMove.Glms.Web.Services;

using TechMove.Glms.Web.Models;



public interface IContractFactory
{

 
    Contract Create(string contractType);
}
