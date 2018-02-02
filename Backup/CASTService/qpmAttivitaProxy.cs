﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.586
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

// 
// This source code was auto-generated by wsdl, Version=4.0.30319.1.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Web.Services.WebServiceBindingAttribute(Name="qpmAttivitaSOAP", Namespace="http://QPM_WORKFLOW/qpmAttivita/")]
public partial class qpmAttivita : System.Web.Services.Protocols.SoapHttpClientProtocol {
    
    private System.Threading.SendOrPostCallback chiudiAttivitaOperationCompleted;
    
    private System.Threading.SendOrPostCallback registraSottoAttivitaOperationCompleted;
    
    /// <remarks/>
    public qpmAttivita() {
        this.Url = "http://localhost/QPM";
    }
    
    /// <remarks/>
    public event chiudiAttivitaCompletedEventHandler chiudiAttivitaCompleted;
    
    /// <remarks/>
    public event registraSottoAttivitaCompletedEventHandler registraSottoAttivitaCompleted;
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://QPM_WORKFLOW/qpmAttivita/chiudiAttivita", RequestNamespace="http://QPM_WORKFLOW/qpmAttivita/", ResponseNamespace="http://QPM_WORKFLOW/qpmAttivita/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    [return: System.Xml.Serialization.XmlElementAttribute("out", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public qpmResult chiudiAttivita([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string idRichiesta, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string idAttivita, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string applicazione, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string timeStamp, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string codiceEsito, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string messaggioEsito, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string azione, [System.Xml.Serialization.XmlElementAttribute("listaDeliverables", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] qpm_fileResult[] listaDeliverables) {
        object[] results = this.Invoke("chiudiAttivita", new object[] {
                    idRichiesta,
                    idAttivita,
                    applicazione,
                    timeStamp,
                    codiceEsito,
                    messaggioEsito,
                    azione,
                    listaDeliverables});
        return ((qpmResult)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginchiudiAttivita(string idRichiesta, string idAttivita, string applicazione, string timeStamp, string codiceEsito, string messaggioEsito, string azione, qpm_fileResult[] listaDeliverables, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("chiudiAttivita", new object[] {
                    idRichiesta,
                    idAttivita,
                    applicazione,
                    timeStamp,
                    codiceEsito,
                    messaggioEsito,
                    azione,
                    listaDeliverables}, callback, asyncState);
    }
    
    /// <remarks/>
    public qpmResult EndchiudiAttivita(System.IAsyncResult asyncResult) {
        object[] results = this.EndInvoke(asyncResult);
        return ((qpmResult)(results[0]));
    }
    
    /// <remarks/>
    public void chiudiAttivitaAsync(string idRichiesta, string idAttivita, string applicazione, string timeStamp, string codiceEsito, string messaggioEsito, string azione, qpm_fileResult[] listaDeliverables) {
        this.chiudiAttivitaAsync(idRichiesta, idAttivita, applicazione, timeStamp, codiceEsito, messaggioEsito, azione, listaDeliverables, null);
    }
    
    /// <remarks/>
    public void chiudiAttivitaAsync(string idRichiesta, string idAttivita, string applicazione, string timeStamp, string codiceEsito, string messaggioEsito, string azione, qpm_fileResult[] listaDeliverables, object userState) {
        if ((this.chiudiAttivitaOperationCompleted == null)) {
            this.chiudiAttivitaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnchiudiAttivitaOperationCompleted);
        }
        this.InvokeAsync("chiudiAttivita", new object[] {
                    idRichiesta,
                    idAttivita,
                    applicazione,
                    timeStamp,
                    codiceEsito,
                    messaggioEsito,
                    azione,
                    listaDeliverables}, this.chiudiAttivitaOperationCompleted, userState);
    }
    
