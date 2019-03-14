namespace TelegramBot.Registrator.IntegrationTests

module private ProxyConverter = 
    open System
    open System.IO
    open Types

    let run: bool =
        let sourceProxy =
            File.ReadAllLines "source_proxy.txt"
        let parseProxy (proxy: string): Proxy =
            let parsed = proxy.Split(':')
            let result = 
                { Host = parsed.[0]
                  Port = parsed.[1] |> Int32.Parse }
            result
        let parsedProxy = sourceProxy |> Array.map parseProxy
        let config =
            { TgToken = "token" 
              Proxy = parsedProxy }
        Tools.serialize "output_config.json" config
        true

module ProxyConverterTest =
    open Expecto

    let tests =
        test "Конвертирование списка прокси в конфиг для бота" {
            Expect.isTrue ProxyConverter.run "Не получилось выдать конфиг для бота"
        }