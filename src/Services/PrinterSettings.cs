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
using System.Text.Json.Serialization;

namespace openZPL.Services;

public class PrinterProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 9100;

    [JsonIgnore] public bool IsDefault { get; set; }
    [JsonIgnore] public string IpPort => $"{IpAddress}:{Port}";
    [JsonIgnore] public string DisplayName => string.IsNullOrWhiteSpace(Name) ? IpPort : $"{Name} ({IpPort})";
}

public class PrinterSettingsData
{
    public List<PrinterProfile> Printers { get; set; } = [];
    public string? DefaultPrinterId { get; set; }
}

/// <summary>Persiste la liste des imprimantes Zebra configurees en JSON dans
/// %ProgramData%\openZPL, partagee entre tous les utilisateurs du poste.</summary>
public class PrinterSettingsStore
{
    private readonly string _filePath;
    private readonly string _legacyFilePath;

    public PrinterSettingsStore()
    {
        // %ProgramData%\openZPL — partage la configuration des imprimantes entre
        // tous les utilisateurs du poste (contrairement a %AppData%, propre a chacun).
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "openZPL");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "printers.json");

        // L'ancien fichier mono-imprimante a ete ecrit avant l'ajout du support
        // multi-imprimantes, dans %AppData% (propre a l'utilisateur) — on continue
        // de le chercher la pour la migration ponctuelle.
        string legacyDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "openZPL");
        _legacyFilePath = Path.Combine(legacyDir, "printer.json");
    }

    public PrinterSettingsData Load()
    {
        PrinterSettingsData data = File.Exists(_filePath)
            ? LoadFrom(_filePath) ?? new PrinterSettingsData()
            : MigrateLegacySettings();

        foreach (PrinterProfile p in data.Printers)
            p.IsDefault = p.Id == data.DefaultPrinterId;

        return data;
    }

    public void Save(PrinterSettingsData data) =>
        File.WriteAllText(_filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));

    private PrinterSettingsData? LoadFrom(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PrinterSettingsData>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Convertit l'ancien fichier printer.json (une seule imprimante) vers le
    /// nouveau format multi-imprimantes, la premiere fois que l'app demarre apres mise a jour.</summary>
    private PrinterSettingsData MigrateLegacySettings()
    {
        if (!File.Exists(_legacyFilePath)) return new PrinterSettingsData();
        try
        {
            string json = File.ReadAllText(_legacyFilePath);
            var legacy = JsonSerializer.Deserialize<LegacyPrinterSettings>(json);
            if (legacy is null || string.IsNullOrWhiteSpace(legacy.IpAddress))
                return new PrinterSettingsData();

            var profile = new PrinterProfile
            {
                Name = "Imprimante Zebra",
                IpAddress = legacy.IpAddress,
                Port = legacy.Port,
            };
            var data = new PrinterSettingsData { Printers = [profile], DefaultPrinterId = profile.Id };
            Save(data);
            return data;
        }
        catch
        {
            return new PrinterSettingsData();
        }
    }

    private class LegacyPrinterSettings
    {
        public bool Enabled { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 9100;
    }
}
