-- Создание основной таблицы Files
CREATE TABLE Files (
    fileID SERIAL PRIMARY KEY,
    fileName TEXT UNIQUE NOT NULL
);

-- Создание таблицы Values
CREATE TABLE Values (
    valuesID SERIAL,
    fileID INTEGER NOT NULL REFERENCES Files(FileID),
    valuesStartDate TIMESTAMP NOT NULL,
    valuesExecutionTime integer NOT NULL,
    valuesIndicator double precision NOT NULL
);

-- Создание таблицы Result
CREATE TABLE Result (
    resultID SERIAL PRIMARY KEY,
    fileID INTEGER NOT NULL REFERENCES Files(FileID),
    resultDeltaTime double precision,
    resultMinDate TIMESTAMP,
	resultMaxDate timestamp,
    resultAvgExecTime double precision,
    resultAvgValIndicator double precision,
    resultMedian double precision,
    resultMax double precision,
    resultMin double precision
);

--команды для тестов
drop table result;
drop table values;
drop table files;

select * from values;
select * from result;
select * from files;