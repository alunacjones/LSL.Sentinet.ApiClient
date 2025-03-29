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
    // Always elect to use a secure mehanism to retrieve those credentials
    c.Username = "my-user";
    c.Password = "my-password";
}); 
```

## `net451` Caveats

Any `net451` application consuming this library will need to....

**PROVIDE DOCS HERE**