module DoorController
open System
open Domain

/// Event stream for access logs
let logStream = new Event<AccessLog>()

/// Function to handle door events and state transitions
/// <param name="state">Current state of the door</param>
/// <param name="event">Event to process</param>
/// <returns>Tuple of new door state and access log entry</returns>
let handleDoorEvent (state: DoorState) (event: DoorEvent) : DoorState * AccessLog =
    let timestamp = DateTime.UtcNow

    let newState, message = 
        match state, event with
        | Open, SwipeCard userId ->
            let newState = Open
            let message = Some (sprintf "User %s swiped card to unlock the door but the door is already open." userId)
            newState, message
        | Open, OpenDoor ->
            let newState = Open
            let message = Some "Door is already open."
            newState, message
        | Open, CloseDoor ->
            let newState = ClosedLocked
            let message = Some "The door was closed and locked."
            newState, message
        | ClosedUnlocked, SwipeCard userId ->
            let newState = ClosedUnlocked
            let message = Some (sprintf "User %s swiped card to unlock the door but the door is already unlocked." userId)
            newState, message
        | ClosedUnlocked, OpenDoor ->
            let newState = Open
            let message = Some "Door opened."
            newState, message
        | ClosedUnlocked, CloseDoor ->
            let newState = ClosedUnlocked
            let message = Some "Door is already closed (unlocked)."
            newState, message
        | ClosedLocked, SwipeCard userId ->
            let newState = ClosedUnlocked
            let message = Some (sprintf "User %s swiped card and unlocked the door." userId)
            newState, message
        | ClosedLocked, OpenDoor ->
            let newState = ClosedLocked
            let message = Some "Door is locked. Cannot open."
            newState, message
        | ClosedLocked, CloseDoor ->
            let newState = ClosedLocked
            let message = Some "The door is already closed (locked)."
            newState, message

    let log = {
        UserId = 
            match event with
            | SwipeCard userId -> Some userId
            | _ -> None
        Timestamp = timestamp
        Event = event
        PreviousState = state
        NewState = newState
        Message = message
    }

    newState, log


/// Agent to process door events asynchronously
/// <param name="initialState">Initial state of the door</param>
/// <returns>MailboxProcessor instance</returns>
let startDoorControllerAgent (initialState: DoorState) =
    MailboxProcessor.Start(fun inbox ->
        let rec loop state =
            async {
                let! event = inbox.Receive()
                let newState, log = handleDoorEvent state event
                logStream.Trigger(log)
                return! loop newState
            }
        loop initialState
    )