Namespace  kCura.Relativity.Export.Types
	<Serializable()> Public Class Pair
		Public Value As String
		Public Display As String
		Public Overrides Function ToString() As String
			Return Me.Display
		End Function
		Public Sub New(ByVal v As String, ByVal d As String)
			Me.Value = v
			Me.Display = d
		End Sub
		Public Shadows Function equals(ByVal other As Pair) As Boolean
			Return (Me.Display = other.Display And Me.Value = other.Value)
		End Function
	End Class
End Namespace