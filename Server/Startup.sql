if object_id('server.SubmittedOrder', 'U') is null
    create table [server].[SubmittedOrder] (
        Id uniqueidentifier not null primary key,
        Value int not null
    )