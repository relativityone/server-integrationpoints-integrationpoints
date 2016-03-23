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
Namespace kCura.EDDS.WebAPI.ProductionManagerBase
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code"),  _
     System.Web.Services.WebServiceBindingAttribute(Name:="ProductionManagerSoap", [Namespace]:="http://www.kCura.com/EDDS/ProductionManager")>  _
    Partial Public Class ProductionManager
        Inherits System.Web.Services.Protocols.SoapHttpClientProtocol
        
        Private RetrieveProducedByContextArtifactIDOperationCompleted As System.Threading.SendOrPostCallback
        
        Private RetrieveImportEligibleByContextArtifactIDOperationCompleted As System.Threading.SendOrPostCallback
        
        Private DoPostImportProcessingOperationCompleted As System.Threading.SendOrPostCallback
        
        Private DoPreImportProcessingOperationCompleted As System.Threading.SendOrPostCallback
        
        Private ReadOperationCompleted As System.Threading.SendOrPostCallback
        
        Private RetrieveProducedWithSecurityOperationCompleted As System.Threading.SendOrPostCallback
        
        Private MigrationJobExistsOperationCompleted As System.Threading.SendOrPostCallback
        
        Private useDefaultCredentialsSetExplicitly As Boolean
        
        '''<remarks/>
        Public Sub New()
            MyBase.New
            Me.Url = "http://localhost/RelativityWebApi/ProductionManager.asmx"
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
        Public Event RetrieveProducedByContextArtifactIDCompleted As RetrieveProducedByContextArtifactIDCompletedEventHandler
        
        '''<remarks/>
        Public Event RetrieveImportEligibleByContextArtifactIDCompleted As RetrieveImportEligibleByContextArtifactIDCompletedEventHandler
        
        '''<remarks/>
        Public Event DoPostImportProcessingCompleted As DoPostImportProcessingCompletedEventHandler
        
        '''<remarks/>
        Public Event DoPreImportProcessingCompleted As DoPreImportProcessingCompletedEventHandler
        
        '''<remarks/>
        Public Event ReadCompleted As ReadCompletedEventHandler
        
        '''<remarks/>
        Public Event RetrieveProducedWithSecurityCompleted As RetrieveProducedWithSecurityCompletedEventHandler
        
        '''<remarks/>
        Public Event MigrationJobExistsCompleted As MigrationJobExistsCompletedEventHandler
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ProductionManager/RetrieveProducedByContextArtifactID", RequestNamespace:="http://www.kCura.com/EDDS/ProductionManager", ResponseNamespace:="http://www.kCura.com/EDDS/ProductionManager", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function RetrieveProducedByContextArtifactID(ByVal caseContextArtifactID As Integer) As System.Data.DataSet
            Dim results() As Object = Me.Invoke("RetrieveProducedByContextArtifactID", New Object() {caseContextArtifactID})
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Function BeginRetrieveProducedByContextArtifactID(ByVal caseContextArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("RetrieveProducedByContextArtifactID", New Object() {caseContextArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRetrieveProducedByContextArtifactID(ByVal asyncResult As System.IAsyncResult) As System.Data.DataSet
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Overloads Sub RetrieveProducedByContextArtifactIDAsync(ByVal caseContextArtifactID As Integer)
            Me.RetrieveProducedByContextArtifactIDAsync(caseContextArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub RetrieveProducedByContextArtifactIDAsync(ByVal caseContextArtifactID As Integer, ByVal userState As Object)
            If (Me.RetrieveProducedByContextArtifactIDOperationCompleted Is Nothing) Then
                Me.RetrieveProducedByContextArtifactIDOperationCompleted = AddressOf Me.OnRetrieveProducedByContextArtifactIDOperationCompleted
            End If
            Me.InvokeAsync("RetrieveProducedByContextArtifactID", New Object() {caseContextArtifactID}, Me.RetrieveProducedByContextArtifactIDOperationCompleted, userState)
        End Sub
        
        Private Sub OnRetrieveProducedByContextArtifactIDOperationCompleted(ByVal arg As Object)
            If (Not (Me.RetrieveProducedByContextArtifactIDCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent RetrieveProducedByContextArtifactIDCompleted(Me, New RetrieveProducedByContextArtifactIDCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ProductionManager/RetrieveImportEligibleByContextArtifa"& _ 
            "ctID", RequestNamespace:="http://www.kCura.com/EDDS/ProductionManager", ResponseNamespace:="http://www.kCura.com/EDDS/ProductionManager", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function RetrieveImportEligibleByContextArtifactID(ByVal caseContextArtifactID As Integer) As System.Data.DataSet
            Dim results() As Object = Me.Invoke("RetrieveImportEligibleByContextArtifactID", New Object() {caseContextArtifactID})
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Function BeginRetrieveImportEligibleByContextArtifactID(ByVal caseContextArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("RetrieveImportEligibleByContextArtifactID", New Object() {caseContextArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRetrieveImportEligibleByContextArtifactID(ByVal asyncResult As System.IAsyncResult) As System.Data.DataSet
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Overloads Sub RetrieveImportEligibleByContextArtifactIDAsync(ByVal caseContextArtifactID As Integer)
            Me.RetrieveImportEligibleByContextArtifactIDAsync(caseContextArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub RetrieveImportEligibleByContextArtifactIDAsync(ByVal caseContextArtifactID As Integer, ByVal userState As Object)
            If (Me.RetrieveImportEligibleByContextArtifactIDOperationCompleted Is Nothing) Then
                Me.RetrieveImportEligibleByContextArtifactIDOperationCompleted = AddressOf Me.OnRetrieveImportEligibleByContextArtifactIDOperationCompleted
            End If
            Me.InvokeAsync("RetrieveImportEligibleByContextArtifactID", New Object() {caseContextArtifactID}, Me.RetrieveImportEligibleByContextArtifactIDOperationCompleted, userState)
        End Sub
        
        Private Sub OnRetrieveImportEligibleByContextArtifactIDOperationCompleted(ByVal arg As Object)
            If (Not (Me.RetrieveImportEligibleByContextArtifactIDCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent RetrieveImportEligibleByContextArtifactIDCompleted(Me, New RetrieveImportEligibleByContextArtifactIDCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ProductionManager/DoPostImportProcessing", RequestNamespace:="http://www.kCura.com/EDDS/ProductionManager", ResponseNamespace:="http://www.kCura.com/EDDS/ProductionManager", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Sub DoPostImportProcessing(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer)
            Me.Invoke("DoPostImportProcessing", New Object() {caseContextArtifactID, productionArtifactID})
        End Sub
        
        '''<remarks/>
        Public Function BeginDoPostImportProcessing(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("DoPostImportProcessing", New Object() {caseContextArtifactID, productionArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Sub EndDoPostImportProcessing(ByVal asyncResult As System.IAsyncResult)
            Me.EndInvoke(asyncResult)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub DoPostImportProcessingAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer)
            Me.DoPostImportProcessingAsync(caseContextArtifactID, productionArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub DoPostImportProcessingAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal userState As Object)
            If (Me.DoPostImportProcessingOperationCompleted Is Nothing) Then
                Me.DoPostImportProcessingOperationCompleted = AddressOf Me.OnDoPostImportProcessingOperationCompleted
            End If
            Me.InvokeAsync("DoPostImportProcessing", New Object() {caseContextArtifactID, productionArtifactID}, Me.DoPostImportProcessingOperationCompleted, userState)
        End Sub
        
        Private Sub OnDoPostImportProcessingOperationCompleted(ByVal arg As Object)
            If (Not (Me.DoPostImportProcessingCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent DoPostImportProcessingCompleted(Me, New System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ProductionManager/DoPreImportProcessing", RequestNamespace:="http://www.kCura.com/EDDS/ProductionManager", ResponseNamespace:="http://www.kCura.com/EDDS/ProductionManager", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Sub DoPreImportProcessing(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer)
            Me.Invoke("DoPreImportProcessing", New Object() {caseContextArtifactID, productionArtifactID})
        End Sub
        
        '''<remarks/>
        Public Function BeginDoPreImportProcessing(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("DoPreImportProcessing", New Object() {caseContextArtifactID, productionArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Sub EndDoPreImportProcessing(ByVal asyncResult As System.IAsyncResult)
            Me.EndInvoke(asyncResult)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub DoPreImportProcessingAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer)
            Me.DoPreImportProcessingAsync(caseContextArtifactID, productionArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub DoPreImportProcessingAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal userState As Object)
            If (Me.DoPreImportProcessingOperationCompleted Is Nothing) Then
                Me.DoPreImportProcessingOperationCompleted = AddressOf Me.OnDoPreImportProcessingOperationCompleted
            End If
            Me.InvokeAsync("DoPreImportProcessing", New Object() {caseContextArtifactID, productionArtifactID}, Me.DoPreImportProcessingOperationCompleted, userState)
        End Sub
        
        Private Sub OnDoPreImportProcessingOperationCompleted(ByVal arg As Object)
            If (Not (Me.DoPreImportProcessingCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent DoPreImportProcessingCompleted(Me, New System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ProductionManager/Read", RequestNamespace:="http://www.kCura.com/EDDS/ProductionManager", ResponseNamespace:="http://www.kCura.com/EDDS/ProductionManager", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function Read(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer) As ProductionInfo
            Dim results() As Object = Me.Invoke("Read", New Object() {caseContextArtifactID, productionArtifactID})
            Return CType(results(0),ProductionInfo)
        End Function
        
        '''<remarks/>
        Public Function BeginRead(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("Read", New Object() {caseContextArtifactID, productionArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRead(ByVal asyncResult As System.IAsyncResult) As ProductionInfo
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),ProductionInfo)
        End Function
        
        '''<remarks/>
        Public Overloads Sub ReadAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer)
            Me.ReadAsync(caseContextArtifactID, productionArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub ReadAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal userState As Object)
            If (Me.ReadOperationCompleted Is Nothing) Then
                Me.ReadOperationCompleted = AddressOf Me.OnReadOperationCompleted
            End If
            Me.InvokeAsync("Read", New Object() {caseContextArtifactID, productionArtifactID}, Me.ReadOperationCompleted, userState)
        End Sub
        
        Private Sub OnReadOperationCompleted(ByVal arg As Object)
            If (Not (Me.ReadCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent ReadCompleted(Me, New ReadCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ProductionManager/RetrieveProducedWithSecurity", RequestNamespace:="http://www.kCura.com/EDDS/ProductionManager", ResponseNamespace:="http://www.kCura.com/EDDS/ProductionManager", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function RetrieveProducedWithSecurity(ByVal caseContextArtifactID As Integer) As System.Data.DataSet
            Dim results() As Object = Me.Invoke("RetrieveProducedWithSecurity", New Object() {caseContextArtifactID})
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Function BeginRetrieveProducedWithSecurity(ByVal caseContextArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("RetrieveProducedWithSecurity", New Object() {caseContextArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndRetrieveProducedWithSecurity(ByVal asyncResult As System.IAsyncResult) As System.Data.DataSet
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),System.Data.DataSet)
        End Function
        
        '''<remarks/>
        Public Overloads Sub RetrieveProducedWithSecurityAsync(ByVal caseContextArtifactID As Integer)
            Me.RetrieveProducedWithSecurityAsync(caseContextArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub RetrieveProducedWithSecurityAsync(ByVal caseContextArtifactID As Integer, ByVal userState As Object)
            If (Me.RetrieveProducedWithSecurityOperationCompleted Is Nothing) Then
                Me.RetrieveProducedWithSecurityOperationCompleted = AddressOf Me.OnRetrieveProducedWithSecurityOperationCompleted
            End If
            Me.InvokeAsync("RetrieveProducedWithSecurity", New Object() {caseContextArtifactID}, Me.RetrieveProducedWithSecurityOperationCompleted, userState)
        End Sub
        
        Private Sub OnRetrieveProducedWithSecurityOperationCompleted(ByVal arg As Object)
            If (Not (Me.RetrieveProducedWithSecurityCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent RetrieveProducedWithSecurityCompleted(Me, New RetrieveProducedWithSecurityCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
            End If
        End Sub
        
        '''<remarks/>
        <System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ProductionManager/MigrationJobExists", RequestNamespace:="http://www.kCura.com/EDDS/ProductionManager", ResponseNamespace:="http://www.kCura.com/EDDS/ProductionManager", Use:=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle:=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)>  _
        Public Function MigrationJobExists(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer) As Boolean
            Dim results() As Object = Me.Invoke("MigrationJobExists", New Object() {caseContextArtifactID, productionArtifactID})
            Return CType(results(0),Boolean)
        End Function
        
        '''<remarks/>
        Public Function BeginMigrationJobExists(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal callback As System.AsyncCallback, ByVal asyncState As Object) As System.IAsyncResult
            Return Me.BeginInvoke("MigrationJobExists", New Object() {caseContextArtifactID, productionArtifactID}, callback, asyncState)
        End Function
        
        '''<remarks/>
        Public Function EndMigrationJobExists(ByVal asyncResult As System.IAsyncResult) As Boolean
            Dim results() As Object = Me.EndInvoke(asyncResult)
            Return CType(results(0),Boolean)
        End Function
        
        '''<remarks/>
        Public Overloads Sub MigrationJobExistsAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer)
            Me.MigrationJobExistsAsync(caseContextArtifactID, productionArtifactID, Nothing)
        End Sub
        
        '''<remarks/>
        Public Overloads Sub MigrationJobExistsAsync(ByVal caseContextArtifactID As Integer, ByVal productionArtifactID As Integer, ByVal userState As Object)
            If (Me.MigrationJobExistsOperationCompleted Is Nothing) Then
                Me.MigrationJobExistsOperationCompleted = AddressOf Me.OnMigrationJobExistsOperationCompleted
            End If
            Me.InvokeAsync("MigrationJobExists", New Object() {caseContextArtifactID, productionArtifactID}, Me.MigrationJobExistsOperationCompleted, userState)
        End Sub
        
        Private Sub OnMigrationJobExistsOperationCompleted(ByVal arg As Object)
            If (Not (Me.MigrationJobExistsCompletedEvent) Is Nothing) Then
                Dim invokeArgs As System.Web.Services.Protocols.InvokeCompletedEventArgs = CType(arg,System.Web.Services.Protocols.InvokeCompletedEventArgs)
                RaiseEvent MigrationJobExistsCompleted(Me, New MigrationJobExistsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState))
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
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1055.0"),  _
     System.SerializableAttribute(),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code"),  _
     System.Xml.Serialization.XmlTypeAttribute([Namespace]:="http://www.kCura.com/EDDS/ProductionManager")>  _
    Partial Public Class ProductionInfo
        
        Private batesNumberingField As Boolean
        
        Private beginBatesReflectedFieldIdField As Integer
        
        Private documentsHaveRedactionsField As Boolean
        
        Private includeImageLevelNumberingForDocumentLevelNumberingField As Boolean
        
        Private nameField As String
        
        Private useDocumentLevelNumberingField As Boolean
        
        '''<remarks/>
        Public Property BatesNumbering() As Boolean
            Get
                Return Me.batesNumberingField
            End Get
            Set
                Me.batesNumberingField = value
            End Set
        End Property
        
        '''<remarks/>
        Public Property BeginBatesReflectedFieldId() As Integer
            Get
                Return Me.beginBatesReflectedFieldIdField
            End Get
            Set
                Me.beginBatesReflectedFieldIdField = value
            End Set
        End Property
        
        '''<remarks/>
        Public Property DocumentsHaveRedactions() As Boolean
            Get
                Return Me.documentsHaveRedactionsField
            End Get
            Set
                Me.documentsHaveRedactionsField = value
            End Set
        End Property
        
        '''<remarks/>
        Public Property IncludeImageLevelNumberingForDocumentLevelNumbering() As Boolean
            Get
                Return Me.includeImageLevelNumberingForDocumentLevelNumberingField
            End Get
            Set
                Me.includeImageLevelNumberingForDocumentLevelNumberingField = value
            End Set
        End Property
        
        '''<remarks/>
        Public Property Name() As String
            Get
                Return Me.nameField
            End Get
            Set
                Me.nameField = value
            End Set
        End Property
        
        '''<remarks/>
        Public Property UseDocumentLevelNumbering() As Boolean
            Get
                Return Me.useDocumentLevelNumberingField
            End Get
            Set
                Me.useDocumentLevelNumberingField = value
            End Set
        End Property
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub RetrieveProducedByContextArtifactIDCompletedEventHandler(ByVal sender As Object, ByVal e As RetrieveProducedByContextArtifactIDCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class RetrieveProducedByContextArtifactIDCompletedEventArgs
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
    Public Delegate Sub RetrieveImportEligibleByContextArtifactIDCompletedEventHandler(ByVal sender As Object, ByVal e As RetrieveImportEligibleByContextArtifactIDCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class RetrieveImportEligibleByContextArtifactIDCompletedEventArgs
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
    Public Delegate Sub DoPostImportProcessingCompletedEventHandler(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub DoPreImportProcessingCompletedEventHandler(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub ReadCompletedEventHandler(ByVal sender As Object, ByVal e As ReadCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class ReadCompletedEventArgs
        Inherits System.ComponentModel.AsyncCompletedEventArgs
        
        Private results() As Object
        
        Friend Sub New(ByVal results() As Object, ByVal exception As System.Exception, ByVal cancelled As Boolean, ByVal userState As Object)
            MyBase.New(exception, cancelled, userState)
            Me.results = results
        End Sub
        
        '''<remarks/>
        Public ReadOnly Property Result() As ProductionInfo
            Get
                Me.RaiseExceptionIfNecessary
                Return CType(Me.results(0),ProductionInfo)
            End Get
        End Property
    End Class
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")>  _
    Public Delegate Sub RetrieveProducedWithSecurityCompletedEventHandler(ByVal sender As Object, ByVal e As RetrieveProducedWithSecurityCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class RetrieveProducedWithSecurityCompletedEventArgs
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
    Public Delegate Sub MigrationJobExistsCompletedEventHandler(ByVal sender As Object, ByVal e As MigrationJobExistsCompletedEventArgs)
    
    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"),  _
     System.Diagnostics.DebuggerStepThroughAttribute(),  _
     System.ComponentModel.DesignerCategoryAttribute("code")>  _
    Partial Public Class MigrationJobExistsCompletedEventArgs
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
