open System.Configuration
open Funogram.Bot

//module CsharpTelegramBotClient =
//    open Telegram.Bot
//    open MihaZupan
//    open System
//    open Telegram.Bot.Args
//    open Telegram.Bot.Types.Enums
//    open System.Linq
//    open Telegram.Bot.Types

//    let token = "token"

//    let proxy = new HttpToSocks5Proxy(" 91.144.167.217", 1080)

//    let private bot = new TelegramBotClient(token)

//    let onMessageReceived (args: MessageEventArgs) =
//        let msg = args.Message
//        if (msg = null || msg.Type <> MessageType.Text) then ()
//        else 
//            match msg.Text.Split(' ').First() with
//            | "/echo" -> 
//                let echoMsg = 
//                    let m = msg.Text.Split(' ')
//                    if Array.isEmpty m then None else m |> Array.skip 1 |> Some
//                let sendingMsg = 
//                    echoMsg
//                    |> Option.defaultValue [| "kek" |]
//                    |> Array.reduce (+)
//                bot.SendTextMessageAsync(msg.Chat.Id |> ChatId, sendingMsg) |> Async.AwaitTask |> ignore
//            | _ -> 
//                let usage = @"
//                    /echo - echo command"
//                bot.SendTextMessageAsync(msg.Chat.Id |> ChatId, usage) |> Async.AwaitTask |> ignore

//    let main() =
//        let me = bot.GetMeAsync().Result
//        Console.Title <- me.Username

//        bot.OnMessage.Add onMessageReceived
//        bot.OnMessageEdited.Add onMessageReceived
//        bot.OnReceiveError.Add (fun x -> printfn "Error %d %s" x.ApiRequestException.ErrorCode x.ApiRequestException.Message)

//        bot.StartReceiving()
//        printfn "start listening for %s" me.Username
//        Console.ReadLine() |> ignore
//        bot.StopReceiving()
//        0

module Types =
    open System.Net
    open System.Net.Http
    
    type Logger = 
        { Log: string -> unit }

    type Proxy =
        { Host: string
          Port: int }
        with 
            member __.createProxy() =
                let proxy = new WebProxy(__.Host, __.Port)
                let handler = new HttpClientHandler()
                handler.Proxy <- proxy
                handler.UseProxy <- true
                let proxyClient = new HttpClient(handler, true)
                proxyClient

    type Config = 
        { TgToken: string
          Proxy: Proxy option }
  
    type Settings = 
        { Config: Config
          Logger: Logger }

module Tools =
    open Types
    open Funogram.Types

    let logResponse<'a> (settings: Settings) (response: Result<'a, ApiResponseError>) =
        match response with
        | Result.Ok _ -> "Successfully received response."
        | Result.Error error -> error.Description
        |> settings.Logger.Log 

open ExtCore.Control
open Funogram.Api
open Types
open Tools

type Commands = 
    { Name: string
      Description: string
      Arguments: string list }

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

    member __.onTestStart =
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

