using System;
using System.Collections.Generic;

namespace DynamicObjectMapper.Tests
{
    public abstract class TestObject
    {
        public static T CreateTestObject<T>(Action<T> action = null) where T : TestObject, new()
        {
            var augmentedObject = new T();

            if (action != null)
            {
                action.Invoke(augmentedObject);
            }

            return augmentedObject;
        }

        public class SimpleObject : TestObject
        {
            public string StringProperty { get; set; }
            public bool BoolProperty { get; set; }
            internal int InternalProperty { get; set; }
        }

        public class ComplexObject : TestObject
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public int? NullableIntProperty { get; set; }
            public double DoubleProperty { get; set; }
            public SimpleObject OtherObjectProperty { get; set; }
            public List<SimpleObject> ListProperty { get; set; }
        }

        public class DeepObject : TestObject
        {
            public int IntProperty { get; set; }
            public ComplexObject ComplexObjectProperty { get; set; }
        }
    }
}
