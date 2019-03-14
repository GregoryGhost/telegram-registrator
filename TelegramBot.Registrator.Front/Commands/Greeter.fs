namespace TelegramBot.Registrator.Front.Commands

open ExtCore.Control
open Funogram.Bot
open Funogram.Api

open Tools
open Types

type Greeter(commands: Commands list) =
    let formatCommand cmd =
        let header = sprintf "%s - %s" cmd.Name cmd.Description
        let body = 
            let args = 
                if List.isEmpty cmd.Arguments then
                    "аргументы отсутствуют"
                else
                    List.reduce (+) cmd.Arguments
            sprintf "%s %s" cmd.Name args
        sprintf "%s\n%s\n\n" header body
    
    let formattedCommands: string =
        let formattedCommands = 
            if List.isEmpty commands then
                "Команды отсутствуют"
            else
                List.reduce (+)
                <| List.map formatCommand commands
        formattedCommands

    member __.printAvailableCommands =
        printfn "%s" formattedCommands
    
    member __.onStart (settings: Settings) (context: UpdateContext) = 
        maybe {
            settings.Logger.Log "Начал обработку команд."
            let! message = context.Update.Message
            let! name = message.Chat.FirstName

            sprintf "Добро пожаловать, %s! Доступны следующие команды: %s" name formattedCommands
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore