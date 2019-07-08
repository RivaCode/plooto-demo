using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Plooto.AF.Todos;
using Plooto.AF.Todos.Models;

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
