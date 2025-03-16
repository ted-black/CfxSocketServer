using CfxSocketServer.Channel;
using CfxSocketServer.Comm.Server;
using CfxSocketServer.Session.Frame;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace CfxSocketServer.Session;

/// <summary>
/// WebSocket session object
/// </summary>
/// <param name="sslStream"></param>
/// <param name="frameHeaderBuilder"></param>
/// <param name="frameResolver"></param>
public class WebSocketSession(ref SslStream sslStream, IFrameHeaderBuilder frameHeaderBuilder, IFrameResolver frameResolver) : IWebSocketSession
{
    #region Interace methods and properties

    /// <inheritdoc cref="IWebSocketSession.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="IWebSocketSession.OnMessage"/>
    public event EventHandler OnMessage;

    /// <inheritdoc cref="IWebSocketSession.OnClose"/>
    public event EventHandler OnClose;

    /// <inheritdoc cref="IWebSocketSession.OnPing"/>
    public event EventHandler OnPing;

    /// <inheritdoc cref="IWebSocketSession.QueryStringParams"/>
    public Dictionary<string, string> QueryStringParams { get; set; }

    /// <inheritdoc cref="IWebSocketSession.Name"/>
    public string Name
    {
        get
        {
            if (QueryStringParams?.ContainsKey(Constant.Channel.UserName) ?? false)
            {
                return QueryStringParams[Constant.Channel.UserName];
            }
            else
            {
                return "Unknown";
            }
        }
    }

    /// <inheritdoc cref="IChannelWriter.ChannelId"/>
    public Guid? ChannelId { get; set; } = null;

    /// <summary>
    /// Is listen to control listening loop
    /// </summary>
    public bool IsListen { get; set; } = true;

    /// <inheritdoc cref="IWebSocketSession.Start"/>
    public void Start()
    {
        _ = Task.Run(async () => await ListenAsync());
    }

    /// <inheritdoc cref="IWebSocketSession.WriteTextAsync"/>
    public async Task WriteTextAsync(string payload)
    {
        List<byte> payloadBytes = BuildTextPayload(payload);
        await sslStream.WriteAsync(payloadBytes.ToArray());
    }

    /// <inheritdoc cref="IWebSocketSession.WriteBinaryAsync"/>
    public async Task WriteBinaryAsync(List<byte> payload)
    {
        List<byte> payloadBytes = BuildBinaryPayload(payload);
        await sslStream.WriteAsync(payloadBytes.ToArray());
    }

    /// <inheritdoc cref="IWebSocketSession.SendConnClose"/>
    public async Task SendConnClose(List<byte> payload)
    {
        List<byte> payloadBytes = BuildConnClosedPayload(payload);
        await writeLock.WaitAsync();
        try
        {
            await sslStream.WriteAsync(payloadBytes.ToArray());
        }
        finally
        {
            writeLock.Release();
        }
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        sslStream.Flush();
        sslStream.Close();
        sslStream.Dispose();
        GC.SuppressFinalize(this);
        Console.WriteLine("Session disposed.");
    }

    #endregion Interface methods and properties

    #region Private members

    /// <summary>
    /// Write lock to control write access for ssl stream
    /// </summary>
    private SemaphoreSlim writeLock = new(1, 1);

    /// <summary>
    /// SSL stream for secure communication
    /// </summary>
    private SslStream sslStream = sslStream;

    #endregion Private members

    #region Private, protected methods

    /// <summary>
    /// Message recieve handler
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnMessageReceived(EventArgs e)
    {
        OnMessage?.Invoke(this, e);
    }

    /// <summary>
    /// Session close handler
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnSessionClose(EventArgs e)
    {
        OnClose?.Invoke(this, e);
    }

    /// <summary>
    /// Ping received handler
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnPingReceived(EventArgs e)
    {
        OnPing?.Invoke(this, e);
    }

