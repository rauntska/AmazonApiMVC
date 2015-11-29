using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using UptimeJob2.Amazon;
using System.Text; 

namespace UptimeJob2.Models
{
    public class BusinessLogic
    {
        public static DataModels.SearchResult SearchItems(string Keyword, double Rate, string SearchIndex)
        {          
            #region init Model
            Models.DataModels.SearchResult Searchresults= new DataModels.SearchResult();
            List<Models.DataModels.SearchResult.SearchResultsList> SearchresultsList = new List<DataModels.SearchResult.SearchResultsList>();
            #endregion

            ItemSearchResponse resp = AmazonRequestByPage(Keyword, 1, SearchIndex);
            #region Check Response for errors
            //Check for null response
            if (resp == null)
                Searchresults.Error="Server Error - no response received!";
            if (Convert.ToInt32(resp.Items[0].TotalResults) == 0)
                Searchresults.Error="No matches found!";
            #endregion

            #region Set the pages to Loop Through according to amazon product advertising api
            ///If SearchIndex==All then Max Pages to query is 5
            ///else Max pages to loop through is 10
            int PagesToLoopTrhough=Convert.ToInt32(resp.Items[0].TotalPages.ToString());
            int ModifiedPagesToLoopThrough = 0;
            if (SearchIndex=="All")
            {
                if (PagesToLoopTrhough>5){ModifiedPagesToLoopThrough = 5;}
                else{ModifiedPagesToLoopThrough = PagesToLoopTrhough;}
            }
            else
            {
                if (PagesToLoopTrhough>10){ModifiedPagesToLoopThrough = 10;}
                else{ModifiedPagesToLoopThrough = PagesToLoopTrhough;}
            }
            #endregion
            #region Loop Through The Search Results And Add to Model
            for (int i = 1; i < ModifiedPagesToLoopThrough+1; i++)
            {
                resp = AmazonRequestByPage(Keyword, i, SearchIndex);

                    foreach (Item item in resp.Items[0].Item)
                    {
                        Models.DataModels.SearchResult.SearchResultsList SingleSearchResult = new Models.DataModels.SearchResult.SearchResultsList();
                        if (item.ItemAttributes.Author != null)
                            SingleSearchResult.Author = item.ItemAttributes.Author[0].ToString() ?? "empty";//not working ?
                       // else SingleSearchResult.Author = "EMPTY";

                        SingleSearchResult.Title = item.ItemAttributes.Title ?? "";
                        SingleSearchResult.ProductGroup = item.ItemAttributes.ProductGroup;
                        if (item.ItemAttributes.ListPrice != null)
                            SingleSearchResult.Price = Convert.ToString(Convert.ToDouble(item.ItemAttributes.ListPrice.FormattedPrice.TrimStart('£')) * Rate);
                        else SingleSearchResult.Price = null;

                        SearchresultsList.Add(SingleSearchResult);
                    }
              }
            
           
            #endregion

            Searchresults._SearchResultsList = SearchresultsList;        
            return Searchresults;
        }

        private static ItemSearchResponse AmazonRequestByPage(string Keyword, int page, string SearchIndex)
        {
            #region Set Amazon Search parameters
            ItemSearch search = new ItemSearch();
            search.AssociateTag = "XX";
            search.AWSAccessKeyId = "XX";

            ItemSearchRequest req = new ItemSearchRequest();

            req.ResponseGroup = new string[] { "Medium" };
            req.SearchIndex = SearchIndex;
            // req.Author = "Lansdale";
            req.ItemPage = Convert.ToString(page);
            req.Keywords = Keyword;

            req.Availability = ItemSearchRequestAvailability.Available;
            search.Request = new ItemSearchRequest[] { req };


            Amazon.AWSECommerceServicePortTypeClient amzwc = new Amazon.AWSECommerceServicePortTypeClient();
            amzwc.ChannelFactory.Endpoint.EndpointBehaviors.Add(new AmazonSigningEndpointBehavior("XX", "XX"));

            try
            {
                ItemSearchResponse resp = amzwc.ItemSearch(search);
                return resp;
            }
            catch (Exception ex)
            {               
                ItemSearchResponse resp = null;
                return resp;

            }
            
            #endregion
            
        }
    }
    public class AmazonSigningMessageInspector : IClientMessageInspector
    {
        private string accessKeyId = "XX";
        private string secretKey = "XXX";

        public AmazonSigningMessageInspector(string accessKeyId, string secretKey)
        {
            this.accessKeyId = accessKeyId;
            this.secretKey = secretKey;
        }

        public Object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            string operation = Regex.Match(request.Headers.Action, "[^/]+$").ToString();
            DateTime now = DateTime.UtcNow;
            String timestamp = now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            String signMe = operation + timestamp;
            Byte[] bytesToSign = Encoding.UTF8.GetBytes(signMe);

            Byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
            HMAC hmacSha256 = new HMACSHA256(secretKeyBytes);
            Byte[] hashBytes = hmacSha256.ComputeHash(bytesToSign);
            String signature = Convert.ToBase64String(hashBytes);

            request.Headers.Add(new AmazonHeader("AWSAccessKeyId", accessKeyId));
            request.Headers.Add(new AmazonHeader("Timestamp", timestamp));
            request.Headers.Add(new AmazonHeader("Signature", signature));
            return null;
        }

        void IClientMessageInspector.AfterReceiveReply(ref System.ServiceModel.Channels.Message Message, Object correlationState)
        {
        }
    }

    public class AmazonSigningEndpointBehavior : IEndpointBehavior
    {
        private string accessKeyId = "";
        private string secretKey = "";

        public AmazonSigningEndpointBehavior(string accessKeyId, string secretKey)
        {
            this.accessKeyId = accessKeyId;
            this.secretKey = secretKey;
        }

        public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new AmazonSigningMessageInspector(accessKeyId, secretKey));
        }

        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatched)
        {
        }

        public void Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        public void AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParemeters)
        {
        }
    }

    public class AmazonHeader : MessageHeader
    {
        private string m_name;
        private string value;

        public AmazonHeader(string name, string value)
        {
            this.m_name = name;
            this.value = value;
        }

        public override string Name
        {
            get { return m_name; }
        }
        public override string Namespace
        {
            get { return "http://security.amazonaws.com/doc/2007-01-01/"; }
        }
        protected override void OnWriteHeaderContents(System.Xml.XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(value);
        }
    }
}