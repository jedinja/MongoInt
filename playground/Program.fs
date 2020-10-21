// Learn more about F# at http://fsharp.org

open MongoDB.Bson.Serialization.Attributes
open MongoInt.MongoEntityFactory

[<BsonIgnoreExtraElements>]
type Me = {
    kuku: string
}

[<EntryPoint>]
let main argv =

    let fac = Factory { defaultConfig with DbName = "play" }
    let me = fac.CreateEntity<Me> "me"

    "5f89a0e7c38d9a2d8a335adf"
    |> me.get
    |> printfn "%A"

    { kuku = "ruku" + System.DateTime.Now.Millisecond.ToString() }
    |> me.update "5f89a0e7c38d9a2d8a335adf"
    |> printfn "%A"

    let created = me.create { kuku = "test" }
    printfn "%A" created

    me.delete created.Id

    me.list (1, 10)
    |> printfn "%A"

    me.all { kuku = "test" }
    |> printfn "%A"

    0 // return an integer exit code
