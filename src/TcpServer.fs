module TcpServer

open System
open System.Net
open System.Net.Sockets
open Domain

let startTcpServer (port: int) (logStream: IObservable<AccessLog>) =
    async {
        let ipEndPoint = new IPEndPoint(IPAddress.Any, port)
        let listener = new TcpListener(ipEndPoint)
        try
            listener.Start()
            printfn "TCP Server started on port %d" port

            let clients = System.Collections.Concurrent.ConcurrentBag<TcpClient>()

            let broadcastLog (log: AccessLog) =
                clients
                |> Seq.iter (fun client ->
                    try
                        let stream = client.GetStream()
                        let jsonMessage = sprintf """{"UserId": "%s", "Timestamp": "%O", "Event": "%A", "PreviousState": "%A", "NewState": "%A", "Message": "%s"}"""
                                            (match log.UserId with Some id -> id | None -> "N/A")
                                            log.Timestamp
                                            log.Event
                                            log.PreviousState
                                            log.NewState
                                            (match log.Message with Some msg -> msg | None -> "")
                        let dataBytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage)
                        stream.WriteAsync(dataBytes, 0, dataBytes.Length) |> ignore
                    with ex ->
                        printfn "Error sending log to client: %s" ex.Message
                )

            use subscription = logStream.Subscribe(broadcastLog)

            let rec acceptClients() =
                async {
                    let! client = listener.AcceptTcpClientAsync() |> Async.AwaitTask
                    clients.Add(client) |> ignore
                    printfn "Client connected: %A" client.Client.RemoteEndPoint
                    return! acceptClients()
                }

            do! acceptClients()

        finally
            listener.Stop()
            printfn "TCP Server stopped."
    }

