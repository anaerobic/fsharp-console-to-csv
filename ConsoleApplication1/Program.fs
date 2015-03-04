open System
open System.IO
open FSharp.Data
open FSharp.Control.Reactive
open FSharp.Control

type consoleReader() =
    let readEvent = new Event<_>()
    let mutable stop = false
    [<CLIEvent>]
    member this.ReadEvent = readEvent.Publish
    member this.StartReader = 
        async {
                while stop = false do
                    Console.ReadLine()
                        |> readEvent.Trigger
            }
            |> Async.RunSynchronously
    member this.StopReader = stop <- true;

let outFile = 
    let desktopFolder = (Environment.GetFolderPath Environment.SpecialFolder.Desktop)
    desktopFolder :: ["fsharp.csv"]
        |> List.toArray
        |> Path.Combine 

let joinLines (lines:string list) = String.Join(Environment.NewLine, lines)

let parseToCsvNoHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = false)

let toCsv (lines: string list) =
    lines
        |> joinLines
        |> parseToCsvNoHeader

[<EntryPoint>]
let main argv = 
    let cReader = new consoleReader()
    let readerSub = cReader.ReadEvent
                        |> Observable.bufferCount 2
                        |> Observable.map (fun a -> a |> List.ofSeq)
                        |> Observable.map toCsv
                        |> Observable.subscribe (fun x -> x.Save(outFile))
    let stopSub = cReader.ReadEvent
                    |> Observable.filter (fun x -> match x with
                                                    | null 
                                                    | "" -> true
                                                    | _ -> false)
                    |> Observable.subscribe (fun x -> cReader.StopReader)
    cReader.StartReader

    0 // return an integer exit code
