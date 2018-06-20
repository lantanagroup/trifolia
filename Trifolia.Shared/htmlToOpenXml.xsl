<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"
    xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
    xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main"
    xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing"
    xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture"
    xmlns:t="https://trifolia.lantanagroup.com" exclude-result-prefixes="xs" version="2.0">
    <xsl:output indent="yes"/>
    
    <xsl:param name="linkStyle" select="'HyperlinkCourierBold'" />
    <xsl:param name="bulletListStyle" select="'ListBullet'" />
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
            <w:tblPr/>
            <w:tblGrid/>

            <xsl:apply-templates/>
        </w:tbl>
    </xsl:template>

    <xsl:template match="tr">
        <w:tr>
            <w:trPr>
                <xsl:if test="parent::thead">
                    <w:tblHeader />
                </xsl:if>
            </w:trPr>
            <xsl:apply-templates/>
        </w:tr>
    </xsl:template>

    <xsl:template match="th | td">
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
        <w:p>
            <w:pPr>
                <w:pStyle>
                    <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="$orderedListStyle" />
                </w:pStyle>
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
        <w:r>
            <w:rPr>
                <xsl:choose>
                    <xsl:when test="$runStyle">
                        <w:rStyle>
                            <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="$runStyle" />
                        </w:rStyle>
                    </xsl:when>
                    <xsl:when test="ancestor::*[@class = $xmlNameClass]">
                        <w:rStyle>
                            <xsl:attribute name="val" namespace="http://schemas.openxmlformats.org/wordprocessingml/2006/main" select="$xmlNameClass" />
                        </w:rStyle>
                    </xsl:when>
                </xsl:choose>
                <xsl:choose>
                    <!-- Italics -->
                    <xsl:when test="ancestor::em or ancestor::i">
                        <w:i w:val="true"/>
                    </xsl:when>
                    <!-- Bold -->
                    <xsl:when test="ancestor::strong or ancestor::b">
                        <w:b w:val="true"/>
                    </xsl:when>
                    <!-- Superscript -->
                    <xsl:when test="ancestor::sup">
                        <w:sz w:val="10"/>
                        <w:vertAlign w:val="superscript"/>
                    </xsl:when>
                </xsl:choose>
            </w:rPr>
            <w:t>
                <xsl:value-of select="."/>
            </w:t>
        </w:r>
    </xsl:template>

    <xsl:template match="text()">
        <w:p>
            <w:pPr/>
            <xsl:apply-templates select="." mode="inParagraph"/>
        </w:p>
    </xsl:template>
</xsl:stylesheet>
