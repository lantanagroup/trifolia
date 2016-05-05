IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template' AND COLUMN_NAME = 'primaryContextType')
BEGIN
	ALTER TABLE template ADD primaryContextType VARCHAR(255)
END

update templatetype set rootContextType = 'ClinicalDocument' where rootContext = 'ClinicalDocument'
update templatetype set rootContextType = 'QualityMeasureDocument' where rootContext = 'QualityMeasureDocument'
update templatetype set rootContextType = 'Entry', rootContext = 'entry' where rootContext='Entry'
update templatetype set rootContextType = 'Section', rootContext = 'section' where rootContext='Section'
update templatetype set rootContext = 'component', rootContextType = 'Component2' where rootContext='Component2'
update templatetype set rootContext = 'entry', rootContextType = 'SourceOf' where rootContext='SourceOf'
update templatetype set rootContextType = '' where rootContext = ''

-- CDA template updates
update template set primaryContextType = primaryContext
update template set primaryContext = 'act', primaryContextType = 'Act' where primaryContextType = 'Act'
update template set primaryContext = 'addr', primaryContextType = 'AD' where primaryContextType = 'AD' or primaryContextType = 'addr'
update template set primaryContext = 'assignedAuthor', primaryContextType = 'AssignedAuthor' where primaryContextType = 'AssignedAuthor'
update template set primaryContext = 'ClinicalDocument', primaryContextType = 'ClinicalDocument' where primaryContextType = 'ClinicalDocument'
update template set primaryContext = 'consumable', primaryContextType = 'Consumable' where primaryContextType = 'Consumable'
update template set primaryContext = 'criterion', primaryContextType = 'Criterion' where primaryContextType = 'Criterion'
update template set primaryContext = 'effectiveTime', primaryContextType = 'effectiveTime' where primaryContextType = 'effectiveTime'
update template set primaryContext = 'encounter', primaryContextType = 'Encounter' where primaryContextType = 'Encounter'
update template set primaryContext = 'encounterParticipant', primaryContextType = 'EncounterParticipant' where primaryContextType = 'EncounterParticipant'
update template set primaryContext = 'entry', primaryContextType = 'Entry' where primaryContextType = 'Entry'
update template set primaryContext = 'entryRelationship', primaryContextType = 'EntryRelationship' where primaryContextType = 'EntryRelationship'
update template set primaryContext = 'manufacturedProduct', primaryContextType = 'ManufacturedProduct' where primaryContextType = 'ManufacturedProduct'
update template set primaryContext = 'observation', primaryContextType = 'Observation' where primaryContextType = 'Observation'
update template set primaryContext = 'organizer', primaryContextType = 'Organizer' where primaryContextType = 'Organizer'
update template set primaryContext = 'participant', primaryContextType = 'Participant2' where primaryContextType = 'Participant2'
update template set primaryContext = 'participant', primaryContextType = 'Participant2' where primaryContextType = 'participant'
update template set primaryContext = 'participantRole', primaryContextType = 'ParticipantRole' where primaryContextType = 'ParticipantRole'
update template set primaryContext = 'performer', primaryContextType = 'Performer1' where primaryContextType = 'Performer1'
update template set primaryContext = 'performer', primaryContextType = 'Performer1' where primaryContextType = 'Performer1'
update template set primaryContext = 'performer', primaryContextType = 'Performer2' where primaryContextType = 'performer'
update template set primaryContext = 'name', primaryContextType = 'PN' where primaryContextType = 'PN' or primaryContextType = 'name'
update template set primaryContext = 'relatedSubject', primaryContextType = 'RelatedSubject' where primaryContextType = 'relatedSubject'
update template set primaryContext = 'section', primaryContextType = 'Section' where primaryContextType = 'section'
update template set primaryContext = 'substanceAdministration', primaryContextType = 'SubstanceAdministration' where primaryContextType = 'substanceAdministration'
update template set primaryContext = 'supply', primaryContextType = 'Supply' where primaryContextType = 'supply'
update template set primaryContext = 'effectiveTime', primaryContextType = 'TS' where primaryContextType = 'TS'
update template set primaryContext = 'procedure', primaryContextType = 'Procedure' where primaryContextType = 'procedure'

-- HQMF R2 template updates
update template set primaryContext = 'actCriteria', primaryContextType = 'ActCriteria' where primaryContextType = 'ActCriteria'
update template set primaryContext = 'dataCriteriaSection', primaryContextType = 'DataCriteriaSection' where primaryContextType = 'dataCriteriaSection'
update template set primaryContext = 'measureDescriptionSection', primaryContextType = 'MeasureDescriptionSection' where primaryContextType = 'measureDescriptionSection'
update template set primaryContext = 'measureObservationsSection', primaryContextType = 'MeasureObservationsSection' where primaryContextType = 'measureObservationsSection'
update template set primaryContext = 'observationCriteria', primaryContextType = 'ObservationCriteria' where primaryContextType = 'observationCriteria'
update template set primaryContext = 'populationCriteriaSection', primaryContextType = 'PopulationCriteriaSection' where primaryContextType = 'populationCriteriaSection'
update template set primaryContext = 'section', primaryContextType = 'Section' where primaryContextType = 'section'
update template set primaryContext = 'substanceAdministrationCriteria', primaryContextType = 'SubstanceAdministrationCriteria' where primaryContextType = 'substanceAdministrationCriteria'
update template set primaryContext = 'supplyCriteria', primaryContextType = 'SupplyCriteria' where primaryContextType = 'SupplyCriteria'