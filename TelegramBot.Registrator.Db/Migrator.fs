namespace TelegramBot.Registrator.Db

open Rezoom.SQL
open Rezoom.SQL.Migrations
open System.Configuration


// Миграции из корневой папки проекта (".")
type private Migrations= SQLModel<".">


/// Работает с миграциями БД.
module Migrator =
    /// Накатить миграции в БД.
    let run =
        let appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)

        let connStrings = appConfig.ConnectionStrings.ConnectionStrings
        for connString in connStrings do
            printfn "%s >>><<< %s" connString.ConnectionString connString.Name

        let config =
            { MigrationConfig.Default with
                LogMigrationRan = fun m -> printfn "Ran migration: %s" m.MigrationName
            }
        // запустить миграции, создавая при этом БД,
        //  если она отсутствует
        Migrations.Migrate(config)
