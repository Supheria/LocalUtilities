using LocalUtilities.SimpleScript.Data;
using LocalUtilities.SimpleScript.Serialization;
using LocalUtilities.StringUtilities;
using LocalUtilities.UIUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalUtilities.Serializations;

public class FormDataSerialization<T> : SsSerialization<T> where T : FormData
{
    public override string LocalName { get; }

    FontDataSerialization LabelFontDataSerialization { get; } = new(nameof(Source.LabelFontData));

    FontDataSerialization ContentFontDataSerialization { get; } = new(nameof(Source.ContentFontData));


    public FormDataSerialization(string localName, T formData) : base(formData)
    {
        LocalName = localName;
        OnSerialize += FormData_Serialize;
        OnDeserialize += FormData_Deserialize;
    }

    private void FormData_Serialize(SsSerializer serializer)
    {
        serializer.AppendTag(nameof(Source.MinimumSize), Source.MinimumSize.ToArrayString());
        serializer.AppendTag(nameof(Source.Size), Source.Size.ToArrayString());
        serializer.AppendTag(nameof(Source.Location), Source.Location.ToArrayString());
        serializer.AppendTag(nameof(Source.WindowState), Source.WindowState.ToString());
        serializer.AppendTag(nameof(Source.Padding), Source.Padding.ToString());
        LabelFontDataSerialization.Source = Source.LabelFontData;
        LabelFontDataSerialization.DoSerialize(serializer);
        ContentFontDataSerialization.Source = Source.ContentFontData;
        ContentFontDataSerialization.DoSerialize(serializer);
    }

    private void FormData_Deserialize(Token token)
    {
        if (token is TagValues tagValues)
        {
            if (token.Name is nameof(Source.MinimumSize))
                Source.MinimumSize = tagValues.Tag.ToSize(Source.MinimumSize);
            if (token.Name is nameof(Source.Size))
                Source.Size = tagValues.Tag.ToSize(Source.Size);
            else if (token.Name is nameof(Source.Location))
                Source.Location = tagValues.Tag.ToPoint(Source.Location);
            else if (token.Name is nameof(Source.WindowState))
                Source.WindowState = tagValues.Tag.ToEnum<FormWindowState>();
            else if (token.Name is nameof(Source.Padding))
                Source.Padding = tagValues.Tag.ToInt(Source.Padding);
        }
        else if (token is Scope scope)
        {
            if (token.Name == LabelFontDataSerialization.LocalName)
                Source.LabelFontData = LabelFontDataSerialization.Deserialize(scope);
            else if (token.Name == ContentFontDataSerialization.LocalName)
                Source.ContentFontData = ContentFontDataSerialization.Deserialize(scope);
        }
    }
}