    private void OnchiudiAttivitaOperationCompleted(object arg) {
        if ((this.chiudiAttivitaCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.chiudiAttivitaCompleted(this, new chiudiAttivitaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://QPM_WORKFLOW/qpmAttivita/registraSottoAttivita", RequestNamespace="http://QPM_WORKFLOW/qpmAttivita/", ResponseNamespace="http://QPM_WORKFLOW/qpmAttivita/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
    [return: System.Xml.Serialization.XmlElementAttribute("codSottoAttivita", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string registraSottoAttivita([System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string idRichiesta, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string idAttivita, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string applicazione, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string sottoattivita, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string descrizione, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string codEsito, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string descEsito, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string tipoNotifica, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] string timeStamp, [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)] out qpmResult @out) {
        object[] results = this.Invoke("registraSottoAttivita", new object[] {
                    idRichiesta,
                    idAttivita,
                    applicazione,
                    sottoattivita,
                    descrizione,
                    codEsito,
                    descEsito,
                    tipoNotifica,
                    timeStamp});
        @out = ((qpmResult)(results[1]));
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public System.IAsyncResult BeginregistraSottoAttivita(string idRichiesta, string idAttivita, string applicazione, string sottoattivita, string descrizione, string codEsito, string descEsito, string tipoNotifica, string timeStamp, System.AsyncCallback callback, object asyncState) {
        return this.BeginInvoke("registraSottoAttivita", new object[] {
                    idRichiesta,
                    idAttivita,
                    applicazione,
                    sottoattivita,
                    descrizione,
                    codEsito,
                    descEsito,
                    tipoNotifica,
                    timeStamp}, callback, asyncState);
    }
    
    /// <remarks/>
    public string EndregistraSottoAttivita(System.IAsyncResult asyncResult, out qpmResult @out) {
        object[] results = this.EndInvoke(asyncResult);
        @out = ((qpmResult)(results[1]));
        return ((string)(results[0]));
    }
    
    /// <remarks/>
    public void registraSottoAttivitaAsync(string idRichiesta, string idAttivita, string applicazione, string sottoattivita, string descrizione, string codEsito, string descEsito, string tipoNotifica, string timeStamp) {
        this.registraSottoAttivitaAsync(idRichiesta, idAttivita, applicazione, sottoattivita, descrizione, codEsito, descEsito, tipoNotifica, timeStamp, null);
    }
    
    /// <remarks/>
    public void registraSottoAttivitaAsync(string idRichiesta, string idAttivita, string applicazione, string sottoattivita, string descrizione, string codEsito, string descEsito, string tipoNotifica, string timeStamp, object userState) {
        if ((this.registraSottoAttivitaOperationCompleted == null)) {
            this.registraSottoAttivitaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnregistraSottoAttivitaOperationCompleted);
        }
        this.InvokeAsync("registraSottoAttivita", new object[] {
                    idRichiesta,
                    idAttivita,
                    applicazione,
                    sottoattivita,
                    descrizione,
                    codEsito,
                    descEsito,
                    tipoNotifica,
                    timeStamp}, this.registraSottoAttivitaOperationCompleted, userState);
    }
    
    private void OnregistraSottoAttivitaOperationCompleted(object arg) {
        if ((this.registraSottoAttivitaCompleted != null)) {
            System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
            this.registraSottoAttivitaCompleted(this, new registraSottoAttivitaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
        }
    }
    
    /// <remarks/>
    public new void CancelAsync(object userState) {
        base.CancelAsync(userState);
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://QPM_WORKFLOW/qpmAttivita/")]
public partial class qpm_fileResult {
    
    private string pathFileField;
    
    private bool allegareField;
    
    private string tipoField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string pathFile {
        get {
            return this.pathFileField;
        }
        set {
            this.pathFileField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public bool allegare {
        get {
            return this.allegareField;
        }
        set {
            this.allegareField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string tipo {
        get {
            return this.tipoField;
        }
        set {
            this.tipoField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://QPM_WORKFLOW/qpmAttivita/")]
public partial class qpmResult {
    
    private string codResultField;
    
    private string descrResultField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string codResult {
        get {
            return this.codResultField;
        }
        set {
            this.codResultField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string descrResult {
        get {
            return this.descrResultField;
        }
        set {
            this.descrResultField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
public delegate void chiudiAttivitaCompletedEventHandler(object sender, chiudiAttivitaCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class chiudiAttivitaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal chiudiAttivitaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public qpmResult Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((qpmResult)(this.results[0]));
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
public delegate void registraSottoAttivitaCompletedEventHandler(object sender, registraSottoAttivitaCompletedEventArgs e);

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "4.0.30319.1")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
public partial class registraSottoAttivitaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
    
    private object[] results;
    
    internal registraSottoAttivitaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
            base(exception, cancelled, userState) {
        this.results = results;
    }
    
    /// <remarks/>
    public string Result {
        get {
            this.RaiseExceptionIfNecessary();
            return ((string)(this.results[0]));
        }
    }
    
    /// <remarks/>
    public qpmResult @out {
        get {
            this.RaiseExceptionIfNecessary();
            return ((qpmResult)(this.results[1]));
        }
    }
}
