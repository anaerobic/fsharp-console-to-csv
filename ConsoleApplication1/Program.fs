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

type consoleReader() = 
    let readEvent = new Event<_>()
    let mutable stop = false
    
    [<CLIEvent>]
    member this.LineStream = readEvent.Publish
    
    member this.StartReader = 
        try 
            async { 
                while stop = false do
                    Console.ReadLine() |> readEvent.Trigger
            }
            |> Async.RunSynchronously
        with ex -> 
            printfn "Nooo! %s" ex.Message
            stop <- true
    
    member this.StopReader = stop <- true

let joinLines (lines : string list) = String.Join(Environment.NewLine, lines)
let parseToCsvWithHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = true)
let parseToCsvWithoutHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = false)

let appendAll file text = File.AppendAllText(file, text)
let writeAll file text = File.WriteAllText(file, text)
let csvAppend file (csv : CsvFile) : Unit = csv.SaveToString() |> appendAll file
let csvWriteAll file (csv : CsvFile) : Unit = csv.SaveToString() |> writeAll file

let isNullOrEmpty str = 
    match str with
       | null | "" -> true
       | _ -> false

let notEmpty str = not (isNullOrEmpty str)

let writeHeader file = 
    parseToCsvWithoutHeader
    >> csvWriteAll file

let appendToFileAsCsv file = 
    Observable.filter notEmpty
    >> Observable.map parseToCsvWithHeader
    >> Observable.subscribe (csvAppend file)

[<EntryPoint>]
let main argv = 
    let cReader = new consoleReader()
    
    let readerSub = 
        cReader.LineStream
        |> appendToFileAsCsv outFile
    
    let stopSub = 
        cReader.LineStream
        |> Observable.filter isNullOrEmpty
        |> Observable.subscribe (fun x -> cReader.StopReader)

    printfn "enter your header row:"
    Console.ReadLine() |> writeHeader outFile
    cReader.StartReader; 0 // return an integer exit code
