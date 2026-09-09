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

namespace openZPL.Services;

/// <summary>Preferences d'interface, propres a l'utilisateur.</summary>
public class AppSettingsData
{
    /// <summary>Dernier dossier utilise pour ouvrir ou enregistrer une etiquette,
    /// pour rouvrir les boites de dialogue au meme endroit.</summary>
    public string? LastDirectory { get; set; }

    /// <summary>Langue de l'interface ("fr", "en"). null au premier lancement :
    /// celle de Windows est alors utilisee.</summary>
    public string? Language { get; set; }
}

/// <summary>Persiste les preferences d'interface en JSON dans %AppData%\openZPL
/// — propre a chaque utilisateur, contrairement aux imprimantes qui sont
/// partagees par le poste.</summary>
public class AppSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _filePath;

    public AppSettingsStore()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "openZPL");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "settings.json");
    }

    public AppSettingsData Load()
    {
        if (!File.Exists(_filePath)) return new AppSettingsData();
        try
        {
            return JsonSerializer.Deserialize<AppSettingsData>(File.ReadAllText(_filePath), JsonOptions)
                ?? new AppSettingsData();
        }
        catch
        {
            return new AppSettingsData();
        }
    }

    /// <summary>Ecrit les preferences. Une preference perdue n'a pas a interrompre
    /// l'utilisateur : un echec d'ecriture est ignore.</summary>
    public void Save(AppSettingsData data)
    {
        try
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(data, JsonOptions));
        }
        catch
        {
            // dossier en lecture seule, disque plein — sans consequence ici
        }
    }
}
