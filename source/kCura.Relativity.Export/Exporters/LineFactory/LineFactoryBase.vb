Namespace kCura.Relativity.Export.Exports.LineFactory
	Public MustInherit Class LineFactoryBase
		Public MustOverride Sub WriteLine(ByVal stream As System.IO.StreamWriter)

		Protected Sub New()
			'Satifies Rule: Abstract types should not have constructors
		End Sub
	End Class
End Namespace
