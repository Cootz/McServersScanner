namespace McServersScanner.Core
{
    internal interface IScanner : IScannerOptions
    {
        /// <summary>
        /// <see cref="Stream"/> to write information into
        /// </summary>
        StreamWriter OutputStream { get; }
    }
}
