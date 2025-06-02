namespace Bruh.Model.Models
{
    internal class DBContextAttribute : Attribute
    {
        public DBContextAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}