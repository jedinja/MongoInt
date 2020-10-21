namespace MongoInt

open MongoDB.Bson

type CollectionName = string
type Id = BsonObjectId
type Page = int * int

type MongoConfig = {
    ConnectionString: string
    DbName: string
}

type MongoCarrier<'a> = {
    Id: string
    Record: 'a
}

module MongoCarrier =
    let from<'a> (id: string) (record:'a) = {
        Id = id
        Record = record
    }

module Page =
    let next (page: Page) = (fst page + 1, snd page)