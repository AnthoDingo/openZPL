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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace openZPL.Models;

public enum LabelElementType { Text, Barcode, Image, Separator }
public enum LabelBarcodeType { Code128, QRCode, DataMatrix }
public enum LabelHAlign { Left, Center, Right }
public enum LabelVAlign { Top, Middle, Bottom }
public enum LabelOrientation { Horizontal, Vertical }

/// <summary>
/// Element positionnable sur une etiquette. Les proprietes inutilisees selon
/// Type sont ignorees. Variables supportees dans Text/BarcodeValue :
/// {{BoxBarcode}}, {{PartReference}}, {{PartName}}, {{Quantity}}, {{Unit}},
/// {{Date}}, {{Operator}}.
/// </summary>
public partial class LabelElement : ObservableObject
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];

    [ObservableProperty] private LabelElementType _type;
    [ObservableProperty] private int _x;
    [ObservableProperty] private int _y;
    [ObservableProperty] private int _width = 200;
    [ObservableProperty] private int _height = 40;

    [ObservableProperty] private LabelHAlign _hAlign = LabelHAlign.Left;
    [ObservableProperty] private LabelVAlign _vAlign = LabelVAlign.Middle;

    // Texte
    [ObservableProperty] private string? _text;
    [ObservableProperty] private int _fontSize = 24;
    [ObservableProperty] private bool _bold;
    [ObservableProperty] private bool _italic;

    // Code-barres — la hauteur des barres suit Height (pas de parametre dedie)
    [ObservableProperty] private string? _barcodeValue;
    [ObservableProperty] private LabelBarcodeType _barcodeType = LabelBarcodeType.Code128;
    [ObservableProperty] private bool _showHri = true;

    // Image — PNG/JPG encode en base64 (data:image/png;base64,...)
    [ObservableProperty] private string? _imageData;

    // Séparateur — trait fin trace au centre de la boite englobante
    [ObservableProperty] private LabelOrientation _orientation = LabelOrientation.Horizontal;
    [ObservableProperty] private int _thickness = 4;

    /// <summary>Etat UI uniquement (surbrillance dans le canvas) — non persiste.</summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private bool _isSelected;

    /// <summary>Etat UI uniquement (glisser/deposer dans la liste des calques) — non persiste.</summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private bool _isDragging;

    /// <summary>Changer l'orientation permute largeur/hauteur pour reorienter
    /// le trait sur place plutot que de laisser une boite incoherente.</summary>
    partial void OnOrientationChanged(LabelOrientation value) => (Width, Height) = (Height, Width);
}

/// <summary>
/// Template d'etiquette pour imprimante Zebra (ZPL II).
/// </summary>
public partial class LabelTemplate : ObservableObject
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    [ObservableProperty] private string _name = "Etiquette";

    /// <summary>Largeur en dots (203 DPI). Ex: 812 = 4"</summary>
    [ObservableProperty] private int _widthDots = 812;

    /// <summary>Hauteur en dots (203 DPI). Ex: 406 = 2"</summary>
    [ObservableProperty] private int _heightDots = 406;

    /// <summary>Resolution DPI de l'imprimante cible.</summary>
    [ObservableProperty] private int _dpi = 203;

    public ObservableCollection<LabelElement> Elements { get; set; } = [];

    // ── Conversion dots <-> mm : dots = mm x DPI / 25.4 ─────────────────────

    [System.Text.Json.Serialization.JsonIgnore]
    public double WidthMm
    {
        get => Math.Round(WidthDots * 25.4 / Dpi, 1);
        set => WidthDots = Math.Max(1, (int)Math.Round(value * Dpi / 25.4));
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public double HeightMm
    {
        get => Math.Round(HeightDots * 25.4 / Dpi, 1);
        set => HeightDots = Math.Max(1, (int)Math.Round(value * Dpi / 25.4));
    }

    partial void OnWidthDotsChanged(int value) => OnPropertyChanged(nameof(WidthMm));
    partial void OnHeightDotsChanged(int value) => OnPropertyChanged(nameof(HeightMm));

    partial void OnDpiChanged(int value)
    {
        OnPropertyChanged(nameof(WidthMm));
        OnPropertyChanged(nameof(HeightMm));
    }

}
