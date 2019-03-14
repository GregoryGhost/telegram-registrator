open ExtCore.Control
open Funogram.Bot
open System

open TelegramBot.Registrator.Db
open TelegramBot.Registrator.Front.Commands
open Types
open Funogram
open System.Net
open System.Web
open System.Net.Http
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
        if not result then ()
        else 
            "Команда не распознана"
            |> Funogram.Api.sendMessage context.Update.Message.Value.Chat.Id   
            |> Api.api context.Config
            |> Async.RunSynchronously
            |> Tools.logResponse settings
            |> ignore
    
    let foundCmd = processCommands context [
        cmd "/start" (_greeter.onStart settings)
        cmd "/read" (Registrator.onRead settings)
        cmd "/delete" (Registrator.onDelete settings)
        cmdScan "/registrate ФИО=%s %s %s, др=%s" (Registrator.onRegistrate settings context)
    ]
    foundCmd |> sendIfUnrecognizedCommands

let rec runTelegramBot (settings: Settings) (numberProxy: int) 
    (newConnection: bool) = 
    maybe {
        let! proxy =
            settings.Config
            |> (fun x -> if Array.isEmpty x.Proxy then None else Some x)
            |> Option.bind (fun x -> 
                let proxyClient = x.createProxy() |> Seq.tryItem numberProxy

                //прокси не задали в конфиге и не сбрасывали прокси
                if (Option.isNone proxyClient) && not newConnection then
                    defaultConfig.Client |> Some
                //есть прокси в конфиге для сброса прокси
                elif (Option.isSome proxyClient) then
                    let (proxy, client) = proxyClient |> Option.get
                    settings.Logger.Log <| sprintf "Proxy config:%A" proxy
                    client |> Some
                //исчерпали все прокси из конфига
                else
                    None)

        settings.Logger.Log "Получил прокси"

        let startBotConfig = {
            defaultConfig with 
                Token = settings.Config.TgToken
                Client = proxy
        }

        settings.Logger.Log "Задал конфиг для бота"

        let restartBot ex newConnect =
            proxy.CancelPendingRequests()
            proxy.Dispose()
            settings.Logger.Log <| sprintf "%A" ex
            let numberResetBot = numberProxy + 1
            settings.Logger.Log <| sprintf "Перезапуск номер:%d" numberResetBot
            runTelegramBot settings numberResetBot newConnect |> ignore

        let checkConnectionToTelegram() = 
            Api.getMe |> Api.api startBotConfig |> Async.RunSynchronously |> ignore
            settings.Logger.Log "Подключился к телеге"
        try
            checkConnectionToTelegram()
            startBot startBotConfig (onUpdate settings) None
            |> Async.RunSynchronously
            |> ignore
        with
        | :? AggregateException as ex 
            when (ex.InnerException :? HttpException 
            || ex.InnerException :? WebException 
            || ex.InnerException :? HttpRequestException ) ->
                let newConnect = true
                restartBot ex newConnect
        //после разрыва соединения по тайм ауту у текущего TCP соединения
        // или отвалилось предыдущее TCP соединение по тайм ауту
        | :? OperationCanceledException as ex ->
            let newConnect = false
            restartBot ex newConnect
    }

[<EntryPoint>]
let main _ = 
    maybe {
        let! config = Tools.deserialize<Config> "config.json"
        let settings = { Logger = { Log = Console.WriteLine }; Config = config }

        _greeter.printAvailableCommands

        Migrator.run
        settings.Logger.Log "Запустили бота."
        let startCount = 0
        let newConnecton = false
        runTelegramBot settings startCount newConnecton |> ignore
    } |> ignore
    0