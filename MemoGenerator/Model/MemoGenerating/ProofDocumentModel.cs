using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace MemoGenerator.Model.MemoGenerating
{
    enum DeliveryRoute
    {
        email,
        registeredMail, // 등기 우편
    }

    enum PreDefinedDocument
    {
        certificateOfInsurancePayment, // 4대보험완납증명서
        receiptOfCard, // 카드 영수증
    }

    static class PreDefinedDocumentExtensions
    {
        internal static string name(this PreDefinedDocument document)
        {
            switch (document)
            {
                case PreDefinedDocument.certificateOfInsurancePayment:
                    return "4대보험완납증명서";
                case PreDefinedDocument.receiptOfCard:
                    return "카드영수증";
                default:
                    return ""; // 바인딩 과정에서 -1값이 들어오는 현상이 있음
            }
        }
    }

    sealed class ProofDocumentModel : BaseINotifyPropertyChanged
    {
        private Dictionary<DeliveryRoute, bool> deliveryRouteSelections;
        private string? emailAddress;
        private string? detailedDocumentList;
        private Dictionary<PreDefinedDocument, bool> preDefinedDocumentSelections;
        internal string memoComponent
        {
            get
            {
                List<String> elements = new List<string>();

                string documentList = "서류";
                if (!String.IsNullOrEmpty(detailedDocumentList))
                {
                    documentList = detailedDocumentList;
                }
                foreach (var key in preDefinedDocumentSelections.Keys)
                {
                    if (preDefinedDocumentSelections[key] == true)
                    {
                        documentList = key.name();
                        break;
                    }
                }

                bool deliversDocument = false;
                if (deliveryRouteSelections[DeliveryRoute.email] == true && deliveryRouteSelections[DeliveryRoute.registeredMail] == true)
                {
                    elements.Add($"{documentList} 메일/등기");
                    deliversDocument = true;
                }
                else if (deliveryRouteSelections[DeliveryRoute.email] == true)
                {
                    elements.Add($"{documentList} 메일");
                    deliversDocument = true;
                }
                else if (deliveryRouteSelections[DeliveryRoute.registeredMail] == true)
                {
                    elements.Add($"{documentList} 등기");
                    deliversDocument = true;
                }

                if (deliversDocument)
                {
                    elements.Add("발송");
                    if (emailAddress is string) elements.Add(emailAddress);
                }

                return String.Join(" ", elements);
            }
        }

        internal ProofDocumentModel()
        {
            this.deliveryRouteSelections = new Dictionary<DeliveryRoute, bool>();
            deliveryRouteSelections.Add(DeliveryRoute.email, false);
            deliveryRouteSelections.Add(DeliveryRoute.registeredMail, false);
            this.preDefinedDocumentSelections = new Dictionary<PreDefinedDocument, bool>();
            disableAllPreDefinedDocumentSelections();
        }

        // Binding

        public bool DeliversByEmail
        {
            get => deliveryRouteSelections[DeliveryRoute.email];
            set
            {
                deliveryRouteSelections[DeliveryRoute.email] = value;
                propertyChanged("DeliversByEmail");
            }
        }

        public bool DeliversByRegisteredMail
        {
            get => deliveryRouteSelections[DeliveryRoute.registeredMail];
            set
            {
                deliveryRouteSelections[DeliveryRoute.registeredMail] = value;
            }
        }

        public string EmailAddress
        {
            get => emailAddress ?? "";
            set
            {
                if (String.IsNullOrEmpty(value)) emailAddress = null;
                else emailAddress = value;
            }
        }

        public string DetailedDocumentList
        {
            get => detailedDocumentList ?? "";
            set
            {
                disableAllPreDefinedDocumentSelections();
                if (String.IsNullOrEmpty(value)) detailedDocumentList = null;
                else detailedDocumentList = value;

                propertyChanged("ChecksCertificateOfInsurancePayment");
                propertyChanged("ChecksReceiptOfCard");
            }
        }

        public bool ChecksCertificateOfInsurancePayment
        {
            get => preDefinedDocumentSelections[PreDefinedDocument.certificateOfInsurancePayment];
            set
            {
                disableAllPreDefinedDocumentSelections();
                preDefinedDocumentSelections[PreDefinedDocument.certificateOfInsurancePayment] = value;
                detailedDocumentList = null;

                propertyChanged("DetailedDocumentList");
                propertyChanged("ChecksReceiptOfCard");
            }
        }

        public bool ChecksReceiptOfCard
        {
            get => preDefinedDocumentSelections[PreDefinedDocument.receiptOfCard];
            set
            {
                disableAllPreDefinedDocumentSelections();
                preDefinedDocumentSelections[PreDefinedDocument.receiptOfCard] = value;
                detailedDocumentList = null;

                propertyChanged("DetailedDocumentList");
                propertyChanged("ChecksCertificateOfInsurancePayment");
            }
        }

        private void disableAllPreDefinedDocumentSelections()
        {
            PreDefinedDocument[] allCase = Enum.GetValues<PreDefinedDocument>();
            foreach (var key in allCase)
            {
                preDefinedDocumentSelections[key] = false;
            }
        }
    }
}

#nullable disable