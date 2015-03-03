open System
open System.IO
open FSharp.Data

let rec procinput lines callback = 
    callback lines
    match Console.ReadLine() with
    | null -> List.rev lines
    | "" -> procinput lines callback
    | x -> procinput (x :: lines) callback

[<Literal>]
let file = @"C:\scratch\fsharp.csv"

let joinLines lines = String.Join(Envornment.NewLine, lines)

let parseToCsvNoHeader str = CsvFile.Parse(str, hasHeader = false)

let toCsv (lines: string list) =
    lines
        |> joinLines
        |> parseToCsvNoHeader

[<EntryPoint>]
let main argv = 
    let foo = procinput [] toCsv
    foo.Save(file, ',', '"')
    0 // return an integer exit code
