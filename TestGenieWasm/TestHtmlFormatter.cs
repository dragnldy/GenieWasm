
namespace TestGenieWasm
{
    [TestClass]
    public sealed class TestHtmlFormatter
    {
        [TestMethod]
        public void HtmlFormatterReturnsInLineSpans()
        {
            string htmlContent = "<html>This is <b>bold</b> and <i>italic</i> text. <font color=\"red\">Red text</font>.</html>";
            //List<InlineRun> formattedRuns = HtmlFormatter.FormatHtmlToInlineRuns(htmlContent);

            //foreach (var run in formattedRuns)
            //{
            //    Console.WriteLine($"Text: \"{run.Text}\", Bold: {run.IsBold}, Italic: {run.IsItalic}, Color: {run.Color}");
            //}
        }
    }
}
