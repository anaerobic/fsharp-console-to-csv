open System
open System.IO
open FSharp.Data

let rec procinput lines callback = 
    callback lines
    match Console.ReadLine() with
    | null -> List.rev lines
    | "" -> procinput lines callback
    | x -> procinput (x :: lines) callback

let outFile = 
    ((Environment.GetFolderPath Environment.SpecialFolder.Desktop)
      :: ["fsharp.csv"])
        |> List.toArray
        |> Path.Combine 

let joinLines (lines:string list) = String.Join(Environment.NewLine, lines)

let parseToCsvNoHeader str = FSharp.Data.CsvFile.Parse(str, hasHeaders = false)

let toCsv (lines: string list) =
    lines
        |> joinLines
        |> parseToCsvNoHeader

let toCsvAndSave (lines: string list) : Unit =
    match lines.Length with
    | 0 -> ignore 0
    | _ ->
        let csv = toCsv lines
        csv.Save(outFile, ',', '"')

[<EntryPoint>]
let main argv = 
    let foo = procinput [] toCsvAndSave
    0 // return an integer exit code
