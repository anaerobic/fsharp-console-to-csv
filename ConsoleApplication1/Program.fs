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

let createConsoleStream (cancelationToken : System.Threading.CancellationToken) = 
    let lineEvent = new Event<_>()
    
    let tryTrigger msg = 
        try 
            msg |> lineEvent.Trigger
        with ex -> printfn "Nooo! %s" ex.Message
    
    let streamSource = 
        async { 
            while not cancelationToken.IsCancellationRequested do
                Console.ReadLine() |> tryTrigger
        }
    
    (streamSource, lineEvent.Publish)

let isNullOrEmpty str = 
    match str with
    | null | "" -> true
    | _ -> false

let notEmpty str = not (isNullOrEmpty str)
let joinLines (lines : string list) = String.Join(Environment.NewLine, lines)
let parseToCsvWithHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = true)
let parseToCsvWithoutHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = false)
let appendAll file text = File.AppendAllText(file, text)
let writeAll file text = File.WriteAllText(file, text)
let csvAppend file (csv : CsvFile) = csv.SaveToString() |> appendAll file
let appendToFileAsCsv file = Observable.map parseToCsvWithHeader >> Observable.subscribe (csvAppend file)
let csvWriteAll file (csv : CsvFile) = csv.SaveToString() |> writeAll file
let writeHeader file = parseToCsvWithoutHeader >> csvWriteAll file

let splitOnData stream = 
    let hasData = stream |> Observable.filter notEmpty
    let noData = stream |> Observable.filter isNullOrEmpty
    (hasData, noData)

[<EntryPoint>]
let main argv = 
    let cancelReader = new System.Threading.CancellationTokenSource()
    let (streamSource, stream) = createConsoleStream cancelReader.Token
    let (data, noData) = stream |> splitOnData
    
    let appender = 
        data
        |> Observable.takeUntilOther noData
        |> appendToFileAsCsv outFile
    
    let canceller = noData |> Observable.subscribe (fun x -> cancelReader.Cancel())
    printfn "enter your header row:"
    Console.ReadLine() |> writeHeader outFile
    streamSource |> Async.RunSynchronously
    0 // return an integer exit code
