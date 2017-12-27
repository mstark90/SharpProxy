using System;
namespace SharpProxyTest
{
    public interface IConsoleWriter
    {
        string Value
        {
            get;
            set;
        }
        void Write(string value);
    }
}
