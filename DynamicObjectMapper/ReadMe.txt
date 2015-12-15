Usage:

            public class MyObject()
            {
                public string StringProperty {get; set;}
                public bool BoolProperty {get; set;}
            };

            ...

            var myObject = new MyObject()
            {
                StringProperty = "abc",
                BoolProperty = true
            };

            var dynamicMapper = new Groundspeak.DynamicPropertyMapper.DynamicObjectMapper<MyObject>();
            // OR 
            // var dynamicMapper = new Groundspeak.DynamicPropertyMapper.DynamicObjectMapperIL<MyObject>();

            var mappedObject = dynamicMapper.Map(myObject, "BoolProperty, StringProperty");


Notes:
	Supports properties of inner objects can be referred to using the dot notations (i.e. "topLevelProperty.childrenPropertyName").

	ArgumentOutOfRangeException is thrown if a requested property is invalid.
