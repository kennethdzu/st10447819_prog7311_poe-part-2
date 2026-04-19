using System.Threading.Tasks;

namespace TechMove.Glms.Web.Services;

public interface ICurrencyConversionStrategy
{
    Task<decimal> ConvertUsdToZarAsync(decimal usdAmount);
}
