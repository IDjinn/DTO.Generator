using System;

namespace DTO.Generator
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class DTOGeneratorAttribute : Attribute
    {
    }
}