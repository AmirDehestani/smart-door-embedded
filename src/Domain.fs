module Domain
open System

type DoorState =
    | Open
    | ClosedUnlocked
    | ClosedLocked

type DoorEvent =
    | SwipeCard of userId: string
    | OpenDoor
    | CloseDoor

type AccessLog = {
    UserId: string option
    Timestamp: DateTime
    Event: DoorEvent
    PreviousState: DoorState
    NewState: DoorState
    Message: string option
}