using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicObjectMapper.Tests
{
    [TestFixture]
    public class MappingTests
    {
        [SetUp]
        public void Init()
        {
        }

        [Test]
        public void DynamicMapper_NoPropertiesRequested_ReturnsEmptyObject(
            [Values(typeof(DynamicObjectMapperIL<TestObject.ComplexObject>), typeof(DynamicObjectMapper<TestObject.ComplexObject>))] Type mapper)
        {
            var constructor = mapper.GetConstructor(Type.EmptyTypes);
            var mapperInstance = constructor.Invoke(null) as IDynamicObjectMapper<TestObject.ComplexObject>;
            var mappedObject = TestObject.CreateTestObject<TestObject.ComplexObject>();
            var result = mapperInstance.Map(mappedObject);
            result.Keys.Count.ShouldBe(0);
        }

        [Test]
        public void DynamicMapper_SimpleObject_RequestAllProperties_ReturnsAllProperties(
            [Values(typeof(DynamicObjectMapperIL<TestObject.ComplexObject>), typeof(DynamicObjectMapper<TestObject.ComplexObject>))] Type mapper)
        {
            var constructor = mapper.GetConstructor(Type.EmptyTypes);
            var mapperInstance = constructor.Invoke(null) as IDynamicObjectMapper<TestObject.ComplexObject>;
            var mappedObject = TestObject.CreateTestObject<TestObject.ComplexObject>();
            var publicPropertiesNames =
                typeof (TestObject.ComplexObject)
                .GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name).ToArray();
            var result = mapperInstance.Map(mappedObject, publicPropertiesNames);
            var combinedSet = result.Keys.Union(publicPropertiesNames, StringComparer.InvariantCultureIgnoreCase).ToArray();

            // Unioning the result set with the requested set shouldn't result in extra elements
            combinedSet.Length.ShouldBe(publicPropertiesNames.Length);

            // We shouldn't have any elements in the requested set missing from the results
            combinedSet.Except(result.Keys, StringComparer.InvariantCultureIgnoreCase).Count().ShouldBe(0);
        }

        [Test]
        public void DynamicMapper_SimpleObject_RequestAllPropertiesDifferentCasing_ReturnsAllProperties(
            [Values(typeof(DynamicObjectMapperIL<TestObject.ComplexObject>), typeof(DynamicObjectMapper<TestObject.ComplexObject>))] Type mapper)
        {
            var constructor = mapper.GetConstructor(Type.EmptyTypes);
            var mapperInstance = constructor.Invoke(null) as IDynamicObjectMapper<TestObject.ComplexObject>;
            var mappedObject = TestObject.CreateTestObject<TestObject.ComplexObject>();
            var publicPropertiesNames =
                typeof(TestObject.ComplexObject)
                .GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name.AlternateCasing()).ToArray();
            var result = mapperInstance.Map(mappedObject, publicPropertiesNames);
            var combinedSet = result.Keys.Union(publicPropertiesNames, StringComparer.InvariantCultureIgnoreCase).ToArray();

            // Unioning the result set with the requested set shouldn't result in extra elements
            combinedSet.Length.ShouldBe(publicPropertiesNames.Length);

            // We shouldn't have any elements in the requested set missing from the results
            combinedSet.Except(result.Keys, StringComparer.InvariantCultureIgnoreCase).Count().ShouldBe(0);
        }

        [Test]
        public void DynamicMapper_SimpleObject_RequestNonExistingProperties_ShouldFail(
            [Values(typeof(DynamicObjectMapperIL<TestObject.SimpleObject>), typeof(DynamicObjectMapper<TestObject.SimpleObject>))] Type mapper)
        {
            var constructor = mapper.GetConstructor(Type.EmptyTypes);
            var mapperInstance = constructor.Invoke(null) as IDynamicObjectMapper<TestObject.SimpleObject>;
            var mappedObject = TestObject.CreateTestObject<TestObject.SimpleObject>();
            Should.Throw<ArgumentOutOfRangeException>(() => mapperInstance.Map(mappedObject, new[] { "fakeProperty" }));
        }

        [Test]
        public void DynamicMapper_SimpleObject_RequestInternalProperties_ShouldFail(
            [Values(typeof(DynamicObjectMapperIL<TestObject.SimpleObject>), typeof(DynamicObjectMapper<TestObject.SimpleObject>))] Type mapper)
        {
            var constructor = mapper.GetConstructor(Type.EmptyTypes);
            var mapperInstance = constructor.Invoke(null) as IDynamicObjectMapper<TestObject.SimpleObject>;
            var mappedObject = TestObject.CreateTestObject<TestObject.SimpleObject>();
            Should.Throw<ArgumentOutOfRangeException>(() => mapperInstance.Map(mappedObject, new [] { "internalProperty" }));
        }

        [Test]
        public void DynamicMapper_SimpleObject_RequestDottedProperty_ReturnsProperty(
            [Values(typeof(DynamicObjectMapperIL<TestObject.ComplexObject>), typeof(DynamicObjectMapper<TestObject.ComplexObject>))] Type mapper)
        {
            var constructor = mapper.GetConstructor(Type.EmptyTypes);
            var mapperInstance = constructor.Invoke(null) as IDynamicObjectMapper<TestObject.ComplexObject>;
            const string RequestedPropertyName = "OtherObjectProperty.StringProperty";
            const string ExpectedPropertyValue = "Test";
            var mappedObject = TestObject.CreateTestObject<TestObject.ComplexObject>(c => 
                c.OtherObjectProperty = new TestObject.SimpleObject() { StringProperty = ExpectedPropertyValue });
            var result = mapperInstance.Map(mappedObject, RequestedPropertyName.AlternateCasing());
            result.Keys.Count.ShouldBe(1);
            this.EnsurePropertyIsReturned((object)result, RequestedPropertyName, ExpectedPropertyValue);
        }

        [Test]
        public void DynamicMapper_SimpleObject_RequestDeepDottedProperty_ReturnsProperty(
            [Values(typeof(DynamicObjectMapperIL<TestObject.DeepObject>), typeof(DynamicObjectMapper<TestObject.DeepObject>))] Type mapper)
        {
            var constructor = mapper.GetConstructor(Type.EmptyTypes);
            var mapperInstance = constructor.Invoke(null) as IDynamicObjectMapper<TestObject.DeepObject>;
            const string RequestedPropertyName = "ComplexObjectProperty.OtherObjectProperty.StringProperty";
            const string ExpectedPropertyValue = "Test";
            var mappedObject = TestObject.CreateTestObject<TestObject.DeepObject>(c =>
                c.ComplexObjectProperty = new TestObject.ComplexObject()
                {
                    OtherObjectProperty = new TestObject.SimpleObject()
                    { 
                        StringProperty = ExpectedPropertyValue
                    }
                });
            var result = mapperInstance.Map(mappedObject, RequestedPropertyName.AlternateCasing());
            result.Keys.Count.ShouldBe(1);
            this.EnsurePropertyIsReturned((object)result, RequestedPropertyName, ExpectedPropertyValue);
        }

        private void EnsurePropertyIsReturned(object result, string requestedPropertyName, object expectedPropertyValue)
        {
            var valuePortion = (object)result;
            requestedPropertyName.Split(new[] { '.' }).ToList().ForEach(p =>
            {
                var asDictionary = valuePortion as Dictionary<string, object>;
                if (asDictionary != null)
                {
                    asDictionary.Keys.Count().ShouldBe(1);
                    asDictionary.Keys.ShouldContain(p);
                    valuePortion = asDictionary[p];
                }
                else
                {
                    valuePortion.ShouldBe(expectedPropertyValue);
                }
            });
        }
    }

    internal static class TestExtensions
    {
        public static string AlternateCasing(this string request)
        {
            string returnValue = string.Empty;
            bool upper = true;
            request.ToCharArray().ToList().ForEach(c =>
                {
                    returnValue += upper
                        ? c.ToString().ToUpperInvariant()
                        : c.ToString().ToLowerInvariant();
                    upper = !upper;
                });
            return returnValue;
        }
    }
}
