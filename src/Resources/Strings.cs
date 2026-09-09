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

// Genere par Resources/gen-strings.py a partir de Strings.resx — ne pas
// modifier a la main, relancer le script apres avoir ajoute un texte.

using System.Globalization;
using System.Resources;

namespace openZPL.Resources;

/// <summary>Textes traduits de l'interface. Les valeurs viennent de
/// Strings.resx (francais, langue neutre) et Strings.en.resx (anglais).</summary>
public static class Strings
{
    public static ResourceManager ResourceManager { get; } =
        new("openZPL.Resources.Strings", typeof(Strings).Assembly);

    /// <summary>Langue lue par toutes les proprietes. null = celle du thread.</summary>
    public static CultureInfo? Culture { get; set; }

    private static string Get(string key) => ResourceManager.GetString(key, Culture) ?? key;

    /// <summary>Fichier</summary>
    public static string MenuFile => Get("MenuFile");

    /// <summary>Nouvelle etiquette</summary>
    public static string MenuNewLabel => Get("MenuNewLabel");

    /// <summary>Ouvrir...</summary>
    public static string MenuOpen => Get("MenuOpen");

    /// <summary>Enregistrer</summary>
    public static string MenuSave => Get("MenuSave");

    /// <summary>Enregistrer sous...</summary>
    public static string MenuSaveAs => Get("MenuSaveAs");

    /// <summary>ZPL</summary>
    public static string MenuZpl => Get("MenuZpl");

    /// <summary>Quitter</summary>
    public static string MenuExit => Get("MenuExit");

    /// <summary>Parametres</summary>
    public static string MenuSettings => Get("MenuSettings");

    /// <summary>Imprimantes</summary>
    public static string MenuPrinters => Get("MenuPrinters");

    /// <summary>Langue</summary>
    public static string MenuLanguage => Get("MenuLanguage");

    /// <summary>Imprimer</summary>
    public static string MenuPrint => Get("MenuPrint");

    /// <summary>Ctrl+Maj+S</summary>
    public static string ShortcutSaveAs => Get("ShortcutSaveAs");

    /// <summary>AJOUTER UN ELEMENT</summary>
    public static string SectionAddElement => Get("SectionAddElement");

    /// <summary>Texte</summary>
    public static string ElementText => Get("ElementText");

    /// <summary>Code-barres</summary>
    public static string ElementBarcode => Get("ElementBarcode");

    /// <summary>Image</summary>
    public static string ElementImage => Get("ElementImage");

    /// <summary>CALQUES</summary>
    public static string SectionLayers => Get("SectionLayers");

    /// <summary>image</summary>
    public static string LayerImage => Get("LayerImage");

    /// <summary>Largeur</summary>
    public static string LabelWidth => Get("LabelWidth");

    /// <summary>Hauteur</summary>
    public static string LabelHeight => Get("LabelHeight");

    /// <summary>Zoom</summary>
    public static string LabelZoom => Get("LabelZoom");

    /// <summary>Proprietes</summary>
    public static string PropertiesTitle => Get("PropertiesTitle");

    /// <summary>Monter</summary>
    public static string TipMoveUp => Get("TipMoveUp");

    /// <summary>Descendre</summary>
    public static string TipMoveDown => Get("TipMoveDown");

    /// <summary>Supprimer</summary>
    public static string TipDelete => Get("TipDelete");

    /// <summary>POSITION ET TAILLE (dots)</summary>
    public static string SectionPositionSize => Get("SectionPositionSize");

    /// <summary>ALIGNEMENT HORIZONTAL</summary>
    public static string SectionHAlign => Get("SectionHAlign");

    /// <summary>ALIGNEMENT VERTICAL</summary>
    public static string SectionVAlign => Get("SectionVAlign");

    /// <summary>Aligner a gauche</summary>
    public static string TipAlignLeft => Get("TipAlignLeft");

    /// <summary>Centrer</summary>
    public static string TipAlignCenter => Get("TipAlignCenter");

    /// <summary>Aligner a droite</summary>
    public static string TipAlignRight => Get("TipAlignRight");

    /// <summary>Aligner en haut</summary>
    public static string TipAlignTop => Get("TipAlignTop");

    /// <summary>Centrer verticalement</summary>
    public static string TipAlignMiddle => Get("TipAlignMiddle");

    /// <summary>Aligner en bas</summary>
    public static string TipAlignBottom => Get("TipAlignBottom");

    /// <summary>CONTENU</summary>
    public static string SectionContent => Get("SectionContent");

    /// <summary>Taille police</summary>
    public static string LabelFontSize => Get("LabelFontSize");

    /// <summary>Gras</summary>
    public static string LabelBold => Get("LabelBold");

    /// <summary>Italique</summary>
    public static string LabelItalic => Get("LabelItalic");

    /// <summary>VALEUR</summary>
    public static string SectionValue => Get("SectionValue");

    /// <summary>TYPE</summary>
    public static string SectionType => Get("SectionType");

    /// <summary>Afficher la valeur en dessous</summary>
    public static string LabelShowHri => Get("LabelShowHri");

