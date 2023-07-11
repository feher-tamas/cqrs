namespace FT.CQRS.Decorators
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class QueryLogAttribute : Attribute
    {
        public QueryLogAttribute()
        {
        }
    }
}