using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace POC_Razor_view_html_to_pdf;

public interface IBrowserFactory
{
    Task<IBrowser> GetOrCreateAsync();
    Task<byte[]> GeneratePdfAsync(string html);
    ValueTask DisposeAsync();
}

public class PuppeteerBrowserFactory : IBrowserFactory
{
    private IBrowser? _browser;
    private readonly SemaphoreSlim _browserLock = new(1, 1);
    private readonly SemaphoreSlim _pageLock = new(1, 1);

    public async Task<IBrowser> GetOrCreateAsync()
    {
        if (_browser != null)
        {
            if (!_browser.IsClosed && await IsBrowserAliveAsync(_browser))
                return _browser;

            try { await _browser.DisposeAsync(); } catch { }
            _browser = null;
        }

        await _browserLock.WaitAsync();
        try
        {
            if (_browser == null)
            {
                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args =
                    [
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-gpu"
                    ]
                });

                _browser.Disconnected += (_, _) =>
                {
                    _browser = null;
                };
            }
        }
        finally
        {
            _browserLock.Release();
        }

        return _browser;
    }

    public async Task<byte[]> GeneratePdfAsync(string html)
    {
        await _pageLock.WaitAsync();
        try
        {
            var browser = await GetOrCreateAsync();
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = [WaitUntilNavigation.Load],
            });

            return await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            });
        }
        finally
        {
            _pageLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null && !_browser.IsClosed)
        {
            try { await _browser.DisposeAsync(); }
            catch { }
            finally { _browser = null; }
        }

        _browserLock.Dispose();
        _pageLock.Dispose();
    }

    private async Task<bool> IsBrowserAliveAsync(IBrowser browser)
    {
        try
        {
            var pages = await browser.PagesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