    /// <summary>
    /// Listen for incoming transmissions, the receive async method will block until a payload is available.
    /// </summary>
    private async Task ListenAsync()
    {
        while (IsListen)
        {
            try
            {
                // Recieve a payload -- this will block until a payload is available.
                // In the case of a continuation frame, null is return to queue the frames
                // until the final frame is encountered.
                //
                Transmission transmission = await RecieveAsync(sslStream);

                // Raise events if not continuing
                //
                if (transmission != null)
                {
                    switch (transmission.OpCode)
                    {
                        case OpCode.ConnClosed:
                            IsListen = false;
                            // Create the on close event and raise it and kill the listening loop i.e. the client has closed the conn
                            //
                            OnWebSocketSessionCloseEventArgs onSessionCloseEventArgs = new() { Transmission = transmission, WebSocketSessionId = Id, WebSocketSessionName = Name };
                            OnSessionClose(onSessionCloseEventArgs);
                            break;
                        case OpCode.Pong:
                            // Create the on ping event and raise it
                            //
                            OnWebSocketSessionPingEventArgs onPingEventArgs = new() { Transmission = transmission, WebSocketSessionId = Id, WebSocketSessionName = Name };
                            OnPingReceived(onPingEventArgs);
                            break;
                        default:
                            // This transmission is a message, so we raise the on message event. 
                            //
                            OnTransmissionArgs onTransmissionEventArgs = new() { Transmission = transmission, WebSocketSessionId = Id, WebSocketSessionName = Name };
                            OnMessageReceived(onTransmissionEventArgs);
                            break;
                    }
                }
            }
            catch (ObjectDisposedException ex)
            {
                IsListen = false;
                string message = ex.Message.Split(':')[^1].Trim().Replace("'", "").Replace(".", "");
                Console.WriteLine($"Object disposed exception: {message}");
            }
            catch (Exception ex)
            {
                IsListen = false;
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        Dispose();
        Console.WriteLine($"Session closed, sessionId: {Id}");
    }

    /// <summary>
    /// Recieve payload -- this will block at the ssl stream call 'Read' until a payload is available
    /// </summary>
    /// <returns></returns>
    private async Task<Transmission> RecieveAsync(SslStream sslStream)
    {
        // Recieve frame header, where Read is called on the ssl stream object and therefore blocks.
        //
        FrameHeader frameHeader = await frameHeaderBuilder.GetInboundFrameHeaderAsync(sslStream);

        List<byte> payloadByteList;

        // Client close handling
        //
        if (frameHeader.OpCode == OpCode.ConnClosed)
        {
            // Get payload
            //
            payloadByteList = await ReadPayloadAsync(sslStream, frameHeader);

            // Un mask payload
            //
            payloadByteList = UnMaskPayload(frameHeader, payloadByteList);

            // Return closed message
            //
            return new Transmission
            {
                Payload = payloadByteList,
                OpCode = OpCode.ConnClosed
            };
        }

        // Client ping handling
        //
        if (frameHeader.OpCode == OpCode.Ping)
        {
            // Get payload
            //
            payloadByteList = await ReadPayloadAsync(sslStream, frameHeader);

            // Un mask payload
            //
            payloadByteList = UnMaskPayload(frameHeader, payloadByteList);

            // Return closed message
            //
            return new Transmission
            {
                Payload = payloadByteList,
                OpCode = OpCode.Pong
            };
        }

        // Now recieve payload per the frame header.
        //
        payloadByteList = await ReadPayloadAsync(sslStream, frameHeader);

        // Un mask payload
        //
        payloadByteList = UnMaskPayload(frameHeader, payloadByteList);

        // Resolve frame, queue payload
        //
        payloadByteList = frameResolver.ResolveFrame(frameHeader, payloadByteList);

        // Return if continuing
        //
        if (payloadByteList is null)
        {
            return null;
        }

        // The message bytes with either the final frame's op code or the intial non final frame's op code.
        //
        Transmission message = new()
        {
            Payload = payloadByteList,
            OpCode = frameResolver.InitialFrameOpCode == null ? frameHeader.OpCode : frameResolver.InitialFrameOpCode
        };

        // Initialize the frame resolver
        //
        frameResolver.Reset();

        return message;
    }

    /// <summary>
    /// Un mask client payload
    /// </summary>
    /// <param name="frameHeader"></param>
    /// <param name="payloadByteList"></param>
    private List<byte> UnMaskPayload(FrameHeader frameHeader, List<byte> payloadByteList)
    {
        if (frameHeader.IsMask)
        {
            for (int i = 0; i < payloadByteList.Count; i++)
            {
                payloadByteList[i] ^= frameHeader.Mask[i % 4];
            }
        }

        return payloadByteList;
    }

    /// <summary>
    /// Reads the payload from the ssl stream per the header.
    /// </summary>
    /// <param name="sslStream"></param>
    /// <param name="frameHeader"></param>
    /// <returns></returns>
    private async Task<List<byte>> ReadPayloadAsync(SslStream sslStream, FrameHeader frameHeader)
    {
        byte[] payloadBuffer = new byte[Constant.BufferSize];
        List<byte> payloadByteList = new List<byte>();
        int bytesRead = 0;
        while ((UInt64)bytesRead < frameHeader.PayloadLength)
        {
            int read = await sslStream.ReadAsync(payloadBuffer);
            bytesRead += read;
            for (int i = 0; i < read; i++)
            {
                payloadByteList.Add(payloadBuffer[i]);
            }
        }

        return payloadByteList;
    }

    /// <summary>
    /// Build text payload with frame header
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    private List<byte> BuildTextPayload(string payload)
    {
        // Get the frame header
        //
        List<byte> payloadBytes = frameHeaderBuilder.GetOutboundFrameHeader((ulong)payload.Length, OpCode.Text);

        // Add the payload
        //
        payloadBytes.AddRange(Encoding.UTF8.GetBytes(payload));

        return payloadBytes;
    }

    /// <summary>
    /// Build binary payload with frame header
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="opCode"></param>
    /// <returns></returns>
    private List<byte> BuildBinaryPayload(List<byte> payload)
    {
        // Get the frame header
        //
        List<byte> payloadBytes = frameHeaderBuilder.GetOutboundFrameHeader((ulong)payload.Count, OpCode.Binary);

        // Add the payload
        //
        payloadBytes.AddRange(payload);

        return payloadBytes;
    }

    /// <summary>
    /// Build conn closed payload with frame header
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    private List<byte> BuildConnClosedPayload(List<byte> payload)
    {
        // Get the frame header
        //
        List<byte> payloadBytes = frameHeaderBuilder.GetOutboundFrameHeader((ulong)payload.Count, OpCode.ConnClosed);

        // Add the payload
        //
        payloadBytes.AddRange(payload);

        return payloadBytes;
    }

    #endregion Private, protected methods

}
