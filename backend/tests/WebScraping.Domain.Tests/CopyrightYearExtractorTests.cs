using FluentAssertions;
using WebScraping.Domain.Services;

namespace WebScraping.Domain.Tests;

public class CopyrightYearExtractorTests
{
    [Theory]
    [InlineData("<footer>© 2016-2026 Empresa</footer>", 2016)]
    [InlineData("<footer>Copyright 2015</footer>", 2015)]
    [InlineData("<footer>2018 - 2024</footer>", 2018)]
    [InlineData("<footer>© 2020</footer>", 2020)]
    public void Extracts_oldest_year_from_footer(string html, int expected)
    {
        CopyrightYearExtractor.TryExtractOldestYear(html).Should().Be(expected);
    }

    [Fact]
    public void Falls_back_to_tail_when_footer_has_no_year()
    {
        var head = new string('x', 800);
        var html = $"<html><body>{head}<div>© 2012-2019 Tail Co</div></body></html>";
        CopyrightYearExtractor.TryExtractOldestYear(html).Should().Be(2012);
    }

    [Fact]
    public void Returns_null_when_no_year()
    {
        CopyrightYearExtractor.TryExtractOldestYear("<footer>Sem anos aqui</footer>").Should().BeNull();
    }

    [Fact]
    public void Ignores_non_19xx_20xx()
    {
        CopyrightYearExtractor.TryExtractOldestYear("<footer>© 1899-abc</footer>").Should().BeNull();
    }
}
