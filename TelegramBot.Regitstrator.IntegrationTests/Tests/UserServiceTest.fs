namespace TelegramBot.Registrator.IntegrationTests

module UserServiceTest =
    open Expecto
    open TelegramBot.Registrator.Db.Services
    open TelegramBot.Registrator.Db.Models
    open System

    let tests idTelegramUser =
        let newUser = {
            SurName = "Yagami"
            Name = "Lite"
            Patronymic = "Soitiro"
            DateOfBirth = new DateTime(1986, 2, 28)
        }
        testList "Проверка CRUD UserService" [
            test "Добавление пользователя" {
                let us = new UserService()
                us.Registrate(idTelegramUser, newUser) |> ignore
                let gotUser =
                    us.GetUserById(idTelegramUser)
                    |> Option.get
                us.Remove(idTelegramUser)
                Expect.equal gotUser newUser "Получить вновь созданного пользователя для idTelegramUser"
            }
        ]