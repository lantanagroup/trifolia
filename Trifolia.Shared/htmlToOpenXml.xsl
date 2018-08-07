<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
    xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
    xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
    xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture"
    xmlns:t="https://trifolia.lantanagroup.com" exclude-result-prefixes="xs" version="2.0">
    <xsl:output indent="no" />
    
    <xsl:param name="linkStyle" select="'HyperlinkCourierBold'" />
    <xsl:param name="bulletListStyle" select="'ListParagraph'" />
    <xsl:param name="orderedListStyle" select="'ListNumber'" />
    <xsl:param name="xmlNameClass" select="'XMLName'" />

    <xsl:template match="/">
        <w:document>
            <w:body>
                <xsl:apply-templates/>
            </w:body>
        </w:document>
    </xsl:template>

    <xsl:template match="text()[string-length(normalize-space(.)) = 0]" priority="1"/>
    <xsl:template match="text()[string-length(normalize-space(.)) = 0]" priority="1" mode="inParagraph"/>

    <xsl:template match="table">
        <w:tbl>
            <w:tblPr>
                <w:tblStyle w:val="TableGrid" />
                <w:tblW w:w="10080" w:type="dxa" />
                <w:jc w:val="center" />
                <w:tblLayout w:type="fixed" />
                <w:tblLook w:val="02A0" w:firstRow="1" w:lastRow="0" w:firstColumn="1" w:lastColumn="0" w:noHBand="1" w:noVBand="0" />
            </w:tblPr>
            <w:tblGrid/>

            <xsl:apply-templates/>
        </w:tbl>
    </xsl:template>

    <xsl:template match="tr">
        <w:tr>
            <w:trPr>
                <xsl:if test="parent::thead">
                    <w:cantSplit />
                    <w:tblHeader />
                </xsl:if>
                <w:jc w:val="center" />
            </w:trPr>
            <xsl:apply-templates/>
        </w:tr>
    </xsl:template>

    <xsl:template match="th">
        <w:tc>
            <w:tcPr>
                <w:shd w:val="clear" w:color="auto" w:fill="E6E6E6" />
            </w:tcPr>
            <w:p>
                <xsl:apply-templates mode="inParagraph"/>
            </w:p>
        </w:tc>
    </xsl:template>
    
    <xsl:template match="td">
        <w:tc>
            <w:p>
                <xsl:apply-templates mode="inParagraph"/>
            </w:p>
        </w:tc>
    </xsl:template>

    <xsl:template match="a" mode="inParagraph">
        <xsl:if test="not(starts-with(@href, '#'))">
            <xsl:comment>
                <xsl:value-of select="@href"/>
            </xsl:comment>
        </xsl:if>
        <w:hyperlink w:history="true">
            <xsl:if test="starts-with(@href, '#')">
                <xsl:attribute name="anchor" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="substring(@href, 2)" />
            </xsl:if>
            <w:proofErr w:type="gramStart"/>
            <xsl:apply-templates mode="inParagraph">
                <xsl:with-param name="runStyle" select="$linkStyle" />
            </xsl:apply-templates>
        </w:hyperlink>
    </xsl:template>

    <xsl:template match="ol/li">
        <xsl:variable name="olId" select="count(../preceding-sibling::ol)+1" />
        
        <w:p>
            <w:pPr>
                <w:pStyle>
                    <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="$bulletListStyle" />
                </w:pStyle>
                <w:numPr>
                    <w:ilvl w:val="0" />
                    <w:numId>
                        <xsl:attribute namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" name="val" select="$olId" />
                    </w:numId>
                </w:numPr>
            </w:pPr>
            <xsl:apply-templates mode="inParagraph"/>
        </w:p>
    </xsl:template>

    <xsl:template match="ul/li">
        <w:p>
            <w:pPr>
                <w:pStyle>
                    <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="$bulletListStyle" />
                </w:pStyle>
            </w:pPr>
            <xsl:apply-templates mode="inParagraph"/>
        </w:p>
    </xsl:template>

    <xsl:template match="p">
        <w:p>
            <w:pPr/>
            <xsl:apply-templates mode="inParagraph"/>
        </w:p>
    </xsl:template>

    <xsl:template match="img" mode="inParagraph">
        <w:r>
           <xsl:comment>
               <xsl:value-of select="@src" />
           </xsl:comment>
           <w:drawing>
               <wp:inline distT="0" distB="0" distL="0" distR="0">
                   <!-- TODO: Width and Height -->
                   <wp:extent cx="7419975" cy="3143250"/>
                   <wp:effectExtent l="0" t="0" r="0" b="0"/>
                   <wp:docPr id="1">
                       <xsl:attribute name="name">
                           <xsl:value-of select="text()" />
                       </xsl:attribute>
                   </wp:docPr>
                   <wp:cNvGraphicFramePr>
                       <a:graphicFrameLocks noChangeAspect="1" />
                   </wp:cNvGraphicFramePr>
                   <a:graphic>
                       <a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture">
                           <pic:pic>
                               <pic:nvPicPr>
                                   <xsl:comment>TODO: Replace name attribute with name of file</xsl:comment>
                                   <pic:cNvPr id="0" name="FILENAME.png"/>
                                   <pic:cNvPicPr/>
                               </pic:nvPicPr>
                               <pic:blipFill>
                                   <xsl:comment>TODO: Replace embed with relationship id</xsl:comment>
                                   <a:blip r:embed="replaceme" cstate="print">
                                       <!--
                                       <a:extLst>
                                           <a:ext uri="{{28A0092B-C50C-407E-A947-70E740481C1C}}"/>
                                       </a:extLst>
                                       -->
                                   </a:blip>
                                   <a:stretch>
                                       <a:fillRect/>
                                   </a:stretch>
                               </pic:blipFill>
                               <pic:spPr>
                                   <a:xfrm>
                                       <a:off x="0" y="0"/>
                                       <a:ext cx="7419975" cy="3143250"/>
                                   </a:xfrm>
                                   <a:prstGeom prst="rect">
                                       <a:avLst/>
                                   </a:prstGeom>
                               </pic:spPr>
                           </pic:pic>
                       </a:graphicData>
                   </a:graphic>
               </wp:inline>
           </w:drawing>
        </w:r>
    </xsl:template>
    
    <xsl:template match="img">
        <w:p>
            <xsl:apply-templates select="." mode="inParagraph" />
        </w:p>
    </xsl:template>

    <xsl:template match="text()" mode="inParagraph">
        <xsl:param name="runStyle" />
        <xsl:param name="hasXmlClass" select="ancestor::*[@class = $xmlNameClass]" />
        <xsl:param name="hasItalics" select="ancestor::em or ancestor::i" />
        <xsl:param name="hasBold" select="ancestor::strong or ancestor::b" />
        <xsl:param name="hasSuperscript" select="ancestor::sup" />
        <xsl:param name="hasExample" select="ancestor::pre or ancestor::code" />
        
        <xsl:variable name="lines" select="tokenize(.,'\n')" />
        <xsl:variable name="isKeyword" select=". = 'SHALL' or . = 'SHOULD' or . = 'MAY' or . = 'SHALL NOT' or . = 'SHOULD NOT' or . = 'MAY NOT'" />
        <xsl:variable name="isXmlName" select=". = 'DYNAMIC' or . = 'STATIC'" />
        <xsl:variable name="isXsiType" select="matches(. , '^xsi:type=&quot;.+?&quot;$')" />
        
        <w:r>
            <w:rPr>
                <xsl:choose>
                    <xsl:when test="$isKeyword and $hasBold">
                        <w:rStyle>
                            <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main">keyword</xsl:attribute>
                        </w:rStyle>
                    </xsl:when>
                    <xsl:when test="($isXmlName or $isXsiType) and $hasBold">
                        <w:rStyle>
                            <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main">XMLnameBold</xsl:attribute>
                        </w:rStyle>
                    </xsl:when>
                    <xsl:when test="$runStyle">
                        <w:rStyle>
                            <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="$runStyle" />
                        </w:rStyle>
                    </xsl:when>
                    <xsl:when test="$hasXmlClass">
                        <w:rStyle>
                            <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="$xmlNameClass" />
                        </w:rStyle>
                    </xsl:when>
                </xsl:choose>
                <xsl:choose>
                    <!-- Italics -->
                    <xsl:when test="$hasItalics">
                        <w:i w:val="true"/>
                    </xsl:when>
                    <!-- Bold -->
                    <xsl:when test="$hasBold and not($isKeyword) and not($isXmlName) and not($isXsiType)">
                        <w:b w:val="true"/>
                    </xsl:when>
                    <!-- Superscript -->
                    <xsl:when test="$hasSuperscript">
                        <w:sz w:val="10"/>
                        <w:vertAlign w:val="superscript"/>
                    </xsl:when>
                </xsl:choose>
            </w:rPr>
            <xsl:for-each select="$lines">
                <w:t xml:space="preserve"><xsl:value-of select="."/></w:t>
                <xsl:if test="position() != count($lines)">
                    <w:br />
                </xsl:if>
            </xsl:for-each>
        </w:r>
    </xsl:template>

    <xsl:template match="text()">
        <w:p>
            <w:pPr>
                <xsl:if test="ancestor::pre or ancestor::code">
                    <w:pStyle>
                        <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="'Example'" />
                    </w:pStyle>
                </xsl:if>
            </w:pPr>
            <xsl:apply-templates select="." mode="inParagraph"/>
        </w:p>
    </xsl:template>
</xsl:stylesheet>
