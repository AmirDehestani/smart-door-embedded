module DomainTests

open Expecto
open Domain

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
    ]