using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;

#if WINDOWS_UWP
using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;
    using System;
    using System.IO;
    using System.Threading.Tasks;
#endif


public class CommToViewer : MonoBehaviour
{        
    private int port = 25552;    
   
    private string clientAddress;
#if WINDOWS_UWP
    IOutputStream outStream;    

    StreamSocketListener streamSocketListener = new Windows.Networking.Sockets.StreamSocketListener();


    // Use this for initialization
    async void Start()
    {
        Debug.Log("Server is Listening for megs from the viewer");


        // The ConnectionReceived event is raised when connections are received.
        streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;
        await Task.Run(() => { StartServer(); });

    }
#endif
#if WINDOWS_UWP
    private async void StartServer()
    {
        try
        {  // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
            await streamSocketListener.BindServiceNameAsync(port.ToString());

            Debug.Log("server is listening...");
        }
        catch (System.Exception ex)
        {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
        }
    }

    private async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
    {
        string request;        
        Debug.Log("reading stream.....");

        var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead(), System.Text.Encoding.UTF8, true, 1024, true);

        request = await streamReader.ReadLineAsync();
        clientAddress = request;
        
        Debug.Log(string.Format("server received the request: \"{0}\"", request));

        // Echo the request back as the response.

        outStream = args.Socket.OutputStream;
                
        Debug.Log(string.Format("server sent back the response: \"{0}\"", request));
        
        Debug.Log("server closed its socket");
    }

#endif

    // Update is called once per frame
    void Update()
    {

    }
#if WINDOWS_UWP
    async void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("in background");
            // action on going background
            try
            {
                
                Stream outputStream = outStream.AsStreamForWrite();

                var streamWriter = new StreamWriter(outputStream, System.Text.Encoding.UTF8, 1024, true);

                await streamWriter.WriteLineAsync("bcgrd");
                await streamWriter.FlushAsync();
                Debug.Log("notifcation sent from Hololens");
                

            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());                
            }
        }
        else
        {
            // action on coming from background
            try
            {
                #if WINDOWS_UWP
                Stream outputStream = outStream.AsStreamForWrite();

                var streamWriter = new StreamWriter(outputStream, System.Text.Encoding.UTF8, 1024, true);

                await streamWriter.WriteLineAsync("back");
                await streamWriter.FlushAsync();
                Debug.Log("notifcation sent from Hololens");
                #endif

            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
    #endif
}
