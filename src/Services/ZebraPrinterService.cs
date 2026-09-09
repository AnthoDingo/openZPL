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

using System.Net.Sockets;
using System.Text;
using openZPL.Resources;

namespace openZPL.Services;

/// <summary>Impression reseau sur imprimante Zebra via TCP/IP RAW (port 9100).
/// Compatible ZD421, ZD620, ZT400 et tout modele ZPL II.</summary>
public static class ZebraPrinterService
{
    public static async Task<(bool Ok, string? Error)> PrintZplAsync(
        PrinterProfile? printer, string zpl, CancellationToken ct = default)
    {
        if (printer is null || string.IsNullOrWhiteSpace(printer.IpAddress) ||
            printer.Port is <= 0 or > 65535)
            return (false, Strings.PrintErrorNoPrinter);

        try
        {
            using var client = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            await client.ConnectAsync(printer.IpAddress, printer.Port, cts.Token);

            await using NetworkStream stream = client.GetStream();
            byte[] data = Encoding.UTF8.GetBytes(zpl);
            await stream.WriteAsync(data, cts.Token);
            await stream.FlushAsync(cts.Token);

            return (true, null);
        }
        catch (OperationCanceledException)
        {
            return (false, string.Format(Strings.PrintErrorTimeout, printer.IpAddress, printer.Port));
        }
        catch (SocketException ex)
        {
            return (false, string.Format(Strings.PrintErrorNetwork, ex.Message));
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
