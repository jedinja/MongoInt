// Learn more about F# at http://fsharp.org

open MongoDB.Bson.Serialization.Attributes
open MongoInt
open MongoInt.MongoEntityFactory
open MongoInt.Abstracts

[<BsonIgnoreExtraElements>]
type Me = {
    kuku: string
    [<BsonDefaultValue(null)>]
    muku: string
}

type MeFilterOnKuku = {
    kuku: string
}

type union = Uniona of int
[<BsonIgnoreExtraElements>]
type UnionObj = {
    field: union
}

type StringLog = {
    Error: string
}

[<EntryPoint>]
let main argv =

    RegisterFSharpTypes()

    let fac = Factory { defaultConfig with DbName = "play" }
    let me = fac.CreateEntity<Me> "me"

    "5f89a0e7c38d9a2d8a335adf"
    |> me.get
    |> printfn "%A"

    { kuku = "ruku" + System.DateTime.Now.Millisecond.ToString(); muku = "muku" }
    |> me.update "5f89a0e7c38d9a2d8a335adf"
    |> printfn "%A"

    let created = me.create { kuku = "test"; muku = "test" }
    printfn "%A" created

    me.delete created.Id

    me.list (1, 10)
    |> printfn "%A"

    me.all { kuku = "test" }
    |> printfn "%A"

    let uni = fac.CreateEntity<UnionObj> "union"
    uni.create { field = Uniona 5 }
    |> printfn "Union %A"

    let meQueue = fac.CreateEntity<MongoQueue> "meQueue"
    let dataFromMeQueue = fac.CreateEntity<MongoQueueData<Me>> "meQueue"

    MongoQueue.Push fac "meQueue" (MongoQueue.Default ()) { kuku = "kukuq2"; muku = "mukuq2" }
    |> printfn "Inserted data in queue %A"

    meQueue.list (1, 10)
    |> printfn "Get queue items %A"

    dataFromMeQueue.list (1, 10)
    |> printfn "Get data items %A"

    meQueue.list (1, 1)
    |> List.head
    |> MongoCarrier.getId
    |> MongoQueue.Error fac "meQueue" { Error = "Test Error" }
    |> printfn "Get data items %A"

    meQueue.list (1, 1)
    |> List.head
    |> MongoCarrier.getId
    |> MongoQueue.Success fac "meQueue"

    0 // return an integer exit code
