namespace NH.Settings
{
    public class NugetSource
    {
        public int Order { get; set; }
        public bool IsPrivate { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public NugetCredentials Credentials { get; set; }
    }
}