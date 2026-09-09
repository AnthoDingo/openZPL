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

using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using openZPL.Resources;

namespace openZPL.Localization;

/// <summary>Source des textes traduits pour le XAML. L'indexeur passe par le
/// ResourceManager a chaque lecture : signaler que l'indexeur a change suffit
/// donc a retraduire l'interface ouverte, sans redemarrer.</summary>
public sealed class Loc : INotifyPropertyChanged
{
    public static Loc Instance { get; } = new();

    private Loc() { }

    public string this[string key] => Strings.ResourceManager.GetString(key, Strings.Culture) ?? key;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>"Item[]" est la convention WPF pour « toutes les valeurs de
    /// l'indexeur ont change ».</summary>
    internal void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
}

/// <summary>Traduit un texte dans le XAML : Text="{loc:Loc PropertiesTitle}".
/// Rend un binding, et non la chaine elle-meme, pour que le texte suive un
/// changement de langue.</summary>
public sealed class LocExtension : MarkupExtension
{
    public LocExtension() { }

    public LocExtension(string key) => Key = key;

    [ConstructorArgument("key")]
    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = Loc.Instance,
            Mode = BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}

/// <summary>Langues proposees et application du choix.</summary>
public static class Language
{
    public const string French = "fr";
    public const string English = "en";

    /// <summary>Code de la langue affichee.</summary>
    public static string Current { get; private set; } = French;

    /// <summary>Applique une langue a l'application entiere. L'interface deja
    /// ouverte se retraduit, les fenetres suivantes naissent traduites.</summary>
    public static void Apply(string code)
    {
        var culture = new CultureInfo(code);

        Strings.Culture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        Current = code;
        Loc.Instance.Refresh();
    }

    /// <summary>Langue a utiliser au premier lancement : celle de Windows si
    /// elle est proposee, sinon l'anglais.</summary>
    public static string Detect() =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == French ? French : English;
}
