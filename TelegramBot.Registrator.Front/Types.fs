module Types
    open System.Net
    open System.Net.Http
    open MihaZupan
    
    type Logger = 
        { Log: string -> unit }

    type Proxy =
        { Host: string
          Port: int
          User: string option
          Password: string option }

    type Config = 
        { TgToken: string
          Proxy: Proxy [] }
        with 
            member __.createProxy(): seq<Proxy * HttpClient> =
                seq {
                    for p in __.Proxy ->
                        let proxy = 
                            if p.User.IsSome && p.Password.IsSome then
                                new HttpToSocks5Proxy(p.Host, p.Port, p.User.Value, p.Password.Value)
                            else
                                new HttpToSocks5Proxy(p.Host, p.Port)
                        let handler = new HttpClientHandler()
                        handler.Proxy <- proxy
                        handler.UseProxy <- true
                        let proxyClient = new HttpClient(handler, true)
                        (p, proxyClient)
                }
  
    type Settings = 
        { Config: Config
          Logger: Logger }

    type Commands = 
        { Name: string
          Description: string
          Arguments: string list }