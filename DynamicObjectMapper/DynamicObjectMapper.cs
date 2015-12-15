using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicObjectMapper
{
    public class DynamicObjectMapper<T> : IDynamicObjectMapper<T> where T : new()
    {
        private class PropertyDetails
        {
            public string Name { get; set; }
            public MethodInfo Getter { get; set; }
            public Dictionary<string, PropertyDetails> Elements { get; set; }
        }

        private readonly Dictionary<string, PropertyDetails> propertiesDelegate;

        public DynamicObjectMapper()
        {
            this.propertiesDelegate = new Dictionary<string, PropertyDetails>(StringComparer.OrdinalIgnoreCase);
            this.CreatePropertiesMap(typeof(T), this.propertiesDelegate);
        }

        private void CreatePropertiesMap(Type objectType, Dictionary<string, PropertyDetails> propertiesDictionary)
        {
            foreach (PropertyInfo propertyInfo in objectType.GetProperties())
            {
                var propertyDetails = new PropertyDetails()
                {
                    Getter = propertyInfo.GetMethod,
                    Name = propertyInfo.Name
                };

                if (propertyInfo.PropertyType.IsClass &&
                    propertyInfo.PropertyType != typeof(string) &&
                    propertyInfo.PropertyType.GetProperties().Any())
                {
                    propertyDetails.Elements = new Dictionary<string, PropertyDetails>(StringComparer.OrdinalIgnoreCase);
                    propertiesDictionary.Add(propertyInfo.Name, propertyDetails);
                    this.CreatePropertiesMap(propertyInfo.PropertyType, propertyDetails.Elements);
                }
                else
                {
                    propertiesDictionary.Add(propertyInfo.Name, propertyDetails);
                }
            }
        }

        public IDictionary<string, object> Map(T source, params string[] properties)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var propertyName in properties)
            {
                this.AddObjectToDictionary(source, propertyName, result, this.propertiesDelegate);
            }

            return result;
        }

        private void AddObjectToDictionary(object source, string propertyName, Dictionary<string, object> outputParent, Dictionary<string, PropertyDetails> delegates)
        {
            var propertySections = propertyName.Split('.');
            var currentProperty = propertySections.FirstOrDefault();
            var currentPropertyChildren = string.Join(".", propertySections.Skip(1));

            PropertyDetails propertyDelegate;

            if (delegates.TryGetValue(currentProperty, out propertyDelegate) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(currentProperty), $"{currentProperty} is not a supported property");
            }

            if (!string.IsNullOrEmpty(currentPropertyChildren))
            {
                object container;
                if (outputParent.TryGetValue(propertyDelegate.Name, out container) == false)
                {
                    container = new Dictionary<string, object>();
                    outputParent.Add(propertyDelegate.Name, container);
                }

                this.AddObjectToDictionary(
                    source.GetType().GetProperty(propertyDelegate.Name).GetValue(source),
                    currentPropertyChildren,
                    (Dictionary<string, object>)container,
                    propertyDelegate.Elements);
            }
            else
            {
                outputParent.Add(propertyDelegate.Name, propertyDelegate.Getter.Invoke(source, null));
            }
        }
    }
}
