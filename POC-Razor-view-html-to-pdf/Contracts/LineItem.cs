namespace POC_Razor_view_html_to_pdf.Contracts;

public sealed class LineItem
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Quantity { get; set; }
}
