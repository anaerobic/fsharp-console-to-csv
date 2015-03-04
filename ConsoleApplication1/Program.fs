open System
open System.IO
open FSharp.Data

let rec procinput lines callback = 
    callback lines
    try 
        match Console.ReadLine() with
        | null -> lines
        | "" -> procinput lines callback
        | x -> procinput (x :: lines) callback
    with ex -> 
        printfn "Nooo! %s" ex.Message
        []

let outFile = 
    let desktopFolder = (Environment.GetFolderPath Environment.SpecialFolder.Desktop)
    desktopFolder :: [ "fsharp.csv" ]
    |> List.toArray
    |> Path.Combine

let reverseLines lines = List.rev lines
let joinLines (lines : string list) = String.Join(Environment.NewLine, lines)

let reverseAndJoinLines lines = 
    lines
    |> reverseLines
    |> joinLines

let parseToCsvNoHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = false)
let parseToCsvWithHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = true)
let saveCsvToString (csv : FSharp.Data.CsvFile) = csv.SaveToString()
let wrapInList s = [ s ]

let toCsv (lines : string list) = 
    lines
    |> reverseAndJoinLines
    |> parseToCsvWithHeader

let toCsvAndSave (lines : string list) : Unit = 
    match lines.Length with
    | 0 -> ignore 0
    | _ -> 
        let csv = toCsv lines
        csv.Save(outFile, ',', '"')

let rec gogogadget lines = 
    File.WriteAllLines(outFile, List.toArray lines)
    match procinput lines toCsvAndSave with
    | [] -> gogogadget lines
    | _ -> ignore 0

let addHeaderRow = 
    printfn "enter your header row:"
    match Console.ReadLine() with
    | line -> 
        line
        |> parseToCsvNoHeader
        |> saveCsvToString
        |> wrapInList

[<EntryPoint>]
let main argv = 
    gogogadget addHeaderRow
    0 // return an integer exit code
