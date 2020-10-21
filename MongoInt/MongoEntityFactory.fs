module MongoInt.MongoEntityFactory

open MongoInt
open CollectionController

type Entity<'a> (col: CollectionName, config: MongoConfig) =
    member this.Config = config
    member this.Collection = col
    member this.get = loadFromCollection<'a> config col
    member this.list = loadPageFromCollection<'a> config col
    member this.update = updateRecordInCollection<'a, 'a> config col
    member this.patch<'a, 'b> item = updateRecordInCollection<'a, 'b> config col item
    member this.create = createRecordInCollection<'a> config col
    member this.delete = deleteFromCollection<'a> config col
    member this.all<'b> filter = getAll<'a, 'b> config col filter

type Factory (config) =
    member this.Config = config
    member this.CreateEntity<'a> (col: CollectionName) = Entity<'a>(col, this.Config)

let defaultConfig = {
    ConnectionString = "mongodb://localhost:27017"
    DbName = ""
}