﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On


'
'This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
'
Namespace kCura.EDDS.WebAPI.FieldQueryBase
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code"),  _
     System.Web.Services.WebServiceBindingAttribute(Name:="FieldQuerySoap", [Namespace]:="http://www.kCura.com/EDDS/FieldQuery")>  _
    Partial Public Class FieldQuery
        Inherits System.Web.Services.Protocols.SoapHttpClientProtocol
        
        Private RetrieveDisplayFieldNameByFieldCategoryIDOperationCompleted As System.Threading.SendOrPostCallback
        
        Private RetrieveAllMappableOperationCompleted As System.Threading.SendOrPostCallback
        
        Private RetrieveAllOperationCompleted As System.Threading.SendOrPostCallback
        
        Private RetrievePotentialBeginBatesFieldsOperationCompleted As System.Threading.SendOrPostCallback
        
        Private IsFieldIndexedOperationCompleted As System.Threading.SendOrPostCallback
        
        Private useDefaultCredentialsSetExplicitly As Boolean
        
        '''<remarks/>
        Public Sub New()
            MyBase.New
            Me.Url = "http://localhost/RelativityWebApi/FieldQuery.asmx"
            If (Me.IsLocalFileSystemWebService(Me.Url) = true) Then
                Me.UseDefaultCredentials = true
                Me.useDefaultCredentialsSetExplicitly = false
            Else
                Me.useDefaultCredentialsSetExplicitly = true
            End If
        End Sub
        
        Public Shadows Property Url() As String
            Get
                Return MyBase.Url
            End Get
            Set
                If (((Me.IsLocalFileSystemWebService(MyBase.Url) = true)  _
                            AndAlso (Me.useDefaultCredentialsSetExplicitly = false))  _
                            AndAlso (Me.IsLocalFileSystemWebService(value) = false)) Then
                    MyBase.UseDefaultCredentials = false
                End If
                MyBase.Url = value
            End Set
        End Property
        
        Public Shadows Property UseDefaultCredentials() As Boolean
            Get
                Return MyBase.UseDefaultCredentials
            End Get
            Set
                MyBase.UseDefaultCredentials = value
                Me.useDefaultCredentialsSetExplicitly = true
            End Set
        End Property
        
        '''<remarks/>
        Public Event RetrieveDisplayFieldNameByFieldCategoryIDCompleted As RetrieveDisplayFieldNameByFieldCategoryIDCompletedEventHandler
        
        '''<remarks/>
        Public Event RetrieveAllMappableCompleted As RetrieveAllMappableCompletedEventHandler
        
        '''<remarks/>
        Public Event RetrieveAllCompleted As RetrieveAllCompletedEventHandler
        
        '''<remarks/>
        Public Event RetrievePotentialBeginBatesFieldsCompleted As RetrievePotentialBeginBatesFieldsCompletedEventHandler
        
        '''<remarks/>
        Public Event IsFieldIndexedCompleted As IsFieldIndexedCompletedEventHandler
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/FieldQuery/RetrieveDisplayFieldNameByFieldCategoryID", RequestNamespace:="http://www.kCura.com/EDDS/FieldQuery", ResponseNamespace:="http://www.kCura.com/EDDS/FieldQuery", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function RetrieveDisplayFieldNameByFieldCategoryID(ByVal caseContextArtifactID As Integer, ByVal fieldCategoryID As Integer) As String
            Dim results() As Object = Me.Invoke("RetrieveDisplayFieldNameByFieldCategoryID", New Object() {caseContextArtifactID, fieldCategoryID})
            Return CType(results(0),String)
        End Function
        
        '''<remarks/>
        Public Function BeginRetrieveDisplayFieldNameByFieldCategoryID(ByVal caseContextArtifactID As Integer, ByVal fieldCategoryID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("RetrieveDisplayFieldNameByFieldCategoryID", New Object() {caseContextArtifactID, fieldCategoryID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRetrieveDisplayFieldNameByFieldCategoryID(ByVal asyncResult As System.IAsyncResult) As String
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),String)
        End Function
        
        '''<remarks/>
        Public Overloads Sub RetrieveDisplayFieldNameByFieldCategoryIDAsync(ByVal caseContextArtifactID As Integer, ByVal fieldCategoryID As Integer)
            Me.RetrieveDisplayFieldNameByFieldCategoryIDAsync(caseContextArtifactID, fieldCategoryID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub RetrieveDisplayFieldNameByFieldCategoryIDAsync(ByVal caseContextArtifactID As Integer, ByVal fieldCategoryID As Integer, ByVal userState As Object)
            If (Me.RetrieveDisplayFieldNameByFieldCategoryIDOperationCompleted Is Nothing) Then
                Me.RetrieveDisplayFieldNameByFieldCategoryIDOperationCompleted = AddressOf Me.OnRetrieveDisplayFieldNameByFieldCategoryIDOperationCompleted
            End If
            Me.InvokeAsync("RetrieveDisplayFieldNameByFieldCategoryID", New Object() {caseContextArtifactID, fieldCategoryID}, Me.RetrieveDisplayFieldNameByFieldCategoryIDOperationCompleted, userState)
        End Sub
        
        Private Sub OnRetrieveDisplayFieldNameByFieldCategoryIDOperationCompleted(ByVal arg As Object)
            If (Not (Me.RetrieveDisplayFieldNameByFieldCategoryIDCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent RetrieveDisplayFieldNameByFieldCategoryIDCompleted(Me, New RetrieveDisplayFieldNameByFieldCategoryIDCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/FieldQuery/RetrieveAllMappable", RequestNamespace:="http://www.kCura.com/EDDS/FieldQuery", ResponseNamespace:="http://www.kCura.com/EDDS/FieldQuery", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function RetrieveAllMappable(ByVal caseContextArtifactID As Integer, ByVal artifactTypeID As Integer) As System.Data.DataSet
            Dim results() As Object = Me.Invoke("RetrieveAllMappable", New Object() {caseContextArtifactID, artifactTypeID})
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Function BeginRetrieveAllMappable(ByVal caseContextArtifactID As Integer, ByVal artifactTypeID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("RetrieveAllMappable", New Object() {caseContextArtifactID, artifactTypeID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRetrieveAllMappable(ByVal asyncResult As System.IAsyncResult) As System.Data.DataSet
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Overloads Sub RetrieveAllMappableAsync(ByVal caseContextArtifactID As Integer, ByVal artifactTypeID As Integer)
            Me.RetrieveAllMappableAsync(caseContextArtifactID, artifactTypeID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub RetrieveAllMappableAsync(ByVal caseContextArtifactID As Integer, ByVal artifactTypeID As Integer, ByVal userState As Object)
            If (Me.RetrieveAllMappableOperationCompleted Is Nothing) Then
                Me.RetrieveAllMappableOperationCompleted = AddressOf Me.OnRetrieveAllMappableOperationCompleted
            End If
            Me.InvokeAsync("RetrieveAllMappable", New Object() {caseContextArtifactID, artifactTypeID}, Me.RetrieveAllMappableOperationCompleted, userState)
        End Sub
        
        Private Sub OnRetrieveAllMappableOperationCompleted(ByVal arg As Object)
            If (Not (Me.RetrieveAllMappableCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent RetrieveAllMappableCompleted(Me, New RetrieveAllMappableCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/FieldQuery/RetrieveAll", RequestNamespace:="http://www.kCura.com/EDDS/FieldQuery", ResponseNamespace:="http://www.kCura.com/EDDS/FieldQuery", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function RetrieveAll(ByVal caseContextArtifactID As Integer) As System.Data.DataSet
            Dim results() As Object = Me.Invoke("RetrieveAll", New Object() {caseContextArtifactID})
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Function BeginRetrieveAll(ByVal caseContextArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("RetrieveAll", New Object() {caseContextArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRetrieveAll(ByVal asyncResult As System.IAsyncResult) As System.Data.DataSet
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Overloads Sub RetrieveAllAsync(ByVal caseContextArtifactID As Integer)
            Me.RetrieveAllAsync(caseContextArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub RetrieveAllAsync(ByVal caseContextArtifactID As Integer, ByVal userState As Object)
            If (Me.RetrieveAllOperationCompleted Is Nothing) Then
                Me.RetrieveAllOperationCompleted = AddressOf Me.OnRetrieveAllOperationCompleted
            End If
            Me.InvokeAsync("RetrieveAll", New Object() {caseContextArtifactID}, Me.RetrieveAllOperationCompleted, userState)
        End Sub
        
        Private Sub OnRetrieveAllOperationCompleted(ByVal arg As Object)
            If (Not (Me.RetrieveAllCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent RetrieveAllCompleted(Me, New RetrieveAllCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/FieldQuery/RetrievePotentialBeginBatesFields", RequestNamespace:="http://www.kCura.com/EDDS/FieldQuery", ResponseNamespace:="http://www.kCura.com/EDDS/FieldQuery", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function RetrievePotentialBeginBatesFields(ByVal caseContextArtifactID As Integer) As System.Data.DataSet
            Dim results() As Object = Me.Invoke("RetrievePotentialBeginBatesFields", New Object() {caseContextArtifactID})
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Function BeginRetrievePotentialBeginBatesFields(ByVal caseContextArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("RetrievePotentialBeginBatesFields", New Object() {caseContextArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRetrievePotentialBeginBatesFields(ByVal asyncResult As System.IAsyncResult) As System.Data.DataSet
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Overloads Sub RetrievePotentialBeginBatesFieldsAsync(ByVal caseContextArtifactID As Integer)
            Me.RetrievePotentialBeginBatesFieldsAsync(caseContextArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub RetrievePotentialBeginBatesFieldsAsync(ByVal caseContextArtifactID As Integer, ByVal userState As Object)
            If (Me.RetrievePotentialBeginBatesFieldsOperationCompleted Is Nothing) Then
                Me.RetrievePotentialBeginBatesFieldsOperationCompleted = AddressOf Me.OnRetrievePotentialBeginBatesFieldsOperationCompleted
            End If
            Me.InvokeAsync("RetrievePotentialBeginBatesFields", New Object() {caseContextArtifactID}, Me.RetrievePotentialBeginBatesFieldsOperationCompleted, userState)
        End Sub
        
        Private Sub OnRetrievePotentialBeginBatesFieldsOperationCompleted(ByVal arg As Object)
            If (Not (Me.RetrievePotentialBeginBatesFieldsCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent RetrievePotentialBeginBatesFieldsCompleted(Me, New RetrievePotentialBeginBatesFieldsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/FieldQuery/IsFieldIndexed", RequestNamespace:="http://www.kCura.com/EDDS/FieldQuery", ResponseNamespace:="http://www.kCura.com/EDDS/FieldQuery", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function IsFieldIndexed(ByVal caseContextArtifactID As Integer, ByVal fieldArtifactID As Integer) As Boolean
            Dim results() As Object = Me.Invoke("IsFieldIndexed", New Object() {caseContextArtifactID, fieldArtifactID})
            Return CType(results(0),Boolean)
        End Function
        
        '''<remarks/>
        Public Function BeginIsFieldIndexed(ByVal caseContextArtifactID As Integer, ByVal fieldArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("IsFieldIndexed", New Object() {caseContextArtifactID, fieldArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndIsFieldIndexed(ByVal asyncResult As System.IAsyncResult) As Boolean
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),Boolean)
        End Function
        
        '''<remarks/>
        Public Overloads Sub IsFieldIndexedAsync(ByVal caseContextArtifactID As Integer, ByVal fieldArtifactID As Integer)
            Me.IsFieldIndexedAsync(caseContextArtifactID, fieldArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub IsFieldIndexedAsync(ByVal caseContextArtifactID As Integer, ByVal fieldArtifactID As Integer, ByVal userState As Object)
            If (Me.IsFieldIndexedOperationCompleted Is Nothing) Then
                Me.IsFieldIndexedOperationCompleted = AddressOf Me.OnIsFieldIndexedOperationCompleted
            End If
            Me.InvokeAsync("IsFieldIndexed", New Object() {caseContextArtifactID, fieldArtifactID}, Me.IsFieldIndexedOperationCompleted, userState)
        End Sub
        
        Private Sub OnIsFieldIndexedOperationCompleted(ByVal arg As Object)
            If (Not (Me.IsFieldIndexedCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent IsFieldIndexedCompleted(Me, New IsFieldIndexedCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        Public Shadows Sub CancelAsync(ByVal userState As Object)
            MyBase.CancelAsync(userState)
        End Sub
        
        Private Function IsLocalFileSystemWebService(ByVal url As String) As Boolean
            If ((url Is Nothing)  _
                        OrElse (url Is String.Empty)) Then
                Return false
            End If
            Dim wsUri As System.Uri = New System.Uri(url)
            If ((wsUri.Port >= 1024)  _
                        AndAlso (String.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) = 0)) Then
                Return true
            End If
            Return false
        End Function
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub RetrieveDisplayFieldNameByFieldCategoryIDCompletedEventHandler(ByVal sender As Object, ByVal e As RetrieveDisplayFieldNameByFieldCategoryIDCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class RetrieveDisplayFieldNameByFieldCategoryIDCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As String
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),String)
            End Get
        End Property
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub RetrieveAllMappableCompletedEventHandler(ByVal sender As Object, ByVal e As RetrieveAllMappableCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class RetrieveAllMappableCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As System.Data.DataSet
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),System.Data.DataSet)
            End Get
        End Property
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub RetrieveAllCompletedEventHandler(ByVal sender As Object, ByVal e As RetrieveAllCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class RetrieveAllCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As System.Data.DataSet
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),System.Data.DataSet)
            End Get
        End Property
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub RetrievePotentialBeginBatesFieldsCompletedEventHandler(ByVal sender As Object, ByVal e As RetrievePotentialBeginBatesFieldsCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class RetrievePotentialBeginBatesFieldsCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As System.Data.DataSet
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),System.Data.DataSet)
            End Get
        End Property
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub IsFieldIndexedCompletedEventHandler(ByVal sender As Object, ByVal e As IsFieldIndexedCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class IsFieldIndexedCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As Boolean
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),Boolean)
            End Get
        End Property
    End Class
End Namespace
