module TcpServer

open System
open System.Net
open System.Net.Sockets
open Domain

/// Generic TCP server that broadcasts messages to connected clients
/// <param name="port">Port number to listen on</param>
/// <param name="serverName">Name of the server for logging purposes</param>
/// <param name="clientName">Name of the client for logging purposes</param>
/// <param name="messageFormatter">Function to format messages for clients</param>
/// <param name="stream">Stream of messages to broadcast</param>
/// <returns>Async workflow representing the server operation</returns>
let private createTcpServer<'T> (port: int) (serverName: string) (clientName: string) (messageFormatter: 'T -> string) (stream: IObservable<'T>) =
    async {
        let ipEndPoint = new IPEndPoint(IPAddress.Any, port)
        let listener = new TcpListener(ipEndPoint)
        try
            listener.Start()
            printfn "%s started on port %d" serverName port

            let clients = System.Collections.Concurrent.ConcurrentBag<TcpClient>()

            let broadcast (data: 'T) =
                clients
                |> Seq.iter (fun client ->
                    try
                        let clientStream = client.GetStream()
                        let jsonMessage = messageFormatter data
                        let dataBytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage)
                        clientStream.WriteAsync(dataBytes, 0, dataBytes.Length) |> ignore
                    with ex ->
                        printfn "Error sending %s to client: %s" clientName ex.Message
                )

            use subscription = stream.Subscribe(broadcast)

            let rec acceptClients() =
                async {
                    let! client = listener.AcceptTcpClientAsync() |> Async.AwaitTask
                    clients.Add(client) |> ignore
                    printfn "%s client connected: %A" clientName client.Client.RemoteEndPoint
                    return! acceptClients()
                }

            do! acceptClients()

        finally
            listener.Stop()
            printfn "%s stopped." serverName
    }

/// Start a TCP server to broadcast access logs
/// <param name="port">Port number to listen on</param>
/// <param name="logStream">Stream of access log entries</param>
/// <returns>Async workflow representing the server operation</returns>
let startLogsTcpServer (port: int) (logStream: IObservable<AccessLog>) =
    let formatAccessLog (log: AccessLog) =
        sprintf """{"UserId": "%s", "Timestamp": "%O", "Event": "%A", "PreviousState": "%A", "NewState": "%A", "Message": "%s"}"""
            (match log.UserId with Some id -> id | None -> "N/A")
            log.Timestamp
            log.Event
            log.PreviousState
            log.NewState
            (match log.Message with Some msg -> msg | None -> "")
    
    createTcpServer port "Access Logs TCP Server" "log" formatAccessLog logStream

/// Start a TCP server to broadcast door state updates
/// <param name="port">Port number to listen on</param>
/// <param name="stateStream">Stream of door state updates</param>
/// <returns>Async workflow representing the server operation</returns>
let startDoorStateTcpServer (port: int) (stateStream: IObservable<DoorState>) =
    let formatDoorState (state: DoorState) =
        sprintf """{"State": "%A", "Timestamp": "%O"}"""
            state
            DateTime.Now
    
    createTcpServer port "Door State TCP Server" "door state" formatDoorState stateStream
