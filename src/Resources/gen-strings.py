# -*- coding: utf-8 -*-
"""Genere Strings.resx (francais, neutre) et Strings.en.resx (anglais)."""
import io
import xml.sax.saxutils as sx

S = [
    # (cle, francais, anglais)
    ("MenuFile", "Fichier", "File"),
    ("MenuNewLabel", "Nouvelle etiquette", "New label"),
    ("MenuOpen", "Ouvrir...", "Open..."),
    ("MenuSave", "Enregistrer", "Save"),
    ("MenuSaveAs", "Enregistrer sous...", "Save as..."),
    ("MenuZpl", "ZPL", "ZPL"),
    ("MenuExit", "Quitter", "Exit"),
    ("MenuSettings", "Parametres", "Settings"),
    ("MenuPrinters", "Imprimantes", "Printers"),
    ("MenuLanguage", "Langue", "Language"),
    ("MenuPrint", "Imprimer", "Print"),
    ("ShortcutSaveAs", "Ctrl+Maj+S", "Ctrl+Shift+S"),

    ("SectionAddElement", "AJOUTER UN ELEMENT", "ADD AN ELEMENT"),
    ("ElementText", "Texte", "Text"),
    ("ElementBarcode", "Code-barres", "Barcode"),
    ("ElementImage", "Image", "Image"),
    ("ElementSeparator", "Separateur", "Separator"),
    ("SectionLayers", "CALQUES", "LAYERS"),
    ("LayerImage", "image", "image"),
    ("LayerSeparator", "separateur", "separator"),
    ("LabelWidth", "Largeur", "Width"),
    ("LabelHeight", "Hauteur", "Height"),
    ("LabelZoom", "Zoom", "Zoom"),

    ("PropertiesTitle", "Proprietes", "Properties"),
    ("TipMoveUp", "Monter", "Move up"),
    ("TipMoveDown", "Descendre", "Move down"),
    ("TipDelete", "Supprimer", "Delete"),
    ("SectionPositionSize", "POSITION ET TAILLE (dots)", "POSITION AND SIZE (dots)"),
    ("SectionHAlign", "ALIGNEMENT HORIZONTAL", "HORIZONTAL ALIGNMENT"),
    ("SectionVAlign", "ALIGNEMENT VERTICAL", "VERTICAL ALIGNMENT"),
    ("TipAlignLeft", "Aligner a gauche", "Align left"),
    ("TipAlignCenter", "Centrer", "Center"),
    ("TipAlignRight", "Aligner a droite", "Align right"),
    ("TipAlignTop", "Aligner en haut", "Align top"),
    ("TipAlignMiddle", "Centrer verticalement", "Center vertically"),
    ("TipAlignBottom", "Aligner en bas", "Align bottom"),
    ("SectionContent", "CONTENU", "CONTENT"),
    ("LabelFontSize", "Taille police", "Font size"),
    ("LabelBold", "Gras", "Bold"),
    ("LabelItalic", "Italique", "Italic"),
    ("SectionValue", "VALEUR", "VALUE"),
    ("SectionType", "TYPE", "TYPE"),
    ("LabelShowHri", "Afficher la valeur en dessous", "Show the value below"),
    ("SectionValueAlign", "ALIGNEMENT DE LA VALEUR", "VALUE ALIGNMENT"),
    ("SectionImageFile", "FICHIER (PNG/JPG, max 2 Mo)", "FILE (PNG/JPG, max 2 MB)"),
    ("ButtonChooseImage", "Choisir une image...", "Choose an image..."),
    ("SectionOrientation", "ORIENTATION", "ORIENTATION"),
    ("TipOrientationHorizontal", "Horizontal", "Horizontal"),
    ("TipOrientationVertical", "Vertical", "Vertical"),
    ("LabelThickness", "Epaisseur", "Thickness"),

    ("PrintTitle", "Imprimer", "Print"),
    ("PrintPrinter", "Imprimante", "Printer"),
    ("PrintCopies", "Nombre d'exemplaires", "Number of copies"),
    ("PrintNoPrinter", "Aucune imprimante configuree. Ajoutez-en une dans Parametres.",
                       "No printer configured. Add one in Settings."),
    ("ButtonCancel", "Annuler", "Cancel"),
    ("ButtonPrint", "Imprimer", "Print"),

    ("SettingsTitle", "Parametres", "Settings"),
    ("PrintersTitle", "Imprimantes", "Printers"),
    ("BadgeDefault", "defaut", "default"),
    ("ButtonAddPrinter", "Ajouter une imprimante", "Add a printer"),
    ("FieldName", "Nom", "Name"),
    ("FieldIp", "Adresse IP", "IP address"),
    ("FieldPort", "Port", "Port"),
    ("ToggleDefaultPrinter", "Utiliser comme imprimante par defaut", "Use as default printer"),
    ("ButtonDelete", "Supprimer", "Delete"),
    ("ButtonSave", "Enregistrer", "Save"),
    ("PrintersEmpty", "Ajoutez une imprimante pour commencer.", "Add a printer to get started."),
    ("NewPrinterName", "Nouvelle imprimante", "New printer"),
    ("PlaceholderPrinterName", "Entrepot 1", "Warehouse 1"),

    ("ZplTitle", u"Apercu ZPL — donnees d'exemple", u"ZPL preview — sample data"),
    ("ZplSize", "Taille :", "Size:"),
    ("ZplHint", "Testez visuellement le rendu sur", "Check the rendering visually on"),

    ("DialogSaveLabel", "Enregistrer l'etiquette", "Save label"),
    ("DialogOpenLabel", "Ouvrir une etiquette", "Open label"),
    ("DialogChooseImage", "Choisir une image", "Choose an image"),
    ("FilterImages", "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                     "Images (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"),
    ("FilterLabelFile", "Etiquette openZPL (*.ozpl)|*.ozpl", "openZPL label (*.ozpl)|*.ozpl"),
    ("FilterAllFiles", "Tous les fichiers (*.*)|*.*", "All files (*.*)|*.*"),

    ("ErrorOpenTitle", "Ouverture impossible", "Cannot open"),
    ("ErrorSaveTitle", "Enregistrement impossible", "Cannot save"),
    ("ErrorPrintTitle", "Impression impossible", "Cannot print"),
    ("ErrorNotOpenZpl", u"« {0} » n'est pas un fichier openZPL exploitable.",
                        u"“{0}” is not a usable openZPL file."),
    ("ErrorUnknown", "Erreur inconnue.", "Unknown error."),
    ("ButtonClose", "Fermer", "Close"),
    ("LabelDefaultName", "Etiquette", "Label"),
    ("DefaultTextContent", "Texte", "Text"),

    ("PrintErrorNoPrinter", "Aucune imprimante configuree ou selectionnee (menu Parametres).",
                            "No printer configured or selected (Settings menu)."),
    ("PrintErrorTimeout", u"Timeout — imprimante {0}:{1} inaccessible.",
                          u"Timeout — printer {0}:{1} unreachable."),
    ("PrintErrorNetwork", "Erreur reseau : {0}", "Network error: {0}"),
]

HEADER = '''<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
'''


def write(path, index):
    out = [HEADER]
    for key, fr, en in S:
        value = (fr, en)[index]
        out.append('  <data name="%s" xml:space="preserve">\n    <value>%s</value>\n  </data>\n'
                   % (key, sx.escape(value)))
    out.append("</root>\n")
    io.open(path, "w", encoding="utf-8", newline="\r\n").write("".join(out))
    print(path, len(S), "cles")


write("src/Resources/Strings.resx", 0)
write("src/Resources/Strings.en.resx", 1)

CS_HEADER = """// openZPL
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
"""


PROP = '\n    /// <summary>%s</summary>\n    public static string %s => Get("%s");\n'


def write_cs(path):
    out = [CS_HEADER]
    for key, fr, _ in S:
        out.append(PROP % (sx.escape(fr), key, key))
    out.append("}\n")
    io.open(path, "w", encoding="utf-8", newline="\r\n").write("".join(out))
    print(path, len(S), "proprietes")


write_cs("src/Resources/Strings.cs")
