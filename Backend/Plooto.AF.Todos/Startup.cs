using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Plooto.AF.Todos;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Plooto.AF.Todos
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

        }
    }
}
