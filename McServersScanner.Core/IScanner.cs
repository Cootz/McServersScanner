namespace McServersScanner.Core
{
    internal interface IScanner : IScannerOptions
    {
        Stream OutputStream { get; }
    }
}
