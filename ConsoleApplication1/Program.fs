open TelegramBot.Registrator.Db
open TelegramBot.Registrator.Db.Models
open TelegramBot.Registrator.Db.Services
open System

[<EntryPoint>]
let main argv = 
    Migrator.run
    let us = new UserService()
    let newUser = {
        SurName = "Yugami1234";
        Name = "Lite";
        Patronymic = "Kira";
        DateOfBirth = DateTime.Now    }
    let idTelegramUser = 11
    if us.Registrate(idTelegramUser, newUser) then
        printfn "Пользователь с id=%d успешно зарегистрирован." idTelegramUser
    else
        printfn "Пользователь с id=%d уже зарегистрирован, удалите его регистрационные данные." idTelegramUser
        let userData =
            us.GetUserById idTelegramUser
        printfn "Регистрационные данные %A пользователя с id=%d." userData idTelegramUser
    printfn "-------------------------------"
    us.PrintAllUsers
    0 // return an integer exit code
