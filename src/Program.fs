open System
open Domain
open DoorController
open TcpServer

[<EntryPoint>]
let main argv =
    let doorAgent = startDoorControllerAgent(ClosedLocked)

    startTcpServer 9000 logStream.Publish 
    |> Async.Start

    printfn "Press any key to start simulating door events..."
    Console.ReadKey() |> ignore

    let users = ["Amir"; "John"; "Alice"; "Jane"]
    let random = Random()

    let simulateUserAction userId =
        async {
            for _ in 1 .. 5 do
                do! Async.Sleep(random.Next(500, 2000))
                let actionIndex = random.Next(3)
                match actionIndex with
                | 0 -> doorAgent.Post(SwipeCard userId)
                | 1 -> doorAgent.Post(OpenDoor)
                | 2 -> doorAgent.Post(CloseDoor)
                | _ -> ()
        }

    users
    |> List.map simulateUserAction
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore

    printfn "Press any key to exit..."
    Console.ReadKey() |> ignore
    0