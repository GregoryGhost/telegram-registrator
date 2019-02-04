namespace TelegramBot.Registrator.Db.Helpers

module DbHelper =
    open System.Data.SqlClient
    open System.Data

    let dropDb(connString: string, dbName: string) =
        use db = new SqlConnection(connString)
        db.Database |> printfn "Current db = %s"
        db.Open()
        let cmd = db.CreateCommand()
        cmd.CommandText <- "USE master;"
            + "ALTER DATABASE " + dbName + " SET SINGLE_USER WITH ROLLBACK IMMEDIATE;"
            + "drop database " + dbName
        cmd.CommandType <- CommandType.Text
        cmd.ExecuteNonQuery() |> ignore
        db.Close()