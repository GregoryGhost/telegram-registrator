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
    us.Registrate(idTelegramUser, newUser) |> ignore
    us.PrintAllUsers
    0 // return an integer exit code
