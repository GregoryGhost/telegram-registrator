module Tools
    open Funogram.Types
    open FSharp.Json
    open System.IO

    open Types

    let logResponse<'a> (settings: Settings) (response: Result<'a, ApiResponseError>) =
        match response with
        | Result.Ok _ -> "Запрос успешно принят."
        | Result.Error error -> error.Description
        |> settings.Logger.Log

    let deserialize<'a> (file: string) = 
        let read = File.ReadAllText >> Json.deserialize<'a>
        if File.Exists file then
            read file |> Some 
        else None