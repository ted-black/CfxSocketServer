using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CfxSocketServer.Session.SessionSocket;

/// <summary>
/// Creates a secure socket for communication, and 
/// authenticates and exposes the SSL stream for secure communication.
/// The handshake is to be implmented by the derived class.
/// </summary>
public abstract class SecureSocket
{
    /// <summary>
    /// This socket will accept incoming connection and return a new socket for communication.
    /// </summary>
    private Socket connectSocket = null;

    /// <summary>
    /// Is run to stop listening
    /// </summary>
    protected bool isListen = true;

    /// <summary>
    /// SSL stream for secure communication
    /// </summary>
    protected SslStream sslStream = null;

    /// <summary>
    /// Threads to listen for sockets
    /// </summary>
    private Thread worker;

    protected SecureSocket()
    {
        // Create a new socket
        //
        connectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    /// <summary>
    /// Start the server
    /// </summary>
    public void Start()
    {
        // Start the server
        //
        worker = new Thread(new ThreadStart(Worker));
        worker.Start();
    }

    /// <summary>
    /// Stop the server
    /// </summary>
    public void Stop()
    {
        isListen = false;
    }

    /// <summary>
    /// Worker starts the loop -- kick this off with a tread to keep from blocking the main thread.
    /// </summary>
    private void Worker()
    {
        Console.WriteLine("Starting server...");

        // Creat the task to listen for incoming connections
        //
        Task task = ListenAsync();

        // Start the task and wait for it to complete
        //
        task.Wait();
    }

    /// <summary>
    /// Listen for incoming connections
    /// </summary>
    /// <returns></returns>
    private async Task ListenAsync()
    {
        // Bind the socket to and prepare to listen
        //
        connectSocket.Bind(new IPEndPoint(IPAddress.Any, Constant.SocketPort));

        // This puts the socket in a listening state, but does not block the thread.
        //
        connectSocket.Listen(Constant.MaxConnCount);

        while (isListen)
        {

            if (connectSocket != null && connectSocket.IsBound)
            {
                try
                {
                    // This is the point where the server starts listening.
                    // Accept returns a new socket for communication, when a client connects.
                    // Hence, the while loop will keep the server running.
                    //
                    Socket sessionSocket = await connectSocket.AcceptAsync();
                    await CreateSslStreamAsync(sessionSocket);
                }
                catch (Exception ex)
                {
                    if (connectSocket.Connected)
                    {
                        connectSocket.Close();
                    }
                    Console.WriteLine($"Failed to accept incoming connection and make hand shake. Message: {ex.Message}");
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                // Exit the while loop if the socket is not bound.
                //
                Console.WriteLine("Failed to bind socket to endpoint.");
                return;
            }
        }
    }

    /// <summary>
    /// Create an SSL stream for secure communication
    /// </summary>
    /// <param name="sessionSocket"></param>
    /// <returns></returns>
    private async Task CreateSslStreamAsync(Socket sessionSocket)
    {
        // Create a new SSL stream
        //
        sslStream = new(new NetworkStream(sessionSocket, true));

        // Get the cert
        //
        X509Certificate x509Certificate = X509CertificateLoader.LoadPkcs12FromFile(Constant.Security.WebSocketCert, Constant.Security.WebSocketKey);

        // Authenticate the server
        //
        await sslStream.AuthenticateAsServerAsync(x509Certificate, false, true);

        // Execute the handshake
        //
        await ExecuteHandshake();
    }

    /// <summary>
    /// Execute the handshake
    /// </summary>
    protected virtual Task ExecuteHandshake()
    {
        return Task.CompletedTask;
    }
}