    /// <summary>ALIGNEMENT DE LA VALEUR</summary>
    public static string SectionValueAlign => Get("SectionValueAlign");

    /// <summary>FICHIER (PNG/JPG, max 2 Mo)</summary>
    public static string SectionImageFile => Get("SectionImageFile");

    /// <summary>Choisir une image...</summary>
    public static string ButtonChooseImage => Get("ButtonChooseImage");

    /// <summary>Imprimer</summary>
    public static string PrintTitle => Get("PrintTitle");

    /// <summary>Imprimante</summary>
    public static string PrintPrinter => Get("PrintPrinter");

    /// <summary>Nombre d'exemplaires</summary>
    public static string PrintCopies => Get("PrintCopies");

    /// <summary>Aucune imprimante configuree. Ajoutez-en une dans Parametres.</summary>
    public static string PrintNoPrinter => Get("PrintNoPrinter");

    /// <summary>Annuler</summary>
    public static string ButtonCancel => Get("ButtonCancel");

    /// <summary>Imprimer</summary>
    public static string ButtonPrint => Get("ButtonPrint");

    /// <summary>Parametres</summary>
    public static string SettingsTitle => Get("SettingsTitle");

    /// <summary>Imprimantes</summary>
    public static string PrintersTitle => Get("PrintersTitle");

    /// <summary>defaut</summary>
    public static string BadgeDefault => Get("BadgeDefault");

    /// <summary>Ajouter une imprimante</summary>
    public static string ButtonAddPrinter => Get("ButtonAddPrinter");

    /// <summary>Nom</summary>
    public static string FieldName => Get("FieldName");

    /// <summary>Adresse IP</summary>
    public static string FieldIp => Get("FieldIp");

    /// <summary>Port</summary>
    public static string FieldPort => Get("FieldPort");

    /// <summary>Utiliser comme imprimante par defaut</summary>
    public static string ToggleDefaultPrinter => Get("ToggleDefaultPrinter");

    /// <summary>Supprimer</summary>
    public static string ButtonDelete => Get("ButtonDelete");

    /// <summary>Enregistrer</summary>
    public static string ButtonSave => Get("ButtonSave");

    /// <summary>Ajoutez une imprimante pour commencer.</summary>
    public static string PrintersEmpty => Get("PrintersEmpty");

    /// <summary>Nouvelle imprimante</summary>
    public static string NewPrinterName => Get("NewPrinterName");

    /// <summary>Entrepot 1</summary>
    public static string PlaceholderPrinterName => Get("PlaceholderPrinterName");

    /// <summary>Apercu ZPL — donnees d'exemple</summary>
    public static string ZplTitle => Get("ZplTitle");

    /// <summary>Taille :</summary>
    public static string ZplSize => Get("ZplSize");

    /// <summary>Testez visuellement le rendu sur</summary>
    public static string ZplHint => Get("ZplHint");

    /// <summary>Enregistrer l'etiquette</summary>
    public static string DialogSaveLabel => Get("DialogSaveLabel");

    /// <summary>Ouvrir une etiquette</summary>
    public static string DialogOpenLabel => Get("DialogOpenLabel");

    /// <summary>Choisir une image</summary>
    public static string DialogChooseImage => Get("DialogChooseImage");

    /// <summary>Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg</summary>
    public static string FilterImages => Get("FilterImages");

    /// <summary>Etiquette openZPL (*.ozpl)|*.ozpl</summary>
    public static string FilterLabelFile => Get("FilterLabelFile");

    /// <summary>Tous les fichiers (*.*)|*.*</summary>
    public static string FilterAllFiles => Get("FilterAllFiles");

    /// <summary>Etiquette openZPL</summary>
    public static string FileTypeLabel => Get("FileTypeLabel");

    /// <summary>Ouverture impossible</summary>
    public static string ErrorOpenTitle => Get("ErrorOpenTitle");

    /// <summary>Enregistrement impossible</summary>
    public static string ErrorSaveTitle => Get("ErrorSaveTitle");

    /// <summary>Impression impossible</summary>
    public static string ErrorPrintTitle => Get("ErrorPrintTitle");

    /// <summary>« {0} » n'est pas un fichier openZPL exploitable.</summary>
    public static string ErrorNotOpenZpl => Get("ErrorNotOpenZpl");

    /// <summary>Erreur inconnue.</summary>
    public static string ErrorUnknown => Get("ErrorUnknown");

    /// <summary>Fermer</summary>
    public static string ButtonClose => Get("ButtonClose");

    /// <summary>Etiquette</summary>
    public static string LabelDefaultName => Get("LabelDefaultName");

    /// <summary>Texte</summary>
    public static string DefaultTextContent => Get("DefaultTextContent");

    /// <summary>Aucune imprimante configuree ou selectionnee (menu Parametres).</summary>
    public static string PrintErrorNoPrinter => Get("PrintErrorNoPrinter");

    /// <summary>Timeout — imprimante {0}:{1} inaccessible.</summary>
    public static string PrintErrorTimeout => Get("PrintErrorTimeout");

    /// <summary>Erreur reseau : {0}</summary>
    public static string PrintErrorNetwork => Get("PrintErrorNetwork");
}
