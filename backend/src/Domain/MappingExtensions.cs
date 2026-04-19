using System.Reflection;

namespace Portal.Domain
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;

    namespace Portal.Extensions
    {
        public static class MappingExtensions
        {
            private static readonly ConcurrentDictionary<(Type origem, Type destino), PropertyInfo[]> _cache
                = new ConcurrentDictionary<(Type, Type), PropertyInfo[]>();

            public static T ToResponse<T>(this object origem, params string[]? propriedades) where T : new()
            {
                if (origem == null) return default!;
                var destino = new T();
                MapProperties(origem, destino, propriedades);
                return destino;
            }

            private static bool IsSimpleType(Type type)
            {
                return type.IsPrimitive
                       || type.IsEnum
                       || type == typeof(string)
                       || type == typeof(decimal)
                       || type == typeof(Guid)
                       || type == typeof(DateTime)
                       || type == typeof(DateTimeOffset)
                       || type == typeof(TimeSpan);
            }

            private static void MapProperties(object origem, object destino, string[] propriedades)
            {
                var tipoOrigem = origem.GetType();
                var tipoDestino = destino.GetType();

                if (propriedades == null || propriedades.Length == 0)
                {
                    propriedades = tipoDestino.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .Select(p => p.Name)
                                              .ToArray();
                }

                var props = tipoDestino.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                       .Where(p => tipoOrigem.GetProperty(p.Name) != null)
                                       .ToArray();

                foreach (var prop in propriedades)
                {
                    var propDestino = props.FirstOrDefault(p => p.Name == prop);
                    var propOrigem = tipoOrigem.GetProperty(prop, BindingFlags.Public | BindingFlags.Instance);

                    if (propOrigem != null && propDestino != null)
                    {
                        var valor = propOrigem.GetValue(origem);

                        if (valor != null && !IsSimpleType(propOrigem.PropertyType))
                        {
                            // Objeto complexo → recursão
                            var subDestino = Activator.CreateInstance(propDestino.PropertyType);
                            MapProperties(valor, subDestino, null);
                            propDestino.SetValue(destino, subDestino);
                        }
                        else
                        {
                            // Tipos simples (inclui Guid, DateTime, etc.) → atribuição direta
                            propDestino.SetValue(destino, valor);
                        }
                    }
                }
            }
        }
    }
}
