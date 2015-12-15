using Benchmark.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicObjectMapper;

/*
In the Package Manager Console, 'cd' to the bin\[Debug|Release] directory.
Then run the 'Start-Benchmark Benchmark.dll' command
*/
namespace Benchmark
{
    public class Constants
    {
        public Random random = new Random((int)DateTime.UtcNow.Ticks);

        public static readonly string[] AvailableProperties =
        {
            "referenceCode",
            "activityOwner.referenceCode",
            "dateTimeCreatedUtc",
            "activityDate",
            "isArchived",
            "activityText",
            "url",
            "activityType",
            "updatedLatitude",
            "updatedLongitude",
            "isTextRot13",
            "isApproved",
            "cannotDelete",
            "geocache.referencecode"
        };

        public static readonly uint PropertiesCount = (uint)AvailableProperties.Length;

        public const double PropertyChances = 0.3;

        public string[] GetRandomProperties()
        {
            return AvailableProperties.Where(p => this.random.NextDouble() < PropertyChances).ToArray();
        }
    }

    public class SimpleObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public interface ISimpleObjectMapper
    {
        void Map(SimpleObject @object);
    }

    public interface IComplexObjectMapper
    {
        void Map(GeocacheActivity @object);
    }

    public class IL_PerCollection_SimpleObjectMapper : ISimpleObjectMapper
    {
        private static readonly DynamicObjectMapperIL<SimpleObject> Mapper = new DynamicObjectMapperIL<SimpleObject>();

        public void Map(SimpleObject @object)
        {
            Mapper.Map(@object, "Id", "Name");
        }
    }

    public class CS_PerType_SimpleObjectMapper : ISimpleObjectMapper
    {
        private static readonly DynamicObjectMapper<SimpleObject> Mapper = new DynamicObjectMapper<SimpleObject>();

        public void Map(SimpleObject @object)
        {
            Mapper.Map(@object, "Id", "Name");
        }
    }

    public class IL_PerCollection_DotProps : IComplexObjectMapper
    {
        private static readonly DynamicObjectMapperIL<GeocacheActivity> Mapper = new DynamicObjectMapperIL<GeocacheActivity>();

        public void Map(GeocacheActivity @object)
        {
            Mapper.Map(@object, "referenceCode", "activityOwner.referenceCode", "dateTimeCreatedUtc", "activityDate", "isArchived", "activityText", "url", "activityType", "updatedLatitude", "updatedLongitude", "isTextRot13", "isApproved", "cannotDelete", "geocache.referencecode");
        }
    }
    public class IL_PerCollection_NoDotProps : IComplexObjectMapper
    {
        private static readonly DynamicObjectMapperIL<GeocacheActivity> Mapper = new DynamicObjectMapperIL<GeocacheActivity>();

        public void Map(GeocacheActivity @object)
        {
            Mapper.Map(@object, "referenceCode", "activityOwner", "dateTimeCreatedUtc", "activityDate", "isArchived", "activityText", "url", "activityType", "updatedLatitude", "updatedLongitude", "isTextRot13", "isApproved", "cannotDelete", "geocache");
        }
    }

    public class CS_PerType_DotProps : IComplexObjectMapper
    {
        private static readonly DynamicObjectMapperIL<GeocacheActivity> Mapper = new DynamicObjectMapperIL<GeocacheActivity>();

        public void Map(GeocacheActivity @object)
        {
            Mapper.Map(@object, "referenceCode", "activityOwner.referenceCode", "dateTimeCreatedUtc", "activityDate", "isArchived", "activityText", "url", "activityType", "updatedLatitude", "updatedLongitude", "isTextRot13", "isApproved", "cannotDelete", "geocache.referencecode");
        }
    }
    public class CS_PerType_NoDotProps : IComplexObjectMapper
    {
        private static readonly DynamicObjectMapper<GeocacheActivity> Mapper = new DynamicObjectMapper<GeocacheActivity>();

        public void Map(GeocacheActivity @object)
        {
            Mapper.Map(@object, "referenceCode", "activityOwner", "dateTimeCreatedUtc", "activityDate", "isArchived", "activityText", "url", "activityType", "updatedLatitude", "updatedLongitude", "isTextRot13", "isApproved", "cannotDelete", "geocache");
        }
    }
    public class IL_PerCollection_RandomProps : IComplexObjectMapper
    {
        private static readonly DynamicObjectMapperIL<GeocacheActivity> Mapper = new DynamicObjectMapperIL<GeocacheActivity>();

        public void Map(GeocacheActivity @object)
        {
            Mapper.Map(@object, (new Constants()).GetRandomProperties());
        }
    }

    public class CS_PerType_RandomProps : IComplexObjectMapper
    {
        private static readonly DynamicObjectMapper<GeocacheActivity> Mapper = new DynamicObjectMapper<GeocacheActivity>();

        public void Map(GeocacheActivity @object)
        {
            Mapper.Map(@object, (new Constants()).GetRandomProperties());
        }
    }

    public class ComplexObjectMapperBenchmark : Benchmarque.Benchmark<IComplexObjectMapper>
    {
        public void WarmUp(IComplexObjectMapper instance)
        {
            for (int i = 0; i < 10; i++)
            {
                instance.Map(new GeocacheActivity
                {
                    ActivityDate = DateTime.Now,
                    ActivityOwner = new Account() { Id = 1, ReferenceCode = "test" },
                    Id = 1,
                    ReferenceCode = "test",
                    ActivityText = "test",
                    ActivityType = GeocacheActivityType.ArchiveNoShow,
                    ActivityTypeID = i,
                    CannotDelete = true,
                    DateTimeCreatedUtc = DateTime.Now,
                    Geocache = new Geocache() { Id = 1000, ReferenceCode = "test" },
                    IsApproved = true,
                    IsArchived = true,
                    IsTextRot13 = true,
                    UpdatedLatitude = double.MaxValue,
                    UpdatedLongitude = double.MaxValue,
                    Url = "test"
                });
            }
        }

        public void Shutdown(IComplexObjectMapper instance)
        { }

        public void Run(IComplexObjectMapper instance, int iterationCount)
        {
            for (int i = 0; i < iterationCount; i++)
            {
                instance.Map(new GeocacheActivity
                {
                    ActivityDate = DateTime.Now,
                    ActivityOwner = new Account() { Id = 1, ReferenceCode = "test" },
                    Id = i,
                    ReferenceCode = "test",
                    ActivityText = "test",
                    ActivityType = GeocacheActivityType.Visited,
                    ActivityTypeID = i,
                    CannotDelete = true,
                    DateTimeCreatedUtc = DateTime.Now,
                    Geocache = new Geocache() { Id = 1000, ReferenceCode = "test" },
                    IsApproved = i % 2 == 0,
                    IsArchived = true,
                    IsTextRot13 = true,
                    UpdatedLatitude = double.MaxValue,
                    UpdatedLongitude = double.MaxValue,
                    Url = "test"
                });

            }
        }

        public IEnumerable<int> Iterations => new[] { 100000 };
    }
}
