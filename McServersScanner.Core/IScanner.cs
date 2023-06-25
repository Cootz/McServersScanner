namespace McServersScanner.Core
{
    internal interface IScanner : IScannerOptions
    {
        /// <summary>
        /// <see cref="Stream"/> to write information into
        /// </summary>
        Stream OutputStream { get; }
    }
}
