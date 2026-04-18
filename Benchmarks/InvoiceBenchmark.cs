using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using POC_Razor_view_html_to_pdf;
using Razor.Templating.Core;

namespace Benchmarks;

[MemoryDiagnoser]
[Config(typeof(InProcessConfig))]
public class InvoiceBenchmark
{
    private InvoiceFactory _factory = null!;
    private PuppeteerBrowserFactory _browserFactory = null!;

    [Params(1, 5, 10)]
    public int Count;

    [GlobalSetup]
    public async Task Setup()
    {
        _factory = new InvoiceFactory();
        _browserFactory = new PuppeteerBrowserFactory();

        var invoices = _factory.CreateMany(1);
        var html = await RazorTemplateEngine.RenderAsync("Views/InvoiceReport.cshtml", invoices);
        await _browserFactory.GeneratePdfAsync(html);
    }

    [Benchmark]
    public async Task<byte[]> GeneratePdf()
    {
        var invoices = _factory.CreateMany(Count);
        var html = await RazorTemplateEngine.RenderAsync("Views/InvoiceReport.cshtml", invoices);
        return await _browserFactory.GeneratePdfAsync(html);


    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _browserFactory.DisposeAsync();
    }

    public class InProcessConfig : ManualConfig
    {
        public InProcessConfig()
        {
            AddJob(Job.ShortRun
                .WithToolchain(InProcessEmitToolchain.Instance));
        }
    }
}
