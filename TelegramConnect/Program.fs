module CsharpTelegramBotClient =
    open Telegram.Bot
    open MihaZupan
    open System
    open Telegram.Bot.Args
    open Telegram.Bot.Types.Enums
    open System.Linq
    open Telegram.Bot.Types

    let token = "token"

    //let proxy = new HttpToSocks5Proxy(" 91.144.167.217", 1080)

    let bot = new TelegramBotClient(token)

    let onMessageReceived (args: MessageEventArgs) =
        let msg = args.Message
        if (msg = null || msg.Type <> MessageType.Text) then ()
        else 
            match msg.Text.Split(' ').First() with
            | "/echo" -> 
                let echoMsg = 
                    let m = msg.Text.Split(' ')
                    if Array.isEmpty m then None else m |> Array.skip 1 |> Some
                let sendingMsg = 
                    echoMsg
                    |> Option.defaultValue [| "kek" |]
                    |> Array.reduce (+)
                bot.SendTextMessageAsync(msg.Chat.Id |> ChatId, sendingMsg) |> Async.AwaitTask |> ignore
            | _ -> 
                let usage = @"
                    /echo - echo command"
                bot.SendTextMessageAsync(msg.Chat.Id |> ChatId, usage) |> Async.AwaitTask |> ignore

    let main() =
        let me = bot.GetMeAsync().Result
        Console.Title <- me.Username

        bot.OnMessage.Add onMessageReceived
        bot.OnMessageEdited.Add onMessageReceived
        bot.OnReceiveError.Add (fun x -> printfn "Error %d %s" x.ApiRequestException.ErrorCode x.ApiRequestException.Message)

        bot.StartReceiving()
        printfn "start listening for %s" me.Username
        Console.ReadLine() |> ignore
        bot.StopReceiving()
        0

module Types =
    type Logger = 
        { Log: string -> unit }

    type Config = 
        { TgToken: string }    
  
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

module Start = 
    open ExtCore.Control
    open Funogram.Bot
    open Funogram.Api
    open Types
    open Tools

    let onStart settings context =
        maybe {
            settings.Logger.Log "Начал обработку команд."
            let! message = context.Update.Message
            let! name = message.Chat.FirstName
            sprintf "Привет, %s! Используй /echo! 😉" name
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

    let onTest settings context (x, y, z) =
        maybe {
            settings.Logger.Log "Принял /test."
            settings.Logger.Log (sprintf "Принял x=%s y=%s z=%s" x y z)
            let! message = context.Update.Message

            sprintf "x=%s y=%s z=%s" x y z
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

let processResultWithValue (result: Result<'a, ApiResponseError>): 'a option =
    match result with
    | Ok v -> Some v
    | _ -> None

let processResult (result: Result<'a, ApiResponseError>) =
    processResultWithValue result |> ignore

let botResult data config = api config data |> Async.RunSynchronously
let bot data config = botResult data config |> processResult

let private onUpdate settings (context: UpdateContext) =
    let config = context.Config
    let fromId() = context.Update.Message.Value.From.Value.Id
    let sayWithArgs text parseMode disableWebPagePreview disableNotification replyToMessageId replyMarkup =
            bot (sendMessageBase (ChatId.Int (fromId())) text parseMode disableWebPagePreview disableNotification replyToMessageId replyMarkup)
    //note: команды с параметрами идут сначала менее общие, затем более общие,
    // то есть с наибольшим количеством параметров идут сначала, если параметры совпадают,
    //  затем с наименьшим количеством параметров.
    processCommands context [
        cmd "/start" (Start.onStart settings)
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
        cmdScan "/test z=%s y=%s x=%s" (Echo.onTest settings context)
        cmdScan "/echo2 %s" (Echo.onEcho settings context)
    ] |> ignore
    
let private deserialize<'a> (file: string) = 
    let read = File.ReadAllText >> JsonConvert.DeserializeObject<'a>
    if File.Exists file then read file |> Some else None



[<EntryPoint>]
let main _ = 
    //deserialize<Config> "config.json"
    Some { TgToken = "token"}
    |> Option.map (fun config ->
        let settings = { Logger = { Log = Console.WriteLine }; Config = config }
        settings.Logger.Log "Запустили бота."
        startBot { 
            defaultConfig with 
                Token = settings.Config.TgToken 
        } (onUpdate settings) None
        |> Async.RunSynchronously )
    |> ignore
    0
    //CsharpTelegramBotClient.main()