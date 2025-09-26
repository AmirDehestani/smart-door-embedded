open System
open Domain
open DoorController
open TcpServer
open Logger

[<EntryPoint>]
let main argv =
    let doorAgent = startDoorControllerAgent(ClosedLocked)
    let logAgent = startLogger($"access_log_{DateTime.Now:yyyyMMdd_HHmmss}.csv")

    startLogsTcpServer 9000 logStream.Publish 
    |> Async.Start

    startDoorStateTcpServer 9001 stateStream.Publish
    |> Async.Start

    Console.ReadLine() |> ignore // Keep the application running
    0