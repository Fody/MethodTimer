Imports MethodTimer

Public Class ClassWithAsyncMethod

    <Time>
    Public Async Function MethodWithAwaitAsync() As Task
        Await Task.Delay(500)
    End Function

End Class
