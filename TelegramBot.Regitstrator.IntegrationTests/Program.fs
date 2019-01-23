open Expecto
open TelegramBot.Registrator.Db.Services
open TelegramBot.Registrator.Db.Models
open System
open TelegramBot.Registrator.Db

type Test = { Name: string }

let tests =
    let idTelegramUser = 666
    testList "simples" [
        test "A simple test" {
            let subject = { Name = "test" }
            Expect.equal subject { Name = "test" } "The strings should equal"
        }
        test "test" {
            let us = new UserService()
            let newUser = { 
                Name = "Kira"
                SurName = "Yagami"
                Patronymic = "Soitiro"
                DateOfBirth = new DateTime(1986, 3, 28)
            }
            us.Registrate(idTelegramUser, newUser) |> ignore
            let gotUser =
                us.GetUserById(idTelegramUser)
                |> Option.get
            us.Remove(idTelegramUser)
            Expect.equal gotUser newUser "Получить вновь созданного пользователя для idTelegramUser"
        }
    ]

[<EntryPoint>]
let main args =
    // TODO:
    // 1. В сборке Db должен быть хелпер по созданию тестовой БД 
        // - просто запрос на SQL по созданию тестовой БД
    // 1.1 Должна быть функцию по перенацеливанию подключения к БД на тестовую БД
    // 1.2 Дропать БД после тестов
        // - просто запрос на SQL по удалению тестовой БД
    // 1.3 Создавать БД перед тестами
    // 1.4 В каждом тесте по работе с записями удалять их в БД перед выходом из теста
    // 1.5 Запуск мигратора после создания тестовой БД
    runTestsWithArgs defaultConfig args tests
