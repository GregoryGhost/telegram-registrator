module Types
    open System.Net
    open System.Net.Http
    open MihaZupan
    
    type Logger = 
        { Log: string -> unit }

    type Proxy =
        { Host: string
          Port: int }

    type Config = 
        { TgToken: string
          Proxy: Proxy [] }
        with 
            member __.createProxy(): HttpClient =
                let proxies = 
                    __.Proxy
                    |> Array.map (fun proxy -> new ProxyInfo(proxy.Host, proxy.Port))
                let proxy = new HttpToSocks5Proxy(proxies)
                let handler = new HttpClientHandler()
                handler.Proxy <- proxy
                handler.UseProxy <- true
                let proxyClient = new HttpClient(handler, true)
                proxyClient
  
    type Settings = 
        { Config: Config
          Logger: Logger }

    type Commands = 
        { Name: string
          Description: string
          Arguments: string list }