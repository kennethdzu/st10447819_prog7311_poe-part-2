namespace TechMove.Glms.Web.Services;

using TechMove.Glms.Web.Models;

/// <summary>
/// Makes new contracts based on the type.
/// </summary>
public interface IContractFactory
{
    /// <summary>
    /// Creates the correct concrete Contract subtype based on the supplied type name.
    /// </summary>
    /// <param name="contractType">One of "Freight", "Warehousing", or "LastMile".</param>
    /// <returns>A new, uninitialised Contract of the correct subtype.</returns>
    /// <exception cref="ArgumentException">Thrown when the contractType is not recognised.</exception>
    Contract Create(string contractType);
}
