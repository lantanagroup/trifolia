<?xml version="1.0" encoding="utf-16" standalone="yes"?>
<sch:schema xmlns:voc="http://www.alschulerassociates.com/voc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:cda="urn:hl7-org:v3" xmlns="urn:hl7-org:v3" xmlns:sch="http://www.ascc.net/xml/schematron">
  <sch:ns prefix="voc" uri="http://www.alschulerassociates.com/voc" />
  <sch:ns prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance" />
  <sch:ns prefix="cda" uri="urn:hl7-org:v3" />
  <sch:phase id="errors">
    <sch:active pattern="testErrorPattern" />
  </sch:phase>
  <sch:phase id="warnings">
  </sch:phase>
  <sch:pattern id="testErrorPattern" name="testErrorPattern">
    <sch:rule id="testErrorRule" context="/">
      <sch:assert id="1" test="not(.)">Triggered for all documents for testing purposes.</sch:assert>
    </sch:rule>
  </sch:pattern>
</sch:schema>