using System;

namespace ArcadeMaker.Core.ExpSrc
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CategoryAttribute(string category) : Attribute
    {
        public string Category => category;
    }

    public static class KeyboardCategories
    {
        public const string Letters = "Letters";
        public const string Digits = "Digits";
        public const string Others = "Others";
        public const string FunctionKeys = "Function Keys";
    }
}