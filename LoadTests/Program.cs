using NBomber.CSharp;

var httpClient = new HttpClient();

var scenario = Scenario.Create("invoice_pdf", async context =>
{
    var httpClient = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    { Timeout = TimeSpan.FromSeconds(120) };

    var response = await httpClient.GetAsync("https://localhost:7261/InvoiceReport");
    return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
})
.WithWarmUpDuration(TimeSpan.FromSeconds(5))
.WithLoadSimulations(
    Simulation.Inject(rate: 2, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();