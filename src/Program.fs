open System
open Domain
open DoorController
open TcpServer

[<EntryPoint>]
let main argv =
    let doorAgent = startDoorControllerAgent(ClosedLocked)

    startLogsTcpServer 9000 logStream.Publish 
    |> Async.Start

    startDoorStateTcpServer 9001 stateStream.Publish
    |> Async.Start
    
    0