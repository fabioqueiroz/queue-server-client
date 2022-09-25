if object_id('server.SubmittedOrder', 'U') is not null
    create table [receiver].[SubmittedOrder] (
        Id uniqueidentifier not null primary key,
        Value int not null
    )