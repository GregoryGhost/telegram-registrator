namespace TelegramBot.Registrator.IntegrationTests

module ProxyCheckerTest =
    open Expecto

    let INPUT_PROXY_FILE_NAME = "proxyList.txt"
    let OUTPUT_PROXY_FILE_NAME = "output.txt"

    let checkRunTestBot (proxy: string): bool =
        true

    let readFile (fileName: string): string list = 
        ["proxy"]

    let writeToFile (fileName: string) (data: string list): string list = 
        ["proxy"]

    let tests idTelegramUser = 
        test "Проверка работоспособности проксей из файла" {
            let result = 
                INPUT_PROXY_FILE_NAME
                |> readFile 
                |> List.filter checkRunTestBot 
                |> writeToFile INPUT_PROXY_FILE_NAME
            Expect.isNonEmpty result "Не найдены работоспособные прокси"
        }