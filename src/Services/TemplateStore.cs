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

/// <summary>Persiste la liste des templates d'etiquettes en JSON dans le dossier
/// AppData de l'utilisateur — pas de base de donnees pour cette application locale.</summary>
public class TemplateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _filePath;

    public TemplateStore()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "openZPL");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "templates.json");
    }

    public List<LabelTemplate> Load()
    {
        if (!File.Exists(_filePath)) return [];
        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<LabelTemplate>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<LabelTemplate> templates) =>
        File.WriteAllText(_filePath, JsonSerializer.Serialize(templates, JsonOptions));
}
