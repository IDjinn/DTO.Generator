using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTO.Generator.Tests
{
    [UsesVerify]
    public class DTOGeneratorTests
    {
        [Fact]
        public Task readonly_record_struct_test()
        {
            var source = @"
                namespace Test;

                public interface IValueTypeThing { 
                    public bool Flag { get; } 
                }
            ";

            return TestHelper.Verify(source);
        }

        [Fact]
        public Task record_test()
        {
            var source = @"
                namespace Test;

                public record Test(string other);    
                public interface IRefTypeThing { 
                    public bool Flag { get; }
                    public Test Test { get; }
                }
            ";

            return TestHelper.Verify(source);
        }


        [Fact]
        public Task record_test_with_protected_members()
        {
            var source = @"
                namespace Test;

                public record Test(string other);    
                public interface IValueTypeAndProtectedRef { 
                    public bool Flag { get; }
                    protected Test Test { get; }
                }
            ";

            return TestHelper.Verify(source);
        }


        [Fact]
        public Task test_attribute_settings()
        {
            var settings = new DTOGeneratorSettings();
            settings.Generate = GenerateMode.Attribute;

            var source = @"
                using DTO.Generator;
                namespace Test;

                public interface IShoudNeverGenerate { 
                    public bool Flag { get; } 
                }

                [DTO]
                public interface IValueTypeThing { 
                    public bool Flag { get; } 
                }
            ";

            return TestHelper.Verify(source, settings);
        }
    }
}