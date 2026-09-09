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

using System.Collections;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using openZPL.Models;
using openZPL.Services;

namespace openZPL.Converters;

/// <summary>Visible si la valeur (enum/objet) correspond au ConverterParameter.</summary>
public sealed class EnumMatchToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return Visibility.Collapsed;
        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Visible si la valeur (enum/objet) NE correspond PAS au ConverterParameter.</summary>
public sealed class EnumMismatchToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return Visibility.Visible;
        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Permet de binder un groupe de RadioButton a une seule propriete enum.</summary>
public sealed class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null && parameter is not null &&
        string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true && parameter is not null
            ? Enum.Parse(targetType, parameter.ToString()!)
            : Binding.DoNothing;
}

/// <summary>Bool -> Visibility. ConverterParameter="Invert" inverse le resultat.</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool b = value is true;
        if (string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase)) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Objet non-null -> Visible. ConverterParameter="Invert" inverse le resultat
/// (utile pour une chaine null-ou-vide -> traitee comme "absente").</summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool hasValue = value is not null && (value is not string s || s.Length > 0);
        if (string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase)) hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>LabelHAlign -> HorizontalAlignment (evite les comparaisons enum/string
/// peu fiables d'un DataTrigger.Value).</summary>
public sealed class HAlignToAlignmentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        LabelHAlign.Center => HorizontalAlignment.Center,
        LabelHAlign.Right => HorizontalAlignment.Right,
        _ => HorizontalAlignment.Left
    };

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>LabelVAlign -> VerticalAlignment.</summary>
public sealed class VAlignToAlignmentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        LabelVAlign.Top => VerticalAlignment.Top,
        LabelVAlign.Bottom => VerticalAlignment.Bottom,
        _ => VerticalAlignment.Center
    };

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>[BarcodeValue, BarcodeType, Width, Height] -> aperçu bitmap du code-barres
/// reel. ConverterParameter="Square" force une image carree basee sur Height
/// (utilise pour QRCode/DataMatrix) au lieu de Width x Height (Code128).</summary>
public sealed class BarcodeImageConverter : IMultiValueConverter
{
    public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 4) return null;
        if (values[0] is not string value) return null;
        if (values[1] is not LabelBarcodeType type) return null;
        if (values[2] is not int width) return null;
        if (values[3] is not int height) return null;

        bool square = string.Equals(parameter as string, "Square", StringComparison.OrdinalIgnoreCase);
        int pixelWidth = square ? height : width;

        return (BitmapSource?)BarcodeImageGenerator.Generate(value, type, pixelWidth, height);
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Nombre/collection vide -> Visible. ConverterParameter="Invert" inverse le resultat.</summary>
public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int count = value switch
        {
            int i => i,
            ICollection c => c.Count,
            IEnumerable e => e.Cast<object>().Count(),
            _ => 0
        };
        bool empty = count == 0;
        if (string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase)) empty = !empty;
        return empty ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Image encodee en base64 (avec ou sans prefixe "data:image/png;base64,")
/// -> BitmapImage. WPF ne sait pas resoudre une data URI tout seul.</summary>
public sealed class Base64ToImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string data || data.Length == 0) return null;

        // "data:image/png;base64,XXXX" -> "XXXX" (une data URI brute est aussi acceptee)
        int comma = data.IndexOf(',');
        if (data.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && comma >= 0)
            data = data[(comma + 1)..];

        try
        {
            using var stream = new MemoryStream(System.Convert.FromBase64String(data));
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;   // libere le flux des la fin du chargement
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Index d'un calque -> Panel.ZIndex. La liste des calques se lit de haut
/// en bas (le premier est au premier plan) alors que le Canvas empile ses enfants
/// dans l'ordre inverse : on nie l'index pour que les deux concordent.</summary>
public sealed class IndexToZIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int index ? -index : 0;

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
