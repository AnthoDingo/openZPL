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
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace openZPL.Services;

/// <summary>Associe l'extension .ozpl a l'application : icone dans
/// l'Explorateur et ouverture au double-clic. L'enregistrement se fait sous
/// HKEY_CURRENT_USER, donc sans droits administrateur et sans toucher aux
/// autres comptes du poste.</summary>
public static class FileAssociation
{
    /// <summary>Identifiant du type de fichier dans la base de registre.</summary>
    private const string ProgId = "openZPL.Label";
    private const string FileTypeLabel = "Etiquette openZPL";

    /// <summary>Enregistre l'association si elle manque ou si le chemin de
    /// l'executable a change (application deplacee, mise a jour). Ne fait rien
    /// si tout est deja en place, pour ne pas ecrire dans le registre a chaque
    /// demarrage.</summary>
    public static void EnsureRegistered()
    {
        string? exe = Environment.ProcessPath;
        if (exe is null || !File.Exists(exe)) return;

        string command = $"\"{exe}\" \"%1\"";
        string icon = $"\"{exe}\",0";

        try
        {
            if (IsUpToDate(command, icon)) return;

            using (RegistryKey ext = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{LabelFile.Extension}"))
                ext.SetValue(null, ProgId);

            using (RegistryKey type = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}"))
                type.SetValue(null, FileTypeLabel);

            using (RegistryKey ico = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}\DefaultIcon"))
                ico.SetValue(null, icon);

            using (RegistryKey cmd = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ProgId}\shell\open\command"))
                cmd.SetValue(null, command);

            // sans cela l'Explorateur garde l'ancienne icone jusqu'a sa relance
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            // registre verrouille par une strategie de groupe : l'application
            // reste utilisable, seul le double-clic depuis l'Explorateur manque
        }
    }

    private static bool IsUpToDate(string command, string icon)
    {
        using RegistryKey? ext = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{LabelFile.Extension}");
        if (ext?.GetValue(null) as string != ProgId) return false;

        using RegistryKey? cmd = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ProgId}\shell\open\command");
        if (cmd?.GetValue(null) as string != command) return false;

        using RegistryKey? ico = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ProgId}\DefaultIcon");
        return ico?.GetValue(null) as string == icon;
    }

    private const uint SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint eventId, uint flags, IntPtr item1, IntPtr item2);
}
