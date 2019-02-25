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

            let resultRegistration =
                let registration = _userService.GetUserById(user.Id)
                if registration.IsSome then
                    sprintf "%s" <| User.toString registration.Value
                else
                    sprintf "Пользовательские данные не найдены, зарегистрируетесь."

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
                let isRemoved = _userService.Remove(user.Id)
                sprintf "Удаление данных: %A" isRemoved

            resultRegistration
            |> sendMessage message.Chat.Id
            |> api context.Config
            |> Async.RunSynchronously
            |> logResponse settings
        } |> ignore