<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:template match="/">
  <html>
  <head>
  <style>
  	table{
  	border-collapse: collapse;
	}
	th{
	text-align: internal center;
	}
	th, td {
    border: 1px solid #ddd;
    padding: 7px 10px;
    vertical-align: top;
    min-width: 8px;
	}
  </style>
  </head>
  <body>
  <h2>Validation Messages</h2>
  <table border="0">
    <tr bgcolor="#f0f0f0">
      <th>Error Code</th>
      <th>Short Message</th>
      <th>Troubleshooting</th>
	  <th>Additional info for CS/Doc team</th>
    </tr>
    <xsl:for-each select="ValidationMessages/Message">
    <tr>
      <td><xsl:value-of select="errorCode"/></td>
      <td><xsl:value-of select="shortMessage"/></td>
      <td><xsl:value-of select="troubleshooting"/></td>
	  <td></td>
    </tr>
    </xsl:for-each>
  </table>
  </body>
  </html>
</xsl:template>

</xsl:stylesheet>