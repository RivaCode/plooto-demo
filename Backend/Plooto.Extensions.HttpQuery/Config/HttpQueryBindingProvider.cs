using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Plooto.Extensions.HttpQuery.Config
{
    internal class HttpQueryBindingProvider : IBindingProvider
    {
        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var pi = context.Parameter;
            var queryAttr = pi.GetCustomAttribute<HttpQueryAttribute>(inherit: false);
            if (queryAttr == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            if (pi.ParameterType.IsAbstract || pi.ParameterType.IsInterface)
            {
                throw new InvalidOperationException($"Can't bind {nameof(HttpQueryAttribute)} to type {pi.ParameterType.Name}");
            }

            var jsonSettings = new JsonSerializerSettings
            {
                Converters = pi.GetCustomAttributes<JsonConverterAttribute>(inherit: false)
                    .Select(x => Activator.CreateInstance(x.ConverterType))
                    .Cast<JsonConverter>()
                    .ToList()
            };
            return Task.FromResult<IBinding>(new HttpQueryBinding(pi.ParameterType, jsonSettings));
        }

        private class HttpQueryBinding : IBinding
        {
            private const string KEY = "Query";
            private readonly Type _parameterType;
            private readonly JsonSerializerSettings _settings;

            public HttpQueryBinding(Type parameterType, JsonSerializerSettings settings)
            {
                _parameterType = parameterType;
                _settings = settings;
            }

            public bool FromAttribute => true;

            public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
            {
                if (value is IDictionary<string, string> queryDic)
                {
                    return Task.FromResult<IValueProvider>(
                        new HttpQueryValueProvider(
                            JObject
                                .FromObject(queryDic)
                                .ToObject(_parameterType, JsonSerializer.Create(_settings))));
                }

                throw new NotSupportedException($"An {nameof(IDictionary<string, string>)} is required");
            }

            public Task<IValueProvider> BindAsync(BindingContext context)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));

                return BindAsync(
                    context.BindingData[KEY], context.ValueContext);
            }

            public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();
        }

        private class HttpQueryValueProvider : IValueProvider
        {
            private readonly object _value;

            public HttpQueryValueProvider(object value) => _value = value;

            public Type Type => _value.GetType();

            public Task<object> GetValueAsync() => Task.FromResult(_value);
            public string ToInvokeString() => _value.ToString();
        }
    }
}
