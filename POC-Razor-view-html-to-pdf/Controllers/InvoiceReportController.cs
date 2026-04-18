using Microsoft.AspNetCore.Mvc;
using POC_Razor_view_html_to_pdf.Contracts;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using Razor.Templating.Core;

namespace POC_Razor_view_html_to_pdf.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoiceReportController : ControllerBase
    {
        private readonly InvoiceFactory _invoiceFactory;
        private readonly IBrowserFactory _browserFactory;

        public InvoiceReportController(InvoiceFactory invoiceFactory, IBrowserFactory browserFactory)
        {
            _invoiceFactory = invoiceFactory;
            _browserFactory = browserFactory;
        }

        [HttpGet]
        [EndpointSummary("Gera invoice em PDF")]
        [EndpointDescription("Cria um PDF do invoice e retorna como download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] int count = 50)
        {
            var invoices = _invoiceFactory.CreateMany(count);
            var html = await RazorTemplateEngine.RenderAsync("Views/InvoiceReport.cshtml", invoices);
            var pdfBytes = await _browserFactory.GeneratePdfAsync(html);
            return File(pdfBytes, "application/pdf", $"invoices-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
    }
}
