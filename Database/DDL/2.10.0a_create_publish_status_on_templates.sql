BEGIN TRAN

ALTER TABLE template
	ADD statusId INT NULL

ALTER TABLE template  WITH CHECK ADD  CONSTRAINT [FK_template_status] FOREIGN KEY([statusId])
REFERENCES publish_status ([id])

COMMIT