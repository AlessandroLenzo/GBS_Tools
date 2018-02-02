using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace CAST
{
    [ServiceContract(Namespace = "http://CAST/Service")]
    public interface ICASTService
    {

        [OperationContract]
        CASTResponseType Call(CASTRequestType request);

    }


    [DataContract]
    public class CASTRequestType
    {

        // Id Richiesta
        [DataMember]
        public string IDR
        {
            get ;
            set ;
        }

        // Id Attività
        [DataMember]
        public string IDA
        {
            get;
            set;
        }

        // Tipo attività (QCH / COF )
        [DataMember]
        public string TAT
        {
            get;
            set;
        }

        // Ambiente [Consolidamento CON, Produzione PRO,...]
        [DataMember]
        public string AMB
        {
            get;
            set;
        }

        // ID Applicazione [riferimento a DB CPF- tabella: Anagrafica Applicazione]
        [DataMember]
        public string APP
        {
            get;
            set;
        }

        // Versione applicazione
        [DataMember]
        public string VER
        {
            get;
            set;
        }

        // Data Snapshot
        [DataMember]
        public string CDT
        {
            get;
            set;
        }

        // Concatenazione APP|CDT
        [DataMember]
        public string SNN
        {
            get;
            set;
        }

        // Changeset
        [DataMember]
        public string CHS
        {
            get;
            set;
        }

        // Linea di Sviluppo [1, 2, 3, … n]
        [DataMember]
        public string LSV
        {
            get;
            set;
        }

        // Modalità Attività Cast (Incrementale/Full)
        [DataMember]
        public string MAC
        {
            get;
            set;
        }

        // Modalità di confronto [con baseline(delta), senza baseline]
        [DataMember]
        public string MAT
        {
            get;
            set;
        }

        // Versione Baseline di riferimento
        [DataMember]
        public string VBS
        {
            get;
            set;
        }

        // Vincolo su Processo	(Vincolante o Non Vincolante)
        [DataMember]
        public string VPR
        {
            get;
            set;
        }

    }


    [DataContract]
    public class CASTResponseType
    {
 
        [DataMember]
        public string CODICE
        {
            get;
            set;
        }

        [DataMember]
        public string MESSAGGIO
        {
            get;
            set;
        }
    }
}
