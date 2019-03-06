open ExtCore.Control
open Funogram.Bot
open System

open TelegramBot.Registrator.Db
open TelegramBot.Registrator.Front.Commands
open Types
open Funogram

let _cmds = 
    [{ Name = "/start"
       Description = "выводит список доступных команд"
       Arguments = []}
     { Name = "/registrate"
       Description = "регистрация пользовательских данных"
       Arguments = ["ФИО=Ягами Лайт Саитиро, "; "др=28-01-1986"]}
     { Name = "/read"
       Description = "вывод пользовательских данных"
       Arguments = []}
     { Name = "/delete"
       Description = "удаление пользовательских данных"
       Arguments = []}]

let _greeter = new Greeter(_cmds)

let private onUpdate (settings: Settings) (context: UpdateContext) = 
    let sendIfUnrecognizedCommands result =
        if result then ()
        else 
            "Команда не распознана"
            |> Funogram.Api.sendMessage context.Update.Message.Value.Chat.Id   
            |> Api.api context.Config
            |> Async.RunSynchronously
            |> Tools.logResponse settings
            |> ignore

    processCommands context [
        cmd "/start" (_greeter.onStart settings)
        cmd "/read" (Registrator.onRead settings)
        cmd "/delete" (Registrator.onDelete settings)
        cmdScan "/registrate ФИО=%s %s %s, др=%s" (Registrator.onRegistrate settings context)
    ] |> sendIfUnrecognizedCommands
    

[<EntryPoint>]
let main _ = 
    maybe {
        let! config = Tools.deserialize<Config> "config.json"
        let settings = { Logger = { Log = Console.WriteLine }; Config = config }

        _greeter.printAvailableCommands

        Migrator.run
        settings.Logger.Log "Запустили бота."

        let! proxy =
            config
            |> (fun x -> if Array.isEmpty x.Proxy then None else Some x)
            |> Option.bind (fun x -> x.createProxy() |> Some) 
            |> Option.defaultValue defaultConfig.Client |> Some

        let startBotConfig = {
            defaultConfig with 
                Token = settings.Config.TgToken
                Client = proxy
        }
        startBot startBotConfig (onUpdate settings) None
        |> Async.RunSynchronously
        |> ignore
    } |> ignore
    0