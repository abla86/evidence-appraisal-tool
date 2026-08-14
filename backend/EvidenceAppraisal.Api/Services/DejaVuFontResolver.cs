using PdfSharp.Fonts;

namespace EvidenceAppraisal.Api.Services;

public sealed class DejaVuFontResolver :
    IFontResolver
{
    private const string RegularFace =
        "DejaVuSans";

    private const string BoldFace =
        "DejaVuSans-Bold";

    private const string FontDirectory =
        "/usr/share/fonts/truetype/dejavu";

    public byte[] GetFont(string faceName)
    {
        var fileName = faceName switch
        {
            BoldFace =>
                "DejaVuSans-Bold.ttf",

            _ =>
                "DejaVuSans.ttf"
        };

        var path = Path.Combine(
            FontDirectory,
            fileName
        );

        return File.ReadAllBytes(path);
    }

    public FontResolverInfo? ResolveTypeface(
        string familyName,
        bool isBold,
        bool isItalic)
    {
        return new FontResolverInfo(
            isBold
                ? BoldFace
                : RegularFace
        );
    }
}