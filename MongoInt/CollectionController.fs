module MongoInt.CollectionController

open System
open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Driver
open MongoInt.IO

let private ID = "_id"
let toBsonId (str: string) = BsonObjectId(ObjectId str)

let private filt () = FilterDefinitionBuilder<BsonDocument> ()
let private stringFieldF str = StringFieldDefinition<BsonDocument,BsonObjectId> str
let private stringField str = StringFieldDefinition<BsonDocument> str
let private emptyFilter () = filt().Empty
let private asc field = SortDefinitionBuilder().Ascending(stringField field)
let private getPage (page: Page) (query: IFindFluent<BsonDocument, BsonDocument>) = query.Skip(Nullable<int>((fst page - 1) * snd page)).Limit(Nullable<int>(snd page))
let private getId (ent: BsonDocument) = (ent.Item(ID).AsObjectId).ToString()
let private fromBsonToUpdate<'a> obj =

    let changes = serialize<'a> obj

    let firstValue = changes |> Seq.head
    let upd = Builders<BsonDocument>.Update.Set (StringFieldDefinition<BsonDocument,BsonValue> firstValue.Name, firstValue.Value)

    changes.Elements
    |> Seq.tail
    |> Seq.fold
        (fun (state: UpdateDefinition<BsonDocument>) elem -> state.Set (StringFieldDefinition<BsonDocument,BsonValue> elem.Name, elem.Value))
        upd

let internal loadFromCollection<'a> config col id =
    let client = getClient<BsonDocument> config col
    let filter = filt().Eq(stringFieldF ID, id |> toBsonId)

    client.FindSync<'a>(filter).First()
    |> MongoCarrier.from id

let internal loadPageFromCollection<'a> config col page =
    let client = getClient<BsonDocument> config col

    (client.Find(emptyFilter ()).Sort(asc ID)
    |> getPage page).ToEnumerable()
    |> List.ofSeq
    |> List.map (fun entity ->
        entity
        |> deserialize<'a>
        |> MongoCarrier.from (getId entity))

let internal createRecordInCollection<'a> config col item =
    let client = getClient<BsonDocument> config col

    let doc = item |> serialize<'a>
    doc |> client.InsertOne

    doc
    |> deserialize<'a>
    |> MongoCarrier.from (getId doc)

let internal updateRecordInCollection<'a, 'b> config col id item =
    let client = getClient<BsonDocument> config col
    let filter = filt().Eq(stringFieldF ID, id |> toBsonId)
    //let upd = BsonDocumentUpdateDefinition(serialize<'b> item)
    let upd = fromBsonToUpdate<'b> item

    client.UpdateOne(filter, upd) |> ignore

    item |>
    MongoCarrier.from id

let internal deleteFromCollection<'a> config col id =
    let client = getClient<BsonDocument> config col
    let filter = filt().Eq(stringFieldF ID, id |> toBsonId)

    client.DeleteOne filter
    |> ignore

let internal getAll<'a, 'b> config col filterObj =
    let client = getClient<BsonDocument> config col
    let filter = BsonDocumentFilterDefinition<BsonDocument>(serialize<'b> filterObj)

    client.Find(filter).ToEnumerable()
    |> List.ofSeq
    |> List.map (fun entity ->
        entity
        |> deserialize<'a>
        |> MongoCarrier.from (getId entity))