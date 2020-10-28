namespace MongoInt.Abstracts

open System
open MongoDB.Bson.Serialization.Attributes
open MongoInt
open MongoInt.MongoEntityFactory

type RemainingTries = RemainingTries of int
module RemainingTries =
    let Create i =
        if(i >= 0)
        then RemainingTries i
        else RemainingTries 0
    let Decrement (RemainingTries rt) = rt - 1 |> Create

[<Measure>] type mins
[<Measure>] type secs
[<Measure>] type hours

type DateTicks = DateTicks of int64
module DateTicks =
    let private nowTicks () = DateTime.UtcNow.Ticks
    let now () = nowTicks () |> DateTicks
    let span (timespan: TimeSpan) = DateTicks timespan.Ticks
    let fromMins (minutes: int<mins>) = span (TimeSpan(0, int minutes, 0))
    let fromSecs (seconds: int<secs>) = span (TimeSpan(0, 0, int seconds))
    let fromHours (hours: int<hours>) = span (TimeSpan(int hours, 0, 0))
    let inFuture (DateTicks ticks) = ticks + nowTicks () |> DateTicks
    let shortenBy (RemainingTries rt) (DateTicks ticks) =
        match rt with
        | 1 | 0 -> ticks
        | a -> ticks / (int64(a - 1))
        |> DateTicks

[<BsonIgnoreExtraElements>]
type MongoQueue = {
    CreateDate: DateTicks
    NextTryDate: DateTicks
    RemainingTries: RemainingTries
    MaxRetryTime: DateTicks
}

[<BsonIgnoreExtraElements>]
type MongoQueueData<'a> = {
    Data: 'a
}

type MongoQueueLogType =
    | Error
    | Success

[<BsonIgnoreExtraElements>]
type MongoQueueLog<'a, 'b> = {
    CreateDate: DateTicks
    LogType: MongoQueueLogType
    RemainingTries: RemainingTries
    QueueData: 'a
    LogData: 'b option
}

module MongoQueue =
    let private logCol col = col + "_logs"
    let private nextDateK (RemainingTries rt) =
        match rt with
        | 1 | 0 -> 1
        | a -> a - 1

    let Default () = {
        CreateDate = DateTicks.now ()
        NextTryDate = DateTicks.now ()
        RemainingTries = RemainingTries 5
        MaxRetryTime = DateTicks.fromMins 30<mins>
    }

    let Push<'data> (factory: Factory) col queueItem data =
        let queueEntity = factory.CreateEntity<MongoQueue> col
        let dataEntity = factory.CreateEntity<MongoQueueData<'data>> col

        let id =
            queueEntity.create queueItem
            |> MongoCarrier.getId
        ()
        dataEntity.update id { Data = data }

    let Error<'data, 'logdata> (factory: Factory) col logData id =
        let queueEntity = factory.CreateEntity<MongoQueue> col
        let dataEntity = factory.CreateEntity<MongoQueueData<'data>> col
        let queueLogEntity = col |> logCol |> factory.CreateEntity<MongoQueueLog<'data, 'logdata>>

        let queue = queueEntity.get id
        let data = dataEntity.get id
        let log = {
            CreateDate = DateTicks.now ()
            LogType = Error
            RemainingTries = queue.Record.RemainingTries
            QueueData = data.Record.Data
            LogData = Some logData;
        }

        queueLogEntity.create log |> ignore

        let updatedQueue = {
            queue.Record with
                RemainingTries = RemainingTries.Decrement queue.Record.RemainingTries
                NextTryDate = queue.Record.MaxRetryTime |> DateTicks.shortenBy queue.Record.RemainingTries |> DateTicks.inFuture
        }

        queueEntity.update id updatedQueue

    let Success<'data> (factory: Factory) col id =
        let queueEntity = factory.CreateEntity<MongoQueue> col
        let dataEntity = factory.CreateEntity<MongoQueueData<'data>> col
        let queueLogEntity = col |> logCol |> factory.CreateEntity<MongoQueueLog<'data, _>>

        let data = dataEntity.get id
        let queue = queueEntity.get id
        let log = {
            CreateDate = DateTicks.now ()
            LogType = Success
            RemainingTries = queue.Record.RemainingTries
            QueueData = data.Record.Data
            LogData = None;
        }

        queueLogEntity.create log |> ignore

        queueEntity.delete id
