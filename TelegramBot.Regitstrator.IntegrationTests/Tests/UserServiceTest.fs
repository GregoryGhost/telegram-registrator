namespace TelegramBot.Registrator.IntegrationTests

module UserServiceTest =
    open Expecto
    open TelegramBot.Registrator.Db.Services
    open TelegramBot.Registrator.Db.Models
    open System

    let tests idTelegramUser =
        // TODO: вынести повторяющиеся шаги в функции вспомогательного модуля
        //  и сделать в них проверку на выполнение шага
        let newUser = {
            SurName = "Yagami"
            Name = "Lite"
            Patronymic = "Soitiro"
            DateOfBirth = new DateTime(1986, 2, 28)
        }
        testList "Проверка CRUD UserService" [
            test "Добавление пользователя" {
                // TODO: сделать сервис IDisposible
                let us = new UserService()
                us.Registrate(idTelegramUser, newUser) |> ignore
                let gotUser =
                    us.GetUserById(idTelegramUser)
                    |> Option.get
                us.Remove(idTelegramUser)
                Expect.equal gotUser newUser "Получить вновь созданного пользователя для idTelegramUser"
            }
            test "Добавление дублирующегося пользователя" {
                // TODO: сделать сервис IDisposible
                let us = new UserService()
                let successRegistration = us.Registrate(idTelegramUser, newUser)
                Expect.isTrue successRegistration "Пользователь не зарегистрирован"

                let failRegisration = us.Registrate(idTelegramUser, newUser)
                us.Remove(idTelegramUser)
                Expect.isFalse failRegisration "Отказ в регистрации дублирующегося пользователя"
            }
            test "Удаление пользователя" {
                // TODO: сделать сервис IDisposible
                let us = new UserService()
                us.Registrate(idTelegramUser, newUser) |> ignore
                us.Remove(idTelegramUser)
                let gotUser = 
                    us.GetUserById(idTelegramUser)
                Expect.isNone gotUser "Пользователь удален"
            }
            test "Создание пользователя после его удаления" {
                // TODO: сделать сервис IDisposible
                let us = new UserService()
                us.Registrate(idTelegramUser, newUser) |> ignore
                us.Remove(idTelegramUser)
                us.Registrate(idTelegramUser, newUser) |> ignore
                let gotUser =
                    us.GetUserById(idTelegramUser)
                    |> Option.get
                us.Remove(idTelegramUser)
                Expect.equal gotUser newUser "Пользователь создан"
            }
        ]