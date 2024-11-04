namespace Core.Attributes
{
    public class MigrationAttribute : FluentMigrator.MigrationAttribute
    {
        public MigrationAttribute(long version, string? description = null) : base(version, description)
        {
        }
        
        public MigrationAttribute(int year, int month, int day, int hours, int minutes, string? description = null) 
            : base(long.Parse(string.Join("", new int[] { year, month, day, hours, minutes }.Select(x => x.ToString("D2")))), description)
        {
            var a = string.Join("", new int[] { year, month, day, hours, minutes }.Select(x => x.ToString("D2")));
        }
    }
}
