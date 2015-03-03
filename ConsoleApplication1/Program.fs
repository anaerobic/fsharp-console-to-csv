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

let toCsv (lines : string list) = 
    match lines.Length with
    | 0 -> File.WriteAllText(file, "")
    | _ -> 
        lines
        |> (fun x -> String.Join(Environment.NewLine, x))
        |> (fun x -> CsvFile.Parse(x, hasHeaders = false))
        |> (fun x -> x.Save(file, ',', '"'))

[<EntryPoint>]
let main argv = 
    let foo = procinput [] toCsv
    0 // return an integer exit code
