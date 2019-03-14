open TelegramBot.Registrator.Db
open TelegramBot.Registrator.Db.Helpers
open System.Configuration
open TelegramBot.Registrator.IntegrationTests
open System.Data.SqlClient

module AppConfig = 
    let [<Literal>] connName = "rzsql"

[<EntryPoint>]
let main args =
    Migrator.run
    
    let result = TestList.runTestList args

    let connString = ConfigurationManager.ConnectionStrings.[AppConfig.connName].ConnectionString
    let dbName = 
        let builder = new SqlConnectionStringBuilder()
        builder.ConnectionString <- connString
        builder.["Initial Catalog"] :?> string |> sprintf "[%s]"

    DbHelper.dropDb(connString, dbName)

    result
