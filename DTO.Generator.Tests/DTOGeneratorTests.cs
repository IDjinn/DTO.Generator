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

                public interface IThing { 
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
                public interface IThing { 
                    public bool Flag { get; }
                    public Test teste { get; }
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
                public interface IThing { 
                    public bool Flag { get; }
                    protected Test teste { get; }
                }
            ";

            return TestHelper.Verify(source);
        }
    }
}