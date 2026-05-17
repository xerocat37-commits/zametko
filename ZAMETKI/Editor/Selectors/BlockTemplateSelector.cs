using ZAMETKI.Editor.Models;

namespace ZAMETKI.Editor.Selectors;

public class BlockTemplateSelector : DataTemplateSelector
{
    public DataTemplate ParagraphTemplate { get; set; } = null!;
    public DataTemplate HeadingTemplate { get; set; } = null!;
    public DataTemplate FileTemplate { get; set; } = null!;

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        => item switch
        {
            HeadingBlock => HeadingTemplate,
            FileBlock => FileTemplate,
            ParagraphBlock => ParagraphTemplate,
            _ => ParagraphTemplate
        };
}
