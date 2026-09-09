// openZPL
// Copyright (C) 2026 AnthoDingo
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using openZPL.Models;

namespace openZPL.Services;

/// <summary>Genere le code ZPL II correspondant a un LabelTemplate.</summary>
public static class ZplGenerator
{
    /// <param name="copies">Nombre d'etiquettes a imprimer. Traduit en ^PQ :
    /// c'est l'imprimante qui repete l'etiquette, un seul envoi suffit.</param>
    public static string Generate(LabelTemplate template, int copies = 1)
    {
        var sb = new StringBuilder();
        sb.AppendLine("^XA");
        sb.AppendLine($"^PW{template.WidthDots}");   // largeur label
        sb.AppendLine($"^LL{template.HeightDots}");  // hauteur label
        sb.AppendLine("^LH0,0");                      // home
        sb.AppendLine("^CI28");                       // encodage UTF-8

        foreach (LabelElement el in template.Elements)
            sb.Append(RenderElement(el));

        if (copies > 1)
            sb.AppendLine($"^PQ{copies},0,0,N");      // quantite

        sb.AppendLine("^XZ");
        return sb.ToString();
    }

    private static string RenderElement(LabelElement el) => el.Type switch
    {
        LabelElementType.Text => RenderText(el),
        LabelElementType.Barcode => RenderBarcode(el),
        LabelElementType.Image => RenderImage(el),
        _ => string.Empty
    };

    // ── Texte ─────────────────────────────────────────────────────────────────

    private static string RenderText(LabelElement el)
    {
        string text = EscapeZpl(el.Text ?? string.Empty);
        int sz = Math.Max(8, el.FontSize);

        string justif = el.HAlign switch
        {
            LabelHAlign.Center => "C",
            LabelHAlign.Right => "R",
            _ => "L"
        };

        int y = el.VAlign switch
        {
            LabelVAlign.Middle => el.Y + Math.Max(0, (el.Height - sz) / 2),
            LabelVAlign.Bottom => el.Y + Math.Max(0, el.Height - sz),
            _ => el.Y
        };

        return $"^FO{el.X},{y}^CF0,{sz}^FB{el.Width},1,0,{justif},0^FD{text}^FS\n";
    }

    // ── Code-barres ───────────────────────────────────────────────────────────

    private static string RenderBarcode(LabelElement el)
    {
        string value = EscapeZpl(el.BarcodeValue ?? string.Empty);
        int h = Math.Max(20, el.Height);
        string hri = el.ShowHri ? "Y" : "N";

        int y = el.VAlign switch
        {
            LabelVAlign.Middle => el.Y + Math.Max(0, (el.Height - h) / 2),
            LabelVAlign.Bottom => el.Y + Math.Max(0, el.Height - h),
            _ => el.Y
        };

        return el.BarcodeType switch
        {
            LabelBarcodeType.Code128 => $"^FO{el.X},{y}^BCN,{h},{hri},N,N^FD{value}^FS\n",
            LabelBarcodeType.QRCode => $"^FO{el.X},{y}^BQN,2,4^FDQA,{value}^FS\n",
            LabelBarcodeType.DataMatrix => $"^FO{el.X},{y}^BXN,4,200^FD{value}^FS\n",
            _ => string.Empty
        };
    }

    // ── Image -> ^GF ──────────────────────────────────────────────────────────

    private static string RenderImage(LabelElement el)
    {
        if (string.IsNullOrEmpty(el.ImageData)) return string.Empty;

        try
        {
            string b64 = el.ImageData;
            int comma = b64.IndexOf(',');
            if (comma >= 0) b64 = b64[(comma + 1)..];
            byte[] imgBytes = Convert.FromBase64String(b64);

            string gfData = ConvertToZplGf(imgBytes, el.Width, el.Height);
            return gfData.Length == 0 ? string.Empty : $"^FO{el.X},{el.Y}{gfData}\n";
        }
        catch
        {
            return string.Empty; // image ignoree si conversion impossible
        }
    }

    /// <summary>Convertit une image (PNG/JPG) en commande ZPL ^GF (ASCII hex, 1-bit)
    /// via le pipeline d'imagerie WPF (pas de dependance a System.Drawing).</summary>
    private static string ConvertToZplGf(byte[] imageBytes, int widthDots, int heightDots)
    {
        if (widthDots <= 0 || heightDots <= 0) return string.Empty;

        try
        {
            using var ms = new MemoryStream(imageBytes);
            BitmapDecoder decoder = BitmapDecoder.Create(
                ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BitmapSource source = decoder.Frames[0];

            var scaled = new TransformedBitmap(
                source,
                new ScaleTransform(
                    (double)widthDots / source.PixelWidth,
                    (double)heightDots / source.PixelHeight));

            var gray = new FormatConvertedBitmap(scaled, PixelFormats.Gray8, null, 0);

            int stride = gray.PixelWidth;
            byte[] pixels = new byte[stride * gray.PixelHeight];
            gray.CopyPixels(pixels, stride, 0);

            int bytesPerRow = (widthDots + 7) / 8;
            int totalBytes = bytesPerRow * heightDots;
            var hex = new StringBuilder(totalBytes * 2);

            for (int row = 0; row < heightDots; row++)
            {
                for (int byteIdx = 0; byteIdx < bytesPerRow; byteIdx++)
                {
                    byte b = 0;
                    for (int bit = 0; bit < 8; bit++)
                    {
                        int col = byteIdx * 8 + bit;
                        if (col < widthDots)
                        {
                            byte lum = pixels[row * stride + col];
                            if (lum < 128) b |= (byte)(0x80 >> bit); // seuil -> point noir
                        }
                    }
                    hex.Append(b.ToString("X2"));
                }
            }

            return $"^GFA,{totalBytes},{totalBytes},{bytesPerRow},{hex}";
        }
        catch
        {
            return string.Empty;
        }
    }

    // ── Utilitaires ───────────────────────────────────────────────────────────

    private static string EscapeZpl(string s) =>
        s.Replace("^", "\\^").Replace("~", "\\~");
}
