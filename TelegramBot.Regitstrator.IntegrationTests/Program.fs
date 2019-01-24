open Expecto
open TelegramBot.Registrator.Db.Services
open TelegramBot.Registrator.Db.Models
open System
open TelegramBot.Registrator.Db
open System.Data
open System.Data.SqlClient
open System.Configuration

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

module AppConfig = 
    let [<Literal>] connName = "rzsql"


[<EntryPoint>]
let main args =
    // TODO:
    // 1.4 В каждом тесте по работе с записями удалять их в БД перед выходом из теста
    Migrator.run
    runTestsWithArgs defaultConfig args tests
    let connString = ConfigurationManager.ConnectionStrings.[AppConfig.connName].ConnectionString
    use db = new SqlConnection(connString)
    db.Open()
    let cmd = db.CreateCommand()
    cmd.CommandText <- "USE master;"
        + "ALTER DATABASE [telegram-test] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
        + "drop database [telegram-test]"
    cmd.CommandType <- CommandType.Text
    cmd.ExecuteNonQuery()
    db.Close()
    0
