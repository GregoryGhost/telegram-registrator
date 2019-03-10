namespace TelegramBot.Registrator.IntegrationTests

open Expecto

/// Модуль тестов.
module TestList = 
    /// Запустить тестовый список.
    ///     args - Аргументы командной строки.
    let runTestList(args: string[]) = 
        testList "Запуск всех тестов:" [
            //DataTests.idTelegramUser |> UserServiceTest.tests  |> testSequencedGroup "последовательная запись в БД"
            ProxyConverterTest.tests
        ]
        |> (runTestsWithArgs defaultConfig args)

