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

using System.Windows.Media;
using System.Windows.Media.Imaging;
using openZPL.Models;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace openZPL.Services;

/// <summary>Genere un aperçu bitmap fidele du code-barres pour le canvas de
/// l'editeur (distinct de la generation ZPL, qui reste geree nativement par
/// l'imprimante a partir de la valeur brute).</summary>
public static class BarcodeImageGenerator
{
    public static BitmapSource? Generate(string? value, LabelBarcodeType type, int pixelWidth, int pixelHeight)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        int width = Math.Max(2, pixelWidth);
        int height = Math.Max(2, pixelHeight);

        BarcodeFormat format = type switch
        {
            LabelBarcodeType.QRCode => BarcodeFormat.QR_CODE,
            LabelBarcodeType.DataMatrix => BarcodeFormat.DATA_MATRIX,
            _ => BarcodeFormat.CODE_128
        };

        try
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 0,
                    PureBarcode = format == BarcodeFormat.CODE_128,
                }
            };

            PixelData pixelData = writer.Write(value);

            var bitmap = BitmapSource.Create(
                pixelData.Width, pixelData.Height, 96, 96,
                PixelFormats.Bgra32, null, pixelData.Pixels, pixelData.Width * 4);
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null; // valeur non encodable dans ce format (ex: caracteres non supportes)
        }
    }
}
