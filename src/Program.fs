open System
open Domain
open DoorController

[<EntryPoint>]
let main argv =
    let doorAgent = startDoorControllerAgent(ClosedLocked)

    // Simulate some door events
    doorAgent.Post(OpenDoor)
    doorAgent.Post(SwipeCard("Amir"))
    doorAgent.Post(OpenDoor)
    doorAgent.Post(OpenDoor)
    doorAgent.Post(CloseDoor)
    doorAgent.Post(CloseDoor)
    doorAgent.Post(OpenDoor)
    doorAgent.Post(SwipeCard("John"))
    doorAgent.Post(SwipeCard("Alice"))
    doorAgent.Post(CloseDoor)
    doorAgent.Post(OpenDoor)
    doorAgent.Post(SwipeCard("Jane"))
    doorAgent.Post(CloseDoor)

    // Keep console open
    printfn "Press any key to exit..."
    Console.ReadKey() |> ignore
    0