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
using System.Text.Json;
using openZPL.Models;

namespace openZPL.Services;

/// <summary>Enveloppe ecrite dans un fichier .ozpl. Le couple Format/Version
/// permet de reconnaitre un fichier etranger et de faire evoluer le format
/// sans casser les projets deja enregistres.</summary>
public class ProjectFileData
{
    public string Format { get; set; } = ProjectFile.FormatMarker;
    public int Version { get; set; } = ProjectFile.CurrentVersion;
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public LabelTemplate? Template { get; set; }
}

/// <summary>Lecture/ecriture d'un projet openZPL (une etiquette et ses elements)
/// dans un fichier .ozpl — du JSON, comme le reste de la persistance de
/// l'application. Les images sont deja stockees en base64 dans les elements,
/// le fichier est donc autonome : il peut etre copie ou envoye tel quel.</summary>
public static class ProjectFile
{
    public const string Extension = ".ozpl";
    public const string FormatMarker = "openZPL";
    public const int CurrentVersion = 1;

    /// <summary>Filtre pour SaveFileDialog — un seul type propose a l'enregistrement.</summary>
    public const string SaveFilter = $"Projet openZPL (*{Extension})|*{Extension}";

    /// <summary>Filtre pour OpenFileDialog — "Tous les fichiers" en second choix,
    /// pour les projets qu'un utilisateur aurait renommes.</summary>
    public const string OpenFilter = $"{SaveFilter}|Tous les fichiers (*.*)|*.*";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>Ecrit le projet. Les exceptions d'ecriture (droits, chemin
    /// invalide, disque plein) remontent a l'appelant, qui previent l'utilisateur.</summary>
    public static void Save(string path, LabelTemplate template)
    {
        var data = new ProjectFileData { Template = template };
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOptions));
    }

    /// <summary>Relit un projet. Retourne null si le fichier n'est pas un projet
    /// openZPL exploitable (JSON invalide, marqueur absent, version plus recente
    /// que celle geree, ou etiquette manquante).</summary>
    public static LabelTemplate? Load(string path)
    {
        ProjectFileData? data;
        try
        {
            data = JsonSerializer.Deserialize<ProjectFileData>(File.ReadAllText(path), JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }

        if (data?.Template is null) return null;
        if (!string.Equals(data.Format, FormatMarker, StringComparison.OrdinalIgnoreCase)) return null;
        if (data.Version > CurrentVersion) return null;

        return data.Template;
    }

    /// <summary>Nom de fichier propose a partir du nom de l'etiquette, sans les
    /// caracteres refuses par Windows.</summary>
    public static string SuggestFileName(string templateName)
    {
        string name = string.Join("_", templateName.Split(Path.GetInvalidFileNameChars(),
            StringSplitOptions.RemoveEmptyEntries)).Trim();
        if (name.Length == 0) name = "etiquette";
        return name + Extension;
    }
}
