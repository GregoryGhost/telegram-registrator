module Types
    open System.Net
    open System.Net.Http
    
    type Logger = 
        { Log: string -> unit }

    type Proxy =
        { Host: string
          Port: int }
        with 
            member __.createProxy() =
                let proxy = new WebProxy(__.Host, __.Port)
                let handler = new HttpClientHandler()
                handler.Proxy <- proxy
                handler.UseProxy <- true
                let proxyClient = new HttpClient(handler, true)
                proxyClient

    type Config = 
        { TgToken: string
          Proxy: Proxy option }
  
    type Settings = 
        { Config: Config
          Logger: Logger }

    type Commands = 
        { Name: string
          Description: string
          Arguments: string list }