module MongoInt.IO

open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Driver

let private _fst = function | (a, _, _) -> a
let private _snd = function | (_, b, _) -> b
let private _thr = function | (_, _, c) -> c

let mutable private clients = []

let private requestClient (conn: string) db =
    match List.tryFind (fun it -> _fst it = conn && _snd it = db) clients with
    | None ->
        let ret = MongoClient(conn).GetDatabase(db)
        clients <- (conn, db, ret)::clients
        ret
    | Some record -> _thr record

let internal getClient<'a> config collectionName =
    let db = requestClient config.ConnectionString config.DbName
    db.GetCollection<'a>(collectionName)

let internal serialize<'a>(data : 'a) =
    let doc = BsonDocument()
    BsonSerializer.Serialize<'a>(new IO.BsonDocumentWriter(doc), data)
    doc

let internal deserialize<'a>(doc : BsonDocument) =
    BsonSerializer.Deserialize<'a>(doc)