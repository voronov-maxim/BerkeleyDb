using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.OData.Formatter.Deserialization;
using System.Xml;

namespace TestBerkeleyDbWebApiClient
{
    public sealed class ODataJsonParser
    {
        private abstract class ODataResult
        {
            private readonly ODataReader _odataReader;

            public ODataResult(ODataReader odataReader)
            {
                _odataReader = odataReader;
            }

            public abstract void AddResult(Object result);

            public ODataReader ODataReader
            {
                get
                {
                    return _odataReader;
                }
            }
            public abstract Object Result
            {
                get;
            }
        }

        private sealed class FeedODataResult : ODataResult
        {
            private readonly List<Object> _result;

            public FeedODataResult(ODataReader odataReader)
                : base(odataReader)
            {
                _result = new List<Object>();
            }

            public override void AddResult(Object result)
            {
                _result.Add(result);
            }

            public override Object Result
            {
                get
                {
                    return _result;
                }
            }
        }

        private sealed class EntryODataResult : ODataResult
        {
            private Object _result;

            public EntryODataResult(ODataReader odataReader)
                : base(odataReader)
            {
            }

            public override void AddResult(Object result)
            {
                _result = result;
            }

            public override Object Result
            {
                get
                {
                    return _result;
                }
            }
        }

        private sealed class ODataResponseMessage : IODataResponseMessage
        {
            private readonly Stream _stream;

            public ODataResponseMessage(Stream stream)
            {
                _stream = stream;
            }

            public String GetHeader(String headerName)
            {
                return null;
            }
            public Stream GetStream()
            {
                return _stream;
            }
            public void SetHeader(String headerName, String headerValue)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<KeyValuePair<String, String>> Headers
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public int StatusCode
            {
                get;
                set;
            }
        }

        private readonly ODataEntityDeserializer _deserializer;
        private readonly IEdmModel _model;
        private readonly ODataMessageReaderSettings _settings;

        public ODataJsonParser(Uri serviceRootUri, String relativeUriMetadata)
            : this(serviceRootUri, GetEdmModel(new Uri(serviceRootUri, relativeUriMetadata)))
        {
        }
        public ODataJsonParser(Uri serviceRootUri, IEdmModel model)
        {
            _model = model;
            _settings = new ODataMessageReaderSettings() { BaseUri = serviceRootUri };

            var provider = new DefaultODataDeserializerProvider();
            _deserializer = new ODataEntityDeserializer(provider);
        }

        private static ODataResult GetODataResult(ODataMessageReader messageReader)
        {
            List<ODataPayloadKindDetectionResult> payloadKinds = messageReader.DetectPayloadKind().ToList();
            if (payloadKinds.Count > 0)
            {
                switch (payloadKinds[0].PayloadKind)
                {
                    case ODataPayloadKind.Feed:
                        return new FeedODataResult(messageReader.CreateODataFeedReader());
                    case ODataPayloadKind.Entry:
                        return new FeedODataResult(messageReader.CreateODataEntryReader());
                }
                throw new InvalidOperationException("unsupported payload kind " + payloadKinds[0].PayloadKind);
            }
            throw new InvalidOperationException("unknown payload kind");
        }
        private static IEdmModel GetEdmModel(Uri serviceRootUri)
        {
            HttpWebRequest request = HttpWebRequest.CreateHttp(serviceRootUri);
            using (WebResponse respones = request.GetResponse())
            using (Stream stream = respones.GetResponseStream())
            using (XmlReader reader = XmlReader.Create(stream))
                return Microsoft.OData.Edm.Csdl.EdmxReader.Parse(reader);
        }
        public Object ParseAsync(HttpResponseMessage response)
        {
            using (Stream stream = response.Content.ReadAsStreamAsync().Result)
            {
                var responseMessage = new ODataResponseMessage(stream);

                var context = new ODataDeserializerContext() { Model = Model, Request = response.RequestMessage };
                using (var messageReader = new ODataMessageReader(responseMessage, _settings, Model))
                {
                    ODataResult result = GetODataResult(messageReader);
                    while (result.ODataReader.Read())
                    {
                        if (result.ODataReader.State == ODataReaderState.EntryEnd)
                        {
                            var entry = (ODataEntry)result.ODataReader.Item;
                            var entityType = (IEdmEntityType)Model.FindType(entry.TypeName);
                            var entityTypeReference = new EdmEntityTypeReference(entityType, false);
                            var navigationLinks = new ODataEntryWithNavigationLinks(entry);

                            result.AddResult(_deserializer.ReadEntry(navigationLinks, entityTypeReference, context));
                        }
                    }

                    return result.Result;
                }
            }
        }

        public IEdmModel Model
        {
            get
            {
                return _model;
            }
        }
    }
}
