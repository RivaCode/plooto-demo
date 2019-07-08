using Microsoft.Azure.WebJobs.Description;
using System;

namespace Plooto.Extensions.HttpQuery
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class HttpQueryAttribute : Attribute
    {
    }
}
