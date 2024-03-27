using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using sip.Documents.Renderers.Handlebars;

namespace sip.Documents.Renderers.Word;

public class MsWordRenderer(HandlebarsService handlebarsRenderer) : IDocRenderer
{
    public async Task<MimePart> Render(MimePart tsource, object context)
    {
        if (!SupportsType(tsource.ContentType))
        {
            throw new FormatException($"{nameof(MsWordRenderer)}: Unsupported source {tsource}");
        }

        var result = new MemoryStream();
        await tsource.Content.DecodeToAsync(result);
        result.Position = 0;
        using var doc = WordprocessingDocument.Open(result, true);
        
        // Fix the document structure so it is prepared for handlebars rendering
        MergeRuns('{', '}', doc.MainDocumentPart!.Document.Body!.Descendants<Run>());
        doc.Save();
        
        // Write the fixed shit to text, so we can pass it to handlebars
        string contents;
        using (var contentStreamReader = new StreamReader(doc.MainDocumentPart!.GetStream()))
        {
            contents = await contentStreamReader.ReadToEndAsync();
        }
        
        // Now lets actually use the handlebars renderer, but first do some replacements
        contents = contents.Replace("{", "{{");
        contents = contents.Replace("}", "}}");
        var newContents = handlebarsRenderer.Render(contents, context);
        await using var contentStreamWriter = new StreamWriter(doc.MainDocumentPart.GetStream(FileMode.Create));
        await contentStreamWriter.WriteAsync(newContents);
        
        var resultMp = new MimePart(tsource.ContentType)
        {
            Content = new MimeContent(result)
        };

        return resultMp;
    }

    public bool SupportsType(ContentType contentType) => contentType.IsMsWord();

    public void MergeRuns(char startDelimiter, char endDelimiter, IEnumerable<Run> runs)
    {
        var runsList = runs.ToList();
        
        // Search for start pattern
        var index = 0;

        while (index < runsList.Count)
        {
            var currentRun = runsList[index];
            
            // Does this run open merge?
            var text = string.Join("", currentRun.Descendants<Text>().Select(t => t.Text));
            if (text.Contains(startDelimiter))
            {
                var mergCount = MergeRunsUntil(runsList.Skip(index), endDelimiter);
                index += mergCount;
            }

            index++;
        }
    }

    /// <summary>
    /// Merge multiple runs into the first run in the enumerable
    /// Only first run with one text is preserved, all texts are merged to the first and
    /// everything further gets deleted.
    /// </summary>
    /// <param name="runs"></param>
    /// <param name="untilChar"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public int MergeRunsUntil(IEnumerable<Run> runs, char untilChar)
    {
        var mergedCount = 0;
        var runsList = runs.ToList();
        var targetRun = runsList.First();
        var targetTexts= targetRun.Descendants<Text>().ToList();
        var targetText = targetTexts.First();
        if (targetText is null) throw new InvalidOperationException("Text element is missing in Run element");

        // Handle addition text elements in the run we are merging to
        foreach (var text in targetTexts.Skip(1))
        {
            targetText.Text += text;
            text.Remove();
        }
        
        // Now merge texts of remaining runs
        foreach (var mergeRun in runsList.Skip(1))
        {
            var text = string.Join("", mergeRun.Descendants<Text>().Select(t => t.Text));
            targetText.Text += text;
            mergeRun.Remove();
            mergedCount++;

            if (text.Contains(untilChar)) return mergedCount;
        }

        return mergedCount;
    }
}