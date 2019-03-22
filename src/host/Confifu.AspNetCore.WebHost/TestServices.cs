namespace Confifu.AspNetCore.WebHost
{
    public class ServiceA
    {
        public ServiceA(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
    }


    public class ServiceB
    {
        public ServiceB(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
    }
}