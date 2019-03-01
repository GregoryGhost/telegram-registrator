namespace TelegramBot.Registrator.Front.Commands

module Registrator =
    open ExtCore.Control
    open Funogram.Bot
    open Funogram.Api
    open System.Globalization
    open System

    open TelegramBot.Registrator.Db.Services
    open TelegramBot.Registrator.Db.Helpers

    open Types
    open Tools

    let private _userService = new UserService()

    let USER_DATA_NOT_FOUND = "пользовательские данные не найдены, зарегистрируйтесь."

    let private checkRegistration(userId: int64): Result<int64, string> =
        let registration = _userService.GetUserById(userId)
        if registration.IsNone then
            Result.Error USER_DATA_NOT_FOUND
        else Ok userId
    
    let formatResult (msg: Printf.StringFormat<(string -> string)>) 
        (v: Result<string, string>): string =
            match v with
            | Ok o | Result.Error o -> sprintf msg o

    let onRegistrate (settings: Settings) (context: UpdateContext) 
        (surname: string, name: string, patronymic: string, birthDate: string) =
        maybe {
            settings.Logger.Log "Принял /registrate."
            settings.Logger.Log (sprintf "Принял Фамилия=%s Имя=%s Отчество=%s Дата_рождения=%s" 
                <| surname
                <| name
                <| patronymic
                <| birthDate)
            let! date = 
                let mutable dateBirth: DateTime = DateTime.UtcNow
                if DateTime.TryParseExact(birthDate, User.datePattern, null, DateTimeStyles.None, &dateBirth) then
                    Some dateBirth
                else None
            
            let! message = context.Update.Message
            let! user = message.From
            let userData = User.ofSourceData surname name patronymic date
            let resultRegistration = 
                let isRegistrated  = _userService.Registrate(user.Id, userData)
                if isRegistrated then
                    "Регистрация успешно завершена."
                else
                    "Регистрация не удалась"

            resultRegistration
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore
    
    let onRead (settings: Settings) (context: UpdateContext) =
        maybe {
            settings.Logger.Log "Принял /read."
            let! message = context.Update.Message
            let! user = message.From

            let getUser userId =
                let userById = _userService.GetUserById userId
                if userById.IsSome then
                    userById.Value |> User.toString
                else USER_DATA_NOT_FOUND

            let resultRegistration =
                checkRegistration user.Id
                |> Result.map getUser
                |> Result.mapError (fun msg -> msg)
                |> formatResult "%s"

            resultRegistration
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore

    let onDelete (settings: Settings) (context: UpdateContext) =         
        maybe {
            settings.Logger.Log "Принял /delete."
            let! message = context.Update.Message
            let! user = message.From

            let resultRegistration =
                let removeUser (userId: int64): string = 
                    _userService.Remove(userId)
                    let missedUser = _userService.GetUserById(userId)
                    if missedUser.IsSome then "неудача"
                    else "удалены"
                checkRegistration user.Id
                |> Result.map removeUser
                |> Result.mapError (fun msg -> msg)
                |> formatResult "Удаление данных: %s"

            resultRegistration
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore