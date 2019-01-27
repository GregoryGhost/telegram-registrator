open TelegramBot.Registrator.Db
open System.Data
open System.Data.SqlClient
open System.Configuration
open TelegramBot.Registrator.IntegrationTests

type Test = { Name: string }

module AppConfig = 
    let [<Literal>] connName = "rzsql"


[<EntryPoint>]
let main args =
    // TODO:
    // 1.4 В каждом тесте по работе с записями удалять их в БД перед выходом из теста
    // 1.5 Вынести удаление БД в функцию и вызывать перед тестированием.
    Migrator.run

    let result = TestList.runTestList args

    let connString = ConfigurationManager.ConnectionStrings.[AppConfig.connName].ConnectionString
    use db = new SqlConnection(connString)
    db.Database |> printfn "Current db = %s"
    db.Open()
    let cmd = db.CreateCommand()
    cmd.CommandText <- "USE master;"
        + "ALTER DATABASE [telegram-test] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
        + "drop database [telegram-test]"
    cmd.CommandType <- CommandType.Text
    cmd.ExecuteNonQuery() |> ignore
    db.Close()

    result
