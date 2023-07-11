namespace FT.CQRS.Decorators
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class EventLogAttribute : Attribute
    {
        public EventLogAttribute()
        {
        }
    }
}