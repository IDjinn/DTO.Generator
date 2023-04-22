using System.Text.RegularExpressions;

namespace DTO.Generator
{
    public enum GenerateMode
    {
        All,
        Attribute
    }

    public enum TypeGenerated
    {
        Auto,
        Record,
        ReadonlyRecordStruct
    }

    public class NamingConvention
    {
        public Regex Interface { get; set; }
        public string Generated { get; set; }
    }


    public class DTOGeneratorSettings
    {
        public GenerateMode Generate { get; set; } = GenerateMode.All;
        public TypeGenerated TypeGenerated { get; set; } = TypeGenerated.Auto;

        public NamingConvention NamingConvention { get; set; } = new NamingConvention()
        {
            Interface = new Regex(@"^I([\w]+)"),
            Generated = "{Name}"
        };
    }
}