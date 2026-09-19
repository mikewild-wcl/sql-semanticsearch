using System.Net;
using System.Net.Sockets;

namespace Sql.SemanticSearch.AppHost.Extensions;

internal static class PortHelpers
{
    public static int GetFreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
