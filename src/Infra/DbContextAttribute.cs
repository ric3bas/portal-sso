namespace Portal.Infra
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DbContextAttribute : Attribute
    {
        public string ContextName { get; }

        public DbContextAttribute(string contextName)
        {
            ContextName = contextName;
        }
    }
}