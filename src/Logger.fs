module Logger

open Domain
open System
open System.IO
open DoorController

/// Starts a logger that appends access logs to a specified CSV file.
/// If the file does not exist, it creates it and writes the header.
let startLogger (logFilePath: string) =
    let fullPath = "logs/" + logFilePath

    try
        let fileExists = 
            if not (Directory.Exists("logs")) then
                Directory.CreateDirectory("logs") |> ignore
            File.Exists(fullPath)

        use writer = new StreamWriter(fullPath, true)
        if not fileExists then
            writer.WriteLine("Timestamp,UserId,PreviousState,NewState,Event,Message")
    with ex ->
        printfn "Failed to initialize log file: %s" ex.Message

    let logAgent = MailboxProcessor<AccessLog>.Start(fun inbox ->
        let rec loop () = async {
            let! logEntry = inbox.Receive()
            let logLine = sprintf "%s,%A,%A,%A,%A,%s" 
                            (DateTime.UtcNow.ToString("o")) 
                            (logEntry.UserId |> Option.defaultValue "N/A") 
                            logEntry.PreviousState 
                            logEntry.NewState 
                            logEntry.Event 
                            (logEntry.Message |> Option.defaultValue "")
            try
                use writer = new StreamWriter(fullPath, true)
                writer.WriteLine(logLine)
            with ex ->
                printfn "Failed to write log: %s" ex.Message
            return! loop ()
        }
        loop ()
    )
    
    logStream.Publish.Subscribe(logAgent.Post) |> ignore

    logAgent
