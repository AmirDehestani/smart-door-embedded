type DoorState =
    | Open
    | ClosedUnlocked
    | ClosedLocked

type DoorEvent =
    | SwipeCard of userId: string
    | OpenDoor
    | CloseDoor

type AccessLog = {
    UserId: string
    Timestamp: System.DateTime
    Event: DoorEvent
    PreviousState: DoorState
    NewState: DoorState
    Message: string option
}