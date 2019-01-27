namespace TelegramBot.Registrator.IntegrationTests

open Expecto

/// Модуль тестов.
module TestList = 
    /// Запустить тестовый список.
    ///     args - Аргументы командной строки.
    let runTestList(args: string[]) = 
        testList "Запуск всех тестов:" [
            DataTests.idTelegramUser |> UserServiceTest.tests 
        ]
        |> (runTestsWithArgs defaultConfig args)

