[![Build status](https://img.shields.io/appveyor/ci/alunacjones/lsl-sentinet-apiclient.svg)](https://ci.appveyor.com/project/alunacjones/lsl-sentinet-apiclient)
[![Coveralls branch](https://img.shields.io/coverallsCoverage/github/alunacjones/LSL.Sentinet.ApiClient)](https://coveralls.io/github/alunacjones/LSL.Sentinet.ApiClient)
[![NuGet](https://img.shields.io/nuget/v/LSL.Sentinet.ApiClient.svg)](https://www.nuget.org/packages/LSL.Sentinet.ApiClient/)

# LSL.Sentinet.ApiClient

Provide documentation here

## Using in a .NET versions 4.6.1 and higher

If the target application is using `Microsoft.Extensions.DependencyInjection` then you can easily register everything
needed to use the services in this library via the following:

```csharp
// services is an `IServiceCollection` as per the standard DI registration process
services.AddSentinetApiClient(c =>
{
    c.BaseUrl = "https://mysentinet/Sentinet";

    // NEVER have your credentials in code or in a configuration file
    // Always elect to use a secure mechanism to retrieve those credentials
    c.Username = "my-user";
    c.Password = "my-password";
}); 
```

## `net451` Caveats

Any `net451` application consuming this library will need to....

**PROVIDE DOCS HERE**