module DomainTests

open Expecto
open Domain
open DoorController
open System

[<Tests>]
let tests =
    testList "Domain Tests" [
        
        testCase "SwipeCard unlocks a locked door" <| fun _ ->
            let initialState = ClosedLocked
            let event = SwipeCard "User1"
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState ClosedUnlocked "Door should be unlocked"
            Expect.equal log.UserId (Some "User1") "Log should contain UserId"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "SwipeCard does nothing on an unlocked door" <| fun _ ->
            let initialState = ClosedUnlocked
            let event = SwipeCard "User1"
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState initialState "Door state should remain unchanged"
            Expect.equal log.UserId (Some "User1") "Log should contain UserId"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "SwipeCard on an open door does nothing" <| fun _ ->
            let initialState = Open
            let event = SwipeCard "User1"
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState Open "Door should remain open"
            Expect.equal log.UserId (Some "User1") "Log should contain UserId"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "OpenDoor opens an unlocked door" <| fun _ ->
            let initialState = ClosedUnlocked
            let event = OpenDoor
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState Open "Door should be opened"
            Expect.equal log.UserId None "Log should not contain UserId for OpenDoor event"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "OpenDoor fails on a locked door" <| fun _ ->
            let initialState = ClosedLocked
            let event = OpenDoor
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState ClosedLocked "Door should remain locked"
            Expect.equal log.UserId None "Log should not contain UserId for OpenDoor event"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "OpenDoor on an already open door does nothing" <| fun _ ->
            let initialState = Open
            let event = OpenDoor
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState Open "Door should remain open"
            Expect.equal log.UserId None "Log should not contain UserId for OpenDoor event"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "CloseDoor closes and locks an open door" <| fun _ ->
            let initialState = Open
            let event = CloseDoor
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState ClosedLocked "Door should be closed and locked"
            Expect.equal log.UserId None "Log should not contain UserId for CloseDoor event"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "CloseDoor on an unlocked door does nothing" <| fun _ ->
            let initialState = ClosedUnlocked
            let event = CloseDoor
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState ClosedUnlocked "Door should remain closed and unlocked"
            Expect.equal log.UserId None "Log should not contain UserId for CloseDoor event"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "CloseDoor on a locked door does nothing" <| fun _ ->
            let initialState = ClosedLocked
            let event = CloseDoor
            let newState, log = DoorController.handleDoorEvent initialState event
            Expect.equal newState ClosedLocked "Door should remain closed and locked"
            Expect.equal log.UserId None "Log should not contain UserId for CloseDoor event"
            Expect.equal log.PreviousState initialState "Log should contain previous state"
            Expect.equal log.NewState newState "Log should contain new state"
            Expect.isSome log.Message "Log should contain a message"

        testCase "Log always contains timestamp and event" <| fun _ ->
            let initialState = ClosedLocked
            let event = SwipeCard "User1"
            let _, log = DoorController.handleDoorEvent initialState event
            Expect.isGreaterThan log.Timestamp System.DateTime.MinValue "Log should contain timestamp"
            Expect.equal log.Event event "Log should contain the event"

        testCase "Concurrent door operations maintain consistency" <| fun _ ->
            let doorAgent = DoorController.startDoorControllerAgent(ClosedLocked)
            let users = ["Amir"; "John"; "Alice"; "Jane"]
            let actionsCount = 5
            let random = Random()
            let logEvents = ResizeArray<AccessLog>()
            let stateEvents = ResizeArray<DoorState>()


            DoorController.logStream.Publish.Add(fun log -> logEvents.Add(log))
            DoorController.stateStream.Publish.Add(fun state -> stateEvents.Add(state))

            let simulateUserAction userId =
                async {
                    for _ in 1 .. actionsCount do
                        do! Async.Sleep(random.Next(50, 200))
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

            Expect.equal logEvents.Count (users.Length * actionsCount) "Should have logged all events"
            Expect.equal stateEvents.Count (users.Length * actionsCount) "Should have recorded all state changes"
            
            for log in logEvents do
                let expectedNewState, _ = DoorController.handleDoorEvent log.PreviousState log.Event
                Expect.equal log.NewState expectedNewState "Each logged transition should be valid"
                Expect.isGreaterThan log.Timestamp System.DateTime.MinValue "Each log should have a timestamp"
    ]