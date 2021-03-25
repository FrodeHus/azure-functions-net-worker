## Sample configuration

The middleware expects Azure AD configuration in the form of:

```json
{
    "AzureAd": {
        "Instance": "https://login.microsoftonline.com/",
        "TenantId": "your tenant id",
        "ClientId": "your application id"
    }
}
```

```csharp
public static void Main()
        {
            var host = new HostBuilder()
             .ConfigureAppConfiguration(c =>
                {
                    c.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    c.Build();
                })
                .ConfigureServices((b, s) =>
                {
                    s.AddOptions();
                    s.Configure<AzureAdConfig>(b.Configuration.GetSection("AzureAd"));
                })
                .ConfigureFunctionsWorkerDefaults(app => app.UseMiddleware<AuthenticationMiddleware>())
                .Build();

            host.Run();
        }
```

To verify authorization, you can tag your functions with `[RequireRoles("User,Admin")]` and then call the extension method `FunctionContext.VerifyUserRoles()` like in this example:

```csharp
        [Function("SecureFunc")]
        [RequireRole("Access.Basic")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext executionContext)
        {
            if (!executionContext.VerifyUserRoles())
            {
                return req.CreateResponse(HttpStatusCode.Forbidden);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            return response;
        }
```