Imports System.Reflection

Public Module MethodTimeLogger
    Public MethodBase As List(Of MethodBase) = New List(Of MethodBase)()
    Public Messages As List(Of String) = New List(Of String)()

    Public Sub Log(method As MethodBase, milliseconds As Long, message As String)
        Console.WriteLine($"{method.Name} {milliseconds}")
        MethodBase.Add(method)

        If message IsNot Nothing Then
            Messages.Add(message)
        End If
    End Sub
End Module