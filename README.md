# McServersScanner

Scan a range of ips for running mc servers

# Building & Running

Please make sure you have the following prerequisites:

- A desktop platform with the [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed.

### Download the source code

Clone the repository:

```shell
git clone https://github.com/Cootz/McServersScanner.git
cd McServersScanner
```

### Building

To build McServersScanner from the command line, run the following command:

```shell
dotnet build McServersScanner.sln
```

If you are not interested in debugging McServerScanner, you can add `-c Release` to gain performance.

### Running

To run McServerScanner go to the build directory and run the executable or run the following commands:

```shell
cd McServersScanner\bin\Debug\net6.0 
McServersScanner.exe --help
```

*Note: Change 'Debug' to 'Release' if you built with `-c Release` option*
