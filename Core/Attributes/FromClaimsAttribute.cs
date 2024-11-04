using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FromClaimsAttribute : BindingBehaviorAttribute
    {
        public string? PropertyName { get; }

        public FromClaimsAttribute(string? propertyName)
            : base(BindingBehavior.Never)
        {
            PropertyName = propertyName;
        }
    }
}
