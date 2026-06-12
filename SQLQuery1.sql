CREATE TABLE dbo.EmailQueue (
	ID int PRIMARY KEY NOT NULL IDENTITY (1, 1),
	Recipient nvarchar(MAX) NOT NULL,
	Subject nvarchar(400) NOT NULL,
	Body nvarchar(MAX) NOT NULL,
	Created datetime NOT NULL,
	Sent datetime NULL
)