create table Album (
    Id int PRIMARY KEY AUTO_INCREMENT,
    Title varchar(31) not null,
    LinkinpediaUrl varchar(63)
);

create table Song (
    Id int primary key AUTO_INCREMENT,
    AlbumId int,
    Isrc varchar(15),
    LinkinpediaUrl varchar(63),
    FOREIGN KEY (AlbumId) REFERENCES Album(Id)
);

create table SongVariant (
    Id int primary key AUTO_INCREMENT,
    SongId int,
    IsrcOverride varchar(15),
    VariantName varchar(31),
    Description varchar(63),
    FOREIGN KEY (SongId) REFERENCES Song(Id)
);

create table SongMashup (
     Id int primary key AUTO_INCREMENT,
     Title varchar(31),
     LinkinpediaUrl varchar(63)
);

create table SongInMashup (
    SongId int,
    MashupId int,
    CONSTRAINT PK_SetlistAct PRIMARY KEY (SongId,MashupId),
    FOREIGN KEY (SongId) REFERENCES Song(Id),
    FOREIGN KEY (MashupId) REFERENCES SongMashup(Id)
);


create table Setlist (
    Id int primary key AUTO_INCREMENT,
    ConcertId varchar(63) not null,
    Title varchar(31) not null,
    LinkinpediaUrl varchar(63)
);

create table SetlistAct (
     SetlistId int not null,
     ActNumber int not null,
     Title varchar(31) not null,
     CONSTRAINT PK_SetlistAct PRIMARY KEY (SetlistId,ActNumber),
     FOREIGN KEY (SetlistId) REFERENCES Setlist(Id)
);

create table SetlistEntry (
    Id varchar(63) primary key not null,
    SetlistId int not null,
    ActNumber int,
    SortNumber int not null,
    TitleOverride varchar(31),
    ExtraNotes varchar(127),
    PlayedSongId int,
    PlayedVariantId int,
    PlayedMashupId int,
    FOREIGN KEY (SetlistId) REFERENCES Setlist(Id),
    FOREIGN KEY (SetlistId, ActNumber) REFERENCES SetlistAct(SetlistId, ActNumber),
    FOREIGN KEY (PlayedSongId) REFERENCES Song(Id),
    FOREIGN KEY (PlayedVariantId) REFERENCES SongVariant(Id)
);

create table SetlistEntrySongExtra (
      Id varchar(63) primary key not null,
      SetlistEntryId varchar(63) not null,
      SongId int,
      Type varchar(31),
      Description varchar(127),
      FOREIGN KEY (SetlistEntryId) REFERENCES SetlistEntry(Id),
      FOREIGN KEY (SongId) REFERENCES Song(Id)
);