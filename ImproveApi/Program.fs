open System
open System.IO
open ImproveApi
open Suave
open Suave.RequestErrors
open Suave.Filters
open Suave.Operators
open FSharp.Data
open Suave.Logging
open MongoInt.MongoEntityFactory
open Token

type Config = JsonProvider<""" { "port":1, "ip":"127.0.0.1", "verbose":true, "conn":"string", "db":"string" } """>
let config = Config.Parse """ { "port":3010, "ip":"127.0.0.1", "verbose": true, "conn": "mongodb://localhost:27017", "db": "improve" } """

let getConfigFile configFile =
    match File.Exists configFile with
    | false -> config
    | true -> File.ReadAllText configFile |> Config.Parse

let getSuaveCfg (configFile: Config.Root) =
    { defaultConfig with
        bindings = [ HttpBinding.createSimple HTTP configFile.Ip configFile.Port ] }

let getMongoCfg (configFile: Config.Root) =
    { defaultMongoConfig with
        ConnectionString = configFile.Conn
        DbName = configFile.Db }

let logLevel (config: Config.Root) =
    match config.Verbose with
    | true -> Verbose
    | _ -> Error

let logger file = Targets.create (logLevel file) [||]

let app configFile factory =
    choose [
        pathStarts "/ok" >=> choose [
            GET >=> (Successful.OK """ { "ok": "OK2" } """)
        ]
        choose [

        ] >=> Writers.setMimeType "application/json; charset=utf-8"
        NOT_FOUND "NOT FOUND"
    ] >=> logStructured (logger configFile) logFormatStructured

[<EntryPoint>]
let main argv =

    let token = buildToken "asd"
    printfn "built %s" token

    let id = validateAndGetUserId token
    printfn "decoded %s" id




//    let confFile = getConfigFile "./config.json"
//    let factory = Factory (getMongoCfg confFile)
//    printfn "%A" confFile
//    startWebServer (getSuaveCfg confFile) (app confFile factory)
    0
