using CfxSocketServer.Session.Frame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CfxSocketServer.Session.SessionSocket;

public class WebSocketListener : SecureSocket
{
    #region Constructors

    public WebSocketListener() : base()
    {
    }

    #endregion Constructors

    /// <summary>
    /// Event handler for open session
    /// </summary>
    public event EventHandler OnWebSocketSessionOpen;

    /// <summary>
    /// Execute the handshake
    /// </summary>
    /// <returns></returns>
    protected override async Task ExecuteHandshake()
    {
        byte[] clientHeader = new byte[Constant.BufferSize];

        int bytesRead = await sslStream.ReadAsync(clientHeader);
        string header = Encoding.UTF8.GetString(clientHeader, 0, bytesRead);

        // Extract query string from the header
        //
        string queryString = ExtractQueryString(header);
        Dictionary<string, string> QueryStringParams = ParseQueryString(queryString);

        // Write the handshake back to the client
        //
        await sslStream.WriteAsync(GetHandShake(header));

        Console.WriteLine("Handshake completed, raising web socket session open event...");

        // Create a frame resolver
        //
        FrameResolver frameResolver = new()
        {
            ContinuationFrameByteQueue = [],
            InitialFrameOpCode = null
        };

        // Raise the web socket session open event
        //
        OnWebSocketSessionOpenEventArgs onWebSocketSessionOpenEvent = new()
        {
            WebSocketSession = new WebSocketSession(ref sslStream, new FrameHeaderBuilder(), frameResolver)
            {
                Id = Guid.NewGuid(),
                QueryStringParams = QueryStringParams
            }
        };

        WebSocketSessionOpen(onWebSocketSessionOpenEvent);
    }

    /// <summary>
    /// Event handler for open session
    /// </summary>
    /// <param name="e"></param>
    protected virtual void WebSocketSessionOpen(EventArgs e)
    {
        OnWebSocketSessionOpen?.Invoke(this, e);
    }

    /// <summary>
    /// Get hand shake bytes
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    private byte[] GetHandShake(string header)
    {
        List<string> keys = [.. header.Split("\r\n")];
        int idx = 0;
        foreach (string s in keys)
        {
            if (s.Contains(Constant.SecWebSocketKey))
            {
                break;
            }
            idx++;
        }
        string acceptKey = keys[idx].Split(":")[1].Trim();
        string handShakeAcceptKey = GetAcceptKey(acceptKey);
        string payLoad = $"{Constant.HandShakeHeader}{handShakeAcceptKey}{Constant.HandShakeNewline}";

        return Encoding.UTF8.GetBytes(payLoad);
    }

    /// <summary>
    /// Get acceptance key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetAcceptKey(string key)
    {
        string longKey = key + Constant.MagicKey;
        byte[] hashBytes = SHA1.HashData(Encoding.ASCII.GetBytes(longKey));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Extracts the query string from the WebSocket request header
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    private string ExtractQueryString(string header)
    {
        string[] lines = header.Split("\r\n");
        string requestLine = lines[0];
        int queryIndex = requestLine.IndexOf('?');
        if (queryIndex >= 0)
        {
            int httpIndex = requestLine.IndexOf(" HTTP/");
            return requestLine.Substring(queryIndex + 1, httpIndex - queryIndex - 1);
        }
        return string.Empty;
    }

    /// <summary>
    /// Parses the query string into a dictionary of key-value pairs
    /// </summary>
    /// <param name="queryString"></param>
    /// <returns></returns>
    private Dictionary<string, string> ParseQueryString(string queryString)
    {
        var queryParams = new Dictionary<string, string>();
        string[] pairs = queryString.Split('&');
        foreach (string pair in pairs)
        {
            string[] keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                queryParams[keyValue[0]] = keyValue[1];
            }
        }
        return queryParams;
    }
}
