IF NOT EXISTS (SELECT * FROM publish_status WHERE [status] = 'Retired')
BEGIN
  INSERT INTO publish_status ([status]) VALUES ('Retired')
END