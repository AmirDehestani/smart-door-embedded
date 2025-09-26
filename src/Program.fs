open System
open Domain
open DoorController

[<EntryPoint>]
let main argv =
    let doorAgent = startDoorControllerAgent(ClosedLocked)

    let users = ["Amir"; "John"; "Alice"; "Jane"]
    let random = Random()

    // Simulate some door events
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

    // Keep console open
    printfn "Press any key to exit..."
    Console.ReadKey() |> ignore
    0