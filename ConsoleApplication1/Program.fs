open System
open System.IO
open FSharp.Data
open FSharp.Control.Reactive
open FSharp.Control

let outFile = 
    let desktopFolder = (Environment.GetFolderPath Environment.SpecialFolder.Desktop)
    desktopFolder :: [ "fsharp.csv" ]
    |> List.toArray
    |> Path.Combine

let joinLines (lines : string list) = String.Join(Environment.NewLine, lines)
let parseToCsvWithHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = true)
let parseToCsvWithoutHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = false)

let toCsv (lines : string list) = 
    lines
    |> joinLines
    |> parseToCsvWithHeader

let appendAll text = File.AppendAllText(outFile, text)
let writeAll text = File.WriteAllText(outFile, text)
let csvAppend (csv : CsvFile) : Unit = csv.SaveToString() |> appendAll
let csvWriteAll (csv : CsvFile) : Unit = csv.SaveToString() |> writeAll

type consoleReader() = 
    let readEvent = new Event<_>()
    let mutable stop = false
    
    [<CLIEvent>]
    member this.ReadEvent = readEvent.Publish
    
    member this.StartReader = 
        try 
            async { 
                printfn "enter your header row:"
                Console.ReadLine()
                |> parseToCsvWithoutHeader
                |> csvWriteAll
                while stop = false do
                    Console.ReadLine() |> readEvent.Trigger
            }
            |> Async.RunSynchronously
        with ex -> 
            printfn "Nooo! %s" ex.Message
            stop <- true
    
    member this.StopReader = stop <- true

[<EntryPoint>]
let main argv = 
    let cReader = new consoleReader()
    
    let readerSub = 
        cReader.ReadEvent
        |> Observable.bufferCount 2
        |> Observable.map (fun a -> a |> List.ofSeq)
        |> Observable.map toCsv
        |> Observable.subscribe csvAppend
    
    let stopSub = 
        cReader.ReadEvent
        |> Observable.filter (fun x -> 
               match x with
               | null | "" -> true
               | _ -> false)
        |> Observable.subscribe (fun x -> cReader.StopReader)
    
    cReader.StartReader; 0 // return an integer exit code