module Start = 
    open ExtCore.Control
    open Funogram.Bot
    open Funogram.Api
    open Types
    open Tools

    let onStart (settings: Settings) (context: UpdateContext)  =
        maybe {
            settings.Logger.Log "Начал обработку команд."
            let! message = context.Update.Message
            let! name = message.Chat.FirstName

            sprintf "Добро пожаловать, %s! Доступны следующие команды:" name
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore

module Echo =
    open ExtCore.Control
    open Funogram.Bot
    open Funogram.Api
    open Types
    open Tools

    let onEcho settings context x =
        maybe {
            settings.Logger.Log "Принял /echo."
            settings.Logger.Log (sprintf "Принял x=%s" x)
            let! message = context.Update.Message
            let! text = message.Text

            x
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore

    open System.Globalization
    open System

    let onTest settings context (x, y, z, d) =
        maybe {
            settings.Logger.Log "Принял /test."
            settings.Logger.Log (sprintf "Принял x=%s y=%s z=%s d=%s" x y z d)
            let datePattern = "dd-MM-yyyy"
            let! date = 
                let mutable dateBirth: DateTime = DateTime.UtcNow
                if DateTime.TryParseExact(d, datePattern, null, DateTimeStyles.None, &dateBirth) then
                    Some dateBirth
                else None
            settings.Logger.Log (sprintf "Принял x=%s y=%s z=%s d=%s" x y z (date.ToString(datePattern)))
            let! message = context.Update.Message

            sprintf "x=%s y=%s z=%s d=%s" x y z (date.ToString(datePattern))
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore
 
open Funogram.Bot
open Funogram.Types
open Types
open Newtonsoft.Json
open System.IO
open System
open Funogram.Api
open ExtCore.Control
open System.Net.Http
open System.Net
open FSharp.Json

let processResultWithValue (result: Result<'a, ApiResponseError>): 'a option =
    match result with
    | Ok v -> Some v
    | Result.Error e -> 
        printfn "Error: %s" e.Description
        None

let processResult (result: Result<'a, ApiResponseError>) =
    processResultWithValue result |> ignore

let botResult data config = api config data |> Async.RunSynchronously
let bot data config = botResult data config |> processResult

   
let _cmds = 
    [{ Name = "/start"
       Description = "начальная команда"
       Arguments = ["kek"]}
     { Name = "/help"
       Description = "тоже самое что /start"
       Arguments = ["kek"]}
     { Name = "/empty_args"
       Description = "пример пустой команды"
       Arguments = []}]

let _greeter = new Greeter(_cmds)
_greeter.onTestStart

let private onUpdate settings (context: UpdateContext) =
    let config = context.Config
    let fromId() = context.Update.Message.Value.From.Value.Id
    let sayWithArgs text parseMode disableWebPagePreview disableNotification replyToMessageId replyMarkup =
            bot (sendMessageBase (ChatId.Int (fromId())) text parseMode disableWebPagePreview disableNotification replyToMessageId replyMarkup)
    
    //note: команды с параметрами идут сначала менее общие, затем более общие,
    // то есть с наибольшим количеством параметров идут сначала, если параметры совпадают,
    //  затем с наименьшим количеством параметров.
    processCommands context [
        cmd "/start" (_greeter.onStart settings)
        cmd "/help" (Start.onStart settings)
        cmd "/say" (fun _ -> sayWithArgs "That's message with reply!" None None None (Some context.Update.Message.Value.MessageId) None config)
        cmd "/send_message5" (fun _ ->
        (
            let keyboard = (Seq.init 2 (fun x -> Seq.init 2 (fun y -> { Text = y.ToString() + x.ToString(); RequestContact = None; RequestLocation = None })))
            let markup = Markup.ReplyKeyboardMarkup {
                Keyboard = keyboard
                ResizeKeyboard = None
                OneTimeKeyboard = None
                Selective = None
            }
            bot (sendMessageMarkup (fromId()) "That's keyboard!" markup) config
        ))
        cmd "/send_action" (fun x -> bot (sendMessage context.Update.Message.Value.Chat.Id x.Update.Message.Value.Text.Value) config)
        cmdScan "/test ФИО=%s %s %s, др=%s" (Echo.onTest settings context)
        cmdScan "/echo2 %s" (Echo.onEcho settings context)
    ] |> ignore
    
let private deserialize<'a> (file: string) = 
    let read = File.ReadAllText >> Json.deserialize<'a>
    if File.Exists file then read file |> Some else None



[<EntryPoint>]
let main _ = 
    maybe {
        let! config = deserialize<Config> "config.json"
        let settings = { Logger = { Log = Console.WriteLine }; Config = config }

        settings.Logger.Log "Запустили бота."

        let! proxy =
            config.Proxy 
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
    //CsharpTelegramBotClient.main()