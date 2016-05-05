IF NOT EXISTS(SELECT * FROM publish_status WHERE [status] = 'Deprecated')
BEGIN
	INSERT INTO publish_status ([status]) VALUES('Deprecated');
END